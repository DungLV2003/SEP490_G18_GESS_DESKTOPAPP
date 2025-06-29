using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LoginDTO
{
    public class LoginResult
    {
        public bool IsSuccess { get; private set; }
        public UserInfo UserInfo { get; private set; }
        public string ErrorMessage { get; private set; }

        private LoginResult() { }

        public static LoginResult Success(UserInfo userInfo)
        {
            return new LoginResult
            {
                IsSuccess = true,
                UserInfo = userInfo
            };
        }

        public static LoginResult Failed(string errorMessage)
        {
            return new LoginResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
