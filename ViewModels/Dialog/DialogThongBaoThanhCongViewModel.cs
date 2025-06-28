using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogThongBaoThanhCongViewModel : BaseViewModel
    {
        private DispatcherTimer _autoCloseTimer;

        #region Properties
        private string _successTitle;
        public string SuccessTitle
        {
            get => _successTitle;
            set => SetProperty(ref _successTitle, value);
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        private string _successDetail;
        public string SuccessDetail
        {
            get => _successDetail;
            set => SetProperty(ref _successDetail, value);
        }

        private int _countdown = 3;
        public int Countdown
        {
            get => _countdown;
            set => SetProperty(ref _countdown, value);
        }

        // Action to execute after success
        private Action _onSuccessAction;
        #endregion

        #region Commands
        public ICommand ContinueCommand { get; }
        #endregion

        public DialogThongBaoThanhCongViewModel()
        {
            // Default values
            SuccessTitle = "Thành công";
            SuccessMessage = "Xác thực thành công";
            SuccessDetail = "Đang chuyển đến trang làm bài thi...";

            ContinueCommand = new RelayCommand(OnContinue);
            StartAutoClose();
        }

        public DialogThongBaoThanhCongViewModel(string title, string message, string detail, Action onSuccessAction = null)
        {
            SuccessTitle = title;
            SuccessMessage = message;
            SuccessDetail = detail;
            _onSuccessAction = onSuccessAction;

            ContinueCommand = new RelayCommand(OnContinue);
            StartAutoClose();
        }

        private void StartAutoClose()
        {
            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Interval = TimeSpan.FromSeconds(1);
            _autoCloseTimer.Tick += (s, e) =>
            {
                Countdown--;
                if (Countdown <= 0)
                {
                    _autoCloseTimer.Stop();
                    OnContinue();
                }
            };
            _autoCloseTimer.Start();
        }

        private void OnContinue()
        {
            _autoCloseTimer?.Stop();

            // Execute custom action if provided
            _onSuccessAction?.Invoke();

            // Close this dialog
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogThongBaoThanhCongView>()
                .FirstOrDefault()?.Close();
        }
    }
}