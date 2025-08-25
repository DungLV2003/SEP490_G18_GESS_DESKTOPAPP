using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    // Converter cho background color của trạng thái chấm
    public class GradeStatusBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGrade)
            {
                return isGrade ? new SolidColorBrush(Color.FromRgb(240, 253, 244)) : new SolidColorBrush(Color.FromRgb(254, 242, 242));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho border color của trạng thái chấm
    public class GradeStatusBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGrade)
            {
                return isGrade ? new SolidColorBrush(Color.FromRgb(187, 247, 208)) : new SolidColorBrush(Color.FromRgb(254, 202, 202));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho text của trạng thái chấm
    public class GradeStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGrade)
            {
                return isGrade ? "Đã chấm" : "Chưa chấm";
            }
            return "Chưa chấm";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho foreground color của text trạng thái chấm
    public class GradeStatusForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isGrade)
            {
                return isGrade ? new SolidColorBrush(Color.FromRgb(22, 163, 74)) : new SolidColorBrush(Color.FromRgb(220, 38, 38));
            }
            return new SolidColorBrush(Color.FromRgb(220, 38, 38));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter cho hiển thị điểm số (hiển thị -- nếu chưa chấm)
    public class ScoreDisplayConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Debug: Log các giá trị nhận được
            System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Values count: {values.Length}");
            if (values.Length > 0)
            {
                System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Score value: {values[0]} (Type: {values[0]?.GetType()})");
            }
            if (values.Length > 1)
            {
                System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - IsGrade value: {values[1]} (Type: {values[1]?.GetType()})");
            }
            
            if (values.Length >= 2 && values[1] is bool isGrade)
            {
                if (!isGrade)
                {
                    return "--";
                }
                
                // Xử lý nhiều kiểu dữ liệu số
                if (values[0] is double doubleScore)
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Processing double score: {doubleScore}");
                    return doubleScore.ToString("F1");
                }
                else if (values[0] is decimal decimalScore)
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Processing decimal score: {decimalScore}");
                    return decimalScore.ToString("F1");
                }
                else if (values[0] is float floatScore)
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Processing float score: {floatScore}");
                    return floatScore.ToString("F1");
                }
                else if (values[0] is int intScore)
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Processing int score: {intScore}");
                    return intScore.ToString("F1");
                }
                else if (values[0] != null)
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Attempting to parse: {values[0]}");
                    // Thử parse nếu là string hoặc kiểu khác
                    if (double.TryParse(values[0].ToString(), out double parsedScore))
                    {
                        System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Successfully parsed: {parsedScore}");
                        return parsedScore.ToString("F1");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Failed to parse: {values[0]}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Score value is null");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ScoreDisplayConverter - Invalid values or IsGrade not bool");
            }
            return "--";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để hiển thị khi danh sách có dữ liệu
    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter để hiển thị khi danh sách không có dữ liệu
    public class CountToVisibilityInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
