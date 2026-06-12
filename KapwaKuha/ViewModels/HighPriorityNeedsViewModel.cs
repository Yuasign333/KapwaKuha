// FILE: ViewModels/HighPriorityNeedsViewModel.cs
using System;
using System.Collections.Generic;
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

        // This is your single source of truth bound to your View's ItemsSource
        public ObservableCollection<NeedsPostModel> NeedsPosts { get; } = new();

        private bool _isBusy;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        // Master reference cache list containing all unprocessed raw posts from DB
        private List<NeedsPostModel> _allPosts = new();

        private string _filterUrgency = "All";
        public string FilterUrgency
        {
            get => _filterUrgency;
            set { _filterUrgency = value; OnPropertyChanged(); ApplyAllFilters(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyAllFilters(); }
        }

        private string _beneTypeFilter = "All";
        public string BeneTypeFilter
        {
            get => _beneTypeFilter;
            set { _beneTypeFilter = value; OnPropertyChanged(); ApplyAllFilters(); }
        }

        /// <summary>
        /// Consolidated Filter Core Logic Evaluation
        /// Combines Search Text, Urgency Level, and Beneficiary Type synchronously
        /// </summary>
        private void ApplyAllFilters()
        {
            NeedsPosts.Clear();
            var searchTarget = _searchText.Trim().ToLower();

            foreach (var post in _allPosts)
            {
                // 1. Evaluate Urgency Filter Condition
                bool matchUrgency = _filterUrgency == "All" ||
                                    string.Equals(post.Urgency, _filterUrgency, StringComparison.OrdinalIgnoreCase);

                // 2. Evaluate Beneficiary Type Condition (Matches tags: "All", "Institutional", "Independent")
                bool matchBeneType = _beneTypeFilter == "All" ||
                                     string.Equals(post.BeneTypeBadge, _beneTypeFilter, StringComparison.OrdinalIgnoreCase);

                // 3. Evaluate Free-Text Search Queries
                bool matchSearch = string.IsNullOrEmpty(searchTarget) ||
                                   (post.Title?.ToLower().Contains(searchTarget) ?? false) ||
                                   (post.Description?.ToLower().Contains(searchTarget) ?? false) ||
                                   (post.Org_Name?.ToLower().Contains(searchTarget) ?? false);

                // If the post matches all 3 criteria simultaneously, display it in the view
                if (matchUrgency && matchBeneType && matchSearch)
                {
                    NeedsPosts.Add(post);
                }
            }
        }

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

                if (string.IsNullOrEmpty(selected.RequesterBeneficiaryId))
                {
                    MessageBox.Show(
                        $"No active beneficiary found in \"{selected.Org_Name}\".\n\n" +
                        "Please ensure the organization has at least one active member registered.",
                        "Cannot Fulfill",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                NavigationService.Navigate(
                    new View.PostItemWindow(
                        _donorId,
                        prefillTitle: selected.Title,
                        lockedOrgId: selected.Org_ID,
                        lockDirect: true,
                        lockedBeneficiaryId: selected.RequesterBeneficiaryId,
                        linkedNeedsPostId: selected.NeedsPost_ID));
            });

            _ = LoadPostsAsync();
        }

        private async System.Threading.Tasks.Task LoadPostsAsync()
        {
            IsBusy = true;
            try
            {
                var posts = await KapwaDataService.GetOpenNeedsPosts();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _allPosts = posts ?? new List<NeedsPostModel>();
                    ApplyAllFilters();
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading posts: {ex.Message}");
            }
            finally { IsBusy = false; }
        }
    }
}