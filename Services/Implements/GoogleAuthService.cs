using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implements
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private const string BASE_URL = "http://14.225.254.72:5000/api/Auth"; // Server API endpoint
        private readonly string _clientId;
        private readonly string _clientSecret;

        public GoogleAuthService(string clientId, string clientSecret)
        {
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        // Custom IDataStore không lưu gì cả (để tránh cache credential)
        private class NoDataStore : IDataStore
        {
            public Task StoreAsync<T>(string key, T value) => Task.CompletedTask;
            public Task DeleteAsync<T>(string key) => Task.CompletedTask;
            public Task<T> GetAsync<T>(string key) => Task.FromResult(default(T));
            public Task ClearAsync() => Task.CompletedTask;
        }

        /// <summary>
        /// Lấy Google ID Token thông qua OAuth flow với timeout
        /// </summary>
        public async Task<string> GetGoogleIdTokenAsync()
        {
            try
            {
                string[] scopes = { "openid", "email", "profile" };
                var clientSecrets = new ClientSecrets
                {
                    ClientId = _clientId,
                    ClientSecret = _clientSecret
                };

                // Sử dụng CancellationTokenSource với timeout 60 giây
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)))
                {
                    try
                    {
                        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                            clientSecrets,
                            scopes,
                            "user",
                            cts.Token, // Sử dụng timeout token
                            new NoDataStore()
                        );

                        if (string.IsNullOrEmpty(credential?.Token?.IdToken))
                        {
                            throw new Exception("Không thể lấy được ID Token từ Google");
                        }

                        return credential.Token.IdToken;
                    }
                    catch (OperationCanceledException)
                    {
                        throw new Exception("Quá trình đăng nhập đã bị hủy hoặc quá thời gian chờ. Vui lòng thử lại.");
                    }
                }
            }
            catch (Exception ex) when (ex.Message.Contains("access_denied") || ex.Message.Contains("Cancelled"))
            {
                throw new Exception("Người dùng đã hủy quá trình đăng nhập. Vui lòng thử lại.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetGoogleIdTokenAsync Error: {ex.Message}");
                throw new Exception($"Lỗi khi đăng nhập Google: {ex.Message}");
            }
        }

        /// <summary>
        /// Đăng nhập bằng Google ID Token
        /// </summary>
        public async Task<UserInfo> LoginWithGoogleAsync(string idToken)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(
                        $"{{\"idToken\":\"{idToken}\"}}",
                        Encoding.UTF8,
                        "application/json"
                    );

                    var response = await client.PostAsync($"{BASE_URL}/google-login", content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorObj = JObject.Parse(responseString);
                        string errorMsg = errorObj["message"]?.ToString() ?? "Đăng nhập thất bại!";
                        throw new Exception(errorMsg);
                    }

                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(responseString);
                    if (userInfo == null)
                        throw new Exception("Dữ liệu người dùng rỗng!");

                    // Lưu token vào ApiHelper để sử dụng cho các API call khác
                    var responseObj = JObject.Parse(responseString);
                    string accessToken = responseObj["accessToken"]?.ToString();
                    string refreshToken = responseObj["refreshToken"]?.ToString();
                    DateTime? expiry = responseObj["expiry"]?.ToObject<DateTime?>();

                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        ApiHelper.SetTokens(accessToken, refreshToken ?? "", expiry);
                    }

                    return userInfo;
                }
                catch (JsonException ex)
                {
                    throw new Exception("Dữ liệu JSON không hợp lệ: " + ex.Message);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }



        /// <summary>
        /// Đăng xuất và xóa token
        /// </summary>
        public Task LogoutAsync()
        {
            try
            {
                // Xóa token từ ApiHelper
                ApiHelper.ClearTokens();
                System.Diagnostics.Debug.WriteLine("User logged out");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LogoutAsync Error: {ex.Message}");
            }

            return Task.CompletedTask;
        }
    }
}
