using Camera.MAUI;
using Camera.MAUI.ZXing;
using System.Diagnostics;

namespace PassXYZ.Vault.Views;

public partial class QrCodeScanPage : ContentPage
{
	public QrCodeScanPage()
	{
		InitializeComponent();
        cameraView.CamerasLoaded += CamerasLoaded;
        cameraView.BarcodeDetected += BarcodeDetected;
        cameraView.BarCodeDecoder = new ZXingBarcodeDecoder();
        cameraView.BarCodeOptions = new BarcodeDecodeOptions
        {
            AutoRotate = true,
            PossibleFormats = { BarcodeFormat.QR_CODE },
            ReadMultipleCodes = false,
            TryHarder = true,
            TryInverted = true
        };
        cameraView.BarCodeDetectionFrameRate = 10;
        cameraView.BarCodeDetectionMaxThreads = 5;
        cameraView.ControlBarcodeResultDuplicate = true;
        cameraView.BarCodeDetectionEnabled = true;
    }

    public delegate void ScanResultDelegate(BarcodeResult result);
    public event ScanResultDelegate OnScanResult;

    private void BarcodeDetected(object? sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        Debug.WriteLine("Barcode Detected");
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await cameraView.StopCameraAsync();
        });
        OnScanResult?.Invoke(args.Result[0]);
    }

    private void CamerasLoaded(object? sender, EventArgs e)
    {
        Debug.WriteLine("Start camera ...");
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                if(await cameraView.StartCameraAsync() == CameraResult.Success)
                {
                    cameraView.ForceAutoFocus();
                    Debug.WriteLine("Camera started.");
                }
                else
                {
                    Debug.WriteLine("Camera start failed.");
                }
            });
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Stop the Camera
        Debug.WriteLine("Stop camera.");
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await cameraView.StopCameraAsync();
        });
    }

}