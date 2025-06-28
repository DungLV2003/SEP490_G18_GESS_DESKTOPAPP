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
using System.Windows.Xps;

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
                var json = await response.Content.ReadAsStringAsync();

                // Parse response trong mọi trường hợp để lấy message
                APIResponse<ExamInfoResponseDTO> apiResponse = null;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<APIResponse<ExamInfoResponseDTO>>(json, _jsonOptions);
                }
                catch (JsonException)
                {
                    // Nếu không parse được, có thể response không đúng format
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                // Kiểm tra response
                if (!response.IsSuccessStatusCode || apiResponse?.Success == false)
                {
                    var errorMessage = apiResponse?.Message ?? $"Lỗi server: {response.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodeME Error: {errorMessage}");
                    throw new APIException(errorMessage);
                }

                return apiResponse?.Data;
            }
            catch (APIException)
            {
                throw; // Re-throw APIException
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodeME Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        public async Task<List<QuestionMultiExamSimpleDTO>?> GetAllQuestionMultiExamByMultiExamIdAsync(int multiExamId)
        {
            try
            {
                var url = $"{BASE_URL}/GetAllQuestionMultiExam/{multiExamId}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                APIResponse<List<QuestionMultiExamSimpleDTO>> apiResponse = null;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<APIResponse<List<QuestionMultiExamSimpleDTO>>>(json, _jsonOptions);
                }
                catch (JsonException)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                if (!response.IsSuccessStatusCode || apiResponse?.Success == false)
                {
                    var errorMessage = apiResponse?.Message ?? $"Lỗi server: {response.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"GetAllQuestionMultiExam Error: {errorMessage}");
                    throw new APIException(errorMessage);
                }

                return apiResponse?.Data;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllQuestionMultiExam Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        public async Task<List<MultiAnswerOfQuestionDTO>?> GetAllMultiAnswerOfQuestionAsync(int multiQuestionId)
        {
            try
            {
                var url = $"{BASE_URL}/GetAllMultiAnswerOfQuestion/{multiQuestionId}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                APIResponse<List<MultiAnswerOfQuestionDTO>> apiResponse = null;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<APIResponse<List<MultiAnswerOfQuestionDTO>>>(json, _jsonOptions);
                }
                catch (JsonException)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                if (!response.IsSuccessStatusCode || apiResponse?.Success == false)
                {
                    var errorMessage = apiResponse?.Message ?? $"Lỗi server: {response.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"GetAllMultiAnswerOfQuestion Error: {errorMessage}");
                    throw new APIException(errorMessage);
                }

                return apiResponse?.Data;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllMultiAnswerOfQuestion Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
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
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Try to parse error message
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<APIResponse>(json, _jsonOptions);
                        throw new APIException(errorResponse?.Message ?? $"Lỗi server: {response.StatusCode}");
                    }
                    catch (JsonException)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                var result = JsonSerializer.Deserialize<UpdateMultiExamProgressResponseDTO>(json, _jsonOptions);
                return result;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateProgress Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
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
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<APIResponse>(json, _jsonOptions);
                        throw new APIException(errorResponse?.Message ?? $"Lỗi server: {response.StatusCode}");
                    }
                    catch (JsonException)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                var result = JsonSerializer.Deserialize<SubmitExamResponseDTO>(json, _jsonOptions);
                return result;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitExam Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        #region Practice Exam Methods

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
                var json = await response.Content.ReadAsStringAsync();

                APIResponse<PracticeExamInfoResponseDTO> apiResponse = null;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<APIResponse<PracticeExamInfoResponseDTO>>(json, _jsonOptions);
                }
                catch (JsonException)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                if (!response.IsSuccessStatusCode || apiResponse?.Success == false)
                {
                    var errorMessage = apiResponse?.Message ?? $"Lỗi server: {response.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodePE Error: {errorMessage}");
                    throw new APIException(errorMessage);
                }

                return apiResponse?.Data;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CheckExamNameAndCodePE Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        public async Task<List<QuestionOrderDTO>?> GetQuestionAndAnswerByPracExamIdAsync(int pracExamId)
        {
            try
            {
                var url = $"{BASE_URL}/GetQuestionAndAnswerByPracExamId/{pracExamId}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<APIResponse>(json, _jsonOptions);
                        throw new APIException(errorResponse?.Message ?? $"Lỗi server: {response.StatusCode}");
                    }
                    catch (JsonException)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                var result = JsonSerializer.Deserialize<List<QuestionOrderDTO>>(json, _jsonOptions);
                return result;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetQuestionAndAnswerByPracExamId Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        public async Task<List<PracticeAnswerOfQuestionDTO>?> GetPracticeAnswerOfQuestionAsync(int pracExamId)
        {
            try
            {
                var url = $"{BASE_URL}/GetPracticeAnswerOfQuestion/{pracExamId}";

                var response = await _httpClient.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<APIResponse>(json, _jsonOptions);
                        throw new APIException(errorResponse?.Message ?? $"Lỗi server: {response.StatusCode}");
                    }
                    catch (JsonException)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                var result = JsonSerializer.Deserialize<List<PracticeAnswerOfQuestionDTO>>(json, _jsonOptions);
                return result;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetPracticeAnswerOfQuestion Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
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
                    var json = await response.Content.ReadAsStringAsync();
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<APIResponse>(json, _jsonOptions);
                        throw new APIException(errorResponse?.Message ?? $"Lỗi server: {response.StatusCode}");
                    }
                    catch (JsonException)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                return true;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePEEach5minutes Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
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
                var json = await response.Content.ReadAsStringAsync();

                APIResponse<SubmitPracticeExamResponseDTO> apiResponse = null;
                try
                {
                    apiResponse = JsonSerializer.Deserialize<APIResponse<SubmitPracticeExamResponseDTO>>(json, _jsonOptions);
                }
                catch (JsonException)
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new APIException($"Lỗi server: {response.StatusCode}");
                    }
                }

                if (!response.IsSuccessStatusCode || apiResponse?.Success == false)
                {
                    var errorMessage = apiResponse?.Message ?? $"Lỗi server: {response.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"SubmitPracticeExam Error: {errorMessage}");
                    throw new APIException(errorMessage);
                }

                return apiResponse?.Data;
            }
            catch (APIException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SubmitPracticeExam Exception: {ex}");
                throw new APIException("Lỗi kết nối đến server", "CONNECTION_ERROR");
            }
        }

        #endregion
    }
}