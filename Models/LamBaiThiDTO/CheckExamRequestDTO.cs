using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO
{
    public class CheckExamRequestDTO
    {
        [Required]
        public int ExamId { get; set; }
        [Required]
        public string Code { get; set; }
        [Required]
        public Guid StudentId { get; set; }
    }
}
