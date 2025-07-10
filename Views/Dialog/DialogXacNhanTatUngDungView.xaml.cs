using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Interaction logic for DialogXacNhanTatUngDungView.xaml
    /// </summary>
    public partial class DialogXacNhanTatUngDungView : Window
    {
        public DialogXacNhanTatUngDungView(DialogXacNhanTatUngDungViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Set owner to current active window
            var activeWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (activeWindow != null && activeWindow != this)
            {
                Owner = activeWindow;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Apply blur effect to parent window
                if (Owner != null)
                {
                    Owner.Effect = new BlurEffect { Radius = 5 };
                }
            }

            // Subscribe to dialog result events
            viewModel.DialogResult += OnDialogResult;
        }

        private void OnDialogResult(bool? result)
        {
            DialogResult = result;
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Remove blur effect from parent window
            if (Owner != null)
            {
                Owner.Effect = null;
            }

            // Unsubscribe from events
            if (DataContext is DialogXacNhanTatUngDungViewModel viewModel)
            {
                viewModel.DialogResult -= OnDialogResult;
            }

            base.OnClosed(e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Close dialog on Escape
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }
    }
} 