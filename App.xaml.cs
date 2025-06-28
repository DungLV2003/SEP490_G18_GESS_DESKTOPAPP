using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SEP490_G18_GESS_DESKTOPAPP.Services.Implement;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using SEP490_G18_GESS_DESKTOPAPP.Views.Dialog;
using System.Configuration;
using System.Data;
using System.Windows;

namespace SEP490_G18_GESS_DESKTOPAPP
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHttpClient<IDanhSachBaiThiService, DanhSachBaiThiService>();
                    services.AddHttpClient<ILamBaiThiService, LamBaiThiService>();
                    services.AddHttpClient<ILichSuBaiThiSinhVienService, LichSuBaiThiSinhVienService>();
                    services.AddSingleton<INavigationService, NavigationService>();

                    // ======= Đăng ký ViewModel =======
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<HomePageViewModel>();
                    services.AddSingleton<DanhSachBaiThiSinhVienViewModel>();
                    services.AddSingleton<LichSuBaiThiSinhVienViewModel>();

                    services.AddTransient<MainWindow>();
                    services.AddTransient<HomePageView>();
                    services.AddTransient<DanhSachBaiThiView>();
                    services.AddTransient<LamBaiThiView>();
                    services.AddTransient<LichSuBaiThiSinhVienView>();


                    services.AddTransient<DialogXacNhanBaiThiView>();
                    services.AddTransient<DialogNhapMaBaiThiView>();
                    services.AddTransient<DialogThongBaoLoiView>();
                    services.AddTransient<DialogThongBaoThanhCongView>();

                    services.AddTransient<DialogNhapMaBaiThiViewModel>();
                    services.AddTransient<DialogXacNhanNopBaiThiViewModel>();
                    services.AddTransient<DialogThongBaoLoiViewModel>(); 
                    services.AddTransient<DialogThongBaoThanhCongViewModel>();
                    services.AddTransient<LamBaiThiViewModel>();

                })
                .Build();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var mainWindow =  AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost.StopAsync();
            AppHost.Dispose();

            base.OnExit(e);
        }

    }
}