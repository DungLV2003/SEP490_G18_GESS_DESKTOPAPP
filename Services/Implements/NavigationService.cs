using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implement
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateWithFade<TCurrent, TNext>()
            where TCurrent : Window
            where TNext : Window
        {
            var current = Application.Current.Windows.OfType<TCurrent>().FirstOrDefault();
            var next = _serviceProvider.GetRequiredService<TNext>();

            if (current != null && next != null)
            {
                AnimationHelper.FadeOutAndSwitch(current, next);
            }
        }


        public void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}