using SEP490_G18_GESS_DESKTOPAPP.Helpers;
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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Interaction logic for DialogNhapMaBaiThiView.xaml
    /// </summary>
    public partial class DialogNhapMaBaiThiView : Window
    {
        public DialogNhapMaBaiThiView(DialogNhapMaBaiThiViewModel dnmbtViewModel)
        {
            InitializeComponent();
            this.DataContext = dnmbtViewModel;
            AnimationHelper.ApplyFadeIn(this);
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (OTPPasswordBox.Visibility == Visibility.Visible)
            {
                // Show password as text
                OTPTextBox.Text = OTPPasswordBox.Password;
                OTPPasswordBox.Visibility = Visibility.Collapsed;
                OTPTextBox.Visibility = Visibility.Visible;
                OTPTextBox.Focus();
            }
            else
            {
                // Hide password
                OTPPasswordBox.Password = OTPTextBox.Text;
                OTPTextBox.Visibility = Visibility.Collapsed;
                OTPPasswordBox.Visibility = Visibility.Visible;
                OTPPasswordBox.Focus();
            }
        }

        private void OTPPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is DialogNhapMaBaiThiViewModel vm)
            {
                vm.OTPCode = OTPPasswordBox.Password;
            }
        }

        private void OTPTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataContext is DialogNhapMaBaiThiViewModel vm)
            {
                vm.OTPCode = OTPTextBox.Text;
            }
        }
    }
}
