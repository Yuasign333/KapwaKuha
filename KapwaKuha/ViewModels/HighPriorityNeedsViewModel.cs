// FILE: ViewModels/HighPriorityNeedsViewModel.cs
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class HighPriorityNeedsViewModel : ObservableObject
    {
        private readonly string _donorId;

        public ObservableCollection<NeedsPostModel> NeedsPosts { get; } = new();
        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        public ICommand BackCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DonateToNeedCommand { get; }

        public HighPriorityNeedsViewModel(string donorId)
        {
            _donorId = donorId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(new View.DonorDashboardWindow(_donorId)));

            RefreshCommand = new AsyncRelayCommand(async _ => await LoadPostsAsync());

            DonateToNeedCommand = new RelayCommand(post =>
            {
                if (post is not NeedsPostModel selected) return;

                // Force DirectTarget and lock the beneficiary to the org's post
                // Navigate to PostItem with the need pre-filled and locked
                NavigationService.Navigate(
                    new View.PostItemWindow(_donorId, selected.Title, selected.Org_ID, lockDirect: true));
            });

            LoadPostsAsync();
        }

        private async System.Threading.Tasks.Task LoadPostsAsync()
        {
            IsBusy = true;
            try
            {
                var posts = await KapwaDataService.GetOpenNeedsPosts();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    NeedsPosts.Clear();
                    // Only show Open posts — Fulfilled ones auto-removed here
                    foreach (var p in posts)
                        if (p.Status == "Open") NeedsPosts.Add(p);
                });
            }
            catch { }
            finally { IsBusy = false; }
        }
    }
}