using StormCloudClient.Services;
using StormCloudClient.Classes;

namespace StormCloudClient;

public partial class Initializer : ContentPage
{
	string ScreenName;
	StackLayout Screen;

	public Initializer()
	{
		InitializeComponent();
		Screen = Screen_Start;
		Screen_Start.IsVisible = true;
	}

	bool screenLock;
	public async Task<bool> GoToScreen(string toScreen)
	{
		if(!screenLock && ScreenName != toScreen)
		{
			screenLock = true;
			ScreenName = toScreen;
			StackLayout goTo = (StackLayout)this.FindByName("Screen_" + toScreen);

			// hide prev screen
			goTo.Opacity = 0;
			await Screen.FadeTo(0, 500, Easing.CubicInOut);
			Screen.IsVisible = false;
			goTo.IsVisible = true;
			await goTo.FadeTo(1, 500, Easing.CubicInOut);
			Screen = goTo;
			screenLock = false;
			return true;
		}
		return false;
	}

    private async void Start_to_Manual(object sender, TappedEventArgs e)
    {
		Frame s = (Frame)sender as Frame;
        Screen_Manual_Server.IsEnabled = true;
        s.BackgroundColor = Color.FromHex("#680991");
        Back.Opacity = 0;
        await GoToScreen("Manual");
        s.BackgroundColor = Color.FromHex("#280338");
		
		Back.IsVisible = true;
        await Back.FadeTo(1, 500, Easing.CubicInOut);
        
    }

    private async void Start_to_NFC(object sender, TappedEventArgs e)
    {

    }

    private async void Start_to_QR(object sender, TappedEventArgs e)
    {
#if IOS
    DisplayAlert("Oops", "QR Code Scanning currently isn't supported on this platform...", "OK");
#elif ANDROID
        var scanner = new ZXing.Mobile.MobileBarcodeScanner();

        var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
        options.UseNativeScanning = true;
        options.TryHarder = true;

        var result = await scanner.Scan(options);

        if (result != null)
        {
            try
            {
                bool success = false;

                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.Text);

                switch ((string)obj.type)
                {
                    case "config":
                        string server = (string)obj.serverAddress;
                        DataManagement.SetValue("server_address", server);
                        Screen_Manual_Server.Text = server;
                        Screen_Manual_Server.IsEnabled = false;
                        success = true;
                        break;

                    default:
                        DisplayAlert("Oops", "That QR Code isn't accepted here...", "OK");
                        break;

                }

                if (success)
                {
                    Frame s = (Frame)sender as Frame;
                    s.BackgroundColor = Color.FromHex("#680991");
                    Back.Opacity = 0;
                    await GoToScreen("Manual");
                    s.BackgroundColor = Color.FromHex("#280338");

                    Back.IsVisible = true;
                    await Back.FadeTo(1, 500, Easing.CubicInOut);
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("Oops", "That QR Code isn't accepted here...", "OK");
            }


        }




#endif
    }

    private async void LinkTestingTested(string area, bool success)
	{
		Frame b = (Frame)this.FindByName("Screen_LinkTesting_" + area);
		b.BackgroundColor = Color.FromHex(success ? "#08503b" : "#910929");

		if (!success)
		{
            Screen_LinkTesting_Next.IsVisible = true;
            Screen_LinkTesting_Loading.IsVisible = false;
            string message = "";
			switch (area)
			{
				case "Internet":
					message = "It looks like we cannot access the internet... To automatically get data, please connect this device to the internet and try again. Would you like to go back and try again?";
					break;
				case "Link":
					message = "It looks like you provided the wrong link to the server... Would you like to edit the link and try again?";
					break;
				case "API":
					message = "It looks like the API is not setup correctly, or you didn't enter in the correct base link. Would you like to edit the link and try again?";
					break;
			}

			bool goBack = await DisplayAlert("Oops!", message, "Yes", "No");
			if (goBack)
			{
                await GoToScreen("Manual");
				return;
            }
			

		}
		else if(area == "API")
		{
            Screen_LinkTesting_Next.IsVisible = true;
            Screen_LinkTesting_Loading.IsVisible = false;
            bool import = await DisplayAlert("Success!", "Would you like to initialize your application further by importing the configuration data from the server you provided?", "Yes", "No");
            if (import)
            {
                var content = savedSetupData.Content;

                dynamic contentObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var selectedSchema = contentObject["settings"]["selectedSchema"];
                dynamic schemaObject = contentObject["schema"];
                StorageManagement.AddData_Schema((string)schemaObject["Name"], Newtonsoft.Json.JsonConvert.SerializeObject(schemaObject), (dynamic)schemaObject["Settings"]);

            }
        }
	}
	private APIResponse savedSetupData;
	private async void Manual_to_LinkTesting(object sender, EventArgs e)
	{

		var option = Screen_Manual_Upload.SelectedIndex;
		var apiLink = Screen_Manual_Server.Text;
		var scouter = Screen_Manual_Scouter.Text;
		var environment = Screen_Manual_Environment.Text;
		if(option == -1 || apiLink == "" || environment == "")
		{
			return;
            
        }
        DataManagement.SetValue("upload_mode", option.ToString());
        DataManagement.SetValue("server_address", apiLink);
        DataManagement.SetValue("default_scouter", scouter);
        DataManagement.SetValue("environment_code", environment);


        await GoToScreen("LinkTesting");
		Task.Run(async () =>
		{
			bool internet = await APIManager.ConnectivityTest("https://google.com");
			Device.BeginInvokeOnMainThread(() =>
			{
				LinkTestingTested("Internet", internet);
			});
			if (!internet)
				return;

            bool server = await APIManager.ConnectivityTest("https://" + apiLink);
            Device.BeginInvokeOnMainThread(() =>
            {
                LinkTestingTested("Link", server);
            });
            if (!server)
                return;

            var apiTestResult = await APIManager.GetSetupData();
			savedSetupData = apiTestResult;
            Device.BeginInvokeOnMainThread(() =>
            {
                LinkTestingTested("API", apiTestResult.Status == System.Net.HttpStatusCode.OK);
            });
            if (apiTestResult.Status != System.Net.HttpStatusCode.OK)
                return;
        });
    }

	private async void Any_to_Finish(object sender, EventArgs e)
	{

		DataManagement.SetValue("setup", "YES");
        await GoToScreen("Finish");
    }
    private async void JustTesting(object sender, TappedEventArgs e)
    {

		Back.Opacity = 0;
        await GoToScreen("Finish");
        Back.IsVisible = true;
        await Back.FadeTo(1, 500, Easing.CubicInOut);
    }

    private async void Any_to_Start(object sender, TappedEventArgs e)
    {
        GoToScreen("Start");
		await Back.FadeTo(0, 500, Easing.CubicInOut);
        Back.IsVisible = false;
        

    }

    private async void Finalize(object sender, EventArgs e)
    {
		Navigation.PushAsync(new MainPage());
    }
}