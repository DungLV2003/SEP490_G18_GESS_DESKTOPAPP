using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    public class APIException : Exception
    {
        public string ErrorCode { get; set; }
        public string ErrorDetail { get; set; }

        public APIException(string message, string errorCode = null, string errorDetail = null)
            : base(message)
        {
            ErrorCode = errorCode;
            ErrorDetail = errorDetail;
        }
    }
}
