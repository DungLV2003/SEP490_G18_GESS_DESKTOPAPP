using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    /// <summary>
    /// DelegatingHandler để tự động thêm Authorization header và handle refresh token
    /// </summary>
    public class AuthTokenHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Lấy token từ ApiHelper
            var tokenInfo = ApiHelper.GetCurrentTokenInfo();
            
            // Nếu có token, thêm vào Authorization header
            if (!string.IsNullOrEmpty(tokenInfo?.AccessToken))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenInfo.AccessToken);
            }

            // Gửi request lần đầu
            var response = await base.SendAsync(request, cancellationToken);
            
            // Nếu gặp 401 Unauthorized và có refresh token, thử refresh và gửi lại
            if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(tokenInfo?.RefreshToken))
            {
                // Thử refresh token
                var refreshSuccess = await RefreshAccessTokenAsync();
                
                if (refreshSuccess)
                {
                    // Lấy token mới và tạo request mới
                    var newTokenInfo = ApiHelper.GetCurrentTokenInfo();
                    if (!string.IsNullOrEmpty(newTokenInfo?.AccessToken))
                    {
                        // Clone request với token mới
                        var newRequest = await CloneHttpRequestMessage(request);
                        newRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newTokenInfo.AccessToken);
                        
                        // Dispose response cũ và gửi request mới
                        response.Dispose();
                        response = await base.SendAsync(newRequest, cancellationToken);
                    }
                }
            }
            
            return response;
        }

        /// <summary>
        /// Refresh access token sử dụng logic có sẵn trong ApiHelper
        /// </summary>
        private async Task<bool> RefreshAccessTokenAsync()
        {
            try
            {
                var tokenInfo = ApiHelper.GetCurrentTokenInfo();
                if (string.IsNullOrEmpty(tokenInfo?.RefreshToken))
                    return false;

                // Sử dụng refresh token logic từ ApiHelper
                // Tạo một HttpClient riêng để gọi refresh API (không qua AuthTokenHandler để tránh loop)
                using var refreshClient = new HttpClient();
                refreshClient.Timeout = TimeSpan.FromSeconds(30);

                var refreshRequest = new
                {
                    refreshToken = tokenInfo.RefreshToken
                };

                var json = System.Text.Json.JsonSerializer.Serialize(refreshRequest);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var response = await refreshClient.PostAsync("https://localhost:7074/api/Auth/refresh-token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(responseJson, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tokenResponse != null)
                    {
                        // Cập nhật token mới vào ApiHelper
                        ApiHelper.SetTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken, tokenResponse.Expiry);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Clone HttpRequestMessage để có thể gửi lại với token mới
        /// </summary>
        private async Task<HttpRequestMessage> CloneHttpRequestMessage(HttpRequestMessage original)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri);

            // Copy headers
            foreach (var header in original.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy content nếu có
            if (original.Content != null)
            {
                var contentBytes = await original.Content.ReadAsByteArrayAsync();
                clone.Content = new ByteArrayContent(contentBytes);

                // Copy content headers
                foreach (var header in original.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return clone;
        }
    }
} 