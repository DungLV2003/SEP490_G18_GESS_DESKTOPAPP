using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implement
{
    public class LamBaiThiService : ILamBaiThiService
    {
        private readonly HttpClient _httpClient;

        public LamBaiThiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ExamInfoResponseDTO?> CheckExamNameAndCode(string ExamId, string ExamCode, Guid StudentId)
        {
            var url = "https://your-api-domain/api/Exam/check-exam-name-code"; // ← sửa lại đúng URL của bạn

            var body = new
            {
                thíexamId = ExamId,
                examCode = ExamCode,
                studentId = StudentId
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<ExamInfoResponseDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return result;
        }
    }
}
