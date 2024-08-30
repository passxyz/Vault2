using PassXYZLib;
using PassXYZ.Vault.ViewModels;

namespace PassXYZ.Vault.Views
{
    public partial class UsersPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        public UsersPage(LoginViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
        }

        private void OnUserTap(object sender, ItemTappedEventArgs args)
        {
            var user = args.Item as User;
            if (user == null)
            {
                return;
            }
            _viewModel.OnUserSelected(user);
        }

        private void OnDeleteUser(object sender, EventArgs e)
        {
            var mi = (MenuItem)sender;

            if (mi.BindingContext is User user)
            {
                _viewModel.DeleteUserCommand.Execute(user);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadUsersCommand.Execute(null);
        }
    }
}