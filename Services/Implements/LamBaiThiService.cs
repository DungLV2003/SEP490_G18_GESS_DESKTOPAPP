using SEP490_G18_GESS_DESKTOPAPP.Helpers;
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
        private const string BASE_URL = "https://localhost:7074/api/StudentExam";
        private readonly JsonSerializerOptions _jsonOptions;

        public LamBaiThiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ExamInfoResponseDTO?> CheckExamNameAndCodeMEAsync(CheckExamRequestDTO request)
        {
            try
            {
                var url = $"{BASE_URL}/CheckExamNameAndCodeME";

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodeME Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<APIResponse<ExamInfoResponseDTO>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodeME Exception: {ex}");
                return null;
            }
        }

        public async Task<List<QuestionMultiExamSimpleDTO>?> GetAllQuestionMultiExamByMultiExamIdAsync(int multiExamId)
        {
            try
            {
                var url = $"{BASE_URL}GetAllQuestionMultiExam/{multiExamId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetAllQuestionMultiExam Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<APIResponse<List<QuestionMultiExamSimpleDTO>>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllQuestionMultiExam Exception: {ex}");
                return null;
            }
        }

        public async Task<List<MultiAnswerOfQuestionDTO>?> GetAllMultiAnswerOfQuestionAsync(int multiQuestionId)
        {
            try
            {
                var url = $"{BASE_URL}/GetAllMultiAnswerOfQuestion/{multiQuestionId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetAllMultiAnswerOfQuestion Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<APIResponse<List<MultiAnswerOfQuestionDTO>>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllMultiAnswerOfQuestion Exception: {ex}");
                return null;
            }
        }

        public async Task<UpdateMultiExamProgressResponseDTO?> UpdateProgressAsync(UpdateMultiExamProgressDTO dto)
        {
            try
            {
                var url = $"{BASE_URL}/update-progress";

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"UpdateProgress Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<UpdateMultiExamProgressResponseDTO>(json, _jsonOptions);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProgress Exception: {ex}");
                return null;
            }
        }

        public async Task<SubmitExamResponseDTO?> SubmitExamAsync(UpdateMultiExamProgressDTO dto)
        {
            try
            {
                var url = $"{BASE_URL}/submit-exam";

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"SubmitExam Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SubmitExamResponseDTO>(json, _jsonOptions);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitExam Exception: {ex}");
                return null;
            }
        }

        #region Practice Exam Methods (Methods mới)

        public async Task<PracticeExamInfoResponseDTO?> CheckExamNameAndCodePEAsync(CheckPracticeExamRequestDTO request)
        {
            try
            {
                var url = $"{BASE_URL}/CheckExamNameAndCodePE";

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodePE Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<APIResponse<PracticeExamInfoResponseDTO>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodePE Exception: {ex}");
                return null;
            }
        }

        public async Task<List<QuestionOrderDTO>?> GetQuestionAndAnswerByPracExamIdAsync(int pracExamId)
        {
            try
            {
                var url = $"{BASE_URL}/GetQuestionAndAnswerByPracExamId/{pracExamId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetQuestionAndAnswerByPracExamId Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<QuestionOrderDTO>>(json, _jsonOptions);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetQuestionAndAnswerByPracExamId Exception: {ex}");
                return null;
            }
        }

        public async Task<List<PracticeAnswerOfQuestionDTO>?> GetPracticeAnswerOfQuestionAsync(int pracExamId)
        {
            try
            {
                var url = $"{BASE_URL}/GetPracticeAnswerOfQuestion/{pracExamId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetPracticeAnswerOfQuestion Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<List<PracticeAnswerOfQuestionDTO>>(json, _jsonOptions);

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPracticeAnswerOfQuestion Exception: {ex}");
                return null;
            }
        }

        public async Task<bool> UpdatePEEach5minutesAsync(UpdatePracticeExamAnswersRequest request)
        {
            try
            {
                var url = $"{BASE_URL}/UpdatePEEach5minutes";

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"UpdatePEEach5minutes Error: {response.StatusCode} - {errorContent}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePEEach5minutes Exception: {ex}");
                return false;
            }
        }

        public async Task<SubmitPracticeExamResponseDTO?> SubmitPracticeExamAsync(SubmitPracticeExamRequest dto)
        {
            try
            {
                var url = $"{BASE_URL}/SubmitPracticeExam";

                var content = new StringContent(
                    JsonSerializer.Serialize(dto),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"SubmitPracticeExam Error: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<APIResponse<SubmitPracticeExamResponseDTO>>(json, _jsonOptions);

                return apiResponse?.Data;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitPracticeExam Exception: {ex}");
                return null;
            }
        }

        #endregion
    }
}
