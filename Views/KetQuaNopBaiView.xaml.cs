using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using System.Windows;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for KetQuaNopBaiView.xaml
    /// </summary>
    public partial class KetQuaNopBaiView : Window
    {
        public KetQuaNopBaiView(KetQuaNopBaiViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            // Full screen
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.WindowStyle = WindowStyle.None;

            AnimationHelper.ApplyFadeIn(this);
        }
    }
}