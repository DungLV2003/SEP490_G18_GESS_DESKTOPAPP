using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class PracticeExamInfoResponseDTO
    {
        public Guid PracExamHistoryId { get; set; }
        public string StudentFullName { get; set; }
        public string StudentCode { get; set; }
        public string SubjectName { get; set; }
        public string ExamCategoryName { get; set; }
        public int Duration { get; set; }
        public DateTime? StartTime { get; set; } // Thêm để tính thời gian còn lại
        public string Message { get; set; }
        public int? ExamSlotRoomId { get; set; } // Lưu trữ ExamSlotRoomId từ API response khi tham gia bài thi
        public List<PracticeExamQuestionDetailDTO> Questions { get; set; }
    }

    public class PracticeExamQuestionDetailDTO
    {
        public int PracticeQuestionId { get; set; } // ID thật của câu hỏi
        public int QuestionOrder { get; set; } // Thứ tự hiển thị
        public string Content { get; set; }
        public string? AnswerContent { get; set; }
        public double Score { get; set; }
    }
}
