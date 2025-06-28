using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogXacNhanNopBaiThiViewModel : BaseViewModel
    {
        #region Properties
        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        private string _timeSpentText;
        public string TimeSpentText
        {
            get => _timeSpentText;
            set => SetProperty(ref _timeSpentText, value);
        }

        public bool IsConfirmed { get; private set; }
        #endregion

        #region Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public DialogXacNhanNopBaiThiViewModel(int answeredQuestions, int totalQuestions, string timeSpent)
        {
            ProgressText = $"{answeredQuestions}/{totalQuestions} câu hỏi";
            TimeSpentText = timeSpent;
            IsConfirmed = false;

            ConfirmCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Confirm()
        {
            IsConfirmed = true;
            CloseDialog();
        }

        private void Cancel()
        {
            IsConfirmed = false;
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogXacNhanNopBaiThiView>()
                .FirstOrDefault()?.Close();
        }
    }
}