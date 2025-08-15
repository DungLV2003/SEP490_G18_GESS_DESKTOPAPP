using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class ExamStatusCheckRequest
    {
        public List<int> ExamIds { get; set; } = new List<int>();
        public string ExamType { get; set; } = string.Empty; // "Multi" hoặc "Practice"
    }

    public class ExamStatusCheckResponse
    {
        public List<ExamStatusItem> Exams { get; set; } = new List<ExamStatusItem>();
    }

    public class ExamStatusItem
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; } = string.Empty;
        public string ExamType { get; set; } = string.Empty; // "MultiExam" hoặc "PracticeExam"
        public string Status { get; set; } = string.Empty; // "Đang mở ca", "Đã đóng ca", etc.
    }
}
