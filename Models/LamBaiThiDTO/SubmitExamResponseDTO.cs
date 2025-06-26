using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class SubmitExamResponseDTO
    {
        public string ExamName { get; set; }
        public string SubjectName { get; set; }
        public string TimeTaken { get; set; }
        public string CorrectAnswersPercentage { get; set; }
        public double FinalScore { get; set; }
        public List<QuestionResultDTO> QuestionResults { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
    }
}
