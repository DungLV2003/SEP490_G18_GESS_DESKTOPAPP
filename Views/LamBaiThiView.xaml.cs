using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
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
    }

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
            if (values.Length >= 3)
            {
                bool isAnswered = values[0] is bool answered && answered;
                bool isCurrent = values[1] is bool current && current;
                bool isMarked = values[2] is bool marked && marked;

                if (isCurrent)
                    return Application.Current.FindResource("CurrentButtonStyle");
                else if (isMarked)
                    return Application.Current.FindResource("MarkedButtonStyle");
                else if (isAnswered)
                    return Application.Current.FindResource("AnsweredButtonStyle");
                else
                    return Application.Current.FindResource("NumberButtonStyle");
            }
            return Application.Current.FindResource("NumberButtonStyle");
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