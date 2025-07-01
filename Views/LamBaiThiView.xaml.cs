using ICSharpCode.AvalonEdit.Highlighting;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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
using static SEP490_G18_GESS_DESKTOPAPP.ViewModels.LamBaiThiViewModel;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for LamBaiThiView.xaml
    /// </summary>
    public partial class LamBaiThiView : Window
    {
        private LamBaiThiViewModel _viewModel;
        private bool _isExamSubmitted = false;
        private bool _isUpdatingEditorFromViewModel = false; // Flag để tránh infinite loop
        // Windows API để chặn phím tắt
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;

        public LamBaiThiView(LamBaiThiViewModel lbtViewModel)
        {
            InitializeComponent();
            _viewModel = lbtViewModel;
            this.DataContext = _viewModel;

            // Full screen mode for exam
            // Full screen mode cho bài thi
            //SetupExamMode();
            // Setup practice answer editor NGAY SAU KHI INITIALIZE
            // Đăng ký sự kiện cho ItemsControl
            this.Loaded += (s, e) =>
            {
                // Lắng nghe sự kiện CurrentQuestionIndex thay đổi để cập nhật editor hiển thị
                _viewModel.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(_viewModel.CurrentQuestionIndex) ||
                        args.PropertyName == nameof(_viewModel.CurrentPracticeQuestion))
                    {
                        // Khi thay đổi câu hỏi, cập nhật UI
                        UpdatePracticeQuestionUI();
                    }
                };

                // Khởi tạo ban đầu
                UpdatePracticeQuestionUI();
            };
                AnimationHelper.ApplyFadeIn(this);
  
        }

        private void SetupExamMode()
        {
            // Full screen
            this.WindowState = WindowState.Maximized;
            //this.WindowStyle = WindowStyle.None;
            //this.ResizeMode = ResizeMode.NoResize;
            this.Topmost = true; // Luôn ở trên cùng

            // Ẩn taskbar
            //this.ShowInTaskbar = false;
        }
        // Thêm method mới để setup practice answer với AvalonEdit
        //private void SetupPracticeAnswerEditor()
        //{
        //    if (PracticeAnswerEditor != null && _viewModel != null)
        //    {
        //        System.Diagnostics.Debug.WriteLine("[DEBUG] SetupPracticeAnswerEditor: Starting setup...");

        //        // Set default syntax highlighting to Text
        //        PracticeAnswerEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Text");

        //        // QUAN TRỌNG: Bind text từ ViewModel sang Editor khi question thay đổi
        //        _viewModel.PropertyChanged += (s, e) =>
        //        {
        //            if (e.PropertyName == nameof(_viewModel.CurrentPracticeQuestion))
        //            {
        //                UpdateEditorFromViewModel();
        //            }
        //        };

        //        // Bind text từ Editor về ViewModel khi user nhập
        //        PracticeAnswerEditor.TextChanged += (s, e) =>
        //        {
        //            if (!_isUpdatingEditorFromViewModel && _viewModel.CurrentPracticeQuestion != null)
        //            {
        //                System.Diagnostics.Debug.WriteLine($"[DEBUG] Editor TextChanged: Updating answer for question {_viewModel.CurrentQuestionIndex + 1}");
        //                _viewModel.CurrentPracticeQuestion.Answer = PracticeAnswerEditor.Text;
        //            }
        //        };

        //        // Load initial content nếu đã có CurrentPracticeQuestion
        //        UpdateEditorFromViewModel();

        //        System.Diagnostics.Debug.WriteLine("[DEBUG] SetupPracticeAnswerEditor: Setup completed");
        //    }
        //}
        private void UpdatePracticeQuestionUI()
        {
            if (_viewModel.ExamType == ExamType.Practice &&
                _viewModel.CurrentPracticeQuestion != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdatePracticeQuestionUI: Question {_viewModel.CurrentQuestionIndex + 1}");
            }
        }
        //private void UpdateEditorFromViewModel()
        //{
        //    if (_viewModel.CurrentPracticeQuestion != null && PracticeAnswerEditor != null)
        //    {
        //        _isUpdatingEditorFromViewModel = true;

        //        var answer = _viewModel.CurrentPracticeQuestion.Answer ?? "";
        //        System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateEditorFromViewModel: Question {_viewModel.CurrentQuestionIndex + 1}, Answer='{answer}'");

        //        PracticeAnswerEditor.Text = answer;

        //        _isUpdatingEditorFromViewModel = false;
        //    }
        //}

        // Method để xử lý thay đổi syntax highlighting
        private void AnswerModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string mode = selectedItem.Tag?.ToString() ?? "Text";

                // Tìm editor tương ứng trong template
                var parentBorder = FindVisualParent<Border>(comboBox);
                if (parentBorder != null)
                {
                    var editor = FindVisualChild<ICSharpCode.AvalonEdit.TextEditor>(parentBorder);
                    if (editor != null)
                    {
                        // Đặt syntax highlighting tương ứng
                        var highlighting = mode switch
                        {
                            "CSharp" => HighlightingManager.Instance.GetDefinition("C#"),
                            "Python" => HighlightingManager.Instance.GetDefinition("Python"),
                            "Java" => HighlightingManager.Instance.GetDefinition("Java"),
                            "HTML" => HighlightingManager.Instance.GetDefinition("HTML"),
                            "CSS" => HighlightingManager.Instance.GetDefinition("CSS"),
                            "JavaScript" => HighlightingManager.Instance.GetDefinition("JavaScript"),
                            "SQL" => HighlightingManager.Instance.GetDefinition("SQL"),
                            _ => HighlightingManager.Instance.GetDefinition("Text")
                        };

                        editor.SyntaxHighlighting = highlighting;

                        // Tùy chỉnh options theo ngôn ngữ
                        if (mode == "Text")
                        {
                            editor.ShowLineNumbers = false;
                            editor.WordWrap = true;
                        }
                        else
                        {
                            editor.ShowLineNumbers = true;
                            editor.WordWrap = false;
                        }
                    }
                }
            }
        }
        private T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            T parent = parentObject as T;
            if (parent != null)
                return parent;
            else
                return FindVisualParent<T>(parentObject);
        }


        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }


        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Lấy handle của window
            var hwnd = new WindowInteropHelper(this).Handle;

            // Disable system menu (Alt+F4, Alt+Space)
            var currentStyle = GetWindowLong(hwnd, GWL_STYLE);
            SetWindowLong(hwnd, GWL_STYLE, currentStyle & ~WS_SYSMENU);
        }
        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Chặn Alt + F4
            //if (e.Key == Key.System && e.SystemKey == Key.F4)
            //{
            //    e.Handled = true;
            //    return;
            //}

            // Chặn Alt + Tab
            if (Keyboard.Modifiers == ModifierKeys.Alt && e.Key == Key.Tab)
            {
                e.Handled = true;
                return;
            }

            // Chặn Windows key
            if (e.Key == Key.LWin || e.Key == Key.RWin)
            {
                e.Handled = true;
                return;
            }

            // Chặn Ctrl + Alt + Del (khó chặn hoàn toàn, nhưng có thể cảnh báo)
            //if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt) && e.Key == Key.Delete)
            //{
            //    e.Handled = true;
            //    MessageBox.Show("Không được phép sử dụng phím tắt này trong khi thi!", "Cảnh báo",
            //        MessageBoxButton.OK, MessageBoxImage.Warning);
            //    return;
            //}
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //Nếu bài thi đã được nộp thì cho phép đóng
            if (_isExamSubmitted)
            {
                base.OnClosing(e);
                return;
            }

            // Ngăn chặn đóng window khi đang thi
            e.Cancel = true;

            MessageBox.Show(
                "Không thể thoát trong khi đang làm bài thi!\nVui lòng nộp bài trước khi thoát.",
                "Thông báo",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }

        // Phương thức để set flag khi nộp bài
        public void SetExamSubmitted()
        {
            _isExamSubmitted = true;
        }
        // Event handlers - logic được xử lý qua PropertyChanged trong ViewModel
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] RadioButton_Checked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {_viewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {_viewModel.CurrentQuestion?.IsMultipleChoice}");
                System.Diagnostics.Debug.WriteLine($"  - ⚠️ SHOULD NOT FIRE for Multiple Choice questions!");
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CheckBox_Checked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {_viewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {_viewModel.CurrentQuestion?.IsMultipleChoice}");
                System.Diagnostics.Debug.WriteLine($"  - ✅ Expected for Multiple Choice questions");
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is AnswerViewModel answer)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] CheckBox_Unchecked: Answer {answer.AnswerId}");
                System.Diagnostics.Debug.WriteLine($"  - CurrentQuestion: {_viewModel.CurrentQuestion?.QuestionId}");
                System.Diagnostics.Debug.WriteLine($"  - IsMultipleChoice: {_viewModel.CurrentQuestion?.IsMultipleChoice}");
            }
        }
    }

    #region Template Selectors

    public class AnswerTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (container is FrameworkElement element && item is QuestionViewModel question)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] AnswerTemplateSelector: Question {question.QuestionId}, IsMultipleChoice={question.IsMultipleChoice}");
                
                if (question.IsMultipleChoice)
                {
                    var template = element.FindResource("MultipleChoiceTemplate") as DataTemplate;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Selected MultipleChoiceTemplate for question {question.QuestionId}");
                    return template;
                }
                else
                {
                    var template = element.FindResource("SingleChoiceTemplate") as DataTemplate;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Selected SingleChoiceTemplate for question {question.QuestionId}");
                    return template;
                }
            }
            
            return base.SelectTemplate(item, container);
        }
    }

    #endregion

    #region Converters

    public class ExamTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var result = Visibility.Collapsed;
            
            if (value is ExamType examType && parameter is string expectedType)
            {
                if (expectedType == "MultipleChoice")
                    result = examType == ExamType.MultipleChoice ? Visibility.Visible : Visibility.Collapsed;
                else if (expectedType == "Practice")
                    result = examType == ExamType.Practice ? Visibility.Visible : Visibility.Collapsed;
            }
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ExamTypeToVisibilityConverter: ExamType={value}, Expected={parameter}, Result={result}");
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để chọn style cho button câu hỏi
    public class QuestionButtonStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4)
            {
                bool isAnswered = values[0] is bool answered && answered;
                bool isCurrent = values[1] is bool current && current;
                bool isMarked = values[2] is bool marked && marked;
                FrameworkElement element = values[3] as FrameworkElement;

                // Debug
                System.Diagnostics.Debug.WriteLine($"QuestionButtonStyleConverter: IsAnswered={isAnswered}, IsCurrent={isCurrent}, IsMarked={isMarked}");

                if (element != null)
                {
                    try
                    {
                        if (isCurrent)
                            return element.FindResource("CurrentButtonStyle");
                        else if (isMarked)
                            return element.FindResource("MarkedButtonStyle");
                        else if (isAnswered)
                            return element.FindResource("AnsweredButtonStyle");
                        else
                            return element.FindResource("NumberButtonStyle");
                    }
                    catch
                    {
                        // Fallback to default style if resource not found
                        return DependencyProperty.UnsetValue;
                    }
                }
            }
            return DependencyProperty.UnsetValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để xác định RadioButton hay CheckBox
    public class BoolToAnswerSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMultipleChoice)
            {
                return isMultipleChoice ? "CheckBox" : "RadioButton";
            }
            return "RadioButton";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho việc hiển thị text của mark button
    public class IsMarkedToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMarked)
            {
                return isMarked ? "Bỏ đánh dấu" : "Đánh dấu";
            }
            return "Đánh dấu";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để format alphabet cho câu trả lời (A, B, C, D...)
    public class IndexToAlphabetConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 && values[0] is IList list && values[1] != null)
            {
                int index = list.IndexOf(values[1]);
                if (index >= 0)
                {
                    return (char)('A' + index);
                }
            }
            return "A";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // ===== CÁC CONVERTERS CÒN THIẾU =====

    // Converter to add 1 to index (for display)
    public class AddOneConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index + 1;
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return index - 1;
            }
            return 0;
        }
    }

    // Converter for null to visibility
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";

            if (value == null || (value is string str && string.IsNullOrEmpty(str)))
            {
                return isInverse ? Visibility.Visible : Visibility.Collapsed;
            }

            return isInverse ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Extended BooleanToVisibilityConverter with parameter support
    public class BoolToVisibilityParameterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter?.ToString() == "Inverse";

            // Debug: Log conversion for IsMultipleChoice
            var resultVisibility = Visibility.Collapsed;
            if (value is bool boolVal)
            {
                if (isInverse)
                {
                    resultVisibility = boolVal ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    resultVisibility = boolVal ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                resultVisibility = isInverse ? Visibility.Visible : Visibility.Collapsed;
            }
            
            string templateType = isInverse ? "SingleChoice (RadioButton)" : "MultipleChoice (CheckBox)";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] BoolToVisibilityConverter ({templateType}): IsMultipleChoice={value}, showing={resultVisibility}");

            if (value is bool boolValue)
            {
                if (isInverse)
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                else
                {
                    return boolValue ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            return isInverse ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}