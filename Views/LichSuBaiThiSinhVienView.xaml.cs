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
    public class ScoreToColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is double score)
            {
                if (score >= 8.0) return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Green
                if (score >= 6.5) return new SolidColorBrush(Color.FromRgb(255, 152, 0)); // Orange
                if (score >= 5.0) return new SolidColorBrush(Color.FromRgb(255, 193, 7)); // Yellow
                return new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
            }
            return new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Default green
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
