using SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implements
{
    public class UserService : IUserService
    {
        private static UserInfo _currentUser;
        private static string _accessToken;
        private static readonly object _lock = new object();

        // Singleton pattern để có thể dùng cho cả static và DI
        private static UserService _instance;
        public static UserService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new UserService();
                        }
                    }
                }
                return _instance;
            }
        }

        // Constructor public để hỗ trợ DI
        public UserService() { }

        public UserInfo CurrentUser
        {
            get
            {
                lock (_lock)
                {
                    return _currentUser;
                }
            }
        }

        public bool IsLoggedIn
        {
            get
            {
                lock (_lock)
                {
                    return _currentUser != null;
                }
            }
        }

        public string AccessToken
        {
            get
            {
                lock (_lock)
                {
                    return _accessToken;
                }
            }
        }

        public void SetCurrentUser(UserInfo user)
        {
            lock (_lock)
            {
                _currentUser = user;
            }
        }

        public void ClearCurrentUser()
        {
            lock (_lock)
            {
                _currentUser = null;
            }
        }

        public void SetAccessToken(string accessToken)
        {
            lock (_lock)
            {
                _accessToken = accessToken;
            }
        }

        public void ClearAccessToken()
        {
            lock (_lock)
            {
                _accessToken = null;
            }
        }

        // Helper methods to get user info
        public string GetUsername()
        {
            return CurrentUser?.Username;
        }

        public string GetStudentName()
        {
            return CurrentUser?.StudentName;
        }

        public string GetStudentId()
        {
            return CurrentUser?.StudentId;
        }

        public string GetStudentCode()
        {
            return CurrentUser?.StudentCode;
        }

        public string GetUserEmail()
        {
            return CurrentUser?.Email;
        }

        public string GetUserToken()
        {
            return CurrentUser?.Token;
        }
    }
}
