using Foundation;
using UIKit;
using Firebase.CloudMessaging;
using UserNotifications;

namespace StormCloudClient;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate
{
	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        
        var result = base.FinishedLaunching(application, launchOptions);
        var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
        UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) =>
        {
            
            if (granted && error == null)
            {
                this.InvokeOnMainThread(() =>
                {
                    UIApplication.SharedApplication.RegisterForRemoteNotifications();
                    this.InitFirebase();
                });
            }
        });

        return result;
    }

    private void InitFirebase()
    {
        //this.Log($"{nameof(this.InitFirebase)}");

        try
        {
            var options = new Firebase.Core.Options("1:836746207009:ios:6f0f33b286f39f5c7c6860", "836746207009");
            options.ApiKey = "AIzaSyBkCMS9ZyQsK3AbvJXi17VEW4KYReL5WAs";
            options.ProjectId = "stormcloud-22895";
            options.BundleId = "com.robosmrt.stormcloud";
            options.ClientId = "836746207009-vppdaknaqb8mb06uli3ku8ldtgk9p3dh.apps.googleusercontent.com";

            Firebase.Core.App.Configure(options);
        }
        catch (Exception x)
        {
            //this.Log("Firebase-configure Exception: " + x.Message);
        }

        UNUserNotificationCenter.Current.Delegate = this;

        if (Messaging.SharedInstance != null)
        {
            Messaging.SharedInstance.Delegate = this;
            Messaging.SharedInstance.Subscribe("queue");
            Messaging.SharedInstance.Subscribe("results");
            Messaging.SharedInstance.Subscribe("resultsAll");
            Messaging.SharedInstance.Subscribe("general");
            //this.Log("Messaging.SharedInstance SET");
        }
        else
        {
            //this.Log("Messaging.SharedInstance IS NULL");
        }
    }

    // indicates that a call to RegisterForRemoteNotifications() failed
    // see developer.apple.com/documentation/uikit/uiapplicationdelegate/1622962-application
    [Export("application:didFailToRegisterForRemoteNotificationsWithError:")]
    public void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
    {
        //this.Log($"{nameof(FailedToRegisterForRemoteNotifications)}: {error?.LocalizedDescription}");
    }

    // this callback is called at each app startup
    // it can be called two times:
    //   1. with old token
    //   2. with new token
    // this callback is called whenever a new token is generated during app run
    [Export("messaging:didReceiveRegistrationToken:")]
    public void DidReceiveRegistrationToken(Messaging messaging, string fcmToken)
    {
        //this.Log($"{nameof(DidReceiveRegistrationToken)} - Firebase token: {fcmToken}");

        //Utils.RefreshCloudMessagingToken(fcmToken);
    }

    // the message just arrived and will be presented to user
    [Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
    public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
    {
        var userInfo = notification.Request.Content.UserInfo;

        var match = userInfo["match"];
        Console.WriteLine(match);

        //var notificationPreferences = (string)DataManagement.GetValue("notifications");

        //if(notificationPreferences == "0" || notificationPreferences == null){
        //    completionHandler(UNNotificationPresentationOptions.None);
        //    return;
        //}


        //this.Log($"{nameof(WillPresentNotification)}: " + userInfo);

        // tell the system to display the notification in a standard way
        // or use None to say app handled the notification locally
        completionHandler(UNNotificationPresentationOptions.Alert);
    }

    // user clicked at presented notification
    [Export("userNotificationCenter:didReceiveNotificationResponse:withCompletionHandler:")]
    public void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
    {
        //this.Log($"{nameof(DidReceiveNotificationResponse)}: " + response.Notification.Request.Content.UserInfo);
        completionHandler();
    }
}
