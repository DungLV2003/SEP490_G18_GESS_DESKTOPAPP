using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.RunningApplicationDTO;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog
{
    public class DialogCanhBaoUngDungCamViewModel : BaseViewModel
    {
        #region Properties
        private string _warningTitle;
        public string WarningTitle
        {
            get => _warningTitle;
            set => SetProperty(ref _warningTitle, value);
        }

        private string _warningMessage;
        public string WarningMessage
        {
            get => _warningMessage;
            set => SetProperty(ref _warningMessage, value);
        }

        private string _warningDetail;
        public string WarningDetail
        {
            get => _warningDetail;
            set => SetProperty(ref _warningDetail, value);
        }

        private ObservableCollection<RunningApplication> _blockedApplications;
        public ObservableCollection<RunningApplication> BlockedApplications
        {
            get => _blockedApplications;
            set => SetProperty(ref _blockedApplications, value);
        }

        private string _primaryButtonText;
        public string PrimaryButtonText
        {
            get => _primaryButtonText;
            set => SetProperty(ref _primaryButtonText, value);
        }

        private string _secondaryButtonText;
        public string SecondaryButtonText
        {
            get => _secondaryButtonText;
            set => SetProperty(ref _secondaryButtonText, value);
        }

        // Properties for UI control
        public bool IsNotificationOnly => _isNotificationOnly;
        public bool ShowActionButtons => !_isNotificationOnly;
        public bool ShowSecondaryButton => !_isNotificationOnly && !string.IsNullOrEmpty(SecondaryButtonText);

        // Actions
        private Action _onCloseApplicationsAction;
        private Action _onCancelAction;
        private Action _onContinueToExamAction;

        // Store exam for continuing
        private ExamListOfStudentResponse _exam;
        private bool _isNotificationOnly;
        #endregion

        #region Commands
        public ICommand CloseApplicationsCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand CloseIndividualAppCommand { get; }
        #endregion

        public DialogCanhBaoUngDungCamViewModel(
            ObservableCollection<RunningApplication> blockedApps,
            ExamListOfStudentResponse exam = null,
            bool isNotificationOnly = false,
            Action onCloseApplicationsAction = null,
            Action onCancelAction = null,
            Action onContinueToExamAction = null)
        {
            BlockedApplications = new ObservableCollection<RunningApplication>(blockedApps);
            _exam = exam;
            _isNotificationOnly = isNotificationOnly;
            _onCloseApplicationsAction = onCloseApplicationsAction;
            _onCancelAction = onCancelAction;
            _onContinueToExamAction = onContinueToExamAction;

            WarningTitle = "Không thể tham gia làm bài";
            WarningMessage = "Phát hiện ứng dụng bị cấm đang chạy";
            
            if (_isNotificationOnly)
            {
                WarningDetail = $"Hệ thống phát hiện {BlockedApplications.Count} ứng dụng bị cấm đang chạy. Vui lòng đóng tất cả các ứng dụng này và nhập lại mã OTP để tiếp tục.";
                PrimaryButtonText = "Đã hiểu";
                SecondaryButtonText = "";
            }
            else
            {
                WarningDetail = $"Hệ thống phát hiện {BlockedApplications.Count} ứng dụng bị cấm. Vui lòng đóng tất cả các ứng dụng này trước khi tham gia làm bài.";
                PrimaryButtonText = "Đóng tất cả ứng dụng";
                SecondaryButtonText = "Hủy";
            }

            CloseApplicationsCommand = new RelayCommand(OnCloseApplications);
            CancelCommand = new RelayCommand(OnCancel);
            CloseIndividualAppCommand = new RelayCommand<RunningApplication>(OnCloseIndividualApp);
        }

        private void OnCloseApplications()
        {
            if (_isNotificationOnly)
            {
                // In notification mode, just close the dialog
                _onCancelAction?.Invoke();
                CloseDialog();
                return;
            }

            // Check current state for interactive mode
            if (BlockedApplications.Count == 0 && PrimaryButtonText == "Tiếp tục làm bài")
            {
                // All apps closed, continue to exam
                _onContinueToExamAction?.Invoke();
            }
            else
            {
                // Execute close all apps action
                _onCloseApplicationsAction?.Invoke();
            }

            // Close this dialog
            CloseDialog();
        }

        private void OnCancel()
        {
            // Execute custom action if provided
            _onCancelAction?.Invoke();

            // Close this dialog
            CloseDialog();
        }

        private void OnCloseIndividualApp(RunningApplication app)
        {
            if (app != null)
            {
                try
                {
                    // Close the actual application processes
                    foreach (var processId in app.ProcessIds.ToList())
                    {
                        try
                        {
                            var process = System.Diagnostics.Process.GetProcessById(processId);
                            if (process != null && !process.HasExited)
                            {
                                process.Kill();
                                System.Diagnostics.Debug.WriteLine($"Killed process: {processId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Failed to kill process {processId}: {ex.Message}");
                        }
                    }

                    // Remove from the dialog list
                    BlockedApplications.Remove(app);
                    
                    // Update warning detail based on remaining apps
                    if (BlockedApplications.Count == 0)
                    {
                        WarningDetail = "Tất cả ứng dụng bị cấm đã được đóng. Bạn có thể tiếp tục tham gia làm bài.";
                        PrimaryButtonText = "Tiếp tục làm bài";
                    }
                    else
                    {
                        WarningDetail = $"Còn lại {BlockedApplications.Count} ứng dụng bị cấm. Vui lòng đóng tất cả trước khi tham gia làm bài.";
                    }

                    System.Diagnostics.Debug.WriteLine($"Closed individual app: {app.ApplicationName}. Remaining: {BlockedApplications.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error closing individual app: {ex.Message}");
                }
            }
        }

        private void CloseDialog()
        {
            Application.Current.Windows.OfType<Views.Dialog.DialogCanhBaoUngDungCamView>()
                .FirstOrDefault()?.Close();
        }
    }
} 