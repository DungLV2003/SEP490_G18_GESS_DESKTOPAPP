using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    public static class ApiHelper
    {
        private static HttpClient _httpClient;
        private static string _accessToken;
        private static string _refreshToken;
        private static DateTime _tokenExpiry;
        private static readonly object _lock = new object();

        static ApiHelper()
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Cấu hình token cho API calls
        /// </summary>
        public static void SetTokens(string accessToken, string refreshToken, DateTime? expiry = null)
        {
            lock (_lock)
            {
                _accessToken = accessToken;
                _refreshToken = refreshToken;
                _tokenExpiry = expiry ?? DateTime.UtcNow.AddHours(1);

                // Set Authorization header với Bearer token
                SetAuthorizationHeader();
            }
        }

        /// <summary>
        /// Set Authorization header với Bearer token
        /// </summary>
        private static void SetAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(_accessToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        /// <summary>
        /// Xóa token (logout)
        /// </summary>
        public static void ClearTokens()
        {
            lock (_lock)
            {
                _accessToken = null;
                _refreshToken = null;
                _tokenExpiry = DateTime.MinValue;
                SetAuthorizationHeader(); // Xóa Authorization header
            }
        }

        /// <summary>
        /// Kiểm tra token có hết hạn không
        /// </summary>
        private static bool IsTokenExpired()
        {
            return DateTime.UtcNow >= _tokenExpiry.AddMinutes(-5); // Refresh 5 phút trước khi hết hạn
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        private static async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(_refreshToken))
                    return false;

                var refreshRequest = new
                {
                    refreshToken = _refreshToken
                };

                var json = JsonSerializer.Serialize(refreshRequest);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Tạm thời xóa Authorization header để gọi refresh
                var currentAuth = _httpClient.DefaultRequestHeaders.Authorization;
                _httpClient.DefaultRequestHeaders.Authorization = null;

                var response = await _httpClient.PostAsync("https://localhost:7074/api/Auth/refresh-token", content);

                // Khôi phục Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = currentAuth;

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tokenResponse != null)
                    {
                        SetTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.Expiry);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshAccessTokenAsync Error: {ex}");
                return false;
            }
        }

        /// <summary>
        /// Gửi HTTP request với tự động refresh token
        /// </summary>
        public static async Task<HttpResponseMessage> SendWithAutoRefreshAsync(string url, HttpMethod method, HttpContent content = null)
        {
            try
            {
                // Kiểm tra và refresh token nếu cần
                if (!string.IsNullOrEmpty(_accessToken) && IsTokenExpired())
                {
                    var refreshSuccess = await RefreshAccessTokenAsync();
                    if (!refreshSuccess)
                    {
                        // Token refresh failed, có thể cần đăng nhập lại
                        System.Diagnostics.Debug.WriteLine("Token refresh failed");
                    }
                }

                // Tạo request
                var request = new HttpRequestMessage(method, url);
                if (content != null)
                {
                    request.Content = content;
                }

                // Gửi request lần đầu
                var response = await _httpClient.SendAsync(request);

                // Nếu unauthorized và có refresh token, thử refresh và gửi lại
                if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(_refreshToken))
                {
                    var refreshSuccess = await RefreshAccessTokenAsync();
                    if (refreshSuccess)
                    {
                        // Tạo lại request với token mới
                        var newRequest = new HttpRequestMessage(method, url);
                        if (content != null)
                        {
                            // Clone content nếu có
                            var contentString = await content.ReadAsStringAsync();
                            newRequest.Content = new StringContent(contentString, Encoding.UTF8, "application/json");
                        }

                        response.Dispose();
                        response = await _httpClient.SendAsync(newRequest);
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SendWithAutoRefreshAsync Error: {ex}");
                throw;
            }
        }

        /// <summary>
        /// GET request với auto refresh
        /// </summary>
        public static async Task<HttpResponseMessage> GetWithAutoRefreshAsync(string url)
        {
            return await SendWithAutoRefreshAsync(url, HttpMethod.Get);
        }

        /// <summary>
        /// POST request với auto refresh
        /// </summary>
        public static async Task<HttpResponseMessage> PostWithAutoRefreshAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendWithAutoRefreshAsync(url, HttpMethod.Post, content);
        }

        /// <summary>
        /// PUT request với auto refresh
        /// </summary>
        public static async Task<HttpResponseMessage> PutWithAutoRefreshAsync(string url, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await SendWithAutoRefreshAsync(url, HttpMethod.Put, content);
        }

        /// <summary>
        /// DELETE request với auto refresh
        /// </summary>
        public static async Task<HttpResponseMessage> DeleteWithAutoRefreshAsync(string url)
        {
            return await SendWithAutoRefreshAsync(url, HttpMethod.Delete);
        }

        /// <summary>
        /// Generic GET method với deserialize
        /// </summary>
        public static async Task<T?> GetAsync<T>(string url) where T : class
        {
            try
            {
                var response = await GetWithAutoRefreshAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAsync Error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Generic POST method với deserialize
        /// </summary>
        public static async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
            where TResponse : class
        {
            try
            {
                var response = await PostWithAutoRefreshAsync(url, data);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostAsync Error: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Lấy thông tin token hiện tại
        /// </summary>
        public static TokenInfo GetCurrentTokenInfo()
        {
            lock (_lock)
            {
                return new TokenInfo
                {
                    AccessToken = _accessToken,
                    RefreshToken = _refreshToken,
                    Expiry = _tokenExpiry,
                    IsExpired = IsTokenExpired()
                };
            }
        }

        /// <summary>
        /// Dispose HttpClient khi không sử dụng
        /// </summary>
        public static void Dispose()
        {
            _httpClient?.Dispose();
        }
    }

    /// <summary>
    /// Response model cho token refresh
    /// </summary>
    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
    }

    /// <summary>
    /// Thông tin token hiện tại
    /// </summary>
    public class TokenInfo
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
        public bool IsExpired { get; set; }
    }
}
