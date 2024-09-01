using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

using Plugin.Fingerprint.Abstractions;
using PassXYZLib;
using PassXYZ.Vault.Views;
using PassXYZ.Vault.Services;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace PassXYZ.Vault.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private LoginService _currentUser;
        public ObservableCollection<PxUser> Users { get; }
        ILogger<LoginViewModel> _logger;
        private readonly IFingerprint _fingerprint;

        public LoginViewModel(LoginService user, ILogger<LoginViewModel> logger, IFingerprint fingerprint)
        {
            _currentUser = user;
            _logger = logger;
            _fingerprint = fingerprint;
            Users = new ObservableCollection<PxUser>();
        }

        [RelayCommand(CanExecute = nameof(ValidateLogin))]
        private async Task Login()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("", Properties.Resources.settings_empty_password, Properties.Resources.alert_id_ok);
                    IsBusy = false;
                    return;
                }

                _currentUser.Username = Username;
                _currentUser.Password = Password;
                _logger.LogDebug("data path: {path}", _currentUser.Path);
                bool status = await _currentUser.LoginAsync();

                if (status)
                {
                    if (AppShell.CurrentAppShell != null)
                    {
                        AppShell.CurrentAppShell.SetRootPageTitle(Username);

                        await Shell.Current.GoToAsync($"//RootPage");
                    }
                    else
                    {
                        throw (new NullReferenceException("CurrentAppShell is null"));
                    }
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                IsBusy = false;
                string msg = ex.Message;
                if (ex is System.IO.IOException ioException)
                {
                    _logger.LogError("Login error, need to recover");
                    msg = Properties.Resources.message_id_recover_datafile;
                }
                await Shell.Current.DisplayAlert(Properties.Resources.LoginErrorMessage, msg, Properties.Resources.alert_id_ok);
            }
        }

        private bool ValidateLogin()
        {
            var canExecute = !String.IsNullOrWhiteSpace(Username)
                && !String.IsNullOrWhiteSpace(Password);
            return canExecute;
        }

        [RelayCommand(CanExecute = nameof(ValidateSignUp))]
        private async Task SignUp()
        {
            try
            {
                IsBusy = true;

                if (string.IsNullOrWhiteSpace(Password2) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(Username))
                {
                    await Shell.Current.DisplayAlert("", Properties.Resources.settings_empty_password, Properties.Resources.alert_id_ok);
                    IsBusy = false;
                    return;
                }

                _currentUser.Username = Username;
                _currentUser.Password = Password;

                if (_currentUser.IsUserExist)
                {
                    await Shell.Current.DisplayAlert(Properties.Resources.SignUpPageTitle, Properties.Resources.SignUpErrorMessage1, Properties.Resources.alert_id_ok);
                    IsBusy = false;
                    return;
                }

                await _currentUser.SignUpAsync();
                await Shell.Current.DisplayAlert(Properties.Resources.SignUpPageTitle, Properties.Resources.SiguUpMessage, Properties.Resources.alert_id_ok);
                Users.Add(_currentUser);
                Username = "";
                Password = "";
                Password2 = "";
                IsBusy = false;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert(Properties.Resources.SignUpPageTitle, ex.Message, Properties.Resources.alert_id_ok);
            }
            Debug.WriteLine($"LoginViewModel: OnSignUpClicked {_currentUser.Username}, DeviceLock: {_currentUser.IsDeviceLockEnabled}");
            await Shell.Current.Navigation.PopModalAsync();
        }

        private bool ValidateSignUp()
        {
            var canExecute = !String.IsNullOrWhiteSpace(Username)
                && !String.IsNullOrWhiteSpace(Password)
                && !String.IsNullOrWhiteSpace(Password2);
            
            if (canExecute)
            {
                return Password!.Equals(Password2);
            }

            return canExecute;
        }

        public async void ImportKeyFile()
        {
            var options = new PickOptions
            {
                PickerTitle = Properties.Resources.import_message1,
                //FileTypes = customFileType,
            };

            try
            {
                var result = await FilePicker.PickAsync(options);
                if (result != null)
                {
                    var stream = await result.OpenReadAsync();
                    var fileStream = File.Create(_currentUser.KeyFilePath);
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                    fileStream.Close();
                }
                else
                {
                    await Shell.Current.DisplayAlert(Properties.Resources.action_id_import, Properties.Resources.import_error_msg, Properties.Resources.alert_id_ok);
                }
            }
            catch (Exception ex)
            {
                // The user canceled or something went wrong
                _logger.LogError($"LoginViewModel: ImportKeyFile, {ex}");
            }
        }

        public async Task FingerprintLogin()
        {
            var cancel = new CancellationTokenSource();
            var dialogConfig = new AuthenticationRequestConfiguration(Username,
                Properties.Resources.fingerprint_login_message)
            {
                CancelTitle = "Cancel fingerprint login",
                FallbackTitle = "Use Password",
                AllowAlternativeAuthentication = true,
            };

            var result = await _fingerprint.AuthenticateAsync(dialogConfig, cancel.Token);

            if (result.Authenticated)
            {
                // Username cannot be null when FingerprintLogin is invokved
                Password = await _currentUser.GetSecurityAsync();
                if (!string.IsNullOrWhiteSpace(Password))
                {
                    await Login();
                }
                else
                {
                    _logger.LogWarning("GetSecurityAsync() error.");
                }
            }
            else
            {
                _logger.LogWarning("Failed to login with fingerprint.");
            }
        }

        public async void CheckFingerprintStatus()
        {
            _currentUser.Username = Username;
            var password = await _currentUser.GetSecurityAsync();
            IsFingerprintAvailable = await _fingerprint.IsAvailableAsync();
            IsFingerprintEnabled = IsFingerprintAvailable && !string.IsNullOrWhiteSpace(password);
        }

        [ObservableProperty]
        private bool isFingerprintEnabled = false;

        [ObservableProperty]
        private bool isFingerprintAvailable = false;

        [ObservableProperty]
        private bool isBusy = false;

        private string? username = default;
        public string? Username
        {
            get => username;
            set
            {
                if (SetProperty(ref username, value))
                {
                    _currentUser.Username = value;
                    LoginCommand.NotifyCanExecuteChanged();
                    SignUpCommand.NotifyCanExecuteChanged();
                    _logger.LogDebug($"set to user {_currentUser.Username}");
                }
            }
        }

        private string? password = default;
        public string? Password
        {
            get => password;
            set
            {
                if (SetProperty(ref password, value))
                {
                    _currentUser.Password = value;
                    LoginCommand.NotifyCanExecuteChanged();
                    SignUpCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private bool CheckDeviceLock()
        {
            if (string.IsNullOrWhiteSpace(this.Username)) 
            {
                // When username is empty in LoginPage, we need to set this.
                IsDeviceLockEnabled = false;
                return false;
            }

            User user = new()
            {
                Username = this.Username,
            };

            if (user.IsUserExist)
            {
                // This is important, since we need to reset device lock status based on existing file.
                _currentUser.IsDeviceLockEnabled = user.IsDeviceLockEnabled;
                _logger.LogDebug($"Checking device lock is {!_currentUser.IsKeyFileExist && _currentUser.IsDeviceLockEnabled}");
                return !_currentUser.IsKeyFileExist && _currentUser.IsDeviceLockEnabled;
            }

            return false;
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SignUpCommand))]
        private string? password2 = default;

        public bool IsDeviceLockEnabled
        {
            get
            {
                CheckDeviceLock();
                return _currentUser.IsDeviceLockEnabled;
            }
            set 
            {
                // This is needed by SignUpPage.
                _currentUser.IsDeviceLockEnabled = value;
            }
        }

        public bool IsKeyFileNotExist
        { 
            get 
            {
                _logger.LogDebug($"user={_currentUser.Username}, IsDeviceLockEnabled={_currentUser.IsDeviceLockEnabled}, IsKeyFileExist={_currentUser.IsKeyFileExist}");
                return (IsDeviceLockEnabled && !_currentUser.IsKeyFileExist);
            } 
        }

        public List<string> GetUsersList()
        {
            return User.GetUsersList();
        }

        public void Logout() 
        {
            _currentUser.Logout();
        }

        public async Task<bool> AuthenticateAsync(string reason, string? cancel = null, string? fallback = null, string? tooFast = null)
        {
            CancellationTokenSource cancelToken;

            cancelToken = new CancellationTokenSource();

            var dialogConfig = new AuthenticationRequestConfiguration("Verify your fingerprint", reason)
            { // all optional
                CancelTitle = cancel,
                FallbackTitle = fallback,
                AllowAlternativeAuthentication = false
            };

            // optional
            dialogConfig.HelpTexts.MovedTooFast = tooFast;

            var result = await _fingerprint.AuthenticateAsync(dialogConfig, cancelToken.Token);

            return result.Authenticated;
        }

        public string GetDeviceLockData()
        {
            return (PxDefs.PxKeyFile + PxDatabase.GetDeviceLockData(_currentUser));
        }

        [RelayCommand]
        private async Task AddUser(object obj)
        {
            await Shell.Current.Navigation.PushModalAsync(new SignUpPage(this));
        }

        [RelayCommand]
        private async Task LoadUsers()
        {
            if (IsBusy)
            {
                _logger.LogDebug("It is busy and cannot load users");
                return;
            }

            try 
            {
                IsBusy = true;
                var users = await PxUser.LoadLocalUsersAsync();
                if (users == null)
                {
                    _logger.LogDebug("LoadUsersCommand, users is null");
                    IsBusy = false;
                    throw new ArgumentNullException(nameof(users));
                }

                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
                IsBusy = false;
                _logger.LogDebug("LoadUsersCommand done");
            }
            catch (Exception ex)
            {
                IsBusy = false;
                _logger.LogError(ex, "LoadUsersCommand error");
            }
        }

        [RelayCommand]
        public void DeleteUser(User user)
        {
            if (IsBusy)
            {
                _logger.LogDebug($"It is busy and cannot delete {user.Username}");
                return;
            }

            IsBusy = true;
            user.Delete();
            Users.Remove((PxUser)user);

            IsBusy = false;
            _logger.LogDebug($"Delete {user.Username}");
        }

        public void OnUserSelected(User user)
        {
            if (_currentUser != null)
            {
                _currentUser.Username = user.Username;
            }
            _logger.LogDebug($"Selected user {user.Username}.");
        }

        /// <summary>
        /// Recreate a key file from a PxKeyData
        /// </summary>
        /// <param name="data">PxKeyData source</param>
        /// <returns>true - created key file, false - failed to create key file.</returns>
        public bool CreateKeyFile(string data)
        {
            return PxDatabase.CreateKeyFile(data, _currentUser.Username);
        }
    }
}
