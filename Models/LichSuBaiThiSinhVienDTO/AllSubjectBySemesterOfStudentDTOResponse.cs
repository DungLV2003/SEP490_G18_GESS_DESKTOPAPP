using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO
{
    public class AllSubjectBySemesterOfStudentDTOResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? SemesterId { get; set; }
        public int Year { get; set; }
        public bool IsDeleted { get; set; }
    }
}
