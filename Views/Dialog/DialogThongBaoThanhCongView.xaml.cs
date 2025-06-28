using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System;
using System.Windows;
using System.Windows.Media.Effects;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    public partial class DialogThongBaoThanhCongView : Window
    {
        public DialogThongBaoThanhCongView(DialogThongBaoThanhCongViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;

            // Cấu hình dialog
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.ShowInTaskbar = false;

            AnimationHelper.ApplyFadeIn(this, 300);
        }
    }
}