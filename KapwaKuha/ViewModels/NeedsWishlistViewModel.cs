// FILE: ViewModels/NeedsWishlistViewModel.cs
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
        private string _imagePath = string.Empty;
        private bool _isBusy;
        private string _errorMessage = string.Empty;
        private bool _errorVisible;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }
        public string Urgency
        {
            get => _urgency;
            set
            {
                _urgency = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsLow));
                OnPropertyChanged(nameof(IsMedium));
                OnPropertyChanged(nameof(IsHigh));
            }
        }
        public string ImagePath
        {
            get => _imagePath;
            set { _imagePath = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage)); }
        }
        public bool HasImage => !string.IsNullOrEmpty(_imagePath);

        public bool IsLow { get => _urgency == "Low"; set { if (value) Urgency = "Low"; } }
        public bool IsMedium { get => _urgency == "Medium"; set { if (value) Urgency = "Medium"; } }
        public bool IsHigh { get => _urgency == "High"; set { if (value) Urgency = "High"; } }

        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
        public bool ErrorVisible { get => _errorVisible; set { _errorVisible = value; OnPropertyChanged(); } }

        public ObservableCollection<NeedsPostModel> MyPosts { get; } = new();

        public ICommand BackCommand { get; }
        public ICommand PostNeedCommand { get; }
        public ICommand BrowseImageCommand { get; }
        public ICommand SetLowCommand { get; }
        public ICommand SetMediumCommand { get; }
        public ICommand SetHighCommand { get; }

        public NeedsWishlistViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            SetLowCommand = new RelayCommand(_ => Urgency = "Low");
            SetMediumCommand = new RelayCommand(_ => Urgency = "Medium");
            SetHighCommand = new RelayCommand(_ => Urgency = "High");

            BrowseImageCommand = new RelayCommand(_ =>
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Select Need Photo"
                };
                if (dlg.ShowDialog() == true) ImagePath = dlg.FileName;
            });

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
                        Org_ID = _beneficiaryId,
                        Title = Title.Trim(),
                        Description = Description.Trim(),
                        Urgency = Urgency,
                        ImagePath = ImagePath,
                        Status = "Open"
                    };
                    await KapwaDataService.PostNeedsRequest(post);
                    MyPosts.Insert(0, post);
                    MessageBox.Show($"✅ Need posted! ID: {postId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Title = Description = ImagePath = string.Empty;
                    Urgency = "Medium";
                }
                catch { }
                finally { IsBusy = false; }
            });
        }
    }
}