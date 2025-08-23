using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class SubmitPracticeExamAnswerDTO
    {
        public int PracticeQuestionId { get; set; }
        public string Answer { get; set; }
    }

    public class SubmitPracticeExamRequest
    {
        public Guid PracExamHistoryId { get; set; }
        public int? ExamSlotRoomId { get; set; } // Có thể null - chỉ dùng cho thi cuối kỳ
        public List<SubmitPracticeExamAnswerDTO> Answers { get; set; }
    }



}
