using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.Enum;
using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Xps;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class LamBaiThiViewModel : BaseViewModel
    {
        private readonly ILamBaiThiService _lamBaiThiService;
        private readonly INavigationService _navigationService;
        private DispatcherTimer _timer;
        private DispatcherTimer _autoSaveTimer;
        private int _totalSeconds;

        #region Properties
        // Exam Type
        private ExamType _examType;
        public ExamType ExamType
        {
            get => _examType;
            set => SetProperty(ref _examType, value);
        }

        // Student Info
        private string _studentFullName;
        public string StudentFullName
        {
            get => _studentFullName;
            set => SetProperty(ref _studentFullName, value);
        }

        private string _studentCode;
        public string StudentCode
        {
            get => _studentCode;
            set => SetProperty(ref _studentCode, value);
        }

        // Exam Info
        private string _examName;
        public string ExamName
        {
            get => _examName;
            set => SetProperty(ref _examName, value);
        }

        private string _subjectName;
        public string SubjectName
        {
            get => _subjectName;
            set => SetProperty(ref _subjectName, value);
        }

        private string _examCategoryName;
        public string ExamCategoryName
        {
            get => _examCategoryName;
            set => SetProperty(ref _examCategoryName, value);
        }

        private int _duration;
        public int Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        private string _timerText;
        public string TimerText
        {
            get => _timerText;
            set => SetProperty(ref _timerText, value);
        }

        // Progress
        private int _currentQuestionIndex;
        public int CurrentQuestionIndex
        {
            get => _currentQuestionIndex;
            set
            {
                if (SetProperty(ref _currentQuestionIndex, value))
                {
                    UpdateCurrentQuestion();
                    UpdateProgress();
                }
            }
        }

        private int _totalQuestions;
        public int TotalQuestions
        {
            get => _totalQuestions;
            set => SetProperty(ref _totalQuestions, value);
        }

        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        // Question Navigation
        private ObservableCollection<QuestionNumberItem> _questionNumbers;
        public ObservableCollection<QuestionNumberItem> QuestionNumbers
        {
            get => _questionNumbers;
            set => SetProperty(ref _questionNumbers, value);
        }

        // Current Question Number Item
        public QuestionNumberItem CurrentQuestionNumberItem
        {
            get
            {
                if (QuestionNumbers != null && CurrentQuestionIndex >= 0 && CurrentQuestionIndex < QuestionNumbers.Count)
                {
                    return QuestionNumbers[CurrentQuestionIndex];
                }
                return null;
            }
        }

        // Current Question (Multiple Choice)
        private QuestionViewModel _currentQuestion;
        public QuestionViewModel CurrentQuestion
        {
            get => _currentQuestion;
            set => SetProperty(ref _currentQuestion, value);
        }

        // Current Practice Question (Practice Exam)
        private PracticeQuestionViewModel _currentPracticeQuestion;
        public PracticeQuestionViewModel CurrentPracticeQuestion
        {
            get => _currentPracticeQuestion;
            set => SetProperty(ref _currentPracticeQuestion, value);
        }

        // IDs for API calls
        private Guid _multiExamHistoryId;
        private Guid _pracExamHistoryId;
        private int _examId;

        // All Questions
        private List<QuestionViewModel> _allQuestions;
        private List<PracticeQuestionViewModel> _allPracticeQuestions;

        // Loading state
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Marked questions
        private HashSet<int> _markedQuestions = new HashSet<int>();
        #endregion

        #region Commands
        public ICommand PreviousQuestionCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand GoToQuestionCommand { get; }
        public ICommand MarkQuestionCommand { get; }
        public ICommand SubmitExamCommand { get; }
        public ICommand SelectAnswerCommand { get; }
        public ICommand ToggleAnswerCommand { get; } // For multiple choice
        #endregion

        public LamBaiThiViewModel(ILamBaiThiService lamBaiThiService, INavigationService navigationService)
        {
            _lamBaiThiService = lamBaiThiService;
            _navigationService = navigationService;

            QuestionNumbers = new ObservableCollection<QuestionNumberItem>();
            _allQuestions = new List<QuestionViewModel>();
            _allPracticeQuestions = new List<PracticeQuestionViewModel>();

            // Initialize commands
            PreviousQuestionCommand = new RelayCommand(PreviousQuestion, () => CurrentQuestionIndex > 0);
            NextQuestionCommand = new RelayCommand(NextQuestion, () => CurrentQuestionIndex < TotalQuestions - 1);
            GoToQuestionCommand = new RelayCommand<int>(GoToQuestion);
            MarkQuestionCommand = new RelayCommand(MarkCurrentQuestion);
            SubmitExamCommand = new RelayCommand(async () => await SubmitExamAsync());
            SelectAnswerCommand = new RelayCommand<string>(SelectAnswer);
            ToggleAnswerCommand = new RelayCommand<string>(ToggleAnswer);
        }

        #region Initialize Methods
        public async Task InitializeExam(ExamType examType, object examData, int examId)
        {
            try
            {
                IsLoading = true;
                ExamType = examType;
                _examId = examId;

                if (examType == ExamType.MultipleChoice)
                {
                    await InitializeMultipleChoiceExam(examData as ExamInfoResponseDTO);
                }
                else
                {
                    await InitializePracticeExam(examData as PracticeExamInfoResponseDTO);
                }

                // Start timer
                StartTimer();
                StartAutoSave();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khởi tạo bài thi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task InitializeMultipleChoiceExam(ExamInfoResponseDTO examInfo)
        {
            if (examInfo == null) return;

            // Set exam info
            _multiExamHistoryId = examInfo.MultiExamHistoryId;
            StudentFullName = examInfo.StudentFullName;
            StudentCode = examInfo.StudentCode;
            SubjectName = examInfo.SubjectName;
            ExamCategoryName = examInfo.ExamCategoryName;
            Duration = examInfo.Duration;
            _totalSeconds = Duration * 60;

            // Get all questions for the exam
            var questionList = await _lamBaiThiService.GetAllQuestionMultiExamByMultiExamIdAsync(_examId);
            if (questionList == null || questionList.Count == 0) return;

            TotalQuestions = questionList.Count;

            // Load all questions and their answers
            for (int i = 0; i < questionList.Count; i++)
            {
                var questionSimple = questionList[i];
                var questionDetail = examInfo.Questions.FirstOrDefault(q => q.MultiQuestionId == questionSimple.Id);

                if (questionDetail != null)
                {
                    // Get answers for this question
                    var answers = await _lamBaiThiService.GetAllMultiAnswerOfQuestionAsync(questionDetail.MultiQuestionId);

                    var questionVm = new QuestionViewModel
                    {
                        QuestionId = questionDetail.MultiQuestionId,
                        QuestionOrder = questionSimple.QuestionOrder,
                        Content = questionDetail.Content,
                        ImageUrl = questionDetail.UrlImg,
                        Answers = answers?.Select(a => new AnswerViewModel
                        {
                            AnswerId = a.AnswerId,
                            Content = a.AnswerContent,
                            IsCorrect = a.IsCorrect,
                            IsSelected = false
                        }).ToList() ?? new List<AnswerViewModel>(),
                        IsMultipleChoice = answers?.Count(a => a.IsCorrect) > 1
                    };

                    _allQuestions.Add(questionVm);

                    // Add to navigation
                    QuestionNumbers.Add(new QuestionNumberItem
                    {
                        Number = i + 1,
                        IsAnswered = false,
                        IsMarked = false,
                        IsCurrent = i == 0
                    });
                }
            }

            // Show first question
            CurrentQuestionIndex = 0;
            UpdateCurrentQuestion(); // Explicitly call to ensure CurrentQuestion is set
        }

        private async Task InitializePracticeExam(PracticeExamInfoResponseDTO examInfo)
        {
            if (examInfo == null) return;

            // Set exam info
            _pracExamHistoryId = examInfo.PracExamHistoryId;
            StudentFullName = examInfo.StudentFullName;
            StudentCode = examInfo.StudentCode;
            SubjectName = examInfo.SubjectName;
            ExamCategoryName = examInfo.ExamCategoryName;
            Duration = examInfo.Duration;
            _totalSeconds = Duration * 60;

            // Get all questions
            var questionOrders = await _lamBaiThiService.GetQuestionAndAnswerByPracExamIdAsync(_examId);
            if (questionOrders == null || questionOrders.Count == 0) return;

            TotalQuestions = questionOrders.Count;

            // Load all practice questions
            foreach (var questionOrder in questionOrders.OrderBy(q => q.QuestionOrder))
            {
                var questionDetail = examInfo.Questions.FirstOrDefault(q => q.QuestionOrder == questionOrder.QuestionOrder);

                if (questionDetail != null)
                {
                    var practiceVm = new PracticeQuestionViewModel
                    {
                        PracticeQuestionId = questionOrder.PracticeQuestionId,
                        QuestionOrder = questionOrder.QuestionOrder,
                        Content = questionDetail.Content,
                        Score = questionDetail.Score,
                        // Answer content will be loaded if exists
                        RichTextAnswer = questionDetail.AnswerContent ?? "",
                        MathAnswer = "",
                        CodeAnswer = ""
                    };

                    _allPracticeQuestions.Add(practiceVm);

                    // Add to navigation
                    QuestionNumbers.Add(new QuestionNumberItem
                    {
                        Number = questionOrder.QuestionOrder,
                        IsAnswered = !string.IsNullOrEmpty(questionDetail.AnswerContent),
                        IsMarked = false,
                        IsCurrent = questionOrder.QuestionOrder == 1
                    });
                }
            }

            // Show first question
            CurrentQuestionIndex = 0;
            UpdateCurrentQuestion(); // Explicitly call to ensure CurrentQuestion is set
        }
        #endregion

        #region Navigation Methods
        private void UpdateCurrentQuestion()
        {
            if (ExamType == ExamType.MultipleChoice && _allQuestions.Count > CurrentQuestionIndex)
            {
                CurrentQuestion = _allQuestions[CurrentQuestionIndex];
            }
            else if (ExamType == ExamType.Practice && _allPracticeQuestions.Count > CurrentQuestionIndex)
            {
                CurrentPracticeQuestion = _allPracticeQuestions[CurrentQuestionIndex];
            }

            // Update navigation buttons
            foreach (var item in QuestionNumbers)
            {
                item.IsCurrent = item.Number == CurrentQuestionIndex + 1;
            }

            // Notify CurrentQuestionNumberItem changed
            OnPropertyChanged(nameof(CurrentQuestionNumberItem));
        }

        private void PreviousQuestion()
        {
            if (CurrentQuestionIndex > 0)
            {
                CurrentQuestionIndex--;
            }
        }

        private void NextQuestion()
        {
            if (CurrentQuestionIndex < TotalQuestions - 1)
            {
                CurrentQuestionIndex++;
            }
        }

        private void GoToQuestion(int questionNumber)
        {
            if (questionNumber > 0 && questionNumber <= TotalQuestions)
            {
                CurrentQuestionIndex = questionNumber - 1;
            }
        }

        private void MarkCurrentQuestion()
        {
            var currentNumber = CurrentQuestionIndex + 1;
            var questionItem = QuestionNumbers.FirstOrDefault(q => q.Number == currentNumber);

            if (questionItem != null)
            {
                questionItem.IsMarked = !questionItem.IsMarked;

                if (questionItem.IsMarked)
                    _markedQuestions.Add(currentNumber);
                else
                    _markedQuestions.Remove(currentNumber);
            }
        }
        #endregion

        #region Answer Methods
        private void SelectAnswer(string answerId)
        {
            System.Diagnostics.Debug.WriteLine($"SelectAnswer called with answerId: {answerId}");
            
            if (CurrentQuestion != null && !CurrentQuestion.IsMultipleChoice)
            {
                System.Diagnostics.Debug.WriteLine($"Processing single choice answer for question {CurrentQuestion.QuestionId}");
                
                // Single choice - clear other selections
                foreach (var answer in CurrentQuestion.Answers)
                {
                    bool wasSelected = answer.IsSelected;
                    answer.IsSelected = answer.AnswerId.ToString() == answerId;
                    System.Diagnostics.Debug.WriteLine($"Answer {answer.AnswerId}: {wasSelected} -> {answer.IsSelected}");
                }

                // Mark question as answered
                var questionItem = QuestionNumbers[CurrentQuestionIndex];
                questionItem.IsAnswered = true;
                System.Diagnostics.Debug.WriteLine($"Question {CurrentQuestionIndex + 1} marked as answered");

                // Auto save progress
                _ = SaveProgressAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CurrentQuestion is null or is multiple choice: {CurrentQuestion?.IsMultipleChoice}");
            }
        }

        private void ToggleAnswer(string answerId)
        {
            System.Diagnostics.Debug.WriteLine($"ToggleAnswer called with answerId: {answerId}");
            
            if (CurrentQuestion != null && CurrentQuestion.IsMultipleChoice)
            {
                System.Diagnostics.Debug.WriteLine($"Processing multiple choice answer for question {CurrentQuestion.QuestionId}");
                
                // Multiple choice - toggle selection
                var answer = CurrentQuestion.Answers.FirstOrDefault(a => a.AnswerId.ToString() == answerId);
                if (answer != null)
                {
                    bool wasSelected = answer.IsSelected;
                    answer.IsSelected = !answer.IsSelected;
                    System.Diagnostics.Debug.WriteLine($"Answer {answer.AnswerId}: {wasSelected} -> {answer.IsSelected}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Answer with ID {answerId} not found");
                }

                // Mark question as answered if at least one answer is selected
                var questionItem = QuestionNumbers[CurrentQuestionIndex];
                bool hasSelected = CurrentQuestion.Answers.Any(a => a.IsSelected);
                questionItem.IsAnswered = hasSelected;
                System.Diagnostics.Debug.WriteLine($"Question {CurrentQuestionIndex + 1} answered status: {hasSelected}");

                // Auto save progress
                _ = SaveProgressAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"CurrentQuestion is null or is not multiple choice: {CurrentQuestion?.IsMultipleChoice}");
            }
        }
        #endregion

        #region Timer Methods
        private void StartTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _totalSeconds--;

            if (_totalSeconds <= 0)
            {
                _timer.Stop();
                _ = SubmitExamAsync(true); // Auto submit when time's up
            }
            else
            {
                UpdateTimerDisplay();
            }
        }

        private void UpdateTimerDisplay()
        {
            var hours = _totalSeconds / 3600;
            var minutes = (_totalSeconds % 3600) / 60;
            var seconds = _totalSeconds % 60;

            TimerText = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
        }

        private void StartAutoSave()
        {
            _autoSaveTimer = new DispatcherTimer();
            _autoSaveTimer.Interval = TimeSpan.FromMinutes(5); // Auto save every 5 minutes
            _autoSaveTimer.Tick += async (s, e) => await SaveProgressAsync();
            _autoSaveTimer.Start();
        }
        #endregion

        #region Save & Submit Methods
        private async Task SaveProgressAsync()
        {
            try
            {
                if (ExamType == ExamType.MultipleChoice)
                {
                    var updateDto = new UpdateMultiExamProgressDTO
                    {
                        MultiExamHistoryId = _multiExamHistoryId,
                        Answers = new List<UpdateAnswerDTO>()
                    };

                    foreach (var question in _allQuestions.Where(q => q.Answers.Any(a => a.IsSelected)))
                    {
                        var selectedAnswers = question.Answers.Where(a => a.IsSelected).Select(a => a.AnswerId.ToString());
                        updateDto.Answers.Add(new UpdateAnswerDTO
                        {
                            QuestionId = question.QuestionId,
                            Answer = string.Join(",", selectedAnswers)
                        });
                    }

                    await _lamBaiThiService.UpdateProgressAsync(updateDto);
                }
                else
                {
                    var updateDto = new UpdatePracticeExamAnswersRequest
                    {
                        Answers = new List<UpdatePracticeExamAnswerDTO>()
                    };

                    foreach (var question in _allPracticeQuestions.Where(q => !string.IsNullOrEmpty(q.GetCombinedAnswer())))
                    {
                        updateDto.Answers.Add(new UpdatePracticeExamAnswerDTO
                        {
                            PracExamHistoryId = _pracExamHistoryId,
                            PracticeQuestionId = question.PracticeQuestionId,
                            Answer = question.GetCombinedAnswer()
                        });
                    }

                    await _lamBaiThiService.UpdatePEEach5minutesAsync(updateDto);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save progress error: {ex.Message}");
            }
        }

        private async Task SubmitExamAsync(bool isAutoSubmit = false)
        {
            try
            {
                _timer?.Stop();
                _autoSaveTimer?.Stop();

                if (!isAutoSubmit)
                {
                    // Tính toán thông tin để hiển thị trong dialog
                    var answeredCount = QuestionNumbers.Count(q => q.IsAnswered);
                    var totalCount = TotalQuestions;
                    var timeSpent = CalculateTimeSpent();

                    // Hiển thị dialog xác nhận
                    var confirmViewModel = new DialogXacNhanNopBaiThiViewModel(answeredCount, totalCount, timeSpent);
                    var confirmDialog = new DialogXacNhanNopBaiThiView(confirmViewModel)
                    {
                        Owner = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault()
                    };

                    confirmDialog.ShowDialog();

                    if (!confirmViewModel.IsConfirmed)
                    {
                        _timer?.Start();
                        _autoSaveTimer?.Start();
                        return;
                    }
                }

                IsLoading = true;

                if (ExamType == ExamType.MultipleChoice)
                {
                    await SubmitMultipleChoiceExam();
                }
                else
                {
                    await SubmitPracticeExam();
                }
            }
            catch (APIException apiEx)
            {
                // Xử lý lỗi từ API
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentWindow = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();

                    string errorTitle = "Nộp bài thất bại";
                    string errorMessage = apiEx.Message;
                    string errorDetail = "";

                    // Map error messages cho submit exam
                    switch (apiEx.Message)
                    {
                        case "Bài thi đã hết thời gian.":
                            errorMessage = "Bài thi đã hết thời gian";
                            errorDetail = "Thời gian làm bài đã kết thúc. Bài thi sẽ được nộp với các câu trả lời hiện tại.";
                            break;

                        case "Bài thi đã được nộp trước đó.":
                            errorMessage = "Bài thi đã được nộp";
                            errorDetail = "Bạn đã nộp bài thi này trước đó. Không thể nộp lại.";
                            break;

                        case "Không tìm thấy bài thi.":
                            errorMessage = "Không tìm thấy bài thi";
                            errorDetail = "Có lỗi xảy ra khi tìm kiếm bài thi. Vui lòng liên hệ giáo viên.";
                            break;

                        default:
                            errorDetail = "Vui lòng thử lại hoặc liên hệ với giáo viên để được hỗ trợ.";
                            break;
                    }

                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    lamBaiThiView?.Close();

                    // Hiển thị dialog lỗi với owner là DanhSachBaiThiView
                    var danhSachView = Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault();
                    DialogHelper.ShowErrorDialog(errorTitle, errorMessage, errorDetail, null, danhSachView);
                });

            }
            catch (Exception ex)
            {
                // Xử lý lỗi chung - thoát ra ngoài và báo lỗi
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Đóng màn hình làm bài
                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    lamBaiThiView?.Close();

                    // Hiển thị dialog lỗi
                    var danhSachView = Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault();
                    if (danhSachView == null)
                    {
                        // Nếu đã đóng, mở lại
                        danhSachView = App.AppHost.Services.GetRequiredService<DanhSachBaiThiView>();
                        danhSachView.Show();
                    }
                    DialogHelper.ShowErrorDialog(
                        "Lỗi nộp bài",
                        "Có lỗi xảy ra khi nộp bài",
                        ex.Message,
                        null,
                        danhSachView
                            );
                });
            }
            finally
            {
                IsLoading = false;
            }
        }
        // Thêm method helper để tính thời gian đã làm
        private string CalculateTimeSpent()
        {
            var totalSecondsSpent = (Duration * 60) - _totalSeconds;
            var minutes = totalSecondsSpent / 60;
            var seconds = totalSecondsSpent % 60;

            if (minutes > 0)
            {
                return $"{minutes} phút {seconds} giây";
            }
            else
            {
                return $"{seconds} giây";
            }
        }
        private async Task SubmitMultipleChoiceExam()
        {
            var submitDto = new UpdateMultiExamProgressDTO
            {
                MultiExamHistoryId = _multiExamHistoryId,
                Answers = new List<UpdateAnswerDTO>()
            };

            foreach (var question in _allQuestions)
            {
                var selectedAnswers = question.Answers.Where(a => a.IsSelected).Select(a => a.AnswerId.ToString());
                submitDto.Answers.Add(new UpdateAnswerDTO
                {
                    QuestionId = question.QuestionId,
                    Answer = string.Join(",", selectedAnswers)
                });
            }

            var result = await _lamBaiThiService.SubmitExamAsync(submitDto);

            if (result != null)
            {
                ShowExamResult(result);
            }
        }

        private async Task SubmitPracticeExam()
        {
            var submitDto = new SubmitPracticeExamRequest
            {
                PracExamHistoryId = _pracExamHistoryId,
                Answers = _allPracticeQuestions.Select(q => new SubmitPracticeExamAnswerDTO
                {
                    PracticeQuestionId = q.PracticeQuestionId,
                    Answer = q.GetCombinedAnswer()
                }).ToList()
            };

            var result = await _lamBaiThiService.SubmitPracticeExamAsync(submitDto);

            if (result != null)
            {
                ShowPracticeExamResult(result);
            }
        }

        private void ShowExamResult(SubmitExamResponseDTO result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Đóng màn hình làm bài
                var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                lamBaiThiView?.Close();

                // Mở màn hình kết quả
                var ketQuaViewModel = App.AppHost.Services.GetRequiredService<KetQuaNopBaiViewModel>();
                ketQuaViewModel.InitializeFromSubmitResult(result);

                var ketQuaView = new KetQuaNopBaiView(ketQuaViewModel);
                ketQuaView.Show();
            });
        }

        private void ShowPracticeExamResult(SubmitPracticeExamResponseDTO result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Đóng màn hình làm bài
                var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                lamBaiThiView?.Close();

                // Mở màn hình kết quả
                var ketQuaViewModel = App.AppHost.Services.GetRequiredService<KetQuaNopBaiViewModel>();
                ketQuaViewModel.InitializeFromPracticeResult(result);

                var ketQuaView = new KetQuaNopBaiView(ketQuaViewModel);
                ketQuaView.Show();
            });
        }
        #endregion

        #region Helper Methods
        private void UpdateProgress()
        {
            var answeredCount = QuestionNumbers.Count(q => q.IsAnswered);
            ProgressText = $"{answeredCount}/{TotalQuestions} câu";
            ProgressValue = (double)answeredCount / TotalQuestions * 100;
        }
        #endregion
    }

    #region ViewModels for Questions
    public class QuestionViewModel : BaseViewModel
    {
        public int QuestionId { get; set; }
        public int QuestionOrder { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public bool IsMultipleChoice { get; set; }
    }

    public class AnswerViewModel : BaseViewModel
    {
        private bool _isSelected;
        public int AnswerId { get; set; }
        public string Content { get; set; }
        public bool IsCorrect { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }

    public class PracticeQuestionViewModel : BaseViewModel
    {
        public int PracticeQuestionId { get; set; }
        public int QuestionOrder { get; set; }
        public string Content { get; set; }
        public double Score { get; set; }

        private string _richTextAnswer;
        public string RichTextAnswer
        {
            get => _richTextAnswer;
            set => SetProperty(ref _richTextAnswer, value);
        }

        private string _mathAnswer;
        public string MathAnswer
        {
            get => _mathAnswer;
            set => SetProperty(ref _mathAnswer, value);
        }

        private string _codeAnswer;
        public string CodeAnswer
        {
            get => _codeAnswer;
            set => SetProperty(ref _codeAnswer, value);
        }

        private int _selectedTabIndex;
        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public string GetCombinedAnswer()
        {
            // Combine answers from all tabs or return the active one
            return SelectedTabIndex switch
            {
                0 => RichTextAnswer,
                1 => $"[MATH]{MathAnswer}[/MATH]",
                2 => $"[CODE]{CodeAnswer}[/CODE]",
                _ => RichTextAnswer
            };
        }
    }

    public class QuestionNumberItem : BaseViewModel
    {
        private int _number;
        public int Number
        {
            get => _number;
            set => SetProperty(ref _number, value);
        }

        private bool _isAnswered;
        public bool IsAnswered
        {
            get => _isAnswered;
            set => SetProperty(ref _isAnswered, value);
        }

        private bool _isMarked;
        public bool IsMarked
        {
            get => _isMarked;
            set => SetProperty(ref _isMarked, value);
        }

        private bool _isCurrent;
        public bool IsCurrent
        {
            get => _isCurrent;
            set => SetProperty(ref _isCurrent, value);
        }
    }
    #endregion
}