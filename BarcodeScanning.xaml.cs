namespace MauiBarcodeApp;

public partial class BarcodeScanning : ContentPage
{
	private void cameraView_CamerasLoaded(object sender, EventArgs e)
	{
        if (cameraView.Cameras.Count > 0)
        {
            cameraView.Camera = cameraView.Cameras.First();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await cameraView.StopCameraAsync();
                await cameraView.StartCameraAsync();
            });
        }
    }

    private async void cameraView_BarcodeDetected(object sender, Camera.MAUI.ZXingHelper.BarcodeEventArgs args)
    {
        string scannedBarcode = args.Result[0].Text; // Get the scanned barcode from the event args

        Device.BeginInvokeOnMainThread(async () =>
        {
            await Navigation.PushAsync(new MainPage(scannedBarcode)); // This is a work around for an issue that I ran into
        });

        //Shell.Current.GoToAsync($"..?format={args.Result[0].BarcodeFormat}&barcode={args.Result[0].Text}");

    }

    public BarcodeScanning()
	{
		InitializeComponent();
	}
}