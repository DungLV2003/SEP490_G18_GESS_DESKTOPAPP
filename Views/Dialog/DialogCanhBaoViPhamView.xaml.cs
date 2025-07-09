using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Dialog cảnh báo vi phạm khi sinh viên tab ra ngoài
    /// Hiển thị countdown timer và xử lý theo từng cấp độ vi phạm
    /// </summary>
    public partial class DialogCanhBaoViPhamView : Window
    {
        private DialogCanhBaoViPhamViewModel ViewModel => DataContext as DialogCanhBaoViPhamViewModel;

        public DialogCanhBaoViPhamView(DialogCanhBaoViPhamViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;

            // Subscribe vào event để tự đóng dialog
            viewModel.RequestClose += () =>
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Nhận RequestClose event - đóng dialog");
                this.Close();
            };

            // Thiết lập dialog không thể đóng bằng các cách thông thường
            SetupSecureDialog();

            System.Diagnostics.Debug.WriteLine($"[DEBUG] DialogCanhBaoViPhamView: Khởi tạo cho vi phạm lần {viewModel.ViolationCount}");
        }

        private void SetupSecureDialog()
        {
            // Làm dialog luôn ở trên cùng và giữa màn hình
            this.Topmost = true;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.ShowInTaskbar = false;

            // Vô hiệu hóa nút đóng (X) và không cho resize
            this.ResizeMode = ResizeMode.NoResize;

            // Chặn Alt+F4 và các phím tắt đóng dialog
            this.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Vô hiệu hóa nút đóng (X) trên title bar
            var hwnd = new WindowInteropHelper(this).Handle;
            var style = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, style & ~WS_SYSMENU);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Chặn Alt+F4, Escape và các phím có thể đóng dialog
            if ((e.Key == Key.F4 && Keyboard.Modifiers == ModifierKeys.Alt) ||
                e.Key == Key.Escape)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Chặn phím tắt đóng dialog");
                e.Handled = true;
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Dialog giờ có thể đóng tự do khi countdown hết hoặc khi được yêu cầu
            // Không cần check CanCloseDialog nữa vì logic đã được handle trong ViewModel
            
            // Cleanup timer khi đóng
            ViewModel?.Dispose();
            base.OnClosing(e);
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] ContinueButton_Click được gọi");
            
            if (ViewModel != null)
            {
                ViewModel.HandleButtonClick();
                // Không cần check CanCloseDialog vì ViewModel sẽ tự gọi RequestClose khi cần
            }
        }

        // Windows API để vô hiệu hóa nút đóng
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }

    #region Converters

    /// <summary>
    /// Converter để hiển thị hướng dẫn dựa trên có countdown hay không
    /// </summary>
    public class ShowCountdownToInstructionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool showCountdown)
            {
                if (showCountdown)
                {
                    return "Dialog sẽ tự động đóng khi hết thời gian phạt. Không được chuyển ứng dụng khác!";
                }
                else
                {
                    return "Nhấn nút để xác nhận nộp bài và xem kết quả.";
                }
            }
            return "Vui lòng làm theo hướng dẫn.";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter để đổi màu button dựa trên số lần vi phạm
    /// </summary>
    public class ViolationCountToButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int violationCount)
            {
                return violationCount switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(40, 167, 69)),   // Green
                    2 => new SolidColorBrush(Color.FromRgb(255, 193, 7)),   // Yellow  
                    3 => new SolidColorBrush(Color.FromRgb(220, 53, 69)),   // Red
                    _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))  // Gray
                };
            }
            return new SolidColorBrush(Color.FromRgb(108, 117, 125));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter để đổi màu text button dựa trên số lần vi phạm
    /// </summary>
    public class ViolationCountToButtonTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int violationCount)
            {
                return violationCount switch
                {
                    1 => new SolidColorBrush(Colors.White),
                    2 => new SolidColorBrush(Colors.Black),
                    3 => new SolidColorBrush(Colors.White),
                    _ => new SolidColorBrush(Colors.White)
                };
            }
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
} 