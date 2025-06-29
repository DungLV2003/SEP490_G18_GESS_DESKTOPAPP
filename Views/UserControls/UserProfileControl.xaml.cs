using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.UserControls
{
    /// <summary>
    /// Interaction logic for UserProfileControl.xaml
    /// </summary>
    public partial class UserProfileControl : UserControl, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IGoogleAuthService _googleAuthService;

        public string StudentName => _userService?.GetStudentName() ?? "Guest";
        public string StudentCode => _userService?.GetStudentCode() ?? "";

        public UserProfileControl()
        {
            InitializeComponent();

            // Get services from DI
            _userService = App.AppHost.Services.GetRequiredService<IUserService>();
            _googleAuthService = App.AppHost.Services.GetRequiredService<IGoogleAuthService>();

            this.DataContext = this;
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
        }

        private async void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;

            var result = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất?",
                "Xác nhận đăng xuất",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Clear user data
                    await _googleAuthService.LogoutAsync();
                    _userService.ClearCurrentUser();

                    // Navigate to login
                    var loginView = App.AppHost.Services.GetRequiredService<DangNhapView>();
                    loginView.Show();

                    // Close all other windows
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window != loginView)
                        {
                            window.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi đăng xuất: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}