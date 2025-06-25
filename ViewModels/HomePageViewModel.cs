using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class HomePageViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        public ICommand ViewHistory { get; }
        public ICommand ExamList { get; }

        public HomePageViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            ViewHistory = new RelayCommand(() => navigationService.NavigateWithFade<HomePageView, LichSuBaiThiSinhVienView>());
            ExamList = new RelayCommand(() => navigationService.NavigateWithFade<HomePageView, DanhSachBaiThiView>());
        }
    }
}
