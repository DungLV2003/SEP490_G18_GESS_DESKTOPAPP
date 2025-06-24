using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
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

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Interaction logic for DialogXacNhanBaiThiView.xaml
    /// </summary>
    public partial class DialogXacNhanBaiThiView : Window
    {
        public DialogXacNhanBaiThiView(DialogXacNhanNopBaiThiViewModel dxnnbtViewModel)
        {
            InitializeComponent();
            this.DataContext = dxnnbtViewModel;
            AnimationHelper.ApplyFadeIn(this);
        }

    }
}
