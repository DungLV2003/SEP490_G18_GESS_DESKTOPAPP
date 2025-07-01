using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogExitConfirmationViewModel : BaseViewModel
    {
        private readonly Action _onConfirmAction;
        public bool IsConfirmed { get; private set; }

        #region Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public DialogExitConfirmationViewModel(Action onConfirmAction)
        {
            _onConfirmAction = onConfirmAction;
            IsConfirmed = false;

            ConfirmCommand = new RelayCommand(Confirm);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Confirm()
        {
            IsConfirmed = true;
            _onConfirmAction?.Invoke();
            CloseDialog();
        }

        private void Cancel()
        {
            IsConfirmed = false;
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogExitConfirmationView>()
                .FirstOrDefault()?.Close();
        }
    }
}
