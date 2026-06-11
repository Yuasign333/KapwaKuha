// FILE: ViewModels/FeedbackViewModel.cs
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class FeedbackViewModel : ObservableObject
    {
        private readonly string _claimId;
        private readonly string _donorId;
        private readonly string _donorName;

        // Half-star: 1–10 (1=0.5 stars, 2=1 star, … 10=5 stars)
        private int _halfStarValue = 10; // default 5 stars
        public int HalfStarValue
        {
            get => _halfStarValue;
            set
            {
                _halfStarValue = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(StarDisplay));
                OnPropertyChanged(nameof(StarDisplayLabel));
                // Notify all 10 half-star fill properties
                for (int i = 1; i <= 10; i++)
                    OnPropertyChanged($"Half{i}Filled");
            }
        }

        // The actual star value submitted to DB (rounds to nearest 0.5, cast to int for compatibility)
        public int SelectedStars => (int)System.Math.Round(_halfStarValue / 2.0, System.MidpointRounding.AwayFromZero);
        public double SelectedStarsDouble => _halfStarValue / 2.0;

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        private bool _errorVisible;
        public bool ErrorVisible
        {
            get => _errorVisible;
            set { _errorVisible = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string DonorLabel => $"Rate your donation from: {_donorName}";

        // Unicode display: full ★, half ⯨, empty ☆
        public string StarDisplay
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                double val = SelectedStarsDouble;
                for (int i = 1; i <= 5; i++)
                {
                    if (val >= i) sb.Append('★');
                    else if (val >= i - 0.5) sb.Append('⯨');
                    else sb.Append('☆');
                }
                return sb.ToString();
            }
        }

        public string StarDisplayLabel => $"{SelectedStarsDouble:0.#} / 5 ★";

        // Half-star fill properties — Half1Filled = left half of star 1, Half2Filled = right half etc.
        // Naming: Half1=left of star1, Half2=right of star1, Half3=left of star2...
        public bool Half1Filled => _halfStarValue >= 1;
        public bool Half2Filled => _halfStarValue >= 2;
        public bool Half3Filled => _halfStarValue >= 3;
        public bool Half4Filled => _halfStarValue >= 4;
        public bool Half5Filled => _halfStarValue >= 5;
        public bool Half6Filled => _halfStarValue >= 6;
        public bool Half7Filled => _halfStarValue >= 7;
        public bool Half8Filled => _halfStarValue >= 8;
        public bool Half9Filled => _halfStarValue >= 9;
        public bool Half10Filled => _halfStarValue >= 10;

        public ICommand SetHalfStarCommand { get; }
        public ICommand SubmitCommand { get; }
        public ICommand CancelCommand { get; }

        public System.Action? OnSubmitted { get; set; }

        public FeedbackViewModel(string claimId, string donorId, string donorName)
        {
            _claimId = claimId;
            _donorId = donorId;
            _donorName = donorName;

            // param = 1–10 (half-star index)
            SetHalfStarCommand = new RelayCommand(param =>
            {
                if (param == null) return;
                if (int.TryParse(param.ToString(), out int val) && val >= 1 && val <= 10)
                    HalfStarValue = val;
            });

            CancelCommand = new RelayCommand(_ => OnSubmitted?.Invoke());

            SubmitCommand = new AsyncRelayCommand(async _ =>
            {
                IsLoading = true;
                ErrorVisible = false;
                try
                {
                    bool alreadyRated = await KapwaDataService.HasAlreadyRatedClaim(_claimId);
                    if (alreadyRated)
                    {
                        ErrorMessage = "You have already submitted feedback for this claim.";
                        ErrorVisible = true;
                        return;
                    }

                    string fbId = await KapwaDataService.GetNextFeedbackId();
                    var fb = new FeedbackModel
                    {
                        Feedback_ID = fbId,
                        Donor_ID = _donorId,
                        Claim_ID = _claimId,
                        Stars = SelectedStars,
                        Comment = Comment
                    };
                    await KapwaDataService.SubmitFeedback(fb);
                    MessageBox.Show(
                        $"Thank you for your feedback! You rated {StarDisplayLabel}",
                        "Feedback Submitted", MessageBoxButton.OK, MessageBoxImage.Information);
                    OnSubmitted?.Invoke();
                }
                catch { }
                finally { IsLoading = false; }
            });
        }
    }
}