using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class DialogThongBaoLoiViewModel : BaseViewModel
    {
        #region Properties
        private string _errorTitle;
        public string ErrorTitle
        {
            get => _errorTitle;
            set => SetProperty(ref _errorTitle, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private string _errorDetail;
        public string ErrorDetail
        {
            get => _errorDetail;
            set => SetProperty(ref _errorDetail, value);
        }

        private string _buttonText;
        public string ButtonText
        {
            get => _buttonText;
            set => SetProperty(ref _buttonText, value);
        }

        // Action to execute when retry button is clicked
        private Action _onRetryAction;
        #endregion

        #region Commands
        public ICommand RetryCommand { get; }
        #endregion

        public DialogThongBaoLoiViewModel()
        {
            // Default values
            ErrorTitle = "Lỗi xác thực";
            ErrorMessage = "Mã OTP không chính xác";
            ErrorDetail = "Vui lòng kiểm tra lại mã OTP và thử lại.";
            ButtonText = "Nhập lại";

            RetryCommand = new RelayCommand(OnRetry);
        }

        public DialogThongBaoLoiViewModel(string title, string message, string detail, string buttonText = "Nhập lại", Action onRetryAction = null)
        {
            ErrorTitle = title;
            ErrorMessage = message;
            ErrorDetail = detail;
            ButtonText = buttonText;
            _onRetryAction = onRetryAction;

            RetryCommand = new RelayCommand(OnRetry);
        }

        private void OnRetry()
        {
            // Execute custom action if provided
            _onRetryAction?.Invoke();

            // Close this dialog
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogThongBaoLoiView>()
                .FirstOrDefault()?.Close();
        }
    }
}