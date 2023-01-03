namespace StormCloudUSB;

public partial class MainPage : ContentPage
{
	int count = 0;

	public MainPage()
	{

		MessagingCenter.Subscribe<MainPage, string>(this, "iOS_USB", (sender, data) =>
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				DisplayAlert("USB Got", data, "Close");
			});
		});
        MessagingCenter.Subscribe<MainPage, string>(this, "iOS_USB_CONNECT", (sender, data) =>
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if(data == "CONNECT")
				{
					iOS_Device.BackgroundColor = Color.FromHex("#280338");
					iOS_Device_Connected.Text = "Connected";

				}
				else if(data == "SOCKET_CONNECT")
				{
                    iOS_Device.BackgroundColor = Color.FromHex("#680991");
                    iOS_Device_Connected.Text = "Socket Connect...";
                }
				else
				{
                    iOS_Device.BackgroundColor = Color.FromHex("#280338");
                    iOS_Device_Connected.Text = "Not Connected";
                }
			});
        });
        InitializeComponent();

	}
}

