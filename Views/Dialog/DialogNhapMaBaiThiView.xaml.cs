using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    public partial class DialogNhapMaBaiThiView : Window
    {
        private Window _parentWindow;
        private Effect _originalEffect;
        private Border _darkOverlay;

        public DialogNhapMaBaiThiView(DialogNhapMaBaiThiViewModel dnmbtViewModel)
        {
            InitializeComponent();
            this.DataContext = dnmbtViewModel;

            // Cấu hình dialog
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;

            AnimationHelper.ApplyFadeIn(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Tìm parent window
            _parentWindow = Application.Current.Windows.OfType<Window>()
                .FirstOrDefault(w => w.IsActive && w != this);

            if (_parentWindow != null)
            {
                // Lưu effect gốc
                _originalEffect = _parentWindow.Effect;

                // Thêm Blur effect
                _parentWindow.Effect = new BlurEffect
                {
                    Radius = 10,
                    KernelType = KernelType.Gaussian
                };

                // Tạo dark overlay
                CreateDarkOverlay();
            }
        }

        private void CreateDarkOverlay()
        {
            // Tìm Grid chính của parent window
            var parentGrid = FindVisualChild<Grid>(_parentWindow);
            if (parentGrid != null)
            {
                // Tạo overlay đen với opacity 50%
                _darkOverlay = new Border
                {
                    Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)), // 50% black
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    IsHitTestVisible = false // Không chặn mouse events
                };

                // Đặt overlay lên trên tất cả các controls khác
                Grid.SetRowSpan(_darkOverlay, parentGrid.RowDefinitions.Count == 0 ? 1 : parentGrid.RowDefinitions.Count);
                Grid.SetColumnSpan(_darkOverlay, parentGrid.ColumnDefinitions.Count == 0 ? 1 : parentGrid.ColumnDefinitions.Count);

                // Thêm overlay vào cuối để nó nằm trên cùng
                parentGrid.Children.Add(_darkOverlay);
                Panel.SetZIndex(_darkOverlay, 9999);
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        protected override void OnClosed(EventArgs e)
        {
            // Khôi phục parent window
            if (_parentWindow != null)
            {
                _parentWindow.Effect = _originalEffect;

                // Xóa dark overlay
                if (_darkOverlay != null && _darkOverlay.Parent is Grid parentGrid)
                {
                    parentGrid.Children.Remove(_darkOverlay);
                }
            }

            base.OnClosed(e);
        }

        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is Viewbox viewbox
                && viewbox.Child is Path pathIcon)
            {
                if (OTPPasswordBox.Visibility == Visibility.Visible)
                {
                    // Show password
                    string currentPassword = OTPPasswordBox.Password;
                    OTPTextBox.Text = currentPassword;
                    OTPPasswordBox.Visibility = Visibility.Collapsed;
                    OTPTextBox.Visibility = Visibility.Visible;

                    // Đặt con trỏ ở cuối TextBox
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OTPTextBox.Focus();
                        OTPTextBox.CaretIndex = OTPTextBox.Text.Length;
                    }), System.Windows.Threading.DispatcherPriority.Render);

                    // Icon mắt gạch
                    pathIcon.Data = Geometry.Parse("M11.83,9L15,12.16C15,12.11 15,12.05 15,12A3,3 0 0,0 12,9C11.94,9 11.89,9 11.83,9M7.53,9.8L9.08,11.35C9.03,11.56 9,11.77 9,12A3,3 0 0,0 12,15C12.22,15 12.44,14.97 12.65,14.92L14.2,16.47C13.53,16.8 12.79,17 12,17A5,5 0 0,1 7,12C7,11.21 7.2,10.47 7.53,9.8M2,4.27L4.28,6.55L4.73,7C3.08,8.3 1.78,10 1,12C2.73,16.39 7,19.5 12,19.5C13.55,19.5 15.03,19.2 16.38,18.66L16.81,19.08L19.73,22L21,20.73L3.27,3M12,7A5,5 0 0,1 17,12C17,12.64 16.87,13.26 16.64,13.82L19.57,16.75C21.07,15.5 22.27,13.86 23,12C21.27,7.61 17,4.5 12,4.5C10.6,4.5 9.26,4.75 8,5.2L10.17,7.35C10.74,7.13 11.35,7 12,7Z");
                }
                else
                {
                    // Hide password
                    string currentText = OTPTextBox.Text;
                    OTPPasswordBox.Password = currentText;
                    OTPTextBox.Visibility = Visibility.Collapsed;
                    OTPPasswordBox.Visibility = Visibility.Visible;

                    // Đặt con trỏ ở cuối PasswordBox bằng cách sử dụng reflection
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        OTPPasswordBox.Focus();

                        // Sử dụng reflection để gọi internal method Select
                        var selectMethod = OTPPasswordBox.GetType().GetMethod("Select",
                            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (selectMethod != null)
                        {
                            selectMethod.Invoke(OTPPasswordBox, new object[] { currentText.Length, 0 });
                        }
                    }), System.Windows.Threading.DispatcherPriority.Render);

                    // Icon mắt thường
                    pathIcon.Data = Geometry.Parse("M12,9A3,3 0 0,0 9,12A3,3 0 0,0 12,15A3,3 0 0,0 15,12A3,3 0 0,0 12,9M12,17A5,5 0 0,1 7,12A5,5 0 0,1 12,7A5,5 0 0,1 17,12A5,5 0 0,1 12,17M12,4.5C7,4.5 2.73,7.61 1,12C2.73,16.39 7,19.5 12,19.5C17,19.5 21.27,16.39 23,12C21.27,7.61 17,4.5 12,4.5Z");
                }
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