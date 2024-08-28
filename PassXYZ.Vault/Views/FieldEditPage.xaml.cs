using System.Diagnostics;

using Camera.MAUI;
using PureOtp;
using KPCLib;
using PassXYZLib;

namespace PassXYZ.Vault.Views
{
    public partial class FieldEditPage : ContentPage
    {
        private Action<string, string, bool> _updateAction;
        private readonly bool _isNewField = true;
        private Color? _checkboxColor = null;
        private Item? _dataEntry = null;

        public FieldEditPage(Action<string, string, bool> updateAction, string key = "", string value = "")
        {
            InitializeComponent();

            Title = keyField.Text = key;
            if (!string.IsNullOrEmpty(key))
            {
                keyField.IsVisible = false;
                // pwCheckBox.IsVisible = false;
                optionGroup.IsVisible = false;
                _isNewField = false;
            }

            valueField.Text = value;

            _updateAction = updateAction;
        }

        /// <summary>
        /// This one is used to add a field in ItemDetailViewModel.OnAddField().
        /// </summary>
        public FieldEditPage(Action<string, string, bool> updateAction, Item entry, string key = "", string value = "") : this(updateAction, key, value)
        {
            _dataEntry = entry;
        }

        /// <summary>
        /// This one is used to update an item. The item can be a group or an entry.
        /// </summary>
        public FieldEditPage(Action<string, string, bool> updateAction, string key, string value, bool isKeyVisible = false) : this(updateAction, key, value)
        {
            // This is the same as the another constructor, except this part
            if (isKeyVisible)
            {
                keyField.IsVisible = true;
            }
        }

        private bool isValidUrl(string rawUrl)
        {
            try
            {
                var otp = KeyUrl.FromUrl(rawUrl);
                if (otp is Totp totp)
                {
                    var url = new Uri(rawUrl);
                    return true;
                }
                else
                {
                    Debug.WriteLine($"{rawUrl} is an invalid URL.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex}");
                return false;
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            bool isProtected = pwCheckBox.IsChecked;

            if (otpCheckBox.IsChecked)
            {
                // If this is an OTP url, we need to valid it.
                if (!isValidUrl(valueField.Text))
                {
                    await DisplayAlert("", Properties.Resources.error_message_invalid_url, Properties.Resources.alert_id_ok);
                    return;
                }
            }

            if (_isNewField)
            {
                _updateAction?.Invoke(keyField.Text, valueField.Text, isProtected);
            }
            else
            {
                _updateAction?.Invoke(keyField.Text, valueField.Text, false);
            }
            _ = await Shell.Current.Navigation.PopAsync();
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            _ = await Shell.Current.Navigation.PopAsync();
        }

        void HandleResult(BarcodeResult result)
        {
            Debug.WriteLine($"FieldEditPage: {result.Text}");

            // Pop the page and show the result
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                _ = await Navigation.PopAsync();
                valueField.Text += result.Text;
            });
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            var scanPage = new QrCodeScanPage();
            scanPage.OnScanResult += HandleResult;

            await Navigation.PushAsync(scanPage);
        }

        private void OnOtpCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                pwCheckBox.IsEnabled = false;
                _checkboxColor = pwCheckBox.Color;
                pwCheckBox.Color = Color.FromRgba(211, 311, 211, 255);
                keyField.Text = PassXYZLib.PxDefs.PxCustomDataOtpUrl;
                keyField.IsEnabled = false;
                if(_dataEntry != null)
                {
                    if(_dataEntry is PxEntry pxEntry) 
                    {
                        valueField.Text = _dataEntry != null ? pxEntry.GetOtpUrl() : throw new ArgumentNullException("dataEntry");
                    }
                }
                Debug.WriteLine("OTP CheckBox is true.");
            }
            else
            {
                pwCheckBox.IsEnabled = true;
                pwCheckBox.Color = _checkboxColor;
                keyField.Text = "";
                keyField.IsEnabled = true;
                valueField.Text = "";
                Debug.WriteLine("OTP CheckBox is false.");
            }
        }

        private void OnPasswordCheckBoxChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                otpCheckBox.IsEnabled = false;
                _checkboxColor = otpCheckBox.Color;
                otpCheckBox.Color = Color.FromRgba(211, 311, 211, 255);
                Debug.WriteLine("Password CheckBox is true.");
            }
            else
            {
                otpCheckBox.IsEnabled = true;
                otpCheckBox.Color = _checkboxColor;
                Debug.WriteLine("Password CheckBox is false.");
            }
        }
    }
}