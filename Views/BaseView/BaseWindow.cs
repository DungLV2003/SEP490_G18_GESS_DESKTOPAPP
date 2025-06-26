using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Base
{
    public class BaseWindow : Window
    {
        // Windows API để chặn phím tắt
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_MAXIMIZEBOX = 0x10000;
        private const int WS_MINIMIZEBOX = 0x20000;

        public BaseWindow()
        {
            // Set fullscreen và không resize được
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None; // Ẩn title bar

            // Hook keyboard events
            this.PreviewKeyDown += OnPreviewKeyDown;
            this.Loaded += OnWindowLoaded;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Lấy handle của window
            var hwnd = new WindowInteropHelper(this).Handle;

            // Disable minimize/maximize buttons
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Đảm bảo window luôn fullscreen
            this.WindowState = WindowState.Maximized;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Chặn các phím tắt Windows
            if (Keyboard.Modifiers == ModifierKeys.Windows)
            {
                // Chặn Win + Down (minimize)
                if (e.Key == Key.Down || e.Key == Key.PageDown)
                {
                    e.Handled = true;
                    return;
                }

                // Chặn Win + Left/Right (snap)
                if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    e.Handled = true;
                    return;
                }

                // Chặn Win + Up (maximize/restore)
                if (e.Key == Key.Up)
                {
                    e.Handled = true;
                    return;
                }

                // Chặn Win + M (minimize all)
                if (e.Key == Key.M)
                {
                    e.Handled = true;
                    return;
                }

                // Chặn Win + D (show desktop)
                if (e.Key == Key.D)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Chặn Alt + F4 nếu cần
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
            {
                // Uncomment nếu muốn chặn Alt+F4
                // e.Handled = true;
            }

            // Chặn Alt + Tab nếu cần (khó hơn, cần hook cấp thấp)
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Tab)
            {
                // Không thể chặn hoàn toàn từ WPF
            }
        }
    }
}