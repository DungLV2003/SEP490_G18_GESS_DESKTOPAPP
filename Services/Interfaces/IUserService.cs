using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces
{
    public interface IUserService
    {
        UserInfo CurrentUser { get; }
        void SetCurrentUser(UserInfo user);
        void ClearCurrentUser();
        bool IsLoggedIn { get; }

        // Token management
        string AccessToken { get; }
        void SetAccessToken(string accessToken);
        void ClearAccessToken();

        // Helper methods to get user info
        string GetUsername();
        string GetStudentName();
        string GetStudentId();
        string GetStudentCode();
        string GetUserEmail();
        string GetUserToken();
    }
}
