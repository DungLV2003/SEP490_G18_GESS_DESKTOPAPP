using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class CheckPracticeExamRequestDTO
    {
        public string ExamName { get; set; }
        public string Code { get; set; }
        public Guid StudentId { get; set; }
    }
}
