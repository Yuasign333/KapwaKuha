// FILE: NeedsWishlistViewModel.cs
// Window: NeedsWishlistWindow.xaml
// Beneficiary/Organization posts new needs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class NeedsWishlistViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

        private string _title = string.Empty;
        private string _description = string.Empty;
        private string _urgency = "Medium";
        private bool _isBusy;
        private string _errorMessage = string.Empty;
        private bool _errorVisible;

        public string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }
        public string Urgency { get => _urgency; set { _urgency = value; OnPropertyChanged(); } }
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
        public bool ErrorVisible { get => _errorVisible; set { _errorVisible = value; OnPropertyChanged(); } }

        public ObservableCollection<NeedsPostModel> MyPosts { get; } = new();
        public ObservableCollection<string> UrgencyOptions { get; } = new() { "Low", "Medium", "High" };

        public ICommand BackCommand { get; }
        public ICommand PostNeedCommand { get; }

        public NeedsWishlistViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            PostNeedCommand = new AsyncRelayCommand(async _ =>
            {
                ErrorVisible = false;
                if (string.IsNullOrWhiteSpace(Title))
                { ErrorMessage = "Title is required."; ErrorVisible = true; return; }
                if (string.IsNullOrWhiteSpace(Description))
                { ErrorMessage = "Description is required."; ErrorVisible = true; return; }

                try
                {
                    IsBusy = true;
                    string postId = await KapwaDataService.GetNextNeedsPostId();
                    var post = new NeedsPostModel
                    {
                        NeedsPost_ID = postId,
                        Org_ID = _beneficiaryId,  // linked through beneficiary's org
                        Title = Title.Trim(),
                        Description = Description.Trim(),
                        Urgency = Urgency,
                        Status = "Open"
                    };
                    await KapwaDataService.PostNeedsRequest(post);
                    MessageBox.Show($"✅ Need posted! ID: {postId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Title = Description = string.Empty;
                    Urgency = "Medium";
                }
                catch { }
                finally { IsBusy = false; }
            });
        }
    }
}