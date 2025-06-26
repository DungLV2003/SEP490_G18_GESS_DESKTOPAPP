using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            try
            {
                // Lấy window hiện tại
                var current = Application.Current.Windows.OfType<TCurrent>().FirstOrDefault();
                if (current == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Current window {typeof(TCurrent).Name} not found");
                    return;
                }

                // Tạo window mới trước khi đóng window cũ
                var next = _serviceProvider.GetRequiredService<TNext>();
                if (next == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Could not create {typeof(TNext).Name}");
                    return;
                }

                // Sử dụng Dispatcher để đảm bảo chạy trên UI thread
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Hiển thị window mới trước
                        next.Show();

                        // Đóng window cũ
                        current.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in navigation: {ex.Message}");
                        // Nếu có lỗi, ít nhất cũng hiển thị window mới
                        if (!next.IsVisible)
                        {
                            next.Show();
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"NavigationService Error: {ex}");
            }
        }

        public void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
