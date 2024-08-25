using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using KPCLib;
using PassXYZLib;

using PassXYZ.Vault.Services;
using PassXYZ.Vault.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PassXYZ.Vault.ViewModels
{
    public partial class OtpListViewModel : ObservableObject
    {
        readonly IDataStore<Item> dataStore;
        readonly ILogger<OtpListViewModel> logger;
        public ObservableCollection<PxEntry> Entries { get; set; }
        public bool UpdateTokenDone = true;

        public OtpListViewModel(IDataStore<Item> dataStore, ILogger<OtpListViewModel> logger)
        {
            this.dataStore = dataStore;
            this.logger = logger;
            Entries = new();
        }

        [ObservableProperty]
        private bool isBusy;

        [RelayCommand]
        private void UpdateTokens()
        {
            if (IsBusy) { return; }

            IsBusy = true;

            foreach (PxEntry entry in Entries)
            {
                entry.UpdateToken();
                Debug.WriteLine($"UpdateTokens({entry.Name} {entry.Progress})");
            }

            IsBusy = false;
        }

        [RelayCommand]
        private async Task GetOtpList()
        {
            if (IsBusy) { return; }

            IsBusy = true;

            try
            {
                Entries.Clear();
                IEnumerable<PxEntry> items = await dataStore.GetOtpEntryListAsync();

                foreach (PxEntry entry in items)
                {
                    if(entry is not null) 
                    {
                        Debug.WriteLine($"Found OTP entry {entry.Name}");
                        entry.SetIcon();
                        Entries.Add(entry);
                    }
                }

                UpdateTokenDone = true;
                Application.Current?.Dispatcher.StartTimer(new TimeSpan(0, 0, PxEntry.TimerStep), () =>
                    {
                        UpdateTokens();
                        return UpdateTokenDone; // True = Repeat again, False = Stop the timer
                    });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OtpListViewModel: GetOtpListCommand: {ex}");
                IsBusy = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async void OnItemSelected(Item item)
        {
            if (item == null)
            {
                return;
            }

            PxEntry entry = (PxEntry)item;
            if (entry.IsNotes())
            {
                await Shell.Current.GoToAsync($"{nameof(NotesPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
            }
        }

    }
}
