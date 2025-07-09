using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using System.Windows;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.Dialog
{
    /// <summary>
    /// Interaction logic for DialogCanhBaoUngDungCamView.xaml
    /// </summary>
    public partial class DialogCanhBaoUngDungCamView : Window
    {
        public DialogCanhBaoUngDungCamView()
        {
            InitializeComponent();
        }

        public DialogCanhBaoUngDungCamView(DialogCanhBaoUngDungCamViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
} 