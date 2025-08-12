using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.RunningApplicationDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
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
using System.Windows.Xps;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogNhapMaBaiThiViewModel : BaseViewModel
    {
        private readonly ILamBaiThiService _lamBaiThiService;
        private readonly ExamListOfStudentResponse _examInfo;
        private readonly Guid _studentId;
        private readonly ExamType _examType;
        private readonly ObservableCollection<RunningApplication> _runningApplications;

        #region Properties
        private string _examCode;
        public string ExamCode
        {
            get => _examCode;
            set => SetProperty(ref _examCode, value);
        }

        private string _otpCode;
        public string OTPCode
        {
            get => _otpCode;
            set => SetProperty(ref _otpCode, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Properties để hiển thị thông tin loại thi
        public ExamType CurrentExamType => _examType;

        public string ExamTypeDisplayText => _examType == ExamType.MultipleChoice ? "Trắc nghiệm" : "Tự luận";

        public string DialogTitle => $"Nhập thông tin bài thi {ExamTypeDisplayText}";

        public string DialogSubtitle => $"Vui lòng nhập OTP để vào thi {ExamTypeDisplayText}";
        #endregion

        #region Commands
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        #endregion

        public DialogNhapMaBaiThiViewModel(
            ILamBaiThiService lamBaiThiService,
            ExamListOfStudentResponse examInfo,
            Guid studentId,
            ExamType examType,
            ObservableCollection<RunningApplication> runningApplications = null)
        {
            _lamBaiThiService = lamBaiThiService;
            _examInfo = examInfo;
            _studentId = studentId;
            _examType = examType;
            _runningApplications = runningApplications ?? new ObservableCollection<RunningApplication>();

            // Auto-fill exam code based on exam type
            ExamCode = _examType == ExamType.MultipleChoice
                ? $"{examInfo.ExamName}"
                : $"{examInfo.ExamName}"; // Tự luận dùng ExamName

            ConfirmCommand = new RelayCommand(async () => await ConfirmAsync(), () => !IsLoading && !string.IsNullOrEmpty(OTPCode));
            CancelCommand = new RelayCommand(Cancel);
        }

        private async System.Threading.Tasks.Task ConfirmAsync()
        {
            try
            {
                IsLoading = true;

                // Debug log for exam entry process
                System.Diagnostics.Debug.WriteLine($"🎯 [EXAM ENTRY] Starting exam entry process");
                System.Diagnostics.Debug.WriteLine($"🎯 [EXAM ENTRY] Exam Type: {_examType}");
                System.Diagnostics.Debug.WriteLine($"🎯 [EXAM ENTRY] Student ID: {_studentId}");
                System.Diagnostics.Debug.WriteLine($"🎯 [EXAM ENTRY] Exam ID: {_examInfo.ExamId}");
                System.Diagnostics.Debug.WriteLine($"🎯 [EXAM ENTRY] OTP Code: {OTPCode}");
                
                Console.WriteLine($"🎯 [EXAM ENTRY] Starting {_examType} exam entry process");

                // Check current token before making request
                var tokenInfo = Helpers.ApiHelper.GetCurrentTokenInfo();
                System.Diagnostics.Debug.WriteLine($"🔑 [EXAM ENTRY] Token available: {!string.IsNullOrEmpty(tokenInfo?.AccessToken)}");
                if (!string.IsNullOrEmpty(tokenInfo?.AccessToken))
                {
                    System.Diagnostics.Debug.WriteLine($"🔑 [EXAM ENTRY] Token expiry: {tokenInfo.Expiry}");
                    System.Diagnostics.Debug.WriteLine($"🔑 [EXAM ENTRY] Token expired: {tokenInfo.IsExpired}");
                }
                Console.WriteLine($"🔑 [EXAM ENTRY] Token available: {!string.IsNullOrEmpty(tokenInfo?.AccessToken)}");

                // Check for blocked applications BEFORE proceeding
                if (_runningApplications?.Count > 0)
                {
                    // Show warning dialog - notification only
                    ShowBlockedApplicationsNotification();
                    return;
                }

                // No blocked applications - proceed with exam
                if (_examType == ExamType.MultipleChoice)
                {
                    await HandleMultipleChoiceExamAsync();
                }
                else
                {
                    await HandlePracticeExamAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ShowBlockedApplicationsNotification()
        {
            try
            {
                // Ẩn dialog nhập OTP trước khi hiện dialog cảnh báo
                var currentDialog = GetCurrentDialog();
                if (currentDialog != null)
                {
                    currentDialog.Visibility = Visibility.Hidden;
                }

                var dialogViewModel = new DialogCanhBaoUngDungCamViewModel(
                    _runningApplications,
                    _examInfo,
                    isNotificationOnly: true, // Chỉ thông báo - không có action buttons
                    onCancelAction: () =>
                    {
                        // Hiện lại dialog nhập OTP khi đóng dialog cảnh báo
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (currentDialog != null)
                            {
                                currentDialog.Visibility = Visibility.Visible;
                                currentDialog.Activate();
                            }
                        });
                    });

                var dialog = new DialogCanhBaoUngDungCamView(dialogViewModel);

                // Set owner to main window instead of current dialog
                var mainWindow = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.GetType().Name != "DialogNhapMaBaiThiView" && w.IsVisible);

                if (mainWindow != null)
                {
                    dialog.Owner = mainWindow;
                }

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                // Log error silently - no action needed
            }
        }

        private DialogNhapMaBaiThiView GetCurrentDialog()
        {
            return Application.Current.Windows.OfType<DialogNhapMaBaiThiView>().FirstOrDefault();
        }

        private async System.Threading.Tasks.Task HandleMultipleChoiceExamAsync()
        {
            var request = new CheckExamRequestDTO
            {
                ExamId = _examInfo.ExamId,
                Code = OTPCode,
                StudentId = _studentId
            };

            try
            {
                var result = await _lamBaiThiService.CheckExamNameAndCodeMEAsync(request);

                if (result != null)
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        // Chuyển thẳng sang LamBaiThiView mà không hiển thị dialog thành công
                        var lamBaiThiView = App.AppHost.Services.GetRequiredService<LamBaiThiView>();
                        var lamBaiThiViewModel = App.AppHost.Services.GetRequiredService<LamBaiThiViewModel>();

                        // Initialize exam data
                        await lamBaiThiViewModel.InitializeExam(ExamType.MultipleChoice, result, _examInfo.ExamId);

                        lamBaiThiView.DataContext = lamBaiThiViewModel;
                        lamBaiThiView.Show();

                        // Close all dialogs and parent window
                        Application.Current.Windows.OfType<DialogNhapMaBaiThiView>().FirstOrDefault()?.Close();
                        Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault()?.Close();
                    });
                }
            }
            catch (APIException apiEx)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentDialog = GetCurrentDialog();
                    
                    // Ẩn dialog nhập OTP trước khi hiện dialog lỗi
                    if (currentDialog != null)
                    {
                        currentDialog.Visibility = Visibility.Hidden;
                    }

                    // Xử lý message lỗi cụ thể từ API
                    string errorTitle = "Xác thực thất bại";
                    string errorMessage = apiEx.Message;
                    string errorDetail = "";
                    Action retryAction = () => 
                    {
                        OTPCode = string.Empty;
                        
                        // Hiện lại dialog nhập OTP khi đóng dialog lỗi
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (currentDialog != null)
                            {
                                currentDialog.Visibility = Visibility.Visible;
                                currentDialog.Activate();
                            }
                        });
                    };

                    // Map error messages to user-friendly messages
                    switch (apiEx.Message)
                    {
                        case "Tên bài thi không đúng.":
                            errorMessage = "Tên bài thi không đúng!";
                            errorDetail = "Vui lòng kiểm tra lại thông tin bài thi và thử lại.";
                            break;

                        case "Mã thi không đúng.":
                            errorMessage = "Mã OTP không chính xác!";
                            errorDetail = "Vui lòng kiểm tra lại mã OTP được cung cấp và thử lại. Mã OTP có phân biệt chữ hoa chữ thường.";
                            break;

                        case "Bài thi chưa được mở.":
                            errorMessage = "Bài thi chưa được mở!";
                            errorDetail = "Bài thi này chưa đến thời gian mở. Vui lòng quay lại sau.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP nhưng không reset OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;

                        case "Bạn không thuộc lớp của bài thi này.":
                            errorTitle = "Không có quyền truy cập";
                            errorMessage = "Bạn không thuộc lớp của bài thi này@";
                            errorDetail = "Vui lòng liên hệ với giáo viên để được hỗ trợ.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;

                        case "Bạn chưa được điểm danh.":
                            errorTitle = "Chưa điểm danh";
                            errorMessage = "Bạn chưa được điểm danh!";
                            errorDetail = "Vui lòng liên hệ với giáo viên để được điểm danh trước khi vào thi.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;


                        default:
                            // Giữ nguyên message từ API nếu không match
                            errorMessage = "Hiện hệ thống đang lỗi (T_T)";
                            errorDetail = "Hãy báo cho đội ngũ hỗ trợ và xin vui lòng chờ thời gian để khắc phục hệ thống.";
                            break;
                    }

                    // Find main window for owner instead of current dialog
                    var mainWindow = Application.Current.Windows.OfType<Window>()
                        .FirstOrDefault(w => w.GetType().Name != "DialogNhapMaBaiThiView" && w.IsVisible);

                    DialogHelper.ShowErrorDialog(errorTitle, errorMessage, errorDetail, retryAction, mainWindow);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentDialog = GetCurrentDialog();
                    
                    // Ẩn dialog nhập OTP trước khi hiện dialog lỗi
                    if (currentDialog != null)
                    {
                        currentDialog.Visibility = Visibility.Hidden;
                    }

                    Action retryAction = () =>
                    {
                        // Hiện lại dialog nhập OTP khi đóng dialog lỗi
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (currentDialog != null)
                            {
                                currentDialog.Visibility = Visibility.Visible;
                                currentDialog.Activate();
                            }
                        });
                    };

                    // Find main window for owner
                    var mainWindow = Application.Current.Windows.OfType<Window>()
                        .FirstOrDefault(w => w.GetType().Name != "DialogNhapMaBaiThiView" && w.IsVisible);

                    DialogHelper.ShowErrorDialog(
                        "Lỗi hệ thống", 
                        "Có lỗi không mong muốn xảy ra!", 
                        ex.Message, 
                        retryAction, 
                        mainWindow);
                });
            }
        }

        private async System.Threading.Tasks.Task HandlePracticeExamAsync()
        {
            var request = new CheckPracticeExamRequestDTO
            {
                ExamId = _examInfo.ExamId,
                Code = OTPCode,
                StudentId = _studentId
            };

            try
            {
                var result = await _lamBaiThiService.CheckExamNameAndCodePEAsync(request);

                if (result != null)
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        // Chuyển thẳng sang LamBaiThiView mà không hiển thị dialog thành công
                        var lamBaiThiView = App.AppHost.Services.GetRequiredService<LamBaiThiView>();
                        var lamBaiThiViewModel = App.AppHost.Services.GetRequiredService<LamBaiThiViewModel>();

                        // Initialize exam data
                        await lamBaiThiViewModel.InitializeExam(ExamType.Practice, result, _examInfo.ExamId);

                        lamBaiThiView.DataContext = lamBaiThiViewModel;
                        lamBaiThiView.Show();

                        // Close all dialogs and parent window
                        Application.Current.Windows.OfType<DialogNhapMaBaiThiView>().FirstOrDefault()?.Close();
                        Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault()?.Close();
                    });
                }
            }
            catch (APIException apiEx)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentDialog = GetCurrentDialog();
                    
                    // Ẩn dialog nhập OTP trước khi hiện dialog lỗi
                    if (currentDialog != null)
                    {
                        currentDialog.Visibility = Visibility.Hidden;
                    }

                    // Xử lý message lỗi cụ thể từ API cho bài thi tự luận
                    string errorTitle = "Xác thực thất bại";
                    string errorMessage = apiEx.Message;
                    string errorDetail = "";
                    Action retryAction = () => 
                    {
                        OTPCode = string.Empty;
                        
                        // Hiện lại dialog nhập OTP khi đóng dialog lỗi
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (currentDialog != null)
                            {
                                currentDialog.Visibility = Visibility.Visible;
                                currentDialog.Activate();
                            }
                        });
                    };

                    // Map error messages cho practice exam
                    switch (apiEx.Message)
                    {
                        case "Tên bài thi không đúng.":
                            errorMessage = "Tên bài thi không đúng";
                            errorDetail = "Vui lòng kiểm tra lại thông tin bài thi tự luận và thử lại.";
                            break;

                        case "Mã thi không đúng.":
                            errorMessage = "Mã OTP không chính xác";
                            errorDetail = "Vui lòng kiểm tra lại mã OTP được cung cấp và thử lại. Mã OTP có phân biệt chữ hoa chữ thường.";
                            break;

                        case "Bài thi chưa được mở.":
                            errorMessage = "Bài thi chưa được mở";
                            errorDetail = "Bài thi tự luận này chưa đến thời gian mở. Vui lòng quay lại sau.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP nhưng không reset OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;

                        case "Bạn không thuộc lớp của bài thi này.":
                            errorTitle = "Không có quyền truy cập";
                            errorMessage = "Bạn không thuộc lớp của bài thi này";
                            errorDetail = "Vui lòng liên hệ với giáo viên để được hỗ trợ.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;

                        case "Bạn chưa được điểm danh.":
                            errorTitle = "Chưa điểm danh";
                            errorMessage = "Bạn chưa được điểm danh";
                            errorDetail = "Vui lòng liên hệ với giáo viên để được điểm danh trước khi vào thi.";
                            retryAction = () =>
                            {
                                // Hiện lại dialog nhập OTP
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    if (currentDialog != null)
                                    {
                                        currentDialog.Visibility = Visibility.Visible;
                                        currentDialog.Activate();
                                    }
                                });
                            };
                            break;

                        default:
                            errorDetail = "Vui lòng thử lại hoặc liên hệ với giáo viên để được hỗ trợ.";
                            break;
                    }

                    // Find main window for owner instead of current dialog
                    var mainWindow = Application.Current.Windows.OfType<Window>()
                        .FirstOrDefault(w => w.GetType().Name != "DialogNhapMaBaiThiView" && w.IsVisible);

                    DialogHelper.ShowErrorDialog(errorTitle, errorMessage, errorDetail, retryAction, mainWindow);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentDialog = GetCurrentDialog();
                    
                    // Ẩn dialog nhập OTP trước khi hiện dialog lỗi
                    if (currentDialog != null)
                    {
                        currentDialog.Visibility = Visibility.Hidden;
                    }

                    Action retryAction = () =>
                    {
                        // Hiện lại dialog nhập OTP khi đóng dialog lỗi
                        Application.Current.Dispatcher.BeginInvoke(() =>
                        {
                            if (currentDialog != null)
                            {
                                currentDialog.Visibility = Visibility.Visible;
                                currentDialog.Activate();
                            }
                        });
                    };

                    // Find main window for owner
                    var mainWindow = Application.Current.Windows.OfType<Window>()
                        .FirstOrDefault(w => w.GetType().Name != "DialogNhapMaBaiThiView" && w.IsVisible);

                    DialogHelper.ShowErrorDialog(
                        "Lỗi hệ thống", 
                        "Có lỗi không mong muốn xảy ra!", 
                        ex.Message, 
                        retryAction, 
                        mainWindow);
                });
            }
        }

        private void Cancel()
        {
            CloseDialog();
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogNhapMaBaiThiView>().FirstOrDefault()?.Close();
        }
    }
}