using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interface
{
    public interface IDanhSachBaiThiService
    {
        Task<List<ExamListOfStudentResponse>?> GetAllMultiExamOfStudentAsync(Guid studentId);
        Task<List<ExamListOfStudentResponse>?> GetAllPracticeExamOfStudentAsync(Guid studentId);
    }
}
