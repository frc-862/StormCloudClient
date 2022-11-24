using StormCloudClient.Classes;
using StormCloudClient.Services;

namespace StormCloudClient;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		APIManager.Initialize();
        StorageManagement.Initialize();
        MainPage = new NavigationPage(new MainPage());
	}
}
