package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSNotificationWillShowInForegroundHandler
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.OSNotificationWillShowInForegroundHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_notificationWillShowInForeground:(Lcom/onesignal/OSNotificationReceivedEvent;)V:GetNotificationWillShowInForeground_Lcom_onesignal_OSNotificationReceivedEvent_Handler:Com.OneSignal.Android.OneSignal/IOSNotificationWillShowInForegroundHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSNotificationWillShowInForegroundHandler, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSNotificationWillShowInForegroundHandler.class, __md_methods);
	}


	public OneSignalImplementation_OSNotificationWillShowInForegroundHandler ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSNotificationWillShowInForegroundHandler.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSNotificationWillShowInForegroundHandler, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void notificationWillShowInForeground (com.onesignal.OSNotificationReceivedEvent p0)
	{
		n_notificationWillShowInForeground (p0);
	}

	private native void n_notificationWillShowInForeground (com.onesignal.OSNotificationReceivedEvent p0);

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
