using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Windows.Threading;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogCanhBaoViPhamViewModel : BaseViewModel
    {
        private readonly Action _onPenaltyComplete;
        private readonly Action _onAutoSubmit;
        private DispatcherTimer _countdownTimer;
        private int _totalSecondsRemaining;

        // Event để thông báo dialog cần tự đóng
        public event Action RequestClose;

        private int _violationCount;
        public int ViolationCount
        {
            get => _violationCount;
            set => SetProperty(ref _violationCount, value);
        }

        private string _countdownText;
        public string CountdownText
        {
            get => _countdownText;
            set => SetProperty(ref _countdownText, value);
        }

        private string _warningMessage;
        public string WarningMessage
        {
            get => _warningMessage;
            set => SetProperty(ref _warningMessage, value);
        }

        private string _nextViolationWarning;
        public string NextViolationWarning
        {
            get => _nextViolationWarning;
            set => SetProperty(ref _nextViolationWarning, value);
        }

        private bool _showCountdown;
        public bool ShowCountdown
        {
            get => _showCountdown;
            set => SetProperty(ref _showCountdown, value);
        }

        // Ẩn button - chỉ hiển thị countdown, dialog tự đóng
        public bool ShowButton => ViolationCount >= 3; // Chỉ hiện button cho vi phạm lần 3

        private string _buttonText;
        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        public DialogCanhBaoViPhamViewModel(int violationCount, Action onPenaltyComplete, Action onAutoSubmit)
        {
            ViolationCount = violationCount;
            _onPenaltyComplete = onPenaltyComplete;
            _onAutoSubmit = onAutoSubmit;

            SetupViolationDetails();
            
            if (ViolationCount < 3)
            {
                StartCountdownTimer();
            }
        }

        private void SetupViolationDetails()
        {
            switch (ViolationCount)
            {
                case 1:
                    WarningMessage = "Bạn đã chuyển sang ứng dụng khác trong quá trình làm bài thi.\nĐây là hành vi không được phép và có thể ảnh hưởng đến kết quả thi.";
                    NextViolationWarning = "Nếu vi phạm lần 2, bạn sẽ bị phạt 30 giây không được làm bài.";
                    
                    _totalSecondsRemaining = 15; // Lần 1: 15 giây
                    
                    ShowCountdown = true;
                    break;

                case 2:
                    WarningMessage = "Bạn đã vi phạm lần thứ 2 bằng cách chuyển sang ứng dụng khác.\nĐây là cảnh báo nghiêm trọng!";
                    NextViolationWarning = "Nếu vi phạm lần 3, bài thi sẽ được tự động nộp ngay lập tức.";
                    
                    _totalSecondsRemaining = 30; // Lần 2: 30 giây
                    
                    ShowCountdown = true;
                    break;

                case 3:
                    WarningMessage = "Vi phạm lần thứ 3! Bạn đã chuyển sang ứng dụng khác quá nhiều lần.\nBài thi sẽ được nộp ngay bây giờ.";
                    NextViolationWarning = "Bài thi của bạn sẽ được tự động nộp với điểm số hiện tại.";
                    ButtonText = "Xác nhận nộp bài";
                    ShowCountdown = false;
                    break;
            }

            if (ShowCountdown)
            {
                UpdateCountdownDisplay();
            }
        }

        private void StartCountdownTimer()
        {
            _countdownTimer = new DispatcherTimer();
            _countdownTimer.Interval = TimeSpan.FromSeconds(1);
            _countdownTimer.Tick += CountdownTimer_Tick;
            _countdownTimer.Start();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Bắt đầu countdown timer với {_totalSecondsRemaining} giây");
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            _totalSecondsRemaining--;
            UpdateCountdownDisplay();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Countdown: {_totalSecondsRemaining} giây còn lại");

            if (_totalSecondsRemaining <= 0)
            {
                // Hết thời gian phạt - TỰ ĐỘNG ĐÓNG DIALOG
                _countdownTimer?.Stop();
                
                System.Diagnostics.Debug.WriteLine("[DEBUG] Hết thời gian phạt - TỰ ĐỘNG ĐÓNG DIALOG");
                
                // Gọi callback để thông báo hết thời gian phạt
                _onPenaltyComplete?.Invoke();
                
                // Yêu cầu View đóng dialog
                RequestClose?.Invoke();
            }
        }

        private void UpdateCountdownDisplay()
        {
            if (_totalSecondsRemaining >= 365 * 24 * 60 * 60) // >= 365 ngày (1 năm)
            {
                var years = _totalSecondsRemaining / (365 * 24 * 60 * 60);
                var remainingAfterYears = _totalSecondsRemaining % (365 * 24 * 60 * 60);
                
                var days = remainingAfterYears / (24 * 60 * 60);
                var remainingAfterDays = remainingAfterYears % (24 * 60 * 60);
                
                var hours = remainingAfterDays / (60 * 60);
                
                CountdownText = $"{years} năm {days} ngày {hours} giờ";
            }
            else if (_totalSecondsRemaining >= 24 * 60 * 60) // >= 1 ngày
            {
                var days = _totalSecondsRemaining / (24 * 60 * 60);
                var remainingAfterDays = _totalSecondsRemaining % (24 * 60 * 60);
                
                var hours = remainingAfterDays / (60 * 60);
                var remainingAfterHours = remainingAfterDays % (60 * 60);
                
                var minutes = remainingAfterHours / 60;
                var seconds = remainingAfterHours % 60;
                
                CountdownText = $"{days} ngày {hours} giờ {minutes} phút {seconds} giây";
            }
            else // < 1 ngày
            {
                var hours = _totalSecondsRemaining / (60 * 60);
                var remainingAfterHours = _totalSecondsRemaining % (60 * 60);
                
                var minutes = remainingAfterHours / 60;
                var seconds = remainingAfterHours % 60;
                
                CountdownText = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            }
        }

        /// <summary>
        /// Xử lý khi người dùng ấn button trong dialog (chỉ cho vi phạm lần 3)
        /// </summary>
        public void HandleButtonClick()
        {
            if (ViolationCount == 3)
            {
                // Vi phạm lần 3 - nộp bài
                System.Diagnostics.Debug.WriteLine("[DEBUG] Vi phạm lần 3 - gọi auto submit");
                _onAutoSubmit?.Invoke();
                
                // Đóng dialog sau khi nộp bài
                RequestClose?.Invoke();
            }
        }

        public void Dispose()
        {
            _countdownTimer?.Stop();
            _countdownTimer = null;
        }
    }
} 