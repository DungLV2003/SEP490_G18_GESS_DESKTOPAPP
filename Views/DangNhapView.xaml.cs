using SEP490_G18_GESS_DESKTOPAPP.Models.LoginDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SEP490_G18_GESS_DESKTOPAPP.Views
{
    /// <summary>
    /// Interaction logic for DangNhapView.xaml
    /// </summary>
    public partial class DangNhapView : Window
    {
    
        private const string ERROR_NO_ID_TOKEN = "Không thể xác thực với Google. Vui lòng thử lại.";
        private const string ERROR_LOGIN_FAILED = "Đăng nhập thất bại. Vui lòng kiểm tra lại thông tin.";
        private const string ERROR_NETWORK = "Lỗi kết nối mạng. Vui lòng kiểm tra kết nối internet.";
       
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IUserService _userService;



        public DangNhapView(IGoogleAuthService googleAuthService, IUserService userService)
        {
            InitializeComponent();

            // Dependency Injection - nhận từ DI Container
            _googleAuthService = googleAuthService;
            _userService = userService;

            InitializeEventHandlers();
        }

        private void InitializeEventHandlers()
        {
            btnGoogleLogin.Click += async (sender, e) => await HandleGoogleLoginAsync();
        }
  
        private async Task HandleGoogleLoginAsync()
        {
            try
            {
                SetLoginState(isLoading: true);
                ClearErrorMessage();

                var result = await PerformGoogleLoginAsync();

                if (result.IsSuccess)
                {
                    ShowSuccessMessage(result.UserInfo);
                }
                else
                {
                    ShowErrorMessage(result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage(GetUserFriendlyErrorMessage(ex));
            }
            finally
            {
                SetLoginState(isLoading: false);
            }
        }
        
        private async Task<LoginResult> PerformGoogleLoginAsync()
        {
            try
            {
                // Lấy ID Token từ Google
                var idToken = await _googleAuthService.GetGoogleIdTokenAsync();

                if (string.IsNullOrEmpty(idToken))
                {
                    return LoginResult.Failed(ERROR_NO_ID_TOKEN);
                }

                // Đăng nhập với ID Token
                var userInfo = await _googleAuthService.LoginWithGoogleAsync(idToken);

                if (userInfo == null)
                {
                    return LoginResult.Failed(ERROR_LOGIN_FAILED);
                }

                // Lưu thông tin user
                _userService.SetCurrentUser(userInfo);
                return LoginResult.Success(userInfo);
            }
            catch (Exception ex)
            {
                return LoginResult.Failed(GetUserFriendlyErrorMessage(ex));
            }
        }

        /// <summary>
        /// Chuyển sang trang chính sau khi login thành công
        /// </summary>
        /// <param name="userInfo">Thông tin người dùng đã đăng nhập</param>
        //private void NavigateToMainPage(Models.UserInfo userInfo)
        //{
        //    try
        //    {
        //        // Sử dụng DI Container để lấy MainWindow
        //        var mainWindow = App.AppHost.Services.GetRequiredService<MainWindow>();
        //        mainWindow.Show();
        //        this.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi chuyển trang: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        /// <summary>
        /// Chuyển trang sử dụng DI Container
        /// </summary>
        /// <typeparam name="TTarget">Loại Window đích đã đăng ký trong DI</typeparam>
        //private void NavigateWithDI<TTarget>() where TTarget : Window
        //{
        //    try
        //    {
        //        var targetWindow = App.AppHost.Services.GetRequiredService<TTarget>();
        //        targetWindow.Show();
        //        this.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Lỗi khi chuyển trang: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}
    
        private void SetLoginState(bool isLoading)
        {
            btnGoogleLogin.IsEnabled = !isLoading;
            btnGoogleLogin.Content = CreateButtonContent(isLoading);
        }

        private object CreateButtonContent(bool isLoading)
        {
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (!isLoading)
            {
                var image = new Image
                {
                    Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("/Assets/web_light_rd_na@4x.png", UriKind.Relative)),
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(0, 0, 8, 0)
                };
                stackPanel.Children.Add(image);
            }

            var textBlock = new TextBlock
            {
                Text = isLoading ? "Đang đăng nhập..." : "Đăng nhập bằng Google",
                FontWeight = FontWeights.SemiBold,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center
            };
            stackPanel.Children.Add(textBlock);

            return stackPanel;
        }

        private void ShowSuccessMessage(UserInfo userInfo)
        {
            MessageBox.Show(
                $"Đăng nhập thành công!\n\nXin chào: {userInfo.StudentName}\nMã sinh viên: {userInfo.StudentCode} \nID: {userInfo.StudentId}",
                "Thành công",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            ClearErrorMessage();
        }

        private void ShowErrorMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        private void ClearErrorMessage()
        {
            txtError.Text = string.Empty;
            txtError.Visibility = Visibility.Collapsed;
        }

        private string GetUserFriendlyErrorMessage(Exception ex)
        {
            return ex switch
            {
                System.Net.Http.HttpRequestException => ERROR_NETWORK,
                TaskCanceledException => "Quá thời gian chờ. Vui lòng thử lại.",
                UnauthorizedAccessException => "Không có quyền truy cập. Vui lòng liên hệ quản trị viên.",
                _ => $"Đã xảy ra lỗi: {ex.Message}"
            };
        }


       
    }

}  
    