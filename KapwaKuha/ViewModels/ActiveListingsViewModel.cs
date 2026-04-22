// FILE: ActiveListingsViewModel.cs
// Window: ActiveListingsWindow.xaml
// Donor's own posted items — parallel to FleetStatusViewModel
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class ActiveListingsViewModel : ObservableObject
    {
        private readonly string _donorId;

        public ObservableCollection<ItemModel> Items { get; } = new();
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        public string StatusMessage { get; private set; } = string.Empty;

        private ItemModel? _selectedItem;
        public ItemModel? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsItemSelected)); }
        }
        public bool IsItemSelected => SelectedItem != null;

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteItemCommand { get; }

        public ActiveListingsViewModel(string donorId)
        {
            _donorId = donorId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorDashboardWindow(_donorId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadItemsAsync());

            DeleteItemCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedItem == null) return;
                if (SelectedItem.Item_Status != "Available")
                {
                    MessageBox.Show("Only Available items can be deleted.", "Cannot Delete",
                    MessageBoxButton.OK, MessageBoxImage.Warning); return;
                }

                var r = MessageBox.Show($"Delete '{SelectedItem.Item_Name}'?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (r != MessageBoxResult.Yes) return;

                try
                {
                    IsBusy = true;
                    await KapwaDataService.DeleteItem(SelectedItem.Item_ID);
                    Items.Remove(SelectedItem);
                    SelectedItem = null;
                    MessageBox.Show("Item deleted.", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
                finally { IsBusy = false; }
            });

            LoadItemsAsync();
        }

        private async System.Threading.Tasks.Task LoadItemsAsync()
        {
            IsBusy = true;
            try
            {
                var items = await KapwaDataService.GetItemsByDonor(_donorId);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Items.Clear();
                    foreach (var i in items) Items.Add(i);
                    StatusMessage = $"{Items.Count} item(s) posted.";
                    OnPropertyChanged(nameof(StatusMessage));
                });
            }
            catch { }
            finally { IsBusy = false; }
        }
    }
}