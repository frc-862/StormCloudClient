namespace StormCloudUSB;

public partial class App : Application
{
	public static USBListener usblistener;
	public App()
	{
		InitializeComponent();

		usblistener = new USBListener();
		usblistener.BeginListening();

		MainPage = new MainPage();

	}
}
