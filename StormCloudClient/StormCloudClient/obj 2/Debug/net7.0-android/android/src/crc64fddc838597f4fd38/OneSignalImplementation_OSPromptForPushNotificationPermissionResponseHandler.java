package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSPromptForPushNotificationPermissionResponseHandler
	extends crc64fddc838597f4fd38.OneSignalImplementation_JavaLaterProxy_1
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.PromptForPushNotificationPermissionResponseHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_response:(Z)V:GetResponse_ZHandler:Com.OneSignal.Android.OneSignal/IPromptForPushNotificationPermissionResponseHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSPromptForPushNotificationPermissionResponseHandler, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSPromptForPushNotificationPermissionResponseHandler.class, __md_methods);
	}


	public OneSignalImplementation_OSPromptForPushNotificationPermissionResponseHandler ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSPromptForPushNotificationPermissionResponseHandler.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSPromptForPushNotificationPermissionResponseHandler, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void response (boolean p0)
	{
		n_response (p0);
	}

	private native void n_response (boolean p0);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
