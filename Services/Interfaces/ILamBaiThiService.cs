using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces
{
    public interface ILamBaiThiService
    {
        Task<ExamInfoResponseDTO?> CheckExamNameAndCode(string examId, string examCode, Guid studentId);
    }
}
