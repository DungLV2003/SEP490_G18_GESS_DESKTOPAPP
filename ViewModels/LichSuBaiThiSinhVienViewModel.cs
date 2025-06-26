using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.LichSuBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class LichSuBaiThiSinhVienViewModel : BaseViewModel
    {
        private readonly ILichSuBaiThiSinhVienService _lichSuBaiThiService;
        // Thêm vào phần Properties
        private readonly INavigationService _navigationService;

        #region Properties
        private ObservableCollection<int> _yearList;
        public ObservableCollection<int> YearList
        {
            get => _yearList;
            set => SetProperty(ref _yearList, value);
        }

        private ObservableCollection<SemesterResponse> _semesterList;
        public ObservableCollection<SemesterResponse> SemesterList
        {
            get => _semesterList;
            set => SetProperty(ref _semesterList, value);
        }

        private ObservableCollection<AllSubjectBySemesterOfStudentDTOResponse> _subjectList;
        public ObservableCollection<AllSubjectBySemesterOfStudentDTOResponse> SubjectList
        {
            get => _subjectList;
            set => SetProperty(ref _subjectList, value);
        }

        private ObservableCollection<HistoryExamOfStudentDTOResponse> _examHistoryList;
        public ObservableCollection<HistoryExamOfStudentDTOResponse> ExamHistoryList
        {
            get => _examHistoryList;
            set => SetProperty(ref _examHistoryList, value);
        }

        private int? _selectedYear;
        public int? SelectedYear
        {
            get => _selectedYear;
            set
            {
                if (SetProperty(ref _selectedYear, value) && !_isInitializing)
                {
                    _ = Task.Run(async () => await OnYearChangedAsync());
                }
            }
        }

        private SemesterResponse _selectedSemester;
        public SemesterResponse SelectedSemester
        {
            get => _selectedSemester;
            set
            {
                if (SetProperty(ref _selectedSemester, value) && !_isInitializing)
                {
                    _ = Task.Run(async () => await OnSemesterChangedAsync());
                }
            }
        }

        private AllSubjectBySemesterOfStudentDTOResponse _selectedSubject;
        public AllSubjectBySemesterOfStudentDTOResponse SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                if (SetProperty(ref _selectedSubject, value) && value != null)
                {
                    _ = Task.Run(async () => await LoadExamHistoryAsync());
                }
                else if (value == null)
                {
                    // Clear exam history when no subject selected
                    Application.Current.Dispatcher.InvokeAsync(() => ExamHistoryList.Clear());
                }
            }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Flag để tránh trigger events khi đang khởi tạo
        private bool _isInitializing = false;

        // Temporary StudentId
        private readonly Guid _currentStudentId = Guid.Parse("59e9f29d-ff3a-4917-b179-08ddb30066ed");
        #endregion

        #region Commands
        public ICommand BackCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        public LichSuBaiThiSinhVienViewModel(ILichSuBaiThiSinhVienService lichSuBaiThiService, INavigationService navigationService)
        {
            _lichSuBaiThiService = lichSuBaiThiService;
            _navigationService = navigationService;
            YearList = new ObservableCollection<int>();
            SemesterList = new ObservableCollection<SemesterResponse>();
            SubjectList = new ObservableCollection<AllSubjectBySemesterOfStudentDTOResponse>();
            ExamHistoryList = new ObservableCollection<HistoryExamOfStudentDTOResponse>();

            LoadDataCommand = new RelayCommand(async () => await LoadInitialDataAsync());
            RefreshCommand = new RelayCommand(async () => await LoadInitialDataAsync());
            BackCommand = new RelayCommand(() =>
      _navigationService.NavigateWithFade<LichSuBaiThiSinhVienView, HomePageView>());


            // Load dữ liệu ban đầu
            _ = Task.Run(async () => await LoadInitialDataAsync());
        }

        #region Private Methods
        private async Task LoadInitialDataAsync()
        {
            try
            {
                _isInitializing = true;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = null;
                    // Clear all data
                    YearList.Clear();
                    SemesterList.Clear();
                    SubjectList.Clear();
                    ExamHistoryList.Clear();
                    SelectedYear = null;
                    SelectedSemester = null;
                    SelectedSubject = null;
                });

                // BƯỚC 1: Load danh sách môn học đầu tiên (không filter)
                System.Diagnostics.Debug.WriteLine("Step 1: Loading initial subjects...");
                var initialSubjects = await _lichSuBaiThiService.GetAllSubjectBySemesterOfStudentAsync(_currentStudentId);

                if (initialSubjects == null || initialSubjects.Count == 0)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = "Không có môn học nào được tìm thấy.";
                        IsLoading = false;
                    });
                    return;
                }

                // BƯỚC 2: Tìm semester và year mới nhất từ danh sách môn học
                var latestSubject = initialSubjects
                    .Where(s => s.Year > 0 && s.SemesterId.HasValue)
                    .OrderByDescending(s => s.Year)
                    .ThenByDescending(s => s.SemesterId)
                    .FirstOrDefault();

                if (latestSubject == null)
                {
                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        ErrorMessage = "Không tìm thấy thông tin kỳ học hợp lệ.";
                        IsLoading = false;
                    });
                    return;
                }

                var currentYear = latestSubject.Year;
                var currentSemesterId = latestSubject.SemesterId.Value;

                System.Diagnostics.Debug.WriteLine($"Step 2: Found latest - Year: {currentYear}, Semester: {currentSemesterId}");

                // BƯỚC 3: Load danh sách tất cả năm học
                System.Diagnostics.Debug.WriteLine("Step 3: Loading all years...");
                var allYears = await _lichSuBaiThiService.GetAllYearOfStudentAsync(_currentStudentId);

                // BƯỚC 4: Load danh sách semester theo năm hiện tại
                System.Diagnostics.Debug.WriteLine($"Step 4: Loading semesters for year {currentYear}...");
                var currentSemesters = await _lichSuBaiThiService.GetSemestersByYearAsync(currentYear, _currentStudentId);

                // BƯỚC 6: Update UI
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    // Set years
                    if (allYears != null && allYears.Count > 0)
                    {
                        foreach (var year in allYears.OrderByDescending(y => y))
                        {
                            YearList.Add(year);
                        }
                    }

                    // Set semesters
                    if (currentSemesters != null && currentSemesters.Count > 0)
                    {
                        foreach (var semester in currentSemesters)
                        {
                            SemesterList.Add(semester);
                        }
                    }

                    // Set subjects
                    if (initialSubjects != null && initialSubjects.Count > 0)
                    {
                        foreach (var subject in initialSubjects)
                        {
                            SubjectList.Add(subject);
                        }
                    }

                    // Set selected values (không trigger events)
                    SelectedYear = currentYear;
                    SelectedSemester = SemesterList.FirstOrDefault(s => s.SemesterId == currentSemesterId);
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải dữ liệu: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"LoadInitialDataAsync Error: {ex}");
                });
            }
            finally
            {
                _isInitializing = false;
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private async Task OnYearChangedAsync()
        {
            if (!SelectedYear.HasValue || _isInitializing) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Year changed to: {SelectedYear}");

                // Load semesters cho năm mới
                var semesters = await _lichSuBaiThiService.GetSemestersByYearAsync(SelectedYear.Value, _currentStudentId);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SemesterList.Clear();
                    SelectedSemester = null;

                    if (semesters != null && semesters.Count > 0)
                    {
                        foreach (var semester in semesters)
                        {
                            SemesterList.Add(semester);
                        }
                        // Auto select first semester
                        SelectedSemester = semesters.FirstOrDefault();
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải kỳ học: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"OnYearChangedAsync Error: {ex}");
                });
            }
        }

        private async Task OnSemesterChangedAsync()
        {
            if (SelectedSemester == null || !SelectedYear.HasValue || _isInitializing) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Semester changed to: {SelectedSemester.SemesterName}");

                // Load subjects theo semester và year đã chọn
                var subjects = await _lichSuBaiThiService.GetAllSubjectBySemesterOfStudentAsync(
                    _currentStudentId, SelectedSemester.SemesterId, SelectedYear.Value);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SubjectList.Clear();
                    SelectedSubject = null; // Clear selected subject

                    if (subjects != null && subjects.Count > 0)
                    {
                        foreach (var subject in subjects)
                        {
                            SubjectList.Add(subject);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải môn học: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"OnSemesterChangedAsync Error: {ex}");
                });
            }
        }

        private async Task LoadExamHistoryAsync()
        {
            if (SelectedSubject == null) return;

            try
            {
                System.Diagnostics.Debug.WriteLine($"Loading exam history for subject: {SelectedSubject.Name}");

                var examHistory = await _lichSuBaiThiService.GetHistoryExamOfStudentBySubIdAsync(
                    SelectedSubject.Id, _currentStudentId, SelectedSemester?.SemesterId, SelectedYear);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ExamHistoryList.Clear();
                    if (examHistory != null && examHistory.Count > 0)
                    {
                        foreach (var exam in examHistory)
                        {
                            ExamHistoryList.Add(exam);
                        }
                        System.Diagnostics.Debug.WriteLine($"Loaded {examHistory.Count} exam history records");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No exam history found");
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải lịch sử thi: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"LoadExamHistoryAsync Error: {ex}");
                });
            }
        }
        #endregion
    }
}