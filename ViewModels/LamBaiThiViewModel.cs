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
            set
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Setting ExamType to: {value}");
                SetProperty(ref _examType, value);
            }
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
            set
            {
                if (SetProperty(ref _subjectName, value))
                {
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        private string _examCategoryName;
        public string ExamCategoryName
        {
            get => _examCategoryName;
            set
            {
                if (SetProperty(ref _examCategoryName, value))
                {
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
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
                    if (ExamType == ExamType.MultipleChoice)
                    {
                        UpdateCurrentMultipleChoiceQuestion();
                        UpdateMultipleChoiceProgress();
                    }
                    else if (ExamType == ExamType.Practice)
                    {
                        UpdateCurrentPracticeQuestion();
                        UpdatePracticeProgress();
                    }
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
            set
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Setting CurrentPracticeQuestion: {value?.Content?.Substring(0, Math.Min(50, value?.Content?.Length ?? 0))}...");
                SetProperty(ref _currentPracticeQuestion, value);
            }
        }

        // IDs for API calls
        private Guid _multiExamHistoryId;
        private Guid _pracExamHistoryId;
        private int _examId;

        // All Questions
        private List<QuestionViewModel> _allQuestions;
        private List<PracticeQuestionViewModel> _allPracticeQuestions;
        public List<PracticeQuestionViewModel> AllPracticeQuestions => _allPracticeQuestions;

        // Loading state
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        // Window Title
        public string WindowTitle => $"Bài thi {SubjectName} - {ExamCategoryName}";

        // Marked questions
        private HashSet<int> _markedQuestions = new HashSet<int>();

        // Flag to prevent infinite loops in PropertyChanged events
        private bool _isUpdatingSelections = false;
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
            System.Diagnostics.Debug.WriteLine($"[DEBUG] LamBaiThiViewModel Constructor: Creating new instance {this.GetHashCode()}");
            
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
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] LamBaiThiViewModel Constructor: Completed for instance {this.GetHashCode()}");
        }

        #region Initialize Methods
        public async Task InitializeExam(ExamType examType, object examData, int examId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] InitializeExam: Starting with ExamType={examType}, ExamId={examId}");

                IsLoading = true;
                ExamType = examType;
                _examId = examId;

                System.Diagnostics.Debug.WriteLine($"[DEBUG] InitializeExam: ExamType set to {ExamType}");

                if (examType == ExamType.MultipleChoice)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] InitializeExam: Calling InitializeMultipleChoiceExam");
                    await InitializeMultipleChoiceExam(examData as ExamInfoResponseDTO);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] InitializeExam: Calling InitializePracticeExam");
                    await InitializePracticeExam(examData as PracticeExamInfoResponseDTO);
                }

                System.Diagnostics.Debug.WriteLine("[DEBUG] InitializeExam: Starting timers");
                // Start timer
                StartTimer();
                StartAutoSave();

                System.Diagnostics.Debug.WriteLine("[DEBUG] InitializeExam: Completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] InitializeExam failed: {ex.Message}");
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
            
            // QUAN TRỌNG: Tính thời gian còn lại dựa trên StartTime từ server
            if (examInfo.StartTime.HasValue)
            {
                var timeElapsed = DateTime.Now - examInfo.StartTime.Value;
                var timeRemaining = Duration - timeElapsed.TotalMinutes;
                _totalSeconds = Math.Max(0, (int)(timeRemaining * 60)); // Đảm bảo không âm
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Continue exam - Time elapsed: {timeElapsed.TotalMinutes:F1} minutes, Time remaining: {timeRemaining:F1} minutes");
            }
            else
            {
                _totalSeconds = Duration * 60; // Lần đầu làm bài
                System.Diagnostics.Debug.WriteLine($"[DEBUG] New exam - Full duration: {Duration} minutes");
            }

            // Get all questions for the exam
            var questionList = await _lamBaiThiService.GetAllQuestionMultiExamByMultiExamIdAsync(_examId);
            if (questionList == null || questionList.Count == 0) return;

            TotalQuestions = questionList.Count;

            // Create a list to hold all questions with their details
            var questionsWithDetails = new List<QuestionViewModel>();

            // Load all questions and their answers
            for (int i = 0; i < questionList.Count; i++)
            {
                var questionSimple = questionList[i];
                var questionDetail = examInfo.Questions.FirstOrDefault(q => q.MultiQuestionId == questionSimple.Id);

                if (questionDetail != null)
                {
                    // Get answers for this question
                    var answers = await _lamBaiThiService.GetAllMultiAnswerOfQuestionAsync(questionDetail.MultiQuestionId);

                    var correctAnswersCount = answers?.Count(a => a.IsCorrect) ?? 0;
                    var isMultipleChoice = correctAnswersCount > 1;

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Question {i + 1} (ID: {questionDetail.MultiQuestionId}): {correctAnswersCount} correct answers → IsMultipleChoice = {isMultipleChoice}");

                    var questionVm = new QuestionViewModel
                    {
                        QuestionId = questionDetail.MultiQuestionId,
                        QuestionOrder = i + 1,
                        Content = questionDetail.Content,
                        ImageUrl = questionDetail.UrlImg,
                        Answers = answers?.Select(a => {
                            var answerVm = new AnswerViewModel
                            {
                                AnswerId = a.AnswerId,
                                Content = a.AnswerContent,
                                IsCorrect = a.IsCorrect,
                                IsSelected = false
                            };

                            // Subscribe to PropertyChanged to detect selection changes
                            answerVm.PropertyChanged += (s, e) => {
                                if (e.PropertyName == nameof(AnswerViewModel.IsSelected) && !_isUpdatingSelections)
                                {
                                    // Use Dispatcher to ensure this runs on UI thread and avoid race conditions
                                    Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                                        // Find which question this answer belongs to
                                        var questionIndex = _allQuestions.FindIndex(q => q.Answers.Contains(answerVm));
                                        if (questionIndex >= 0)
                                        {
                                            var question = _allQuestions[questionIndex];

                                            System.Diagnostics.Debug.WriteLine($"[DEBUG] PropertyChanged: Answer {answerVm.AnswerId} IsSelected = {answerVm.IsSelected}");
                                            System.Diagnostics.Debug.WriteLine($"  - Question {questionIndex + 1} IsMultipleChoice: {question.IsMultipleChoice}");

                                            // CRITICAL: Only clear other selections for single choice questions when selecting
                                            if (!question.IsMultipleChoice && answerVm.IsSelected && !_isUpdatingSelections)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"  - Clearing other selections for SINGLE choice question {questionIndex + 1}");
                                                _isUpdatingSelections = true;
                                                try
                                                {
                                                    foreach (var otherAnswer in question.Answers)
                                                    {
                                                        if (otherAnswer.AnswerId != answerVm.AnswerId && otherAnswer.IsSelected)
                                                        {
                                                            System.Diagnostics.Debug.WriteLine($"    - Clearing answer {otherAnswer.AnswerId}");
                                                            otherAnswer.IsSelected = false;
                                                        }
                                                    }
                                                }
                                                finally
                                                {
                                                    _isUpdatingSelections = false;
                                                }
                                            }
                                            else if (question.IsMultipleChoice)
                                            {
                                                System.Diagnostics.Debug.WriteLine($"  - MULTIPLE choice question {questionIndex + 1}: allowing multiple selections");
                                            }

                                            // Update question status
                                            if (questionIndex < QuestionNumbers.Count)
                                            {
                                                var hasAnswers = _allQuestions[questionIndex].Answers.Any(ans => ans.IsSelected);
                                                QuestionNumbers[questionIndex].IsAnswered = hasAnswers;
                                            }

                                            UpdateMultipleChoiceProgress();
                                        }
                                    }));
                                }
                            };

                            return answerVm;
                        }).ToList() ?? new List<AnswerViewModel>(),
                        IsMultipleChoice = isMultipleChoice
                    };

                    questionsWithDetails.Add(questionVm);
                }
            }

            // Xáo trộn câu hỏi
            var random = new Random();
            questionsWithDetails = questionsWithDetails.OrderBy(q => random.Next()).ToList();

            // Re-assign QuestionOrder after shuffling
            for (int i = 0; i < questionsWithDetails.Count; i++)
            {
                questionsWithDetails[i].QuestionOrder = i + 1;
            }

            // Clear and add to _allQuestions
            _allQuestions.Clear();
            _allQuestions.AddRange(questionsWithDetails);

            // Clear and rebuild navigation
            QuestionNumbers.Clear();
            for (int i = 0; i < _allQuestions.Count; i++)
            {
                QuestionNumbers.Add(new QuestionNumberItem
                {
                    Number = i + 1,
                    IsAnswered = false,
                    IsMarked = false,
                    IsCurrent = i == 0
                });
            }

            // QUAN TRỌNG: Load các đáp án đã lưu từ server (TH3 - tiếp tục thi)
            if (examInfo.SavedAnswers != null && examInfo.SavedAnswers.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Loading {examInfo.SavedAnswers.Count} saved answers...");
                LoadSavedMultipleChoiceAnswers(examInfo.SavedAnswers);
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Saved answers loaded successfully");
            }

            // Show first question
            CurrentQuestionIndex = 0;
            UpdateCurrentMultipleChoiceQuestion();
            UpdateMultipleChoiceProgress();
        }

        private async Task InitializePracticeExam(PracticeExamInfoResponseDTO examInfo)
        {
            if (examInfo == null)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] InitializePracticeExam: examInfo is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] InitializePracticeExam: Starting...");

            // Set exam info
            _pracExamHistoryId = examInfo.PracExamHistoryId;
            StudentFullName = examInfo.StudentFullName;
            StudentCode = examInfo.StudentCode;
            SubjectName = examInfo.SubjectName;
            ExamCategoryName = examInfo.ExamCategoryName;
            Duration = examInfo.Duration;
            
            // QUAN TRỌNG: Tính thời gian còn lại dựa trên StartTime từ server
            if (examInfo.StartTime.HasValue)
            {
                var timeElapsed = DateTime.Now - examInfo.StartTime.Value;
                var timeRemaining = Duration - timeElapsed.TotalMinutes;
                _totalSeconds = Math.Max(0, (int)(timeRemaining * 60)); // Đảm bảo không âm
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Continue practice exam - Time elapsed: {timeElapsed.TotalMinutes:F1} minutes, Time remaining: {timeRemaining:F1} minutes");
            }
            else
            {
                _totalSeconds = Duration * 60; // Lần đầu làm bài
                System.Diagnostics.Debug.WriteLine($"[DEBUG] New practice exam - Full duration: {Duration} minutes");
            }

            // Get all questions
            var questionOrders = await _lamBaiThiService.GetQuestionAndAnswerByPracExamIdAsync(_examId);
            if (questionOrders == null || questionOrders.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[ERROR] InitializePracticeExam: No question orders found");

                // Fallback: Load directly from examInfo if questionOrders is empty
                if (examInfo.Questions != null && examInfo.Questions.Count > 0)
                {
                    LoadPracticeQuestionsFromExamInfo(examInfo);
                    return;
                }
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {questionOrders.Count} question orders");

            TotalQuestions = questionOrders.Count;

            // Clear existing data
            _allPracticeQuestions.Clear();
            QuestionNumbers.Clear();

            // Load all practice questions với PropertyChanged binding
            foreach (var questionOrder in questionOrders.OrderBy(q => q.QuestionOrder))
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Processing question order {questionOrder.QuestionOrder}");

                // Handle API ordering inconsistency - try both 0-based and 1-based matching
                var questionDetail = examInfo.Questions.FirstOrDefault(q => q.QuestionOrder == questionOrder.QuestionOrder) ??
                                   examInfo.Questions.FirstOrDefault(q => q.QuestionOrder == questionOrder.QuestionOrder - 1);

                if (questionDetail != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Found question detail: {questionDetail.Content?.Substring(0, Math.Min(50, questionDetail.Content?.Length ?? 0))}...");

                    var practiceVm = new PracticeQuestionViewModel
                    {
                        PracticeQuestionId = questionOrder.PracticeQuestionId,
                        QuestionOrder = questionOrder.QuestionOrder,
                        Content = questionDetail.Content,
                        Score = questionDetail.Score,
                        TotalQuestions = TotalQuestions
                    };

                    // QUAN TRỌNG: Set answer và setup binding
                    // Nếu có sẵn answer content từ server thì set (TH3 - tiếp tục thi), nếu không thì để trống (TH1, TH2)
                    if (!string.IsNullOrWhiteSpace(questionDetail.AnswerContent))
                    {
                        // DECODE line breaks nếu có (từ format ___NEWLINE___ về \n)
                        var decodedAnswer = questionDetail.AnswerContent.Replace("___NEWLINE___", "\n");
                        practiceVm.SetAnswerSilently(decodedAnswer);
                        
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] TH3 - Loaded saved answer for Question {practiceVm.PracticeQuestionId}: '{decodedAnswer.Substring(0, Math.Min(50, decodedAnswer.Length))}...'");
                    }
                    else
                    {
                        practiceVm.ClearAnswer(); // Đảm bảo bắt đầu với answer trống
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] TH1/TH2 - Question {practiceVm.PracticeQuestionId} starts with empty answer");
                    }

                    // QUAN TRỌNG: Subscribe to PropertyChanged để detect answer changes
                    SetupPracticeQuestionBinding(practiceVm);

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Created PracticeVM: ID={practiceVm.PracticeQuestionId}, Content='{practiceVm.Content}', Score={practiceVm.Score}, HasAnswer={practiceVm.HasAnswer}");

                    _allPracticeQuestions.Add(practiceVm);

                    // Add to navigation - Use 1-based numbering for UI display
                    QuestionNumbers.Add(new QuestionNumberItem
                    {
                        Number = _allPracticeQuestions.Count, // Use sequential numbering for display
                        IsAnswered = practiceVm.HasAnswer,
                        IsMarked = false,
                        IsCurrent = _allPracticeQuestions.Count == 1 // First question should be current
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] No question detail found for order {questionOrder.QuestionOrder}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] InitializePracticeExam: Loaded {_allPracticeQuestions.Count} practice questions");

            // Show first question
            CurrentQuestionIndex = 0;
            UpdateCurrentPracticeQuestion();

            // Force UI refresh
            OnPropertyChanged(nameof(ExamType));
            OnPropertyChanged(nameof(CurrentPracticeQuestion));
            OnPropertyChanged(nameof(TotalQuestions));
            OnPropertyChanged(nameof(QuestionNumbers));
        }
        private void SetupPracticeQuestionBinding(PracticeQuestionViewModel practiceVm)
        {
            practiceVm.PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(PracticeQuestionViewModel.Answer) ||
                    e.PropertyName == nameof(PracticeQuestionViewModel.HasAnswer))
                {
                    // Tìm index của question này
                    var questionIndex = _allPracticeQuestions.FindIndex(q => q.PracticeQuestionId == practiceVm.PracticeQuestionId);
                    if (questionIndex >= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DEBUG] Practice Question {questionIndex + 1} Answer changed: HasAnswer={practiceVm.HasAnswer}");

                        // Update QuestionNumbers
                        if (questionIndex < QuestionNumbers.Count)
                        {
                            // QUAN TRỌNG: Cập nhật IsAnswered dựa trên HasAnswer
                            QuestionNumbers[questionIndex].IsAnswered = practiceVm.HasAnswer;
                            System.Diagnostics.Debug.WriteLine($"[DEBUG] Updated QuestionNumbers[{questionIndex}].IsAnswered = {practiceVm.HasAnswer}");
                        }

                        // Update progress
                        UpdatePracticeProgress();
                    }
                }
            };
        }
        // Thêm method để force update trạng thái cho practice question hiện tại
        private void ForceUpdateCurrentPracticeQuestionStatus()
        {
            if (ExamType == ExamType.Practice && CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _allPracticeQuestions.Count)
            {
                var currentQuestion = _allPracticeQuestions[CurrentQuestionIndex];
                var questionItem = QuestionNumbers[CurrentQuestionIndex];

                // Force update trạng thái
                questionItem.IsAnswered = currentQuestion.HasAnswer;

                System.Diagnostics.Debug.WriteLine($"[DEBUG] ForceUpdateCurrentPracticeQuestionStatus: Question {CurrentQuestionIndex + 1}, IsAnswered: {questionItem.IsAnswered}");

                // Update progress
                UpdatePracticeProgress();
            }
        }
        private void LoadPracticeQuestionsFromExamInfo(PracticeExamInfoResponseDTO examInfo)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] LoadPracticeQuestionsFromExamInfo: Starting fallback loading...");

            TotalQuestions = examInfo.Questions.Count;

            // Clear existing data
            _allPracticeQuestions.Clear();
            QuestionNumbers.Clear();

            // Load questions directly from examInfo
            for (int i = 0; i < examInfo.Questions.Count; i++)
            {
                var questionDetail = examInfo.Questions[i];
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Loading question {i}: {questionDetail.Content?.Substring(0, Math.Min(50, questionDetail.Content?.Length ?? 0))}...");

                var practiceVm = new PracticeQuestionViewModel
                {
                    PracticeQuestionId = i + 1, // Use index as ID if not available
                    QuestionOrder = questionDetail.QuestionOrder,
                    Content = questionDetail.Content,
                    Score = questionDetail.Score,
                    TotalQuestions = TotalQuestions
                };

                // QUAN TRỌNG: Load answer đã lưu (TH3) với decode line breaks
                if (!string.IsNullOrWhiteSpace(questionDetail.AnswerContent))
                {
                    var decodedAnswer = questionDetail.AnswerContent.Replace("___NEWLINE___", "\n");
                    practiceVm.SetAnswerSilently(decodedAnswer);
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Fallback - Loaded saved answer for Question {i + 1}: '{decodedAnswer.Substring(0, Math.Min(50, decodedAnswer.Length))}...'");
                }
                else
                {
                    practiceVm.ClearAnswer();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Fallback - Question {i + 1} starts with empty answer");
                }

                // Setup binding cho từng question
                SetupPracticeQuestionBinding(practiceVm);

                _allPracticeQuestions.Add(practiceVm);

                // Add to navigation - Convert 0-based to 1-based for UI display
                QuestionNumbers.Add(new QuestionNumberItem
                {
                    Number = i + 1, // Use sequential numbering for display
                    IsAnswered = practiceVm.HasAnswer,
                    IsMarked = false,
                    IsCurrent = i == 0 // First question should be current
                });
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadPracticeQuestionsFromExamInfo: Loaded {_allPracticeQuestions.Count} questions");

            // Show first question
            CurrentQuestionIndex = 0;
            UpdateCurrentPracticeQuestion();

            // Force UI refresh
            OnPropertyChanged(nameof(ExamType));
            OnPropertyChanged(nameof(CurrentPracticeQuestion));
            OnPropertyChanged(nameof(TotalQuestions));
            OnPropertyChanged(nameof(QuestionNumbers));
        }

        // Helper method: Load đáp án đã lưu cho Multiple Choice (TH3)
        private void LoadSavedMultipleChoiceAnswers(List<SavedAnswerDTO> savedAnswers)
        {
            foreach (var savedAnswer in savedAnswers)
            {
                // Tìm question tương ứng
                var question = _allQuestions.FirstOrDefault(q => q.QuestionId == savedAnswer.QuestionId);
                if (question == null) continue;

                // Parse các answer IDs đã chọn
                if (!string.IsNullOrEmpty(savedAnswer.Answer))
                {
                    var selectedAnswerIds = savedAnswer.Answer.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(id => int.TryParse(id.Trim(), out int answerId) ? answerId : 0)
                        .Where(id => id > 0)
                        .ToList();

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Question {savedAnswer.QuestionId}: Loading saved answer IDs [{string.Join(", ", selectedAnswerIds)}]");

                    // Set IsSelected cho các đáp án đã chọn
                    _isUpdatingSelections = true;
                    try
                    {
                        foreach (var answer in question.Answers)
                        {
                            answer.IsSelected = selectedAnswerIds.Contains(answer.AnswerId);
                        }
                    }
                    finally
                    {
                        _isUpdatingSelections = false;
                    }
                }
            }

            // Update progress sau khi load xong tất cả đáp án
            UpdateMultipleChoiceProgress();
            
            // Update question navigation status
            for (int i = 0; i < _allQuestions.Count && i < QuestionNumbers.Count; i++)
            {
                var hasAnswers = _allQuestions[i].Answers.Any(a => a.IsSelected);
                QuestionNumbers[i].IsAnswered = hasAnswers;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] LoadSavedMultipleChoiceAnswers completed - Updated progress and navigation");
        }
        #endregion

        #region Navigation Methods - Separated for Multiple Choice and Practice
        private void UpdateCurrentMultipleChoiceQuestion()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _allQuestions.Count)
                {
                    // CRITICAL: Always use direct reference to ensure state consistency
                    var questionReference = _allQuestions[CurrentQuestionIndex];
                    CurrentQuestion = questionReference;

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateCurrentMultipleChoiceQuestion:");
                    System.Diagnostics.Debug.WriteLine($"  - Index: {CurrentQuestionIndex}");
                    System.Diagnostics.Debug.WriteLine($"  - Question ID: {CurrentQuestion?.QuestionId}");

                    // Update question status
                    if (CurrentQuestionIndex < QuestionNumbers.Count)
                    {
                        var questionItem = QuestionNumbers[CurrentQuestionIndex];
                        bool hasAnswered = CurrentQuestion.Answers.Any(a => a.IsSelected);
                        questionItem.IsAnswered = hasAnswered;
                    }

                    OnPropertyChanged(nameof(CurrentQuestion));
                }

                // Update navigation buttons
                foreach (var item in QuestionNumbers)
                {
                    item.IsCurrent = item.Number == CurrentQuestionIndex + 1;
                }

                OnPropertyChanged(nameof(CurrentQuestionNumberItem));
                CommandManager.InvalidateRequerySuggested();
            });
        }

        private void UpdateCurrentPracticeQuestion()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _allPracticeQuestions.Count)
                {
                    // CRITICAL: Always use direct reference for practice questions
                    var practiceReference = _allPracticeQuestions[CurrentQuestionIndex];
                    CurrentPracticeQuestion = practiceReference;

                    // Cập nhật IsCurrent cho từng câu hỏi
                    for (int i = 0; i < _allPracticeQuestions.Count; i++)
                        _allPracticeQuestions[i].IsCurrent = (i == CurrentQuestionIndex);

                    System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdateCurrentPracticeQuestion:");
                    System.Diagnostics.Debug.WriteLine($"  - Index: {CurrentQuestionIndex}");
                    System.Diagnostics.Debug.WriteLine($"  - Question ID: {CurrentPracticeQuestion?.PracticeQuestionId}");
                    System.Diagnostics.Debug.WriteLine($"  - Question Content: {CurrentPracticeQuestion?.Content}");
                    System.Diagnostics.Debug.WriteLine($"  - Current Answer: '{CurrentPracticeQuestion?.Answer}'");
                    System.Diagnostics.Debug.WriteLine($"  - HasAnswer: {CurrentPracticeQuestion?.HasAnswer}");

                    // Update question status for practice questions
                    UpdatePracticeQuestionStatus();

                    OnPropertyChanged(nameof(CurrentPracticeQuestion));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] UpdateCurrentPracticeQuestion - Invalid state:");
                    System.Diagnostics.Debug.WriteLine($"  - CurrentQuestionIndex: {CurrentQuestionIndex}");
                    System.Diagnostics.Debug.WriteLine($"  - _allPracticeQuestions.Count: {_allPracticeQuestions.Count}");
                }

                // Update navigation buttons
                UpdateNavigationButtons();
                OnPropertyChanged(nameof(CurrentQuestionNumberItem));
                CommandManager.InvalidateRequerySuggested();
            });
        }
        private void UpdatePracticeQuestionStatus()
        {
            if (CurrentQuestionIndex >= 0 && CurrentQuestionIndex < QuestionNumbers.Count && CurrentPracticeQuestion != null)
            {
                var questionItem = QuestionNumbers[CurrentQuestionIndex];
                bool hasAnswered = CurrentPracticeQuestion.HasAnswer;

                System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdatePracticeQuestionStatus: Question {CurrentQuestionIndex + 1}, HasAnswered: {hasAnswered}");

                questionItem.IsAnswered = hasAnswered;

                // Update progress sau khi update trạng thái
                UpdatePracticeProgress();
            }
        }

        private void UpdateNavigationButtons()
        {
            // Update navigation buttons cho tất cả questions
            foreach (var item in QuestionNumbers)
            {
                item.IsCurrent = item.Number == CurrentQuestionIndex + 1;
            }
        }
        private void PreviousQuestion()
        {
            SaveCurrentQuestionState();
            if (CurrentQuestionIndex > 0)
            {
                CurrentQuestionIndex--;

                if (ExamType == ExamType.Practice)
                {
                    // Đảm bảo CurrentPracticeQuestion được update
                    UpdateCurrentPracticeQuestion();
                }
                else
                {
                    UpdateCurrentMultipleChoiceQuestion();
                    UpdateMultipleChoiceProgress();
                }
            }
        }

        private void NextQuestion()
        {
            SaveCurrentQuestionState();
            if (CurrentQuestionIndex < TotalQuestions - 1)
            {
                CurrentQuestionIndex++;

                if (ExamType == ExamType.Practice)
                {
                    // Đảm bảo CurrentPracticeQuestion được update
                    UpdateCurrentPracticeQuestion();
                }
                else
                {
                    UpdateCurrentMultipleChoiceQuestion();
                    UpdateMultipleChoiceProgress();
                }
            }
        }

        private void GoToQuestion(int questionNumber)
        {
            SaveCurrentQuestionState();
            if (questionNumber > 0 && questionNumber <= TotalQuestions)
            {
                CurrentQuestionIndex = questionNumber - 1;

                if (ExamType == ExamType.Practice)
                {
                    // Đảm bảo CurrentPracticeQuestion được update
                    UpdateCurrentPracticeQuestion();
                }
                else
                {
                    UpdateCurrentMultipleChoiceQuestion();
                    UpdateMultipleChoiceProgress();
                }
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

        #region Answer Selection Methods
        private void SelectAnswer(string answerId)
        {
            // This method is now deprecated since we use PropertyChanged events
            // All logic is handled in PropertyChanged handlers of AnswerViewModel
        }

        private void ToggleAnswer(string answerId)
        {
            // This method is now deprecated since we use PropertyChanged events  
            // All logic is handled in PropertyChanged handlers of AnswerViewModel
        }
        #endregion

        #region Timer Methods
        private void StartTimer()
        {
            // Cập nhật display ngay lập tức trước khi bắt đầu timer
            UpdateTimerDisplay();
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Timer started with {_totalSeconds} seconds remaining");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _totalSeconds--;

            if (_totalSeconds <= 0)
            {
                _timer.Stop();
                _autoSaveTimer?.Stop();

                // Show notification before auto submit
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        "Thời gian làm bài đã hết. Bài thi sẽ được tự động nộp.",
                        "Hết giờ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    );
                });

                _ = SubmitExamAsync(true);
            }
            else
            {
                UpdateTimerDisplay();

                // Warning when 5 minutes left
                if (_totalSeconds == 300)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(
                            "Còn 5 phút nữa sẽ hết giờ làm bài!",
                            "Cảnh báo",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );
                    });
                }
            }
        }

        private void UpdateTimerDisplay()
        {
            // Đảm bảo không hiển thị thời gian âm
            var displaySeconds = Math.Max(0, _totalSeconds);
            
            var hours = displaySeconds / 3600;
            var minutes = (displaySeconds % 3600) / 60;
            var seconds = displaySeconds % 60;

            TimerText = $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Timer: {TimerText} (Total seconds: {_totalSeconds})");
        }

        private void StartAutoSave()
        {
            _autoSaveTimer = new DispatcherTimer();
            _autoSaveTimer.Interval = TimeSpan.FromSeconds(10); // DEBUG: Changed from 5 minutes to 10 seconds for testing
            _autoSaveTimer.Tick += async (s, e) => {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] ⏰ AUTO-SAVE TIMER TRIGGERED at {DateTime.Now:HH:mm:ss}");
                await SaveProgressAsync();
            };
            _autoSaveTimer.Start();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ✅ Auto-save timer started - will save every 10 seconds");
        }
        #endregion

        #region Save & Submit Methods - Separated by Exam Type
        private async Task SaveProgressAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 💾 SaveProgressAsync STARTED - ExamType: {ExamType}");
                
                if (ExamType == ExamType.MultipleChoice)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] 📝 Calling SaveMultipleChoiceProgressAsync...");
                    await SaveMultipleChoiceProgressAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ✅ SaveMultipleChoiceProgressAsync COMPLETED");
                }
                else if (ExamType == ExamType.Practice)
                {
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] 📝 Calling SavePracticeProgressAsync...");
                    await SavePracticeProgressAsync();
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] ✅ SavePracticeProgressAsync COMPLETED");
                }
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] 💾 SaveProgressAsync FINISHED SUCCESSFULLY at {DateTime.Now:HH:mm:ss}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ❌ SaveProgressAsync FAILED: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            }
        }

        private async Task SaveMultipleChoiceProgressAsync()
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

            System.Diagnostics.Debug.WriteLine($"[DEBUG] 📤 Multiple Choice Auto-Save Request:");
            System.Diagnostics.Debug.WriteLine($"[DEBUG]   - MultiExamHistoryId: {updateDto.MultiExamHistoryId}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG]   - Total Answers: {updateDto.Answers.Count}");
            
            foreach (var answer in updateDto.Answers)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG]   - QuestionId: {answer.QuestionId}, Answer: {answer.Answer}");
            }

            await _lamBaiThiService.UpdateProgressAsync(updateDto);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] 📥 Multiple Choice Auto-Save API call completed successfully");
        }

        private async Task SavePracticeProgressAsync()
        {
            try
            {
                var updateDto = new UpdatePracticeExamAnswersRequest
                {
                    Answers = new List<UpdatePracticeExamAnswerDTO>()
                };

                // Lưu tất cả câu trả lời, kể cả câu trống (để server biết câu nào đã được "touched")
                foreach (var question in _allPracticeQuestions)
                {
                    updateDto.Answers.Add(new UpdatePracticeExamAnswerDTO
                    {
                        PracExamHistoryId = _pracExamHistoryId,
                        PracticeQuestionId = question.PracticeQuestionId,
                        Answer = question.GetAnswer()
                    });
                }

                System.Diagnostics.Debug.WriteLine($"[DEBUG] 📤 Practice Exam Auto-Save Request:");
                System.Diagnostics.Debug.WriteLine($"[DEBUG]   - PracExamHistoryId: {_pracExamHistoryId}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG]   - Total Questions: {updateDto.Answers.Count}");
                
                foreach (var answer in updateDto.Answers)
                {
                    var answerPreview = answer.Answer?.Length > 50 ? answer.Answer.Substring(0, 50) + "..." : answer.Answer;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG]   - QuestionId: {answer.PracticeQuestionId}, Answer: '{answerPreview}'");
                }

                await _lamBaiThiService.UpdatePEEach5minutesAsync(updateDto);

                System.Diagnostics.Debug.WriteLine($"[DEBUG] 📥 Practice Exam Auto-Save API call completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ❌ SavePracticeProgressAsync failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                // Log error nhưng không throw để không ảnh hưởng đến UX
            }
        }

        public async Task SubmitExamAsync(bool isAutoSubmit = false)
        {
            try
            {
                if (!isAutoSubmit)
                {
                    int answeredCount;
                    if (ExamType == ExamType.MultipleChoice)
                    {
                        answeredCount = QuestionNumbers.Count(q => q.IsAnswered);
                    }
                    else
                    {
                        answeredCount = QuestionNumbers.Count(q => q.IsAnswered);
                    }

                    var totalCount = TotalQuestions;
                    var timeSpent = CalculateTimeSpent();

                    // Show confirmation dialog
                    var confirmViewModel = new DialogXacNhanNopBaiThiViewModel(answeredCount, totalCount, timeSpent);
                    var confirmDialog = new DialogXacNhanNopBaiThiView(confirmViewModel)
                    {
                        Owner = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault()
                    };

                    confirmDialog.ShowDialog();

                    if (!confirmViewModel.IsConfirmed)
                    {
                        return;
                    }
                }

                // Stop timers when actually submitting
                _timer?.Stop();
                _autoSaveTimer?.Stop();

                // Chỉ hiển thị loading nếu không phải auto submit (để tránh delay khi thoát)
                if (!isAutoSubmit)
                {
                    IsLoading = true;
                }

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
                // Handle API exceptions
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var currentWindow = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();

                    string errorTitle = "Nộp bài thất bại";
                    string errorMessage = apiEx.Message;
                    string errorDetail = "";

                    // Map error messages
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

                        default:
                            errorDetail = "Vui lòng thử lại hoặc liên hệ với giáo viên để được hỗ trợ.";
                            break;
                    }

                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    lamBaiThiView?.Close();

                    var danhSachView = Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault();
                    DialogHelper.ShowErrorDialog(errorTitle, errorMessage, errorDetail, null, danhSachView);
                });
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    lamBaiThiView?.Close();

                    var danhSachView = Application.Current.Windows.OfType<DanhSachBaiThiView>().FirstOrDefault();
                    if (danhSachView == null)
                    {
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
                // Chỉ tắt loading nếu đã bật (không phải auto submit)
                if (!isAutoSubmit)
                {
                    IsLoading = false;
                }
            }
        }

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

        private void SaveCurrentQuestionState()
        {
            if (ExamType == ExamType.MultipleChoice && CurrentQuestion != null && CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _allQuestions.Count)
            {
                var storedQuestion = _allQuestions[CurrentQuestionIndex];

                // Verify that CurrentQuestion is the same reference as stored question
                if (!object.ReferenceEquals(CurrentQuestion, storedQuestion))
                {
                    // If not same reference, sync the state
                    for (int i = 0; i < Math.Min(CurrentQuestion.Answers.Count, storedQuestion.Answers.Count); i++)
                    {
                        storedQuestion.Answers[i].IsSelected = CurrentQuestion.Answers[i].IsSelected;
                    }
                }
            }
            else if (ExamType == ExamType.Practice && CurrentPracticeQuestion != null && CurrentQuestionIndex >= 0 && CurrentQuestionIndex < _allPracticeQuestions.Count)
            {
                var storedQuestion = _allPracticeQuestions[CurrentQuestionIndex];

                System.Diagnostics.Debug.WriteLine($"[DEBUG] SaveCurrentQuestionState: Practice Question {CurrentQuestionIndex + 1}");
                System.Diagnostics.Debug.WriteLine($"  - Current Answer: '{CurrentPracticeQuestion.Answer}'");
                System.Diagnostics.Debug.WriteLine($"  - Stored Answer: '{storedQuestion.Answer}'");

                // Verify that CurrentPracticeQuestion is the same reference
                if (!object.ReferenceEquals(CurrentPracticeQuestion, storedQuestion))
                {
                    System.Diagnostics.Debug.WriteLine($"  - Different references detected, syncing...");
                    storedQuestion.Answer = CurrentPracticeQuestion.GetAnswer();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"  - Same references, no sync needed");
                }
            }
        }

        private async Task SubmitMultipleChoiceExam()
        {
            SaveCurrentQuestionState();

            var submitDto = new UpdateMultiExamProgressDTO
            {
                MultiExamHistoryId = _multiExamHistoryId,
                Answers = new List<UpdateAnswerDTO>()
            };

            foreach (var question in _allQuestions)
            {
                var selectedAnswers = question.Answers.Where(a => a.IsSelected).Select(a => a.AnswerId.ToString()).ToList();

                submitDto.Answers.Add(new UpdateAnswerDTO
                {
                    QuestionId = question.QuestionId,
                    Answer = string.Join(",", selectedAnswers)
                });
            }

            // Tối ưu: Thêm timeout cho auto submit để tránh chờ quá lâu
            var result = await _lamBaiThiService.SubmitExamAsync(submitDto);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] SubmitMultipleChoiceExam result: {result != null}");
            if (result != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Result - Subject: {result.SubjectName}, Score: {result.FinalScore}");
                ShowExamResult(result);
            }
        }

        private async Task SubmitPracticeExam()
        {
            SaveCurrentQuestionState();

            // Gọi debug log trước khi submit
            Application.Current.Dispatcher.Invoke(() =>
            {
                var lamBaiThiView = Application.Current.Windows.OfType<SEP490_G18_GESS_DESKTOPAPP.Views.LamBaiThiView>().FirstOrDefault();
                lamBaiThiView?.DebugLogPracticeAnswersBeforeSubmit();
            });

            var submitDto = new SubmitPracticeExamRequest
            {
                PracExamHistoryId = _pracExamHistoryId,
                Answers = _allPracticeQuestions.Select(q => new SubmitPracticeExamAnswerDTO
                {
                    PracticeQuestionId = q.PracticeQuestionId,
                    Answer = q.GetAnswer()
                }).ToList()
            };

            // Tối ưu: Thêm timeout cho auto submit để tránh chờ quá lâu
            var result = await _lamBaiThiService.SubmitPracticeExamAsync(submitDto);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] SubmitPracticeExam result: {result != null}");
            if (result != null)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Practice Result - Subject: {result.SubjectName}, Time: {result.TimeTaken}");
                ShowPracticeExamResult(result);
            }
        }

        private void ShowExamResult(SubmitExamResponseDTO result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExamResult: Starting...");

                    // Set flag submitted
                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    if (lamBaiThiView != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Setting exam submitted flag");
                        lamBaiThiView.SetExamSubmitted();
                    }

                    // Show result
                    var ketQuaViewModel = App.AppHost.Services.GetRequiredService<KetQuaNopBaiViewModel>();
                    ketQuaViewModel.InitializeFromSubmitResult(result);

                    var ketQuaView = new KetQuaNopBaiView(ketQuaViewModel);
                    ketQuaView.Show();

                    ketQuaView.Activate();
                    ketQuaView.Topmost = true;
                    ketQuaView.Topmost = false;

                    lamBaiThiView?.Close();

                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowExamResult: Completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] ShowExamResult failed: {ex.Message}");
                    MessageBox.Show($"Lỗi hiển thị kết quả: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void ShowPracticeExamResult(SubmitPracticeExamResponseDTO result)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowPracticeExamResult: Starting...");

                    // Set flag submitted
                    var lamBaiThiView = Application.Current.Windows.OfType<LamBaiThiView>().FirstOrDefault();
                    if (lamBaiThiView != null)
                    {
                        System.Diagnostics.Debug.WriteLine("[DEBUG] Setting practice exam submitted flag");
                        lamBaiThiView.SetExamSubmitted();
                    }

                    // Show result
                    var ketQuaViewModel = App.AppHost.Services.GetRequiredService<KetQuaNopBaiViewModel>();
                    ketQuaViewModel.InitializeFromPracticeResult(result);

                    var ketQuaView = new KetQuaNopBaiView(ketQuaViewModel);
                    ketQuaView.Show();

                    ketQuaView.Activate();
                    ketQuaView.Topmost = true;
                    ketQuaView.Topmost = false;

                    lamBaiThiView?.Close();

                    System.Diagnostics.Debug.WriteLine("[DEBUG] ShowPracticeExamResult: Completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] ShowPracticeExamResult failed: {ex.Message}");
                    MessageBox.Show($"Lỗi hiển thị kết quả: {ex.Message}", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
        #endregion

        #region Helper Methods - Separated Progress Updates
        private void UpdateMultipleChoiceProgress()
        {
            int answeredCount = _allQuestions.Count(q => q.Answers.Any(a => a.IsSelected));
            ProgressText = $"{answeredCount}/{TotalQuestions} câu";
            ProgressValue = TotalQuestions > 0 ? (double)answeredCount / TotalQuestions * 100 : 0;
        }

        private void UpdatePracticeProgress()
        {
            // Đếm số câu đã trả lời dựa trên HasAnswer property
            int answeredCount = _allPracticeQuestions.Count(q => q.HasAnswer);
            ProgressText = $"{answeredCount}/{TotalQuestions} câu";
            ProgressValue = TotalQuestions > 0 ? (double)answeredCount / TotalQuestions * 100 : 0;

            System.Diagnostics.Debug.WriteLine($"[DEBUG] UpdatePracticeProgress: {answeredCount}/{TotalQuestions} answered");

            // QUAN TRỌNG: Cập nhật lại QuestionNumbers.IsAnswered cho tất cả câu hỏi
            for (int i = 0; i < _allPracticeQuestions.Count && i < QuestionNumbers.Count; i++)
            {
                var wasAnswered = QuestionNumbers[i].IsAnswered;
                var isAnswered = _allPracticeQuestions[i].HasAnswer;

                if (wasAnswered != isAnswered)
                {
                    QuestionNumbers[i].IsAnswered = isAnswered;
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Question {i + 1}: IsAnswered changed from {wasAnswered} to {isAnswered}");
                }
            }
        }

        #endregion
    }

    #region ViewModels for Questions
    public class QuestionViewModel : BaseViewModel
    {
        private bool _isMultipleChoice;

        public int QuestionId { get; set; }
        public int QuestionOrder { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; }
        public List<AnswerViewModel> Answers { get; set; }

        public bool IsMultipleChoice
        {
            get => _isMultipleChoice;
            set => _isMultipleChoice = value;
        }
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

        private bool _isCurrent;
        public bool IsCurrent
        {
            get => _isCurrent;
            set => SetProperty(ref _isCurrent, value);
        }

        private string _answer;
        public string Answer
        {
            get => _answer ?? "";
            set
            {
                if (SetProperty(ref _answer, value))
                {
                    OnPropertyChanged(nameof(HasAnswer));
                    UpdateProgressValue();
                }
            }
        }

        public bool HasAnswer => !string.IsNullOrWhiteSpace(Answer);

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            set => SetProperty(ref _progressValue, value);
        }

        // Cần truyền tổng số câu vào để tính
        public int TotalQuestions { get; set; }

        public void UpdateProgressValue()
        {
            if (TotalQuestions > 0)
                ProgressValue = HasAnswer ? 100.0 / TotalQuestions : 0;
            else
                ProgressValue = 0;
        }

        public string GetAnswer()
        {
            // IMPORTANT: Preserve line breaks for code formatting
            // Use simple encoding to preserve line breaks in database
            var answer = Answer ?? "";
            
            if (string.IsNullOrEmpty(answer))
                return answer;
            
            // Method 1: Replace line breaks with unique markers
            // Use markers that are unlikely to be typed by users in code
            answer = answer.Replace("\r\n", "___NEWLINE___")   // Windows line ending
                          .Replace("\r", "___NEWLINE___")      // Mac line ending  
                          .Replace("\n", "___NEWLINE___");     // Unix line ending
            
            System.Diagnostics.Debug.WriteLine($"[DEBUG] GetAnswer: Question {PracticeQuestionId}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] GetAnswer: Original='{Answer}'");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] GetAnswer: Encoded='{answer}'");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] GetAnswer: Contains markers={answer.Contains("___NEWLINE___")}");
            
            return answer;
        }

        public void SetAnswerSilently(string answer)
        {
            _answer = answer ?? "";
            OnPropertyChanged(nameof(Answer));
            OnPropertyChanged(nameof(HasAnswer));
            UpdateProgressValue();
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SetAnswerSilently: Question {PracticeQuestionId}, Answer='{_answer}', HasAnswer={HasAnswer}");
        }

        public void ClearAnswer()
        {
            SetAnswerSilently("");
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