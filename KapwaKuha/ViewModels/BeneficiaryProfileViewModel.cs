// FILE: ViewModels/BeneficiaryProfileViewModel.cs
using System;
using System.Windows;
using System.Windows.Input;
using KapwaKuha.Commands;
using KapwaKuha.Models;
using KapwaKuha.Services;

namespace KapwaKuha.ViewModels
{
    public class BeneficiaryProfileViewModel : ObservableObject
    {
        private readonly string _beneficiaryId;
        private string _fullName = string.Empty;
        private string _username = string.Empty;
        private string _contact = string.Empty;
        private string _orgName = string.Empty;
        private string _picturePath = string.Empty;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }
        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }
        public string Contact
        {
            get => _contact;
            set { _contact = value; OnPropertyChanged(); }
        }
        public string OrgName
        {
            get => _orgName;
            set { _orgName = value; OnPropertyChanged(); }
        }
        public string PicturePath
        {
            get => _picturePath;
            set
            {
                _picturePath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPicture));
            }
        }

        // HasPicture: true only when path is set AND file actually exists
        public bool HasPicture =>
            !string.IsNullOrEmpty(_picturePath) &&
            System.IO.File.Exists(_picturePath);

        public string BeneficiaryIdLabel => $"ID: {_beneficiaryId}";

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand BrowsePictureCommand { get; }

        public BeneficiaryProfileViewModel(string beneficiaryId)
        {
            _beneficiaryId = beneficiaryId;

            BackCommand = new RelayCommand(_ =>
                NavigationService.Navigate(
                    new View.BeneficiaryDashboardWindow(_beneficiaryId)));

            // File picker — identical logic to DonorProfileViewModel
            BrowsePictureCommand = new RelayCommand(_ =>
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
                    Title = "Select Profile Picture"
                };
                if (dlg.ShowDialog() == true)
                    PicturePath = dlg.FileName;
            });

            SaveCommand = new AsyncRelayCommand(async _ =>
            {
                try
                {
                    await KapwaDataService.UpdateBeneficiaryProfile(
                        _beneficiaryId, Username, PicturePath);
                    MessageBox.Show("✅ Profile updated!",
                        "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Save failed: " + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            LoadProfile();
        }

        private async void LoadProfile()
        {
            try
            {
                var bene = await KapwaDataService.GetBeneficiaryById(_beneficiaryId);
                if (bene == null) return;

                FullName = bene.Beneficiary_FullName;
                Username = bene.Beneficiary_Username;
                Contact = bene.Beneficiary_Contact;
                OrgName = bene.Organization_Name;
                PicturePath = bene.ProfilePicturePath ?? string.Empty;
            }
            catch { /* silently handled — form stays empty */ }
        }
    }
}