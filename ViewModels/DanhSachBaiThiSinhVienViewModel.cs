using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using SEP490_G18_GESS_DESKTOPAPP.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xaml;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class DanhSachBaiThiSinhVienViewModel : BaseViewModel
    {
        private readonly IDanhSachBaiThiService _danhSachBaiThiService;
        private readonly INavigationService _navigationService;
        private readonly ILamBaiThiService _lamBaiThiService;


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

        // Thêm property để xác định loại thi hiện tại
        public ExamType CurrentExamType => IsMultiExamSelected ? ExamType.MultipleChoice : ExamType.Practice;


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
        // Thêm vào Commands section
        public ICommand BackCommand { get; }
        #endregion

        public DanhSachBaiThiSinhVienViewModel(
    IDanhSachBaiThiService danhSachBaiThiService,
    INavigationService navigationService,
    ILamBaiThiService lamBaiThiService)
        {
            _danhSachBaiThiService = danhSachBaiThiService;
            _navigationService = navigationService;
            _lamBaiThiService = lamBaiThiService;
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
            BackCommand = new RelayCommand(() => _navigationService.NavigateWithFade<DanhSachBaiThiView, HomePageView>());
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

        // Cập nhật JoinExam method
        private void JoinExam(ExamListOfStudentResponse exam)
        {
            if (exam == null) return;

            try
            {
                // Truyền thêm CurrentExamType dựa trên tab đang được chọn
                var dialogViewModel = new DialogNhapMaBaiThiViewModel(
                    _lamBaiThiService,
                    exam,
                    _currentStudentId,
                    CurrentExamType); // Truyền loại thi từ tab hiện tại

             
                var dialog = new DialogNhapMaBaiThiView(dialogViewModel);
                dialog.ShowDialog(); 
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi mở dialog: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"JoinExam Error: {ex}");
            }
        }
        #endregion
    }

}
