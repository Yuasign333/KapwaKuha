// FILE: BrowseItemsViewModel.cs
// Window: BrowseItemsWindow.xaml
// Marketplace for General Post items — parallel to BrowseCarsViewModel
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class BrowseItemsViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;

        public ObservableCollection<ItemModel> Items { get; } = new();

        private ItemModel? _selectedItem;
        public ItemModel? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); }
        }

        private string _filterCategory = "All";
        public string FilterCategory
        {
            get => _filterCategory;
            set { _filterCategory = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string StatusMessage { get; private set; } = string.Empty;

        private ObservableCollection<ItemModel> _allItems = new();

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SelectItemCommand { get; }

        public BrowseItemsViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadItemsAsync());

            SelectItemCommand = new RelayCommand(item =>
            {
                if (item is ItemModel selected)
                    NavigationService.Navigate(new View.ClaimItemWindow(_beneficiaryId, selected));
            });

            LoadItemsAsync();
        }

        private async System.Threading.Tasks.Task LoadItemsAsync()
        {
            IsBusy = true;
            try
            {
                var items = await KapwaDataService.GetAvailableItems();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allItems.Clear();
                    foreach (var i in items) _allItems.Add(i);
                    ApplyFilter();
                });
            }
            catch { }
            finally { IsBusy = false; }
        }

        private void ApplyFilter()
        {
            Items.Clear();
            foreach (var i in _allItems)
                if (_filterCategory == "All" || i.Category_Name == _filterCategory)
                    Items.Add(i);
        }
    }
}