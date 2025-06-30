using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using static SEP490_G18_GESS_DESKTOPAPP.ViewModels.LamBaiThiViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using ICSharpCode.AvalonEdit.Highlighting;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for LamBaiThiView.xaml
    /// </summary>
    public partial class LamBaiThiView : Window
    {
        private LamBaiThiViewModel _viewModel;

        public LamBaiThiView(LamBaiThiViewModel lbtViewModel)
        {
            InitializeComponent();
            _viewModel = lbtViewModel;
            this.DataContext = _viewModel;

            // Full screen mode for exam
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;

            AnimationHelper.ApplyFadeIn(this);

            // Setup code editor
            SetupCodeEditor();

            // Bind RichTextBox for practice exam
            SetupRichTextBinding();
        }

        private void SetupCodeEditor()
        {
            if (CodeEditor != null)
            {
                // Set default syntax highlighting
                CodeEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");

                // Bind text to ViewModel
                CodeEditor.TextChanged += (s, e) =>
                {
                    if (_viewModel.CurrentPracticeQuestion != null)
                    {
                        _viewModel.CurrentPracticeQuestion.CodeAnswer = CodeEditor.Text;
                    }
                };

                // Update editor when question changes
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_viewModel.CurrentPracticeQuestion) &&
                        _viewModel.CurrentPracticeQuestion != null)
                    {
                        CodeEditor.Text = _viewModel.CurrentPracticeQuestion.CodeAnswer ?? "";
                    }
                };
            }
        }

        private void SetupRichTextBinding()
        {
            if (RichTextAnswer != null && _viewModel != null)
            {
                // Bind RichTextBox content to ViewModel
                RichTextAnswer.TextChanged += (s, e) =>
                {
                    if (_viewModel.CurrentPracticeQuestion != null)
                    {
                        var textRange = new TextRange(
                            RichTextAnswer.Document.ContentStart,
                            RichTextAnswer.Document.ContentEnd
                        );
                        _viewModel.CurrentPracticeQuestion.RichTextAnswer = textRange.Text;
                    }
                };

                // Update RichTextBox when question changes
                _viewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(_viewModel.CurrentPracticeQuestion) &&
                        _viewModel.CurrentPracticeQuestion != null)
                    {
                        RichTextAnswer.Document.Blocks.Clear();
                        if (!string.IsNullOrEmpty(_viewModel.CurrentPracticeQuestion.RichTextAnswer))
                        {
                            RichTextAnswer.Document.Blocks.Add(
                                new Paragraph(new Run(_viewModel.CurrentPracticeQuestion.RichTextAnswer))
                            );
                        }
                    }
                };
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Prevent closing during exam
            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn thoát khỏi bài thi?\nTất cả câu trả lời chưa nộp sẽ bị mất.",
                "Xác nhận thoát",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }

            base.OnClosing(e);
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
            if (value is ExamType examType && parameter is string expectedType)
            {
                if (expectedType == "MultipleChoice")
                    return examType == ExamType.MultipleChoice ? Visibility.Visible : Visibility.Collapsed;
                else if (expectedType == "Practice")
                    return examType == ExamType.Practice ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
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