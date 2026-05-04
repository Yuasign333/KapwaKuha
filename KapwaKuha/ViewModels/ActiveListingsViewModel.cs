// FILE: ViewModels/ActiveListingsViewModel.cs
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
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsItemSelected));
                OnPropertyChanged(nameof(CanEditSelected));
            }
        }
        public bool IsItemSelected => SelectedItem != null;

        // Edit only allowed when item is still Available
        public bool CanEditSelected => SelectedItem?.Item_Status == "Available";

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand EditPostCommand { get; }

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
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
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
                    MessageBox.Show("Item deleted.", "Done",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch { }
                finally { IsBusy = false; }
            });

            EditPostCommand = new AsyncRelayCommand(async _ =>
            {
                if (SelectedItem == null) return;
                if (SelectedItem.Item_Status != "Available")
                {
                    MessageBox.Show("Only Available items can be edited.", "Cannot Edit",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Open edit dialog inline using simple input dialogs
                string newName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit item name:", "Edit Post", SelectedItem.Item_Name);
                if (string.IsNullOrWhiteSpace(newName)) return;

                string newDesc = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit description:", "Edit Post", SelectedItem.Item_Description);

                string newCond = Microsoft.VisualBasic.Interaction.InputBox(
                    "Edit condition (New/Good/Fair/Poor):", "Edit Post", SelectedItem.Item_Condition);
                if (newCond != "New" && newCond != "Good" && newCond != "Fair" && newCond != "Poor")
                    newCond = SelectedItem.Item_Condition;

                // Optionally browse for new image
                string newImage = SelectedItem.Item_ImagePath;
                var imgResult = MessageBox.Show("Do you want to change the photo?",
                    "Change Photo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (imgResult == MessageBoxResult.Yes)
                {
                    var dlg = new Microsoft.Win32.OpenFileDialog
                    {
                        Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                        Title = "Select New Item Image"
                    };
                    if (dlg.ShowDialog() == true) newImage = dlg.FileName;
                }

                var confirm = MessageBox.Show(
                    $"Save changes to \"{newName}\"?", "Confirm Edit",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                try
                {
                    IsBusy = true;
                    SelectedItem.Item_Name = newName;
                    SelectedItem.Item_Description = newDesc;
                    SelectedItem.Item_Condition = newCond;
                    SelectedItem.Item_ImagePath = newImage;
                    await KapwaDataService.UpdateItem(SelectedItem);
                    MessageBox.Show("✅ Item updated successfully!", "Saved",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadItemsAsync();
                }
                catch { }
                finally { IsBusy = false; }
            });

            _ = LoadItemsAsync();
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