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
            
                            Console.WriteLine("=== GUIDE BUTTON CLICKED ===");
            
            try
            {
                // Console log token để test API
                var tokenInfo = Helpers.ApiHelper.GetCurrentTokenInfo();
                Console.WriteLine($"TokenInfo retrieved. AccessToken null/empty: {string.IsNullOrEmpty(tokenInfo?.AccessToken)}");
                
                if (tokenInfo != null && !string.IsNullOrEmpty(tokenInfo.AccessToken))
                {
                    Console.WriteLine("=== TOKEN INFO FOR API TESTING ===");
                    Console.WriteLine($"Access Token: {tokenInfo.AccessToken}");
                    Console.WriteLine($"Refresh Token: {tokenInfo.RefreshToken}");
                    Console.WriteLine($"Expiry: {tokenInfo.Expiry}");
                    Console.WriteLine($"Is Expired: {tokenInfo.IsExpired}");
                    Console.WriteLine("==================================");
                    
                    // Copy vào clipboard luôn cho tiện
                    System.Windows.Clipboard.SetText(tokenInfo.AccessToken);
                    
                    // Hiển thị full token info trong MessageBox để dễ thấy
                    var tokenDisplay = $"=== TOKEN INFO FOR API TESTING ===\n\n" +
                                     $"Access Token:\n{tokenInfo.AccessToken}\n\n" +
                                     $"Refresh Token:\n{tokenInfo.RefreshToken}\n\n" +
                                     $"Expiry: {tokenInfo.Expiry}\n" +
                                     $"Is Expired: {tokenInfo.IsExpired}\n\n" +
                                     $"✅ Access Token đã được copy vào clipboard!";
                    
                    MessageBox.Show(tokenDisplay, "Token Info", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    Console.WriteLine("No token available. Please login first.");
                    Console.WriteLine($"TokenInfo is null: {tokenInfo == null}");
                    if (tokenInfo != null)
                    {
                        Console.WriteLine($"AccessToken is null/empty: {string.IsNullOrEmpty(tokenInfo.AccessToken)}");
                    }
                    MessageBox.Show("No token available. Please login first.", "No Token", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            // TODO: Open guide/help
            MessageBox.Show("Token đã được in ra console. Kiểm tra Output window (Debug).", "Thông báo", 
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