using PassXYZLib;
using PassXYZ.Vault.ViewModels;
using System.Diagnostics;
using PassXYZ.Vault.Resources.Styles;
using Camera.MAUI;
using Microsoft.Extensions.Logging;

namespace PassXYZ.Vault.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        private readonly LoginViewModel _viewModel;
        ILogger<LoginPage> _logger;
        private List<string> _users => User.GetUsersList();

        public LoginPage(LoginViewModel viewModel, ILogger<LoginPage> logger)
        {
            InitializeComponent();
            BindingContext = _viewModel = viewModel;
            _logger = logger;
            if (_users != null && _users.Count > 1)
            {
                switchUsersButton.IsVisible = true;
            }
            _viewModel.FingerPrintStatusChanged += FingerPrintStatusChanged;
        }

        private async void OnSwitchUsersClicked(object sender, EventArgs e)
        {
            if (_users != null)
            {
                var username = await DisplayActionSheet(Properties.Resources.pt_id_switchusers, Properties.Resources.action_id_cancel, null, _users.ToArray());
                if (username != Properties.Resources.action_id_cancel && !string.IsNullOrWhiteSpace(username))
                {
                    messageLabel.Text = "";
                    _viewModel.Username = usernameEntry.Text = username;
                    _viewModel.Password = passwordEntry.Text = "";
                    InitFingerPrintButton();
                }
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Logout();
            passwordEntry.Text = "";
            InitFingerPrintButton();
        }

        private void FingerPrintStatusChanged(object sender, EventArgs e)
        {
            _logger.LogDebug($"Fingerprint status changed {_viewModel.IsFingerprintEnabled}");
            if (_viewModel.IsKeyFileNotExist)
            {
                Debug.WriteLine("LoginPage: SetupQRCode");
                SetToQrCode();
            }
            else
            {
                bool isVisible = false;
                if (_viewModel.IsFingerprintEnabled)
                {
                    isVisible = true;
                }
                SetToFingerprint(isVisible);
            }
        }

        private void SetToQrCode() 
        {
            fpButton.Source = new FontImageSource
            {
                Glyph = FontAwesomeSolid.Qrcode,
                FontFamily = "FontAwesomeSolid",
                Color = (Color)Application.Current.Resources["Primary"],
                Size = 32
            };
            // fpButton.Clicked += OnScanQRCodeClicked;
            fpButton.IsVisible = true;
            passwordEntry.IsEnabled = false;
            messageLabel.Text = Properties.Resources.settings_security_DLK_message1 + _viewModel.Username + ".";
        }

        private void SetToFingerprint(bool isVisible = false)
        {
            fpButton.IsVisible = isVisible;
            passwordEntry.IsEnabled = true;
            messageLabel.Text = "";
            fpButton.Source = new FontImageSource
            {
                Glyph = FontAwesomeSolid.Fingerprint,
                FontFamily = "FontAwesomeSolid",
                Color = (Color)Application.Current.Resources["Primary"],
                Size = 32
            };
        }

        private async void InitFingerPrintButton()
        {
            // if device lock is enabled, but key file doesn't exist
            if (_viewModel.IsKeyFileNotExist)
            {
                Debug.WriteLine("LoginPage: SetupQRCode");
                SetToQrCode();
            }
            else
            {
                await _viewModel.CheckFingerprintStatus();
                bool isVisible = false;
                if (_viewModel.IsFingerprintEnabled)
                {
                    isVisible = true;
                }
                SetToFingerprint(isVisible);
            }
        }

        void HandleResult(BarcodeResult result)
        {
            var info = Properties.Resources.settings_security_DLK_Created_success;
            // Stop scanning
            // scanPage.IsScanning = false;
            bool updateUI = false;
            Debug.WriteLine("Processing key file data ...");

            if (result.Text.StartsWith(PxDefs.PxKeyFile))
            {
                string data = result.Text.Substring(PxDefs.PxKeyFile.Length);
                if (_viewModel.CreateKeyFile(data))
                {
                    updateUI = true;
                    Debug.WriteLine("Key file created.");
                }
                else
                {
                    info = Properties.Resources.settings_security_DLK_Created_failure;
                }

            }
            else { info = Properties.Resources.settings_security_DLK_Wrong_Format; }


            // Pop the page and show the result
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Navigation.PopAsync();
                Debug.WriteLine("Return to login page.");
                if (updateUI)
                {
                    SetToFingerprint(false);
                }

                await DisplayAlert(Properties.Resources.settings_security_scan_result, info, Properties.Resources.alert_id_ok);
            });
        }


        private async void ScanKeyFileQRCode()
        {
            var scanPage = new QrCodeScanPage();
            scanPage.OnScanResult += HandleResult;

            // Navigate to our scanner page
            await Navigation.PushAsync(scanPage);
        }

        private async void OnFingerprintClicked(object sender, EventArgs e) 
        {
            if (_viewModel.IsKeyFileNotExist)
            {
                // Import key file or scan QR code
                string[] templates = {
                    Properties.Resources.import_keyfile,
                    Properties.Resources.import_keyfile_scan
                };
                var template = await Shell.Current.DisplayActionSheet(Properties.Resources.import_message1, Properties.Resources.action_id_cancel, null, templates);
                if (template == Properties.Resources.import_keyfile)
                {
                    _viewModel.ImportKeyFile();
                }
                else if (template == Properties.Resources.import_keyfile_scan)
                {
                    ScanKeyFileQRCode();
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    SetToFingerprint(false);
                });
            }
            else
            {
                await _viewModel.FingerprintLogin();
                Debug.WriteLine("LoginPage: OnFingerprintClicked");
            }
        }
    }
}