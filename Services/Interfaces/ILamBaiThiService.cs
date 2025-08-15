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
        /// <summary>
        /// Kiểm tra tên và mã bài thi
        /// </summary>
        Task<ExamInfoResponseDTO?> CheckExamNameAndCodeMEAsync(CheckExamRequestDTO request);

        /// <summary>
        /// Lấy tất cả câu hỏi của bài thi trắc nghiệm
        /// </summary>
        Task<List<QuestionMultiExamSimpleDTO>?> GetAllQuestionMultiExamByMultiExamIdAsync(int multiExamId);

        /// <summary>
        /// Lấy tất cả đáp án của câu hỏi
        /// </summary>
        Task<List<MultiAnswerOfQuestionDTO>?> GetAllMultiAnswerOfQuestionAsync(int multiQuestionId);

        /// <summary>
        /// Cập nhật tiến độ làm bài
        /// </summary>
        Task<UpdateMultiExamProgressResponseDTO?> UpdateProgressAsync(UpdateMultiExamProgressDTO dto);

        /// <summary>
        /// Nộp bài thi
        /// </summary>
        Task<SubmitExamResponseDTO?> SubmitExamAsync(UpdateMultiExamProgressDTO dto);

        // Practice Exam APIs
        /// <summary>
        /// Kiểm tra tên và mã bài thi tự luận
        /// </summary>
        Task<PracticeExamInfoResponseDTO?> CheckExamNameAndCodePEAsync(CheckPracticeExamRequestDTO request);

        /// <summary>
        /// Lấy câu hỏi và thứ tự theo ID bài thi tự luận
        /// </summary>
        Task<List<QuestionOrderDTO>?> GetQuestionAndAnswerByPracExamIdAsync(int pracExamId);

        /// <summary>
        /// Lấy đáp án của bài thi tự luận
        /// </summary>
        Task<List<PracticeAnswerOfQuestionDTO>?> GetPracticeAnswerOfQuestionAsync(int pracExamId);

        /// <summary>
        /// Cập nhật bài thi tự luận mỗi 5 phút
        /// </summary>
        Task<bool> UpdatePEEach5minutesAsync(UpdatePracticeExamAnswersRequest request);

        /// <summary>
        /// Nộp bài thi tự luận
        /// </summary>
        Task<SubmitPracticeExamResponseDTO?> SubmitPracticeExamAsync(SubmitPracticeExamRequest dto);

        /// <summary>
        /// Kiểm tra trạng thái thi (để auto-submit khi giáo viên đóng ca)
        /// </summary>
        Task<ExamStatusCheckResponse?> CheckExamStatusAsync(ExamStatusCheckRequest request);
    }
}
