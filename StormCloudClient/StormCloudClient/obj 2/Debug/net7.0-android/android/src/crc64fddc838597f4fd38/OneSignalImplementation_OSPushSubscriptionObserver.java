package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSPushSubscriptionObserver
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OSSubscriptionObserver
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onOSSubscriptionChanged:(Lcom/onesignal/OSSubscriptionStateChanges;)V:GetOnOSSubscriptionChanged_Lcom_onesignal_OSSubscriptionStateChanges_Handler:Com.OneSignal.Android.IOSSubscriptionObserverInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSPushSubscriptionObserver, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSPushSubscriptionObserver.class, __md_methods);
	}


	public OneSignalImplementation_OSPushSubscriptionObserver ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSPushSubscriptionObserver.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSPushSubscriptionObserver, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onOSSubscriptionChanged (com.onesignal.OSSubscriptionStateChanges p0)
	{
		n_onOSSubscriptionChanged (p0);
	}

	private native void n_onOSSubscriptionChanged (com.onesignal.OSSubscriptionStateChanges p0);

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
