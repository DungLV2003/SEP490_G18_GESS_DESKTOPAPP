using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
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
            this.DataContext = dsbtViewModel;
            AnimationHelper.ApplyFadeIn(this);
        }
    }
}
