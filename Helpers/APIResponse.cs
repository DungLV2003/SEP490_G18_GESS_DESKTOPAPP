using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Helpers
{
    /// <summary>
    /// Generic API response wrapper for all API calls
    /// </summary>
    /// <typeparam name="T">Type of data returned from API</typeparam>
    public class APIResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
    }

    /// <summary>
    /// API response without data (for operations like delete, update)
    /// </summary>
    public class APIResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public static class ApiResponseExtensions
    {
        public static bool IsSuccessful<T>(this APIResponse<T> response)
        {
            return response != null && response.Success;
        }

        public static T GetDataOrDefault<T>(this APIResponse<T> response, T defaultValue = default(T))
        {
            return response?.IsSuccessful() == true ? response.Data : defaultValue;
        }
    }
}
