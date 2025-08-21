using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class ExamInfoResponseDTO
    {
        public Guid MultiExamHistoryId { get; set; }
        public string StudentFullName { get; set; }
        public string StudentCode { get; set; }
        public string SubjectName { get; set; }
        public string ExamCategoryName { get; set; }
        public int Duration { get; set; }
        public DateTime? StartTime { get; set; }
        public string Message { get; set; }
        public List<MultiQuestionDetailDTO> Questions { get; set; }
        public List<SavedAnswerDTO> SavedAnswers { get; set; } = new List<SavedAnswerDTO>();
    }
    public class MultiQuestionDetailDTO
    {
        public int MultiQuestionId { get; set; }
        public string Content { get; set; }
        public string UrlImg { get; set; }
        public int ChapterId { get; set; }
        public int LevelQuestionId { get; set; }
        public int QuestionOrder { get; set; }
        // Thêm các trường khác nếu cần
    }

    public class SavedAnswerDTO
    {
        public int QuestionId { get; set; }
        public string Answer { get; set; }
    }
}
