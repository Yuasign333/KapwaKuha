// FILE: PostItemViewModel.cs
// Window: PostItemWindow.xaml
// Two-mode posting: DirectTarget or GeneralPost
// Parallel to RentCarViewModel in CarRentals
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class PostItemViewModel : ObservableObject
    {
        private readonly string _donorId;

        // ── Form fields ───────────────────────────────────────────────────────
        private string _itemName = string.Empty;
        private string _selectedCategory = string.Empty;
        private string _selectedCondition = "Good";
        private string _postType = "GeneralPost";
        private string _selectedBeneficiaryId = string.Empty;
        private bool _isBusy;
        private string _errorMessage = string.Empty;
        private bool _errorVisible;

        public string ItemName
        {
            get => _itemName;
            set { _itemName = value; OnPropertyChanged(); }
        }
        public string SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }
        public string SelectedCondition
        {
            get => _selectedCondition;
            set { _selectedCondition = value; OnPropertyChanged(); }
        }
        public string PostType
        {
            get => _postType;
            set
            {
                _postType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDirectTarget));
                OnPropertyChanged(nameof(IsGeneralPost));
            }
        }
        public bool IsDirectTarget
        {
            get => _postType == "DirectTarget";
            set { if (value) PostType = "DirectTarget"; }
        }
        public bool IsGeneralPost
        {
            get => _postType == "GeneralPost";
            set { if (value) PostType = "GeneralPost"; }
        }

        public string SelectedBeneficiaryId
        {
            get => _selectedBeneficiaryId;
            set { _selectedBeneficiaryId = value; OnPropertyChanged(); }
        }

        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string ErrorMessage { get => _errorMessage; set { _errorMessage = value; OnPropertyChanged(); } }
        public bool ErrorVisible { get => _errorVisible; set { _errorVisible = value; OnPropertyChanged(); } }

        public ObservableCollection<string> Categories { get; } = new();
        public ObservableCollection<string> Conditions { get; } =
            new() { "New", "Good", "Fair", "Poor" };
        public ObservableCollection<BeneficiaryRow> Beneficiaries { get; } = new();

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }

        public PostItemViewModel(string donorId)
        {
            _donorId = donorId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorDashboardWindow(_donorId)));

            SaveCommand = new AsyncRelayCommand(async _ =>
            {
                ErrorVisible = false;

                if (string.IsNullOrWhiteSpace(ItemName))
                { ErrorMessage = "Item name is required."; ErrorVisible = true; return; }
                if (string.IsNullOrWhiteSpace(SelectedCategory))
                { ErrorMessage = "Please select a category."; ErrorVisible = true; return; }
                if (IsDirectTarget && string.IsNullOrWhiteSpace(SelectedBeneficiaryId))
                { ErrorMessage = "Select a target beneficiary for Direct Target post."; ErrorVisible = true; return; }

                var confirm = MessageBox.Show(
                    $"Post item?\n\nName: {ItemName}\nCategory: {SelectedCategory}\n" +
                    $"Condition: {SelectedCondition}\nType: {PostType}",
                    "Confirm Post", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    IsBusy = true;
                    string itemId = await KapwaDataService.GetNextItemId();
                    string catId = await KapwaDataService.GetCategoryIdByName(SelectedCategory);

                    var item = new ItemModel
                    {
                        Item_ID = itemId,
                        Item_Name = ItemName.Trim(),
                        Item_Condition = SelectedCondition,
                        Item_Status = "Available",
                        Date_Found = DateTime.Now,
                        Donor_ID = _donorId,
                        Category_ID = catId,
                        PostType = PostType,
                        TargetBeneficiary_ID = IsDirectTarget ? SelectedBeneficiaryId : ""
                    };

                    await KapwaDataService.AddItem(item);
                    MessageBox.Show($"✅ Item posted! ID: {itemId}",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.Navigate(new View.DonorDashboardWindow(_donorId));
                }
                catch { /* service already showed error */ }
                finally { IsBusy = false; }
            });

            LoadData();
        }

        private async void LoadData()
        {
            var cats = await KapwaDataService.GetAllCategories();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Categories.Clear();
                foreach (var c in cats) Categories.Add(c);
                if (Categories.Count > 0) SelectedCategory = Categories[0];
            });

            var benes = await KapwaDataService.GetActiveBeneficiaries();
            Application.Current.Dispatcher.Invoke(() =>
            {
                Beneficiaries.Clear();
                foreach (var (id, name) in benes)
                    Beneficiaries.Add(new BeneficiaryRow { Id = id, DisplayName = name });
            });
        }
    }
}