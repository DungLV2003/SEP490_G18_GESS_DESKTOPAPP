using System;
using System.Collections.Generic;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.RunningApplicationDTO
{
    public class RunningApplication
    {
        public string ProcessName { get; set; }
        public string ApplicationName { get; set; }
        public List<int> ProcessIds { get; set; }
        public int ProcessCount { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public string WindowTitle { get; set; }
        public bool IsCloseable { get; set; }
        public string IconPath { get; set; }
        public string IconText { get; set; }
        public bool HasActiveWindow { get; set; }
        
        public RunningApplication()
        {
            ProcessIds = new List<int>();
            Status = "Đang mở";
            IsCloseable = true;
            ProcessCount = 1;
        }
    }
} 