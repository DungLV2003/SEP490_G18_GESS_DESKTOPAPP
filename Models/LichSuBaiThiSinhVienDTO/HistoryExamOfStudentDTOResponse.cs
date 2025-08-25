using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO
{
    public class HistoryExamOfStudentDTOResponse
    {
        public string ExamName { get; set; }
        public string ExamType { get; set; }
        public string CategoryExamName { get; set; }
        public int Duration { get; set; }
        public DateTime? SubmittedDateTime { get; set; }
        public double Score { get; set; }
        
        [JsonPropertyName("isGrade")]
        public bool IsGrade { get; set; }
    }
}
