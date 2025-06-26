using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class SubmitPracticeExamResponseDTO
    {
        public string ExamName { get; set; }
        public string SubjectName { get; set; }
        public string StudentName { get; set; }
        public string StudentCode { get; set; }
        public string TimeTaken { get; set; }
        public List<PracticeExamQuestionResultDTO> QuestionResults { get; set; }
    }

    public class PracticeExamQuestionResultDTO
    {
        public int PracticeQuestionId { get; set; }
        public string QuestionContent { get; set; }
        public string StudentAnswer { get; set; }
    }
}
