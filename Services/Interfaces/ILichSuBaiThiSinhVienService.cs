using SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Interface
{
    public interface ILichSuBaiThiSinhVienService
    {
        /// <summary>
        /// Lấy tất cả môn học theo kỳ học của sinh viên
        /// </summary>
        Task<List<AllSubjectBySemesterOfStudentDTOResponse>?> GetAllSubjectBySemesterOfStudentAsync(
            Guid studentId, int? semesterId = null, int? year = null);

        /// <summary>
        /// Lấy tất cả năm học của sinh viên
        /// </summary>
        Task<List<int>?> GetAllYearOfStudentAsync(Guid studentId);

        /// <summary>
        /// Lấy lịch sử thi của sinh viên theo môn học
        /// </summary>
        Task<List<HistoryExamOfStudentDTOResponse>?> GetHistoryExamOfStudentBySubIdAsync(
            int subjectId, Guid studentId, int? semesterId = null, int? year = null);

        /// <summary>
        /// Lấy danh sách kỳ học theo năm
        /// </summary>
        Task<List<SemesterResponse>?> GetSemestersByYearAsync(int? year, Guid userId);
    }
}
