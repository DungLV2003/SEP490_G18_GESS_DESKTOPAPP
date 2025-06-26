using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO
{
    public class ExamListOfStudentResponse
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string SubjectName { get; set; }
        public int Duration { get; set; }
        public DateTime ExamDay { get; set; }
        public string? Status { get; set; }
        public string? RoomName { get; set; }
        public string? ExamSlotName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
