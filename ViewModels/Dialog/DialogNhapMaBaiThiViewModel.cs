using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogNhapMaBaiThiViewModel : BaseViewModel
    {
        private readonly ILamBaiThiService _lamBaiThiService;
        private readonly ExamListOfStudentResponse _examInfo;
        private readonly Guid _studentId;
        private readonly ExamType _examType;

        #region Properties
        private string _examCode;
        public string ExamCode
        {
            get => _examCode;
            set => SetProperty(ref _examCode, value);
        }

        private string _otpCode;
        public string OTPCode
        {
            get => _otpCode;
            set => SetProperty(ref _otpCode, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Properties để hiển thị thông tin loại thi
        public ExamType CurrentExamType => _examType;

        public string ExamTypeDisplayText => _examType == ExamType.MultipleChoice ? "Trắc nghiệm" : "Tự luận";

        public string DialogTitle => $"Nhập thông tin bài thi {ExamTypeDisplayText}";

        public string DialogSubtitle => $"Vui lòng nhập OTP để vào thi {ExamTypeDisplayText}";
        #endregion

        #region Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public DialogNhapMaBaiThiViewModel(
            ILamBaiThiService lamBaiThiService,
            ExamListOfStudentResponse examInfo,
            Guid studentId,
            ExamType examType)
        {
            _lamBaiThiService = lamBaiThiService;
            _examInfo = examInfo;
            _studentId = studentId;
            _examType = examType;

            // Auto-fill exam code based on exam type
            ExamCode = _examType == ExamType.MultipleChoice
                ? $"{examInfo.ExamName}"
                : $"{examInfo.ExamName}"; // Tự luận dùng ExamName

            ConfirmCommand = new RelayCommand(async () => await ConfirmAsync(), () => !IsLoading && !string.IsNullOrEmpty(OTPCode));
            CancelCommand = new RelayCommand(Cancel);
        }

        private async System.Threading.Tasks.Task ConfirmAsync()
        {
            try
            {
                IsLoading = true;

                if (_examType == ExamType.MultipleChoice)
                {
                    await HandleMultipleChoiceExamAsync();
                }
                else
                {
                    await HandlePracticeExamAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task HandleMultipleChoiceExamAsync()
        {
            var request = new CheckExamRequestDTO
            {
                ExamId = _examInfo.ExamId,
                Code = OTPCode,
                StudentId = _studentId
            };

            var result = await _lamBaiThiService.CheckExamNameAndCodeMEAsync(request);

            if (result != null)
            {
                MessageBox.Show($"Vào thi trắc nghiệm thành công!\n{result.Message}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: Navigate to Multiple Choice Exam view
                System.Diagnostics.Debug.WriteLine($"Navigating to Multiple Choice Exam with data: {result.SubjectName}");

                CloseDialog();
            }
            else
            {
                MessageBox.Show("Mã OTP không đúng hoặc bài thi trắc nghiệm không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task HandlePracticeExamAsync()
        {
            var request = new CheckPracticeExamRequestDTO
            {
                ExamName = _examInfo.ExamName,
                Code = OTPCode,
                StudentId = _studentId
            };

            var result = await _lamBaiThiService.CheckExamNameAndCodePEAsync(request);

            if (result != null)
            {
                MessageBox.Show($"Vào thi tự luận thành công!\n{result.Message}", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                // TODO: Navigate to Practice Exam view
                System.Diagnostics.Debug.WriteLine($"Navigating to Practice Exam with data: {result.SubjectName}");

                CloseDialog();
            }
            else
            {
                MessageBox.Show("Mã OTP không đúng hoặc bài thi tự luận không tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogNhapMaBaiThiView>().FirstOrDefault()?.Close();
        }

    }
}
