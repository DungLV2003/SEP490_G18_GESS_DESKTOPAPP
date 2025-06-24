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
    public class MainViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        public ICommand ViewHomePage { get; }
        public ICommand JoinExamCommand { get; }
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            // Khi click thì điều hướng và chạy hiệu ứng
            ViewHomePage = new RelayCommand(() =>
                _navigationService.NavigateWithFade<MainWindow, HomePageView>());

            // Initialize commands
            //ViewHistoryCommand = new RelayCommand(ExecuteViewHistory);
            //JoinExamCommand = new RelayCommand(ExecuteJoinExam);
        }
    }
}
