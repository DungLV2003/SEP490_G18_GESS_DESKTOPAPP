using Microsoft.Extensions.DependencyInjection;
using SEP490_G18_GESS_DESKTOPAPP.Helpers;
using SEP490_G18_GESS_DESKTOPAPP.Models.LamBaiThiDTO;
using SEP490_G18_GESS_DESKTOPAPP.Services.Interface;
using SEP490_G18_GESS_DESKTOPAPP.ViewModels.Base;
using SEP490_G18_GESS_DESKTOPAPP.Views;
using System.Windows;
using System.Windows.Input;

namespace SEP490_G18_GESS_DESKTOPAPP.ViewModels
{
    public class KetQuaNopBaiViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;

        #region Properties
        private string _subjectName;
        public string SubjectName
        {
            get => _subjectName;
            set => SetProperty(ref _subjectName, value);
        }

        private string _timeTaken;
        public string TimeTaken
        {
            get => _timeTaken;
            set => SetProperty(ref _timeTaken, value);
        }

        private int _correctCount;
        public int CorrectCount
        {
            get => _correctCount;
            set => SetProperty(ref _correctCount, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        private double _percentage;
        public double Percentage
        {
            get => _percentage;
            set => SetProperty(ref _percentage, value);
        }

        private double _finalScore;
        public double FinalScore
        {
            get => _finalScore;
            set => SetProperty(ref _finalScore, value);
        }

        private bool _showScore;
        public bool ShowScore
        {
            get => _showScore;
            set => SetProperty(ref _showScore, value);
        }
        #endregion

        #region Commands
        public ICommand BackToHomeCommand { get; }
        #endregion

        public KetQuaNopBaiViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            BackToHomeCommand = new RelayCommand(BackToHome);
        }

        public void InitializeFromSubmitResult(SubmitExamResponseDTO result)
        {
            SubjectName = result.SubjectName;
            TimeTaken = result.TimeTaken;
            CorrectCount = result.CorrectCount;
            TotalCount = result.TotalCount;
            Percentage = result.TotalCount > 0 ? (double)result.CorrectCount / result.TotalCount * 100 : 0;
            FinalScore = result.FinalScore;
            ShowScore = true; // Show score for multiple choice
        }

        public void InitializeFromPracticeResult(SubmitPracticeExamResponseDTO result)
        {
            SubjectName = result.SubjectName;
            TimeTaken = result.TimeTaken;
            ShowScore = false; // Hide score for practice exam
        }

        private void BackToHome()
        {
            // Close current window
            Application.Current.Windows.OfType<KetQuaNopBaiView>().FirstOrDefault()?.Close();

            // Open home page
            var homeView = App.AppHost.Services.GetRequiredService<HomePageView>();
            homeView.Show();
        }
    }
}