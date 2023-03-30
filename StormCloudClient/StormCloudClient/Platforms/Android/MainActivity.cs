using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase.Messaging;

namespace StormCloudClient;

[Activity(Theme = "@style/Maui.SplashTheme", WindowSoftInputMode = Android.Views.SoftInput.AdjustNothing, MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
    {
        ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
        base.OnCreate(savedInstanceState, persistentState);

        
        
    }
}
