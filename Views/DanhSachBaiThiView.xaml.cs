using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for DanhSachBaiThiView.xaml
    /// </summary>
    public partial class DanhSachBaiThiView : Window
    {
        public DanhSachBaiThiView(DanhSachBaiThiSinhVienViewModel dsbtViewModel)
        {
            InitializeComponent();
            
            // Apply consistent styling like KetQuaNopBaiView
            this.DataContext = dsbtViewModel;
            
            // Không gọi AnimationHelper.ApplyFadeIn ở đây nữa
            // Sẽ gọi trong Window_Loaded event
            
            // Setup window to be maximized and non-resizable
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Force window to be maximized
            this.WindowState = WindowState.Maximized;
            
            // Prevent resizing and minimizing
            this.ResizeMode = ResizeMode.NoResize;
            
            // Ensure window is topmost and focused
            this.Topmost = true;
            this.Focus();
            
            // Get screen dimensions and set window to cover entire screen immediately
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            var bounds = screen.Bounds;
            
            // Set window to cover entire screen including taskbar
            this.Left = bounds.Left;
            this.Top = bounds.Top;
            this.Width = bounds.Width;
            this.Height = bounds.Height;
            
            // Force window to be visible and on top
            this.Show();
            this.Activate();
            this.Focus();
            
            // Prevent window from being minimized by overriding the minimize command
            this.SourceInitialized += (s, e) =>
            {
                // Disable minimize button in taskbar
                var hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
                var value = GetWindowLong(hwnd, GWL_STYLE);
                SetWindowLong(hwnd, GWL_STYLE, (int)(value & ~WS_MINIMIZEBOX));
                
                // Ensure window is activated and focused
                this.Activate();
                this.Focus();
                
                // Force window to front using Win32 API
                SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_SHOWWINDOW);
            };
            
            // Prevent minimize via Alt+Space menu (nhưng cho phép chuyển đổi ngôn ngữ)
            this.PreviewKeyDown += (s, e) =>
            {
                // Cho phép Ctrl+Shift và Alt+Shift để chuyển đổi ngôn ngữ
                if ((Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.LeftShift) ||
                    (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.RightShift) ||
                    (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.LeftShift) ||
                    (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.RightShift))
                {
                    e.Handled = false; // Cho phép chuyển đổi ngôn ngữ
                    return;
                }
                
                // Chặn Alt+Space để ngăn minimize
                if (e.Key == System.Windows.Input.Key.Space && 
                    (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Alt) == System.Windows.Input.ModifierKeys.Alt)
                {
                    e.Handled = true;
                }
            };
            
            // Prevent minimize via window commands
            this.CommandBindings.Add(new System.Windows.Input.CommandBinding(
                System.Windows.Input.ApplicationCommands.Close,
                (s, e) => { },
                (s, e) => e.CanExecute = false));
        }
        
        // Win32 API constants and methods to disable minimize button and force window to front
        private const int GWL_STYLE = -16;
        private const int WS_MINIMIZEBOX = 0x20000;
        private const int HWND_TOPMOST = -1;
        private const int SWP_NOMOVE = 0x0002;
        private const int SWP_NOSIZE = 0x0001;
        private const int SWP_SHOWWINDOW = 0x0040;
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đóng ứng dụng?", 
                "Xác nhận đóng", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Ensure window is topmost and focused when activated
            this.Topmost = true;
            this.Focus();
            this.Activate();
            
            // Auto refresh data when returning to this page
            if (DataContext is DanhSachBaiThiSinhVienViewModel viewModel)
            {
                // Use Dispatcher to avoid blocking UI thread
                Dispatcher.BeginInvoke(() =>
                {
                    try
                    {
                        viewModel.RefreshCommand.Execute(null);
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't show to user to avoid spam
                        System.Diagnostics.Debug.WriteLine($"Auto refresh error: {ex.Message}");
                    }
                }, System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Không áp dụng hiệu ứng fade in để chuyển view mượt mà
            
            // Ensure window is properly positioned and focused when loaded
            this.Topmost = true;
            this.Focus();
            this.Activate();
            
            // Force window to front
            this.BringIntoView();
        }


    }
    
    // Converter cho STT
    public class AddOneMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int index)
                return (index + 1).ToString();
            return "1";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}