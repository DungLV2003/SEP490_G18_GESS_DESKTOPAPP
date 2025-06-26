using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class UpdatePracticeExamAnswerDTO
    {
        public Guid PracExamHistoryId { get; set; }
        public int PracticeQuestionId { get; set; }
        public string Answer { get; set; }
    }

    public class UpdatePracticeExamAnswersRequest
    {
        public List<UpdatePracticeExamAnswerDTO> Answers { get; set; }
    }
}
