using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.UserDTO
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public DateTime LoginTime { get; set; }

        public UserInfo()
        {
            LoginTime = DateTime.Now;
        }
    }
}
