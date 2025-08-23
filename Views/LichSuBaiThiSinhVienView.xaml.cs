using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO;
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
    /// Interaction logic for LichSuBaiThiSinhVienView.xaml
    /// </summary>
    public partial class LichSuBaiThiSinhVienView : Window
    {
        public LichSuBaiThiSinhVienView(LichSuBaiThiSinhVienViewModel lsbtsvViewModel)
        {
            InitializeComponent();
            this.DataContext = lsbtsvViewModel;
            
            // Setup window to be maximized and non-resizable
            SetupWindow();
            
            // Không gọi AnimationHelper.ApplyFadeIn ở đây nữa
            // Sẽ gọi trong Window_Loaded event
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
            
            // Prevent minimize via Alt+Space menu
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Space && 
                    (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                {
                    e.Handled = true;
                }
            };
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
                "Bạn có chắc chắn muốn đóng trang lịch sử bài thi?", 
                "Xác nhận đóng", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }

        private void CustomTitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Allow dragging the window by the title bar
            this.DragMove();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            // Ensure window is topmost and focused when activated
            this.Topmost = true;
            this.Focus();
            this.Activate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Không áp dụng hiệu ứng fade in để chuyển view mượt mà
        }

        private void SubjectItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is AllSubjectBySemesterOfStudentDTOResponse subject)
            {
                var viewModel = this.DataContext as LichSuBaiThiSinhVienViewModel;
                if (viewModel != null)
                {
                    viewModel.SelectedSubject = subject;
                }
            }
        }
    }

    // Converters cần thiết
    public class ExamTypeToVietnameseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var raw = value?.ToString()?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(raw)) return "";
            if (raw.Contains("multi") || raw.Contains("trac") || raw.Contains("trắc")) return "Trắc nghiệm";
            if (raw.Contains("practice") || raw.Contains("tu luan") || raw.Contains("tự luận") || raw.Contains("essay")) return "Tự luận";
            return char.ToUpper(raw[0]) + raw.Substring(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class SelectedSubjectToBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int selectedId && values[1] is int currentId)
            {
                return selectedId == currentId ? new SolidColorBrush(Color.FromRgb(33, 150, 243)) : new SolidColorBrush(Color.FromRgb(227, 242, 253));
            }
            return new SolidColorBrush(Color.FromRgb(227, 242, 253));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SelectedSubjectToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int selectedId && values[1] is int currentId)
            {
                return selectedId == currentId ? Brushes.White : Brushes.Black;
            }
            return Brushes.Black;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AverageScoreConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ObjectModel.ObservableCollection<HistoryExamOfStudentDTOResponse> examList && examList.Count > 0)
            {
                var average = examList.Average(x => x.Score);
                return average.ToString("F2");
            }
            return "0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SubjectSelectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is AllSubjectBySemesterOfStudentDTOResponse selectedSubject &&
                values[1] is AllSubjectBySemesterOfStudentDTOResponse currentSubject)
            {
                if (selectedSubject != null && currentSubject != null && selectedSubject.Id == currentSubject.Id)
                {
                    return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // Blue for selected
                }
            }
            return new SolidColorBrush(Color.FromRgb(227, 242, 253)); // Light blue for normal
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}