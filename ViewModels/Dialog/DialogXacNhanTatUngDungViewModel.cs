using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.RunningApplicationDTO;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogXacNhanTatUngDungViewModel : BaseViewModel
    {
        private readonly RunningApplication _application;
        private readonly Action _onConfirm;

        #region Properties
        public string ApplicationName => _application?.ApplicationName ?? "ứng dụng này";

        #endregion

        #region Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        #region Events
        public event Action<bool?> DialogResult;
        #endregion

        public DialogXacNhanTatUngDungViewModel(RunningApplication application, Action onConfirm = null)
        {
            _application = application;
            _onConfirm = onConfirm;

            ConfirmCommand = new RelayCommand(OnConfirm);
            CancelCommand = new RelayCommand(OnCancel);
        }

        private void OnConfirm()
        {
            try
            {
                // Execute the confirm action if provided
                _onConfirm?.Invoke();

                // Close dialog with positive result
                DialogResult?.Invoke(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during confirm action: {ex.Message}");
                // Still close the dialog even if action fails
                DialogResult?.Invoke(true);
            }
        }

        private void OnCancel()
        {
            // Close dialog with negative result
            DialogResult?.Invoke(false);
        }
    }
} 