using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SEP490_G18_GESS_DESKTOPAPP.Views.UserControls
{
    /// <summary>
    /// Interaction logic for UserProfileControl.xaml
    /// </summary>
    public partial class UserProfileControl : UserControl, INotifyPropertyChanged
    {
        private readonly IUserService? _userService;
        private readonly IGoogleAuthService? _googleAuthService;

        public string StudentName => _userService?.GetStudentName() ?? "Guest";
        public string StudentCode => _userService?.GetStudentCode() ?? "";
        public string StudentEmail => _userService?.GetUserEmail() ?? "No email";

        // Chữ cái đầu của tên để hiển thị trong avatar
        public string AvatarLetter 
        { 
            get 
            { 
                var name = StudentName;
                if (!string.IsNullOrEmpty(name) && name != "Guest")
                {
                    return name.Substring(0, 1).ToUpper();
                }
                return "D"; // Default letter
            } 
        }

        public UserProfileControl()
        {
            InitializeComponent();

            // Get services from DI safely
            try
            {
                if (App.AppHost?.Services != null)
                {
                    _userService = App.AppHost.Services.GetRequiredService<IUserService>();
                    _googleAuthService = App.AppHost.Services.GetRequiredService<IGoogleAuthService>();
                }
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                System.Diagnostics.Debug.WriteLine($"Error initializing UserProfileControl services: {ex.Message}");
            }

            this.DataContext = this;
        }

        private void AvatarButton_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = !UserMenuPopup.IsOpen;
        }



        private void Guide_Click(object sender, RoutedEventArgs e)
        {
            UserMenuPopup.IsOpen = false;
            
            // TODO: Open guide/help
            MessageBox.Show("Chức năng hướng dẫn đang được phát triển", "Thông báo", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DarkMode_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement dark mode toggle
            var toggleButton = sender as ToggleButton;
            if (toggleButton?.IsChecked == true)
            {
                MessageBox.Show("Dark Mode đã bật", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Dark Mode đã tắt", "Thông báo", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                    if (_googleAuthService != null)
                        await _googleAuthService.LogoutAsync();
                    
                    if (_userService != null)
                        _userService.ClearCurrentUser();

                    // Navigate to login
                    if (App.AppHost?.Services != null)
                    {
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