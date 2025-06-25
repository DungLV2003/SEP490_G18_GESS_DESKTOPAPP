using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implement
{
    public class DanhSachBaiThiService : IDanhSachBaiThiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://localhost:7074/api/Exam"; // Thay đổi URL này

        public DanhSachBaiThiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<ExamListOfStudentResponse>?> GetAllMultiExamOfStudentAsync(Guid studentId)
        {
            try
            {
                var url = $"{BASE_URL}/student-exams/multiexam?StudentId={studentId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<ExamListOfStudentResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Log exception here
                return null;
            }
        }

        public async Task<List<ExamListOfStudentResponse>?> GetAllPracticeExamOfStudentAsync(Guid studentId)
        {
            try
            {
                var url = $"{BASE_URL}/student-exams/pracexam?StudentId={studentId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<ExamListOfStudentResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Log exception here
                return null;
            }
        }
    }
}
