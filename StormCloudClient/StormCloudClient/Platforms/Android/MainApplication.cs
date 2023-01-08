using Android.App;
using Android.Runtime;
using ZXing.Mobile;

namespace StormCloudClient;

[Application]
public class MainApplication : MauiApplication
{
	public MainApplication(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
        
    }

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
