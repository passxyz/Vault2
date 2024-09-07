using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

using KPCLib;
using PassXYZLib;
using PassXYZ.Vault.Services;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Mvvm.Input;

namespace PassXYZ.Vault.ViewModels
{
    public partial class IconSearchViewModel : ObservableObject, IQueryAttributable
    {
        readonly IDataStore<Item> dataStore;
        readonly ILogger<IconSearchViewModel> logger;
        
        public IconSearchViewModel(IDataStore<Item> dataStore, ILogger<IconSearchViewModel> logger)
        {
            this.dataStore = dataStore;
            this.logger = logger;
            LoadIcons();
        }

        public ObservableCollection<PxIcon>? PxIcons { get; set; }

        [ObservableProperty]
        private bool isBusy;

        [ObservableProperty]
        private PxIcon? selectedIcon = default;

        [ObservableProperty]
        private Item? selectedItem = default;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            SelectedItem = query["Item"] as Item;
        }

        [RelayCommand]
        private void LoadIcons()
        {
            try
            {
                PxIcons = dataStore.GetIcons();
                Debug.WriteLine($"Search keyword {PxIcons.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
            }
        }

        public async Task<bool> DeleteCustomIconAsync(PxIcon icon)
        {
            var result = await dataStore.DeleteCustomIconAsync(icon.Uuid);
            if (result && icon is not null && PxIcons is not null)
            {
                _ = PxIcons.Remove(icon);
                logger.LogDebug("Deleted icon");
            }
            return result;
        }

        public void Search(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) { return; }
            logger.LogDebug($"Search keyword {searchText}");

            PxIcons = dataStore.GetIcons(searchText);
        }

        [RelayCommand]
        private async Task Save(object obj)
        {
            if (SelectedIcon != null && SelectedItem != null)
            {
                logger.LogDebug("Saved icon change");
                await dataStore.UpdateItemAsync(SelectedItem);
            }
            _ = await Shell.Current.Navigation.PopModalAsync();
        }

        [RelayCommand]
        private async Task Cancel(object obj)
        {
            logger.LogDebug("Cancelled icon change");
            _ = await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
