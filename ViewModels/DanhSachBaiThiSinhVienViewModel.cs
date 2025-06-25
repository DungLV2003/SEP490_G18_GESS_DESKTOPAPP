using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class DanhSachBaiThiSinhVienViewModel : BaseViewModel
    {
        private readonly IDanhSachBaiThiService _danhSachBaiThiService;
        private readonly INavigationService _navigationService;

        #region Properties
        private ObservableCollection<ExamListOfStudentResponse> _examList;
        public ObservableCollection<ExamListOfStudentResponse> ExamList
        {
            get => _examList;
            set => SetProperty(ref _examList, value);
        }

        private bool _isMultiExamSelected = true;
        public bool IsMultiExamSelected
        {
            get => _isMultiExamSelected;
            set => SetProperty(ref _isMultiExamSelected, value);
        }

        private bool _isPracticeExamSelected = false;
        public bool IsPracticeExamSelected
        {
            get => _isPracticeExamSelected;
            set => SetProperty(ref _isPracticeExamSelected, value);
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

        // Temporary StudentId - trong thực tế sẽ lấy từ session/login
        // f4ed4675-fe72-413f-b178-08ddb30066ed cuoi ky
        // 17d4a105-511d-41aa-b177-08ddb30066ed giua ky
        // 59e9f29d-ff3a-4917-b179-08ddb30066ed ca 2

        private readonly Guid _currentStudentId = Guid.Parse("59e9f29d-ff3a-4917-b179-08ddb30066ed");
        #endregion

        #region Commands
        public ICommand LoadMultiExamCommand { get; }
        public ICommand LoadPracticeExamCommand { get; }
        public ICommand JoinExamCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion

        public DanhSachBaiThiSinhVienViewModel(
    IDanhSachBaiThiService danhSachBaiThiService,
    INavigationService navigationService)
        {
            _danhSachBaiThiService = danhSachBaiThiService;
            _navigationService = navigationService;

            ExamList = new ObservableCollection<ExamListOfStudentResponse>();

            // Commands không có parameter
            LoadMultiExamCommand = new RelayCommand(
                execute: async () => await LoadMultiExamAsync(),
                canExecute: () => !IsLoading
            );

            LoadPracticeExamCommand = new RelayCommand(
                execute: async () => await LoadPracticeExamAsync(),
                canExecute: () => !IsLoading
            );

            RefreshCommand = new RelayCommand(
                execute: async () => await RefreshCurrentTabAsync(),
                canExecute: () => !IsLoading
            );

            // Command có parameter
            JoinExamCommand = new RelayCommand<ExamListOfStudentResponse>(
                execute: JoinExam,
                canExecute: exam => exam != null && !IsLoading
            );
            // SỬA: Load dữ liệu mặc định đúng cách
            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            await LoadMultiExamAsync();
        }
        #region Private Methods
        private async Task LoadMultiExamAsync()
        {
            try
            {
                // SỬA: Đảm bảo chạy trên UI thread
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = null;
                    IsMultiExamSelected = true;
                    IsPracticeExamSelected = false;
                });

                var result = await _danhSachBaiThiService.GetAllMultiExamOfStudentAsync(_currentStudentId);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ExamList.Clear();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var exam in result)
                        {
                            ExamList.Add(exam);
                        }
                        ErrorMessage = null; // Clear error message when successful
                    }
                    else
                    {
                        ErrorMessage = "Không có bài thi trắc nghiệm nào.";
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải dữ liệu trắc nghiệm: {ex.Message}";
                    // THÊM: Log chi tiết để debug
                    System.Diagnostics.Debug.WriteLine($"LoadMultiExamAsync Error: {ex}");
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private async Task LoadPracticeExamAsync()
        {
            try
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = true;
                    ErrorMessage = null;
                    IsMultiExamSelected = false;
                    IsPracticeExamSelected = true;
                });

                var result = await _danhSachBaiThiService.GetAllPracticeExamOfStudentAsync(_currentStudentId);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ExamList.Clear();
                    if (result != null && result.Count > 0)
                    {
                        foreach (var exam in result)
                        {
                            ExamList.Add(exam);
                        }
                        ErrorMessage = null;
                    }
                    else
                    {
                        ErrorMessage = "Không có bài thi tự luận nào.";
                    }
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ErrorMessage = $"Lỗi tải dữ liệu tự luận: {ex.Message}";
                    System.Diagnostics.Debug.WriteLine($"LoadPracticeExamAsync Error: {ex}");
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoading = false;
                });
            }
        }

        private async Task RefreshCurrentTabAsync()
        {
            if (IsMultiExamSelected)
            {
                await LoadMultiExamAsync();
            }
            else if (IsPracticeExamSelected)
            {
                await LoadPracticeExamAsync();
            }
        }

        private void JoinExam(ExamListOfStudentResponse exam)
        {
            if (exam == null) return;

            try
            {
                // TODO: Implement navigation to exam view
                // Ví dụ: Navigate tới màn hình làm bài thi với ExamId

                // Option 1: Nếu có NavigationService
                // _navigationService.NavigateToExam(exam.ExamId, _currentStudentId);

                // Option 2: Nếu cần show dialog xác nhận trước
                // var confirmResult = MessageBox.Show(
                //     $"Bạn có muốn vào thi '{exam.ExamName}'?", 
                //     "Xác nhận", 
                //     MessageBoxButton.YesNo);
                // if (confirmResult == MessageBoxResult.Yes)
                // {
                //     // Navigate
                // }

                System.Windows.MessageBox.Show(
                    $"Đang vào thi: {exam.ExamName}\nMôn: {exam.SubjectName}\nThời gian: {exam.Duration} phút",
                    "Thông báo"
                );
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi vào thi: {ex.Message}";
            }
        }
        #endregion
    }

}
