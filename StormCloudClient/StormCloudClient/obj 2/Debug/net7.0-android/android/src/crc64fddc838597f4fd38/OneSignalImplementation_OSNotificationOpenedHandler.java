package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSNotificationOpenedHandler
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.OSNotificationOpenedHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_notificationOpened:(Lcom/onesignal/OSNotificationOpenedResult;)V:GetNotificationOpened_Lcom_onesignal_OSNotificationOpenedResult_Handler:Com.OneSignal.Android.OneSignal/IOSNotificationOpenedHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSNotificationOpenedHandler, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSNotificationOpenedHandler.class, __md_methods);
	}


	public OneSignalImplementation_OSNotificationOpenedHandler ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSNotificationOpenedHandler.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSNotificationOpenedHandler, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void notificationOpened (com.onesignal.OSNotificationOpenedResult p0)
	{
		n_notificationOpened (p0);
	}

	private native void n_notificationOpened (com.onesignal.OSNotificationOpenedResult p0);

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
