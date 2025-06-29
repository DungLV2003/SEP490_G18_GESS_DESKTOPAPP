using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces
{
    public interface IGoogleAuthService
    {
        /// <summary>
        /// Lấy Google ID Token thông qua OAuth flow
        /// </summary>
        /// <returns>Google ID Token</returns>
        Task<string> GetGoogleIdTokenAsync();

        /// <summary>
        /// Đăng nhập bằng Google ID Token
        /// </summary>
        /// <param name="idToken">Google ID Token</param>
        /// <returns>Thông tin người dùng sau khi đăng nhập thành công</returns>
        Task<UserInfo> LoginWithGoogleAsync(string idToken);

        /// <summary>
        /// Đăng xuất và xóa token
        /// </summary>
        Task LogoutAsync();
    }
}
