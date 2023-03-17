using StormCloudClient.Classes;
using StormCloudClient.Services;
using Plugin.LocalNotification;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core;

namespace StormCloudClient;
using Plugin.LocalNotification;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core;

public partial class App : Application
{
	public static USBService USBService;
	public bool OR = false;
	public App()
	{
		InitializeComponent();
		
		APIManager.Initialize();

		
        StorageManagement.Initialize();
        OneSignal.Default.RequiresPrivacyConsent = true;
        OneSignal.Default.Initialize("87497f10-4ff9-4d08-8da1-191301bad90d");
        OneSignal.Default.PrivacyConsent = true;
        OneSignal.Default.PromptForPushNotificationsWithUserResponse(true);


        

#if IOS
		USBService = new USBService();
		USBService.StartService();
#else

#endif


        try
        {
            var deviceId = (string)DataManagement.GetValue("deviceId");
            if(deviceId == "" || deviceId == null)
            {
                DataManagement.SetValue("deviceId", DataManagement.GenerateRandomCharacters(8));
            }
        }catch(Exception ex)
        {
            DataManagement.SetValue("deviceId", DataManagement.GenerateRandomCharacters(8));
        }

        try
		{
			var isSetup = (string)DataManagement.GetValue("setup");
			if (isSetup == "YES" || OR)
			{
                MainPage = new NavigationPage(new MainPage());
            }
			else
			{
                MainPage = new NavigationPage(new Initializer());
            }
                
        }
        catch(Exception e)
		{
            MainPage = new NavigationPage(new Initializer());

            
            
        }

        MessagingCenter.Subscribe<NavigationPage, MessageContent>((NavigationPage)MainPage, "USB", (sender, mc) =>
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                ((NavigationPage)MainPage).DisplayAlert("USB Started", "We have started a USB transfer with a connected device...", "OK");
                PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
                await Task.Delay(400);
                PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
                await Task.Delay(400);
                PhysicalVibrations.TryVibrate(1000);
                MessagingCenter.Send<NavigationPage, MessageContent>((NavigationPage)MainPage, "REFRESH", new MessageContent()
                {
                    Data = "REFRESH",
                    Type = MessageType.REFRESH
                });
            });

        });

        //MainPage = new NavigationPage(new MainPage());

        OneSignal.Default.Initialize("e5b08ab2-484c-4e7f-ad43-3635832e137e");
        OneSignal.Default.PromptForPushNotificationsWithUserResponse();


    }

    protected override async void OnStart()
    {

        if(await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
        {
            // then enable them lol
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        base.OnStart();
    }
     protected override async void OnStart()
     {

         if(await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
         {
             // then enable them lol
             await LocalNotificationCenter.Current.RequestNotificationPermission();
         }

         base.OnStart();
     }
}
