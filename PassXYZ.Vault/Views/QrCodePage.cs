using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Camera.MAUI;
using Camera.MAUI.ZXing;
using PassXYZ.Vault.Properties;

namespace PassXYZ.Vault.Views;

public class QrCodePage : ContentPage
{
    public QrCodePage(string msg, string name)
    {
        var layout = new StackLayout
        {
            Spacing = 10,
            Padding = new Thickness(10, 20, 0, 0)
        };
        BarcodeImage barcode = new()
        {
            Aspect = Aspect.AspectFit,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            HeightRequest = 300,
            WidthRequest = 300,
            BarcodeMargin = 5,
            BarcodeFormat = BarcodeFormat.QR_CODE,
            BarcodeEncoder = new ZXingBarcodeEncoder(),
            Barcode = msg
        };

        var qrcodeTitle = new Label()
        {
            Text = Properties.Resources.action_id_generateqrcode + ": " + name,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold
        };

        var button = new Button
        {
            Text = Properties.Resources.alert_id_ok,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
            Padding = new Thickness(20, 10, 20, 10)
        };
        button.Clicked += async (sender1, e1) => { await Navigation.PopModalAsync(); };

        layout.Children.Add(qrcodeTitle);
        layout.Children.Add(barcode);
        layout.Children.Add(button);
        Content = layout;
    }
}
