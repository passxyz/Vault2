using System.Diagnostics;

using KPCLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    public partial class OtpListPage : ContentPage
    {
        private readonly OtpListViewModel viewModel;

        public OtpListPage (OtpListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = this.viewModel = viewModel;
        }

        private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
        {
            if (args.SelectedItem is not Item item)
            {
                Debug.WriteLine("OtpListPage: item is null in OnItemSelected().");
                return;
            }

            viewModel.OnItemSelected(item);

            // Manually deselect item.
            OtpListView.SelectedItem = null;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = viewModel.GetOtpList();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            // Stop the timer
            viewModel.UpdateTokenDone = false;
            Debug.WriteLine($"OtpListPage:OnDisappearing and UpdateTokenDone={viewModel.UpdateTokenDone}.");
        }

    }
}