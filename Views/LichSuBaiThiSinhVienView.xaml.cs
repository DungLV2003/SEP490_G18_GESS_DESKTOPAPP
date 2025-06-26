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
            this.ResizeMode = ResizeMode.CanMinimize;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            AnimationHelper.ApplyFadeIn(this);
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