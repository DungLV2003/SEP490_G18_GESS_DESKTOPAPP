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
            
            // Apply fade-in animation
            AnimationHelper.ApplyFadeIn(this);
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