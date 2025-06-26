using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using SEP490_G18_GESS_DESKTOPAPP.Views.Base;
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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for HomePageView.xaml
    /// </summary>
    public partial class HomePageView : Window
    {
        //// Windows API để chặn phím tắt
        //[DllImport("user32.dll")]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        //[DllImport("user32.dll")]
        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        //[DllImport("user32.dll")]
        //private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        //[DllImport("user32.dll")]
        //private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        //private const int GWL_STYLE = -16;
        //private const int WS_MAXIMIZEBOX = 0x10000;
        //private const int WS_MINIMIZEBOX = 0x20000;

        //// Hot key constants
        //private const int MOD_WIN = 0x0008;
        //private const int VK_DOWN = 0x28;
        //private const int VK_UP = 0x26;
        //private const int VK_LEFT = 0x25;
        //private const int VK_RIGHT = 0x27;
        //private const int VK_M = 0x4D;
        //private const int VK_D = 0x44;

        public HomePageView(HomePageViewModel hpViewModel)
        {
            InitializeComponent();
            this.DataContext = hpViewModel;

            // Cấu hình window để fullscreen và không thể resize
            SetupWindow();

            // Đăng ký event handlers
            //RegisterEventHandlers();

            AnimationHelper.ApplyFadeIn(this);
        }

        private void SetupWindow()
        {
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            //this.WindowStyle = WindowStyle.None; // Ẩn hoàn toàn title bar
            //this.Topmost = true; // Luôn ở trên cùng

            //// Prevent window from being moved by dragging
            //this.MouseLeftButtonDown += (s, e) => { /* Do nothing - prevent dragging */ };
        }

        //private void RegisterEventHandlers()
        //{
        //    // Đăng ký keyboard events
        //    this.PreviewKeyDown += OnPreviewKeyDown;
        //    this.KeyDown += OnKeyDown;
        //    this.Loaded += OnWindowLoaded;

        //    // Prevent window state changes
        //    this.StateChanged += OnStateChanged;
        //}

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);

        //    // Lấy handle của window
        //    var hwnd = new WindowInteropHelper(this).Handle;

        //    // Disable minimize/maximize buttons
        //    var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
        //    SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX);

        //    // Register system-wide hotkeys to block them
        //    RegisterSystemHotKeys(hwnd);

        //    // Hook window messages để chặn system commands
        //    HwndSource source = HwndSource.FromHwnd(hwnd);
        //    source.AddHook(new HwndSourceHook(WndProc));
        //}

        //private void RegisterSystemHotKeys(IntPtr hwnd)
        //{
        //    // Block Win+Down
        //    RegisterHotKey(hwnd, 1, MOD_WIN, VK_DOWN);
        //    // Block Win+Up  
        //    RegisterHotKey(hwnd, 2, MOD_WIN, VK_UP);
        //    // Block Win+Left
        //    RegisterHotKey(hwnd, 3, MOD_WIN, VK_LEFT);
        //    // Block Win+Right
        //    RegisterHotKey(hwnd, 4, MOD_WIN, VK_RIGHT);
        //    // Block Win+M
        //    RegisterHotKey(hwnd, 5, MOD_WIN, VK_M);
        //    // Block Win+D
        //    RegisterHotKey(hwnd, 6, MOD_WIN, VK_D);
        //}

        //private void OnStateChanged(object sender, EventArgs e)
        //{
        //    // Force window to stay maximized
        //    if (this.WindowState != WindowState.Maximized)
        //    {
        //        this.WindowState = WindowState.Maximized;
        //    }
        //}

        //private void OnWindowLoaded(object sender, RoutedEventArgs e)
        //{
        //    // Đảm bảo window luôn fullscreen
        //    this.WindowState = WindowState.Maximized;
        //    this.Focus(); // Ensure window has focus to capture key events
        //}

        //private void OnKeyDown(object sender, KeyEventArgs e)
        //{
        //    // Additional key blocking
        //    OnPreviewKeyDown(sender, e);
        //}

        //private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    // Chặn các phím tắt Windows
        //    if (Keyboard.Modifiers == ModifierKeys.Windows)
        //    {
        //        // Chặn Win + Down (minimize)
        //        if (e.Key == Key.Down || e.Key == Key.PageDown)
        //        {
        //            e.Handled = true;
        //            return;
        //        }

        //        // Chặn Win + Left/Right (snap)
        //        if (e.Key == Key.Left || e.Key == Key.Right)
        //        {
        //            e.Handled = true;
        //            return;
        //        }

        //        // Chặn Win + Up (maximize/restore)
        //        if (e.Key == Key.Up)
        //        {
        //            e.Handled = true;
        //            return;
        //        }

        //        // Chặn Win + M (minimize all)
        //        if (e.Key == Key.M)
        //        {
        //            e.Handled = true;
        //            return;
        //        }

        //        // Chặn Win + D (show desktop)
        //        if (e.Key == Key.D)
        //        {
        //            e.Handled = true;
        //            return;
        //        }

        //        // Block any other Win key combinations
        //        e.Handled = true;
        //        return;
        //    }

        //    // Chặn Alt + F4
        //    if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4)
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    // Chặn Alt + Tab
        //    if (Keyboard.Modifiers == ModifierKeys.Alt && (e.SystemKey == Key.Tab || e.Key == Key.Tab))
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    // Block Ctrl+Alt+Del, Ctrl+Shift+Esc attempts
        //    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.Delete)
        //    {
        //        e.Handled = true;
        //        return;
        //    }

        //    if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Escape)
        //    {
        //        e.Handled = true;
        //        return;
        //    }
        //}

        //// Hook window messages using HwndSource
        //private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    const int WM_SYSCOMMAND = 0x0112;
        //    const int SC_MINIMIZE = 0xF020;
        //    const int SC_MAXIMIZE = 0xF030;
        //    const int SC_RESTORE = 0xF120;
        //    const int SC_MOVE = 0xF010;

        //    if (msg == WM_SYSCOMMAND)
        //    {
        //        int command = wParam.ToInt32() & 0xFFF0;
        //        if (command == SC_MINIMIZE || command == SC_MAXIMIZE || command == SC_RESTORE || command == SC_MOVE)
        //        {
        //            handled = true;
        //            return IntPtr.Zero;
        //        }
        //    }

        //    return IntPtr.Zero;
        //}

        //protected override void OnClosed(EventArgs e)
        //{
        //    // Unregister hotkeys when window closes
        //    var hwnd = new WindowInteropHelper(this).Handle;
        //    for (int i = 1; i <= 6; i++)
        //    {
        //        UnregisterHotKey(hwnd, i);
        //    }
        //    base.OnClosed(e);
        //}
    }
}
