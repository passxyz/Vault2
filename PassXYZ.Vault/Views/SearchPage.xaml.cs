using System;
using System.Collections.Generic;
using System.Diagnostics;


using KPCLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    public partial class SearchPage : ContentPage
    {
        private readonly ItemsViewModel _viewModel;

        public SearchPage (ItemsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _ = _viewModel.ExecuteSearch(null);
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            var item = args.SelectedItem as Item;
            if (item == null)
            {
                Debug.WriteLine("OnItemSelected: item is null.");
                return;
            }

            _viewModel.OnItemSelected(item);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}