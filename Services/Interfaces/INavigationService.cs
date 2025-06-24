using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interface
{
    public interface INavigationService
    {
        void NavigateWithFade<TCurrent, TNext>()
        where TCurrent : Window
        where TNext : Window;

        void CloseApplication();
    }
}
