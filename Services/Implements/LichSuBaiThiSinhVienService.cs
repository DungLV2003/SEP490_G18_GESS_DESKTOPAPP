using SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using System.Net.Http;
using System.Text.Json;

namespace SEP490_G18_GESS_DESKTOPAPP.Services.Implement
{
    public class LichSuBaiThiSinhVienService : ILichSuBaiThiSinhVienService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://localhost:7074/api/Student"; // Thay đổi URL này
        private const string BASE_URL_SEMESTERS = "https://localhost:7074/api/Semesters"; // Thay đổi URL này


        public LichSuBaiThiSinhVienService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "WPF-App/1.0");
        }

        public async Task<List<AllSubjectBySemesterOfStudentDTOResponse>?> GetAllSubjectBySemesterOfStudentAsync(
            Guid studentId, int? semesterId = null, int? year = null)
        {
            try
            {
                var url = $"{BASE_URL}/subjects/{studentId}";
                var queryParams = new List<string>();

                if (semesterId.HasValue)
                    queryParams.Add($"semesterId={semesterId.Value}");

                if (year.HasValue)
                    queryParams.Add($"year={year.Value}");

                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<AllSubjectBySemesterOfStudentDTOResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Log exception here
                System.Diagnostics.Debug.WriteLine($"GetAllSubjectBySemesterOfStudentAsync Error: {ex}");
                return null;
            }
        }

        public async Task<List<int>?> GetAllYearOfStudentAsync(Guid studentId)
        {
            try
            {
                var url = $"{BASE_URL}/years/{studentId}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<int>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Log exception here
                System.Diagnostics.Debug.WriteLine($"GetAllYearOfStudentAsync Error: {ex}");
                return null;
            }
        }

        public async Task<List<HistoryExamOfStudentDTOResponse>?> GetHistoryExamOfStudentBySubIdAsync(
            int subjectId, Guid studentId, int? semesterId = null, int? year = null)
        {
            try
            {
                var url = $"{BASE_URL}/exams/{subjectId}/{studentId}";
                var queryParams = new List<string>();

                if (semesterId.HasValue)
                    queryParams.Add($"semesterId={semesterId.Value}");

                if (year.HasValue)
                    queryParams.Add($"year={year.Value}");

                if (queryParams.Count > 0)
                    url += "?" + string.Join("&", queryParams);

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<List<HistoryExamOfStudentDTOResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                // Log exception here
                System.Diagnostics.Debug.WriteLine($"GetHistoryExamOfStudentBySubIdAsync Error: {ex}");
                return null;
            }
        }

        public async Task<List<SemesterResponse>?> GetSemestersByYearAsync(int? year, Guid userId)
        {
            try
            {
                // Sử dụng API CurrentSemester thay vì by-year (vì by-year không tồn tại trong database)
                //var url = $"{BASE_URL_SEMESTERS}/by-year";
                var url = $"{BASE_URL_SEMESTERS}/CurrentSemester";
                
                System.Diagnostics.Debug.WriteLine($"API Call: {url}");
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Semesters API Response JSON: {json}");
                
                var result = JsonSerializer.Deserialize<List<SemesterResponse>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetSemestersByYearAsync Error: {ex}");
                return null;
            }
        }
    }
}
