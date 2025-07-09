using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.DanhSachBaiThiSinhVienDTO;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.Models.RunningApplicationDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Implements;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interfaces;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Dialog;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using SEP490_G18_GESS_DESKTOPAPP.Views.Dialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xaml;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class DanhSachBaiThiSinhVienViewModel : BaseViewModel, IDisposable
    {
        private readonly IDanhSachBaiThiService _danhSachBaiThiService;
        private readonly INavigationService _navigationService;
        private readonly ILamBaiThiService _lamBaiThiService;
        private readonly IUserService _userService;


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

        // Properties cho quản lý ứng dụng đang chạy
        private ObservableCollection<RunningApplication> _runningApplications;
        public ObservableCollection<RunningApplication> RunningApplications
        {
            get => _runningApplications;
            set => SetProperty(ref _runningApplications, value);
        }

        private bool _isLoadingApplications = false;
        public bool IsLoadingApplications
        {
            get => _isLoadingApplications;
            set => SetProperty(ref _isLoadingApplications, value);
        }

        // Properties cho smooth UI updates
        private bool _isUpdatingApplications = false;
        public bool IsUpdatingApplications
        {
            get => _isUpdatingApplications;
            set 
            { 
                if (SetProperty(ref _isUpdatingApplications, value))
                {
                    OnPropertyChanged(nameof(ApplicationsStatus));
                }
            }
        }

        private DateTime _lastSuccessfulUpdate = DateTime.MinValue;
        private readonly object _updateLock = new object();

        // Cache cho smooth updates
        private Dictionary<string, RunningApplication> _applicationCache = new Dictionary<string, RunningApplication>();

        // Real-time monitoring properties
        private ManagementEventWatcher _processStartWatcher;
        private ManagementEventWatcher _processStopWatcher;
        private DispatcherTimer _monitoringTimer;
        private bool _isMonitoringActive = false;
        private readonly object _monitoringLock = new object();
        private HashSet<int> _lastKnownProcessIds = new HashSet<int>();

        // Debug properties để theo dõi
        private int _processStartEventCount = 0;
        private int _processStopEventCount = 0;
        private int _timerTickCount = 0;
        private DateTime _lastEventTime = DateTime.MinValue;

        // Danh sách ứng dụng cần theo dõi - browsers và các ứng dụng khác
        private readonly List<string> _blockedApplications = new List<string>
        {
            // Browsers
           /* "chrome",*/ "firefox", "msedge", "brave", "opera",
            // Communication Apps
            "zalo", "discord", "telegram", "viber", "whatsapp", "messenger", "teams", "zoom", "skype",
            // Remote Access Apps
            "teamviewer", "ultraviewer", "deskin", "anydesk", "remotedesktop", "chrome remote", "vnc",
            // Media & Entertainment
            "spotify", "youtube", "vlc", "itunes", "tiktok", "facebook",
            // Games
            "steam", "origin", "epicgames", "battlenet", "roblox", "minecraft",
            // Development Tools
            //"visualstudio", "vscode", "notepad++", "sublime", "atom", "intellij", "eclipse",
            // Other Tools
            "obs", "photoshop", "gimp", "utorrent", "bittorrent", "winrar", "7zip"
        };

        // Temporary StudentId - trong thực tế sẽ lấy từ session/login
        // f4ed4675-fe72-413f-b178-08ddb30066ed cuoi ky
        // 17d4a105-511d-41aa-b177-08ddb30066ed giua ky
        // 59e9f29d-ff3a-4917-b179-08ddb30066ed ca 2

        private Guid _currentStudentId
        {
            get
            {
                var studentIdString = _userService.GetStudentId();
                if (Guid.TryParse(studentIdString, out Guid studentId))
                {
                    return studentId;
                }
                // Fallback nếu chưa có thông tin
                return Guid.Parse("59e9f29d-ff3a-4917-b179-08ddb30066ed");
            }
        }
        #endregion

        #region Commands
        public ICommand LoadMultiExamCommand { get; }
        public ICommand LoadPracticeExamCommand { get; }
        public ICommand JoinExamCommand { get; }
        public ICommand RefreshCommand { get; }
        // Thêm vào Commands section
        public ICommand BackCommand { get; }
        // Commands cho quản lý ứng dụng
        public ICommand LoadRunningApplicationsCommand { get; }
        public ICommand CloseApplicationCommand { get; }
        public ICommand RefreshApplicationsCommand { get; }
        public ICommand CloseAllApplicationsCommand { get; }

        // Command để control monitoring
        public ICommand ToggleMonitoringCommand { get; }
        public ICommand RestartMonitoringCommand { get; }
        public ICommand TestMonitoringCommand { get; }

        // Property để hiển thị trạng thái monitoring
        public bool IsMonitoringActive
        {
            get => _isMonitoringActive;
            private set => SetProperty(ref _isMonitoringActive, value);
        }

        // Properties để debug monitoring
        public string MonitoringStatus => $"Events: Start={_processStartEventCount}, Stop={_processStopEventCount}, Timer={_timerTickCount}";

        // Properties cho UI feedback
        public string ApplicationsStatus => _isUpdatingApplications ? "Đang cập nhật..." : 
                                          RunningApplications?.Count > 0 ? $"{RunningApplications.Count} ứng dụng" : "Không có ứng dụng nào";

        public bool HasApplications => RunningApplications?.Count > 0;

        // Quick commands cho UI
        public ICommand QuickRefreshCommand { get; }
        public ICommand ForceRefreshCommand { get; }
        
        // Test command để preview dialog
        public ICommand TestWarningDialogCommand { get; }
        public ICommand TestNotificationDialogCommand { get; }

        #endregion

        public DanhSachBaiThiSinhVienViewModel(
    IDanhSachBaiThiService danhSachBaiThiService,
    INavigationService navigationService,
    ILamBaiThiService lamBaiThiService,
    IUserService userService)
        {
            _danhSachBaiThiService = danhSachBaiThiService;
            _navigationService = navigationService;
                _userService = userService; 
            _lamBaiThiService = lamBaiThiService;
            ExamList = new ObservableCollection<ExamListOfStudentResponse>();
            RunningApplications = new ObservableCollection<RunningApplication>();

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
            
            // Commands cho quản lý ứng dụng
            LoadRunningApplicationsCommand = new RelayCommand(
                execute: async () => await LoadRunningApplicationsAsync(),
                canExecute: () => !IsLoadingApplications
            );

            CloseApplicationCommand = new RelayCommand<RunningApplication>(
                execute: CloseApplication,
                canExecute: app => app != null && app.IsCloseable && !IsLoadingApplications
            );

            RefreshApplicationsCommand = new RelayCommand(
                execute: async () => await LoadRunningApplicationsAsync(),
                canExecute: () => !IsLoadingApplications
            );

            CloseAllApplicationsCommand = new RelayCommand(
                execute: async () => await CloseAllApplicationsAsync(),
                canExecute: () => !IsLoadingApplications && RunningApplications?.Count > 0
            );

            ToggleMonitoringCommand = new RelayCommand(
                execute: () =>
                {
                    if (IsMonitoringActive)
                    {
                        StopRealTimeMonitoring();
                    }
                    else
                    {
                        StartRealTimeMonitoring();
                    }
                },
                canExecute: () => !IsLoadingApplications
            );

            RestartMonitoringCommand = new RelayCommand(
                execute: () =>
                {
                    RestartMonitoringIfNeeded();
                },
                canExecute: () => !IsLoadingApplications
            );

            TestMonitoringCommand = new RelayCommand(
                execute: () =>
                {
                    System.Diagnostics.Debug.WriteLine("=== MONITORING STATUS ===");
                    System.Diagnostics.Debug.WriteLine($"Monitoring Active: {IsMonitoringActive}");
                    System.Diagnostics.Debug.WriteLine($"Timer Ticks: {_timerTickCount}");
                    System.Diagnostics.Debug.WriteLine($"Process Events: Start={_processStartEventCount}, Stop={_processStopEventCount}");
                    System.Diagnostics.Debug.WriteLine($"Last Event: {_lastEventTime}");
                    System.Diagnostics.Debug.WriteLine($"Last Update: {_lastSuccessfulUpdate}");
                    System.Diagnostics.Debug.WriteLine($"Current Apps in UI: {RunningApplications?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"Cache Count: {_applicationCache.Count}");
                    System.Diagnostics.Debug.WriteLine($"Is Updating: {_isUpdatingApplications}");
                    System.Diagnostics.Debug.WriteLine($"Is Loading: {IsLoadingApplications}");
                    
                    // Show current running blocked processes
                    var processes = Process.GetProcesses()
                        .Where(p => !string.IsNullOrEmpty(p.ProcessName) && 
                                   _blockedApplications.Any(blocked => 
                                       p.ProcessName.ToLower().Contains(blocked.ToLower())))
                        .ToList();
                    
                    System.Diagnostics.Debug.WriteLine($"System Blocked Processes: {processes.Count}");
                    foreach (var process in processes.Take(10)) // Show first 10
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {process.ProcessName} (PID: {process.Id}, Window: '{process.MainWindowTitle}')");
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine($"  - {process.ProcessName} (PID: {process.Id}, Window: <access denied>)");
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine("=== END STATUS ===");
                },
                canExecute: () => !IsLoadingApplications
            );

            QuickRefreshCommand = new RelayCommand(
                execute: async () => await RefreshApplicationsQuickly(),
                canExecute: () => !IsLoadingApplications
            );

            ForceRefreshCommand = new RelayCommand(
                execute: async () => await RefreshApplicationsQuickly(),
                canExecute: () => !IsLoadingApplications
            );

            TestWarningDialogCommand = new RelayCommand(
                execute: () =>
                {
                    var dialogViewModel = new DialogCanhBaoUngDungCamViewModel(
                        RunningApplications,
                        null, // No exam passed for this dialog
                        onCloseApplicationsAction: async () =>
                        {
                            await CloseAllApplicationsAsync();
                            System.Diagnostics.Debug.WriteLine("User closed blocked apps dialog");
                        },
                        onCancelAction: () =>
                        {
                            System.Diagnostics.Debug.WriteLine("User cancelled blocked apps dialog");
                        },
                        onContinueToExamAction: () =>
                        {
                            System.Diagnostics.Debug.WriteLine("User chose to continue to exam despite blocked apps");
                        });

                    var dialog = new DialogCanhBaoUngDungCamView(dialogViewModel);
                    dialog.ShowDialog();
                },
                canExecute: () => !IsLoadingApplications
            );

            TestNotificationDialogCommand = new RelayCommand(
                execute: () =>
                {
                    var dialogViewModel = new DialogCanhBaoUngDungCamViewModel(
                        RunningApplications,
                        null, // No exam
                        isNotificationOnly: true, // Notification mode
                        onCancelAction: () =>
                        {
                            System.Diagnostics.Debug.WriteLine("Notification dialog acknowledged");
                        });

                    var dialog = new DialogCanhBaoUngDungCamView(dialogViewModel);
                    dialog.ShowDialog();
                },
                canExecute: () => !IsLoadingApplications
            );


            // SỬA: Load dữ liệu mặc định đúng cách
            LoadInitialData();
            
            // Khởi tạo và bắt đầu monitoring real-time
            InitializeMonitoring();
            StartRealTimeMonitoring();
        }

        private async void LoadInitialData()
        {
            // Load exams first (priority)
            await LoadMultiExamAsync();
            
            // Load applications immediately in background - không block UI
            _ = Task.Run(async () =>
            {
                await Task.Delay(500); // Cho app khởi động xong
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    await RefreshApplicationsQuickly(); // Dùng method nhanh
                });
            });
            
            // Initialize known process IDs in background  
            _ = Task.Run(async () =>
            {
                await InitializeKnownProcessIds();
            });
        }

        private async Task InitializeKnownProcessIds()
        {
            try
            {
                await Task.Run(() =>
                {
                    var currentProcessIds = new HashSet<int>();
                    var processes = Process.GetProcesses()
                        .Where(p => !string.IsNullOrEmpty(p.ProcessName) && 
                                   _blockedApplications.Any(blocked => 
                                       p.ProcessName.ToLower().Contains(blocked.ToLower())))
                        .ToList();

                    foreach (var process in processes)
                    {
                        try
                        {
                            if (!IsSystemProtectedProcess(process) && 
                                CanCloseProcess(process))
                            {
                                currentProcessIds.Add(process.Id);
                            }
                        }
                        catch { }
                    }

                    lock (_monitoringLock)
                    {
                        _lastKnownProcessIds = currentProcessIds;
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeKnownProcessIds Error: {ex}");
            }
        }


        #region Private Methods
        
        #region Real-time Monitoring Methods
        private void InitializeMonitoring()
        {
            try
            {
                // Khởi tạo timer để backup monitoring
                _monitoringTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1) // Giảm xuống 1 giây để detect tức thì
                };
                _monitoringTimer.Tick += async (s, e) => await MonitoringTimer_Tick();

                // Khởi tạo WMI watchers
                InitializeWMIWatchers();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeMonitoring Error: {ex}");
            }
        }

        private void InitializeWMIWatchers()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Initializing WMI Watchers ===");
                
                // Dispose existing watchers if any
                _processStartWatcher?.Stop();
                _processStartWatcher?.Dispose();
                _processStopWatcher?.Stop();
                _processStopWatcher?.Dispose();

                // Watcher cho process creation với timeout
                var startQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
                _processStartWatcher = new ManagementEventWatcher(startQuery);
                _processStartWatcher.Options.Timeout = TimeSpan.FromSeconds(10);
                _processStartWatcher.EventArrived += OnProcessStarted;

                // Watcher cho process termination với timeout  
                var stopQuery = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
                _processStopWatcher = new ManagementEventWatcher(stopQuery);
                _processStopWatcher.Options.Timeout = TimeSpan.FromSeconds(10);
                _processStopWatcher.EventArrived += OnProcessStopped;
                
                System.Diagnostics.Debug.WriteLine("WMI Watchers initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"InitializeWMIWatchers Error: {ex}");
                // Fallback to timer-only monitoring if WMI fails
                System.Diagnostics.Debug.WriteLine("Falling back to timer-only monitoring");
            }
        }

        private void StartRealTimeMonitoring()
        {
            try
            {
                lock (_monitoringLock)
                {
                    if (_isMonitoringActive)
                    {
                        System.Diagnostics.Debug.WriteLine("Monitoring already active, skipping start");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("=== Starting Real-time Monitoring ===");

                    // Reset counters
                    _processStartEventCount = 0;
                    _processStopEventCount = 0;
                    _timerTickCount = 0;
                    _lastEventTime = DateTime.Now;

                    // Bắt đầu WMI watchers với error handling
                    try
                    {
                        _processStartWatcher?.Start();
                        System.Diagnostics.Debug.WriteLine("Process start watcher started");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to start process start watcher: {ex.Message}");
                    }

                    try
                    {
                        _processStopWatcher?.Start();
                        System.Diagnostics.Debug.WriteLine("Process stop watcher started");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to start process stop watcher: {ex.Message}");
                    }

                    // Bắt đầu timer backup - này luôn phải work
                    _monitoringTimer?.Start();
                    System.Diagnostics.Debug.WriteLine("Monitoring timer started");

                    _isMonitoringActive = true;
                    IsMonitoringActive = true;
                    System.Diagnostics.Debug.WriteLine("Real-time monitoring started successfully");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StartRealTimeMonitoring Error: {ex}");
            }
        }

        private void StopRealTimeMonitoring()
        {
            try
            {
                lock (_monitoringLock)
                {
                    if (!_isMonitoringActive) return;

                    // Dừng WMI watchers
                    _processStartWatcher?.Stop();
                    _processStopWatcher?.Stop();

                    // Dừng timer
                    _monitoringTimer?.Stop();

                    _isMonitoringActive = false;
                    IsMonitoringActive = false;
                    System.Diagnostics.Debug.WriteLine("Real-time monitoring stopped");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"StopRealTimeMonitoring Error: {ex}");
            }
        }

        private async void OnProcessStarted(object sender, EventArrivedEventArgs e)
        {
            try
            {
                _processStartEventCount++;
                _lastEventTime = DateTime.Now;
                
                var processName = e.NewEvent["ProcessName"]?.ToString();
                var processId = e.NewEvent["ProcessID"]?.ToString();
                
                if (string.IsNullOrEmpty(processName)) return;

                // Kiểm tra xem có phải ứng dụng cần theo dõi không
                if (IsBlockedApplication(processName))
                {
                    System.Diagnostics.Debug.WriteLine($"🚨 INSTANT DETECT: {processName} (PID: {processId})");
                    
                    // IMMEDIATE UPDATE - không delay gì cả!
                    _ = Task.Run(async () =>
                    {
                        // Chỉ delay tối thiểu để process có window
                        await Task.Delay(300); // Giảm từ 800ms xuống 300ms
                        
                        // Instant UI update
                        await Application.Current.Dispatcher.InvokeAsync(async () =>
                        {
                            await RefreshApplicationsQuickly();
                        }, System.Windows.Threading.DispatcherPriority.Send); // Highest priority
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnProcessStarted Error: {ex.Message}");
            }
        }

        private async void OnProcessStopped(object sender, EventArrivedEventArgs e)
        {
            try
            {
                _processStopEventCount++;
                _lastEventTime = DateTime.Now;
                
                var processName = e.NewEvent["ProcessName"]?.ToString();
                var processId = e.NewEvent["ProcessID"]?.ToString();
                
                if (string.IsNullOrEmpty(processName)) return;

                if (IsBlockedApplication(processName))
                {
                    System.Diagnostics.Debug.WriteLine($"🔴 PROCESS STOPPED: {processName} (PID: {processId})");
                    
                    // IMMEDIATE UI UPDATE when process stops
                    await Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        await RefreshApplicationsQuickly();
                    }, System.Windows.Threading.DispatcherPriority.Send); // Highest priority
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"OnProcessStopped Error: {ex.Message}");
            }
        }

        private async Task MonitoringTimer_Tick()
        {
            try
            {
                _timerTickCount++;
                
                // Log mỗi 30 ticks (30 giây) để không spam
                if (_timerTickCount % 30 == 0)
                {
                    var timeSinceLastUpdate = DateTime.Now - _lastSuccessfulUpdate;
                    System.Diagnostics.Debug.WriteLine($"[Monitor] Apps: {RunningApplications?.Count ?? 0}, LastUpdate: {timeSinceLastUpdate.TotalSeconds:F0}s ago");
                }
                
                // Chạy continuous monitoring thay vì chỉ backup
                await RefreshApplicationsQuickly();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MonitoringTimer_Tick Error: {ex.Message}");
            }
        }

        private void RestartMonitoringIfNeeded()
        {
            try
            {
                lock (_monitoringLock)
                {
                    if (!_isMonitoringActive)
                    {
                        System.Diagnostics.Debug.WriteLine("Monitoring not active, skipping restart");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine("Restarting monitoring watchers...");
                    
                    // Stop current watchers
                    StopRealTimeMonitoring();
                    
                    // Re-initialize and start
                    InitializeWMIWatchers();
                    StartRealTimeMonitoring();
                    
                    System.Diagnostics.Debug.WriteLine("Monitoring watchers restarted");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RestartMonitoringIfNeeded Error: {ex}");
            }
        }

        private bool IsBlockedApplication(string processName)
        {
            if (string.IsNullOrEmpty(processName)) return false;
            
            var lowerProcessName = processName.ToLower();
            return _blockedApplications.Any(blocked => lowerProcessName.Contains(blocked.ToLower()));
        }

        private async Task RefreshApplicationsQuickly()
        {
            try
            {
                // Prevent multiple concurrent updates
                lock (_updateLock)
                {
                    if (_isUpdatingApplications || IsLoadingApplications) 
                    {
                        return;
                    }
                    _isUpdatingApplications = true;
                }

                // Run detection in background without blocking UI
                var newApplications = await Task.Run(DetectBlockedApplications);
                
                // Always update - don't skip based on cache to ensure real-time
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    UpdateApplicationListInstant(newApplications);
                }, System.Windows.Threading.DispatcherPriority.Send);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshApplicationsQuickly Error: {ex}");
            }
            finally
            {
                lock (_updateLock)
                {
                    _isUpdatingApplications = false;
                }
            }
        }

        private void UpdateApplicationListInstant(Dictionary<string, RunningApplication> newApplications)
        {
            try
            {
                // Always update for real-time response - don't cache check
                _applicationCache = new Dictionary<string, RunningApplication>(newApplications);

                // Get current app keys
                var currentAppKeys = RunningApplications.Select(a => GetApplicationKey(a.ProcessName)).ToHashSet();
                var newAppKeys = newApplications.Keys.ToHashSet();

                // Remove apps that are no longer running
                var appsToRemove = RunningApplications.Where(a => !newAppKeys.Contains(GetApplicationKey(a.ProcessName))).ToList();
                foreach (var app in appsToRemove)
                {
                    RunningApplications.Remove(app);
                    System.Diagnostics.Debug.WriteLine($"➖ REMOVED: {app.ApplicationName}");
                }

                // Add new apps that just started
                var newAppsToAdd = newApplications.Where(kvp => !currentAppKeys.Contains(kvp.Key)).ToList();
                foreach (var kvp in newAppsToAdd)
                {
                    RunningApplications.Add(kvp.Value);
                    System.Diagnostics.Debug.WriteLine($"➕ ADDED: {kvp.Value.ApplicationName}");
                }

                // Update existing apps
                foreach (var existingApp in RunningApplications)
                {
                    var appKey = GetApplicationKey(existingApp.ProcessName);
                    if (newApplications.ContainsKey(appKey))
                    {
                        var newApp = newApplications[appKey];
                        existingApp.ProcessCount = newApp.ProcessCount;
                        existingApp.Status = newApp.Status;
                        existingApp.WindowTitle = newApp.WindowTitle;
                        existingApp.ProcessIds = newApp.ProcessIds;
                    }
                }

                _lastSuccessfulUpdate = DateTime.Now;
                
                // Trigger UI updates for status properties
                OnPropertyChanged(nameof(ApplicationsStatus));
                OnPropertyChanged(nameof(HasApplications));
                
                // Log changes
                if (appsToRemove.Count > 0 || newAppsToAdd.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 INSTANT UPDATE: +{newAppsToAdd.Count} -{appsToRemove.Count} = {RunningApplications.Count} total");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateApplicationListInstant Error: {ex}");
            }
        }

        private Dictionary<string, RunningApplication> DetectBlockedApplications()
        {
            var detectedApps = new Dictionary<string, RunningApplication>();
            
            try
            {
                var processes = Process.GetProcesses()
                    .Where(p => !string.IsNullOrEmpty(p.ProcessName) && 
                               _blockedApplications.Any(blocked => 
                                   p.ProcessName.ToLower().Contains(blocked.ToLower())))
                    .ToList();

                // Group processes by application type
                var groupedApps = new Dictionary<string, RunningApplication>();

                foreach (var process in processes)
                {
                    try
                    {
                        // Skip system protected processes
                        if (IsSystemProtectedProcess(process) || !CanCloseProcess(process))
                            continue;

                        var appKey = GetApplicationKey(process.ProcessName);
                        var friendlyName = GetFriendlyName(process.ProcessName);

                        if (!groupedApps.ContainsKey(appKey))
                        {
                            groupedApps[appKey] = new RunningApplication
                            {
                                ProcessName = process.ProcessName,
                                ApplicationName = friendlyName,
                                ProcessIds = new List<int>(),
                                StartTime = process.StartTime,
                                WindowTitle = process.MainWindowTitle ?? process.ProcessName,
                                Status = "Đang mở",
                                IsCloseable = !IsSystemCriticalProcess(process.ProcessName),
                                IconText = GetAppIcon(process.ProcessName),
                                ProcessCount = 0,
                                HasActiveWindow = false
                            };
                        }

                        groupedApps[appKey].ProcessIds.Add(process.Id);
                        groupedApps[appKey].ProcessCount++;
                        
                        // Check if this process has active window
                        if (!string.IsNullOrEmpty(process.MainWindowTitle) && 
                            process.MainWindowHandle != IntPtr.Zero)
                        {
                            groupedApps[appKey].HasActiveWindow = true;
                            if (string.IsNullOrEmpty(groupedApps[appKey].WindowTitle) || 
                                groupedApps[appKey].WindowTitle == process.ProcessName)
                            {
                                groupedApps[appKey].WindowTitle = process.MainWindowTitle;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing {process.ProcessName}: {ex.Message}");
                    }
                }

                // Only return apps with active windows
                foreach (var app in groupedApps.Values.Where(a => a.HasActiveWindow))
                {
                    if (app.ProcessCount > 1)
                    {
                        app.Status = $"Đang mở ({app.ProcessCount})";
                    }
                    detectedApps[GetApplicationKey(app.ProcessName)] = app;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DetectBlockedApplications Error: {ex}");
            }

            return detectedApps;
        }

        // Cleanup method
        public void Dispose()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Disposing DanhSachBaiThiSinhVienViewModel ===");
                
                StopRealTimeMonitoring();
                
                // Cleanup WMI watchers
                try
                {
                    _processStartWatcher?.Stop();
                    _processStartWatcher?.Dispose();
                    _processStartWatcher = null;
                    System.Diagnostics.Debug.WriteLine("Process start watcher disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing start watcher: {ex.Message}");
                }

                try
                {
                    _processStopWatcher?.Stop();
                    _processStopWatcher?.Dispose();
                    _processStopWatcher = null;
                    System.Diagnostics.Debug.WriteLine("Process stop watcher disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing stop watcher: {ex.Message}");
                }

                // Cleanup timer
                try
                {
                    _monitoringTimer?.Stop();
                    _monitoringTimer = null;
                    System.Diagnostics.Debug.WriteLine("Monitoring timer disposed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing timer: {ex.Message}");
                }

                System.Diagnostics.Debug.WriteLine($"Final stats - Events: Start={_processStartEventCount}, Stop={_processStopEventCount}, Timer={_timerTickCount}");
                System.Diagnostics.Debug.WriteLine("ViewModel disposed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in Dispose: {ex}");
            }
        }
        #endregion

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
                // Bỏ check ứng dụng cấm ở đây - sẽ check khi nhập code
                ProceedToExam(exam);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi mở dialog thi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"JoinExam Error: {ex}");
            }
        }

        private void ShowBlockedApplicationsWarning(ExamListOfStudentResponse exam)
        {
            try
            {
                var dialogViewModel = new DialogCanhBaoUngDungCamViewModel(
                    RunningApplications,
                    exam, // Pass exam to dialog
                    onCloseApplicationsAction: async () =>
                    {
                        // Đóng tất cả ứng dụng cấm
                        await CloseAllApplicationsAsync();
                        
                        // Đợi một chút để đảm bảo apps đã đóng
                        await Task.Delay(1000);
                        
                        // Kiểm tra lại và tiếp tục vào thi nếu sạch
                        if (RunningApplications?.Count == 0)
                        {
                            ProceedToExam(exam);
                        }
                        else
                        {
                            // Vẫn còn apps - hiển thị lại dialog
                            ShowBlockedApplicationsWarning(exam);
                        }
                    },
                    onCancelAction: () =>
                    {
                        // User hủy - không làm gì
                        System.Diagnostics.Debug.WriteLine("User cancelled joining exam");
                    },
                    onContinueToExamAction: () =>
                    {
                        // Tiếp tục vào thi khi đã đóng hết apps
                        ProceedToExam(exam);
                    });

                var dialog = new DialogCanhBaoUngDungCamView(dialogViewModel);

                // Tìm window hiện tại làm Owner
                var currentWindow = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.IsActive);

                if (currentWindow != null)
                {
                    dialog.Owner = currentWindow;
                }

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi hiển thị cảnh báo: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ShowBlockedApplicationsWarning Error: {ex}");
            }
        }

        private void ProceedToExam(ExamListOfStudentResponse exam)
        {
            try
            {
                var dialogViewModel = new DialogNhapMaBaiThiViewModel(
                    _lamBaiThiService,
                    exam,
                    _currentStudentId,
                    CurrentExamType,
                    RunningApplications); // Pass running applications

                var dialog = new DialogNhapMaBaiThiView(dialogViewModel);

                // Tìm window hiện tại làm Owner
                var currentWindow = Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w.IsActive);

                if (currentWindow != null)
                {
                    dialog.Owner = currentWindow;
                }

                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi khi mở dialog thi: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"ProceedToExam Error: {ex}");
            }
        }

        // Methods cho quản lý ứng dụng đang chạy
        private async Task LoadRunningApplicationsAsync()
        {
            try
            {
                // Only use this for full refresh - usually use RefreshApplicationsQuickly instead
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoadingApplications = true;
                });

                // Use optimized detection method
                var detectedApps = await Task.Run(DetectBlockedApplications);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    RunningApplications.Clear();
                    foreach (var app in detectedApps.Values.OrderBy(a => a.ApplicationName))
                    {
                        RunningApplications.Add(app);
                    }
                    
                    // Update cache
                    _applicationCache = new Dictionary<string, RunningApplication>(detectedApps);
                    _lastSuccessfulUpdate = DateTime.Now;
                });
            }
            catch (Exception ex)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    System.Diagnostics.Debug.WriteLine($"LoadRunningApplicationsAsync Error: {ex}");
                });
            }
            finally
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    IsLoadingApplications = false;
                });
            }
        }

        private async void CloseApplication(RunningApplication app)
        {
            if (app == null || !app.IsCloseable) return;

            try
            {
                // INSTANT UI UPDATE - Remove ngay lập tức
                var appKey = GetApplicationKey(app.ProcessName);
                var appToRemove = RunningApplications.FirstOrDefault(a => GetApplicationKey(a.ProcessName) == appKey);
                if (appToRemove != null)
                {
                    RunningApplications.Remove(appToRemove);
                    _applicationCache.Remove(appKey);
                    
                    // Update UI status
                    OnPropertyChanged(nameof(ApplicationsStatus));
                    OnPropertyChanged(nameof(HasApplications));
                    
                    System.Diagnostics.Debug.WriteLine($"✅ INSTANT REMOVE: {app.ApplicationName}");
                }

                // Close processes in background - không block UI
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CloseProcessesInstant(app.ProcessIds.ToList());
                        System.Diagnostics.Debug.WriteLine($"🔥 PROCESSES CLOSED: {app.ApplicationName}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error closing processes: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseApplication Error: {ex}");
            }
        }

        private async Task CloseProcessesInstant(List<int> processIds)
        {
            foreach (var processId in processIds)
            {
                try
                {
                    var process = Process.GetProcessById(processId);
                    if (process != null && !process.HasExited)
                    {
                        // Force kill ngay lập tức - không gentle close
                        process.Kill();
                        System.Diagnostics.Debug.WriteLine($"💀 KILLED: PID {processId}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Kill process {processId} failed: {ex.Message}");
                }
            }
            
            // Short delay để ensure processes are closed
            await Task.Delay(200);
        }

        private async Task CloseAllApplicationsAsync()
        {
            if (!RunningApplications.Any()) return;

            try
            {
                // INSTANT UI CLEAR - Xóa tất cả ngay lập tức
                var appsToClose = RunningApplications.Where(a => a.IsCloseable).ToList();
                var allProcessIds = new List<int>();
                
                foreach (var app in appsToClose)
                {
                    allProcessIds.AddRange(app.ProcessIds);
                }
                
                // Clear UI immediately
                RunningApplications.Clear();
                _applicationCache.Clear();
                
                // Update UI status
                OnPropertyChanged(nameof(ApplicationsStatus));
                OnPropertyChanged(nameof(HasApplications));
                
                System.Diagnostics.Debug.WriteLine($"✅ INSTANT CLEAR: {appsToClose.Count} apps removed from UI");

                // Close all processes in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await CloseProcessesInstant(allProcessIds);
                        System.Diagnostics.Debug.WriteLine($"🔥 ALL PROCESSES CLOSED: {allProcessIds.Count} processes");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error closing all processes: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseAllApplications Error: {ex}");
            }
        }

        private void CloseProcesses(List<int> processIds, Action<int> onProgressUpdate)
        {
            var maxRetries = 3; // Increased for Edge processes
            
            System.Diagnostics.Debug.WriteLine($"=== Starting CloseProcesses for {processIds.Count} processes ===");
            
            for (int retry = 0; retry < maxRetries; retry++)
            {
                var remainingProcesses = new List<int>();
                
                // Check which processes are still running
                foreach (var processId in processIds)
                {
                    try
                    {
                        var process = Process.GetProcessById(processId);
                        if (process != null && !process.HasExited)
                        {
                            System.Diagnostics.Debug.WriteLine($"Process {processId} ({process.ProcessName}): Still running, MainWindow={process.MainWindowTitle}");
                            remainingProcesses.Add(processId);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Process {processId} check failed: {ex.Message}");
                    }
                }
                
                if (remainingProcesses.Count == 0)
                {
                    onProgressUpdate(0);
                    System.Diagnostics.Debug.WriteLine("All processes closed successfully");
                    break;
                }
                
                System.Diagnostics.Debug.WriteLine($"Retry {retry + 1}: {remainingProcesses.Count} processes remaining");
                onProgressUpdate(remainingProcesses.Count);
                
                // Try to close remaining processes
                foreach (var processId in remainingProcesses)
                {
                    CloseIndividualProcess(processId, retry);
                }
                
                // Wait between retries
                if (retry < maxRetries - 1)
                {
                    Task.Delay(800).Wait(); // Longer wait for Edge
                }
            }
            
            System.Diagnostics.Debug.WriteLine("=== CloseProcesses completed ===");
        }

        private void CloseIndividualProcess(int processId, int retryAttempt)
        {
            try
            {
                var process = Process.GetProcessById(processId);
                if (process == null || process.HasExited)
                    return;

                var isEdge = process.ProcessName.ToLower().Contains("msedge");
                System.Diagnostics.Debug.WriteLine($"Closing {process.ProcessName} (PID: {processId}, Retry: {retryAttempt}, IsEdge: {isEdge})");

                if (retryAttempt == 0)
                {
                    // First try: gentle close
                    if (process.MainWindowHandle != IntPtr.Zero)
                    {
                        process.CloseMainWindow();
                        System.Diagnostics.Debug.WriteLine($"Sent CloseMainWindow to {process.ProcessName}");
                        Task.Delay(isEdge ? 1000 : 300).Wait(); // Longer wait for Edge
                    }
                }

                if (!process.HasExited && retryAttempt >= 1)
                {
                    // Force kill - since app now runs with admin privileges
                    process.Kill();
                    System.Diagnostics.Debug.WriteLine($"Force killed {process.ProcessName} (PID: {processId})");
                    Task.Delay(200).Wait();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CloseIndividualProcess {processId} failed: {ex.Message}");
            }
        }

        private bool CanCloseProcess(Process process)
        {
            try
            {
                // Skip system-critical processes
                if (IsSystemProtectedProcess(process))
                    return false;

                var processName = process.ProcessName.ToLower();
                
                // For blocked applications: only target processes with active windows
                if (_blockedApplications.Any(blocked => processName.Contains(blocked)))
                {
                    // Must have a visible main window to be considered "in use"
                    var hasActiveWindow = !string.IsNullOrEmpty(process.MainWindowTitle) && 
                                         process.MainWindowHandle != IntPtr.Zero;
                    
                    System.Diagnostics.Debug.WriteLine($"App {process.ProcessName} (PID: {process.Id}): " +
                        $"HasActiveWindow={hasActiveWindow}, Title='{process.MainWindowTitle}'");
                    
                    return hasActiveWindow;
                }

                return false; // Only care about blocked applications
            }
            catch
            {
                return false;
            }
        }

        private bool IsSystemProtectedProcess(Process process)
        {
            try
            {
                // Skip system processes that might cause issues
                var protectedNames = new[]
                {
                    "winlogon", "csrss", "wininit", "services", "lsass", "svchost", 
                    "dwm", "explorer", "System", "Registry", "smss", "dllhost"
                };
                
                return protectedNames.Any(name => 
                    process.ProcessName.ToLower().Contains(name.ToLower())) ||
                    process.SessionId != Process.GetCurrentProcess().SessionId ||
                    string.IsNullOrEmpty(process.MainWindowTitle);
            }
            catch
            {
                return true; // If we can't determine, assume it's protected
            }
        }

        private string GetFriendlyName(string processName)
        {
            var friendlyNames = new Dictionary<string, string>
            {
                // Browsers
                //{ "chrome", "Google Chrome" },
                { "firefox", "Mozilla Firefox" },
                { "msedge", "Microsoft Edge" },
                { "brave", "Brave Browser" },
                { "opera", "Opera Browser" },
                // Communication Apps
                { "zalo", "Zalo" },
                { "discord", "Discord" },
                { "telegram", "Telegram" },
                { "viber", "Viber" },
                { "whatsapp", "WhatsApp" },
                { "messenger", "Messenger" },
                { "teams", "Microsoft Teams" },
                { "zoom", "Zoom" },
                { "skype", "Skype" },
                // Remote Access Apps
                { "teamviewer", "TeamViewer" },
                { "ultraviewer", "UltraViewer" },
                { "deskin", "DeskIn" },
                { "anydesk", "AnyDesk" },
                { "remotedesktop", "Remote Desktop" },
                { "chrome remote", "Chrome Remote Desktop" },
                { "vnc", "VNC Viewer" },
                // Media & Entertainment
                { "spotify", "Spotify" },
                { "youtube", "YouTube" },
                { "vlc", "VLC Media Player" },
                { "itunes", "iTunes" },
                { "tiktok", "TikTok" },
                { "facebook", "Facebook" },
                // Games
                { "steam", "Steam" },
                { "origin", "Origin" },
                { "epicgames", "Epic Games" },
                { "battlenet", "Battle.net" },
                { "roblox", "Roblox" },
                { "minecraft", "Minecraft" },
                // Development Tools
                //{ "visualstudio", "Visual Studio" },
                //{ "vscode", "Visual Studio Code" },
                { "notepad++", "Notepad++" },
                { "sublime", "Sublime Text" },
                { "atom", "Atom" },
                { "intellij", "IntelliJ IDEA" },
                { "eclipse", "Eclipse" },
                // Other Tools
                { "obs", "OBS Studio" },
                { "photoshop", "Adobe Photoshop" },
                { "gimp", "GIMP" },
                { "utorrent", "uTorrent" },
                { "bittorrent", "BitTorrent" },
                { "winrar", "WinRAR" },
                { "7zip", "7-Zip" }
            };

            var lowerProcessName = processName.ToLower();
            var match = friendlyNames.FirstOrDefault(kv => lowerProcessName.Contains(kv.Key));
            return match.Key != null ? match.Value : processName;
        }

        private bool IsSystemCriticalProcess(string processName)
        {
            var criticalProcesses = new List<string>
            {
                "winlogon", "csrss", "wininit", "services", "lsass", "svchost", "dwm", "explorer"
            };
            
            return criticalProcesses.Any(critical => 
                processName.ToLower().Contains(critical.ToLower()));
        }

        private string GetApplicationKey(string processName)
        {
            var lowerProcessName = processName.ToLower();
            
            // Map process names to application keys
            // Browsers
            //if (lowerProcessName.Contains("chrome")) return "chrome";
            if (lowerProcessName.Contains("firefox")) return "firefox";
            if (lowerProcessName.Contains("msedge") || lowerProcessName.Contains("edge")) return "edge";
            if (lowerProcessName.Contains("brave")) return "brave";
            if (lowerProcessName.Contains("opera")) return "opera";
            
            // Communication Apps
            if (lowerProcessName.Contains("zalo")) return "zalo";
            if (lowerProcessName.Contains("discord")) return "discord";
            if (lowerProcessName.Contains("telegram")) return "telegram";
            if (lowerProcessName.Contains("viber")) return "viber";
            if (lowerProcessName.Contains("whatsapp")) return "whatsapp";
            if (lowerProcessName.Contains("messenger")) return "messenger";
            if (lowerProcessName.Contains("teams")) return "teams";
            if (lowerProcessName.Contains("zoom")) return "zoom";
            if (lowerProcessName.Contains("skype")) return "skype";
            
            // Remote Access Apps
            if (lowerProcessName.Contains("teamviewer")) return "teamviewer";
            if (lowerProcessName.Contains("ultraviewer")) return "ultraviewer";
            if (lowerProcessName.Contains("deskin")) return "deskin";
            if (lowerProcessName.Contains("anydesk")) return "anydesk";
            if (lowerProcessName.Contains("remotedesktop")) return "remotedesktop";
            if (lowerProcessName.Contains("chrome remote")) return "chromeremote";
            if (lowerProcessName.Contains("vnc")) return "vnc";
            
            // Media & Entertainment
            if (lowerProcessName.Contains("spotify")) return "spotify";
            if (lowerProcessName.Contains("youtube")) return "youtube";
            if (lowerProcessName.Contains("vlc")) return "vlc";
            if (lowerProcessName.Contains("itunes")) return "itunes";
            if (lowerProcessName.Contains("tiktok")) return "tiktok";
            if (lowerProcessName.Contains("facebook")) return "facebook";
            
            // Games
            if (lowerProcessName.Contains("steam")) return "steam";
            if (lowerProcessName.Contains("origin")) return "origin";
            if (lowerProcessName.Contains("epicgames")) return "epicgames";
            if (lowerProcessName.Contains("battlenet")) return "battlenet";
            if (lowerProcessName.Contains("roblox")) return "roblox";
            if (lowerProcessName.Contains("minecraft")) return "minecraft";
            
            // Development Tools
            //if (lowerProcessName.Contains("visualstudio")) return "visualstudio";
            //if (lowerProcessName.Contains("vscode")) return "vscode";
            if (lowerProcessName.Contains("notepad++")) return "notepadplus";
            if (lowerProcessName.Contains("sublime")) return "sublime";
            if (lowerProcessName.Contains("atom")) return "atom";
            if (lowerProcessName.Contains("intellij")) return "intellij";
            if (lowerProcessName.Contains("eclipse")) return "eclipse";
            
            // Other Tools
            if (lowerProcessName.Contains("obs")) return "obs";
            if (lowerProcessName.Contains("photoshop")) return "photoshop";
            if (lowerProcessName.Contains("gimp")) return "gimp";
            if (lowerProcessName.Contains("utorrent")) return "utorrent";
            if (lowerProcessName.Contains("bittorrent")) return "bittorrent";
            if (lowerProcessName.Contains("winrar")) return "winrar";
            if (lowerProcessName.Contains("7zip")) return "7zip";
            
            return processName; // Default to process name if no match
        }

        private string GetAppIcon(string processName)
        {
            var friendlyName = GetFriendlyName(processName);
            
            // Trả về chữ cái đầu của tên ứng dụng
            if (!string.IsNullOrEmpty(friendlyName))
            {
                return friendlyName.Substring(0, 1).ToUpper();
            }
            
            return "A"; // Default letter
        }
        #endregion
    }

}
