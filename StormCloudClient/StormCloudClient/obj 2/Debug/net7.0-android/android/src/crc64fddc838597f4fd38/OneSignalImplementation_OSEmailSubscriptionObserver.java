package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSEmailSubscriptionObserver
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OSEmailSubscriptionObserver
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onOSEmailSubscriptionChanged:(Lcom/onesignal/OSEmailSubscriptionStateChanges;)V:GetOnOSEmailSubscriptionChanged_Lcom_onesignal_OSEmailSubscriptionStateChanges_Handler:Com.OneSignal.Android.IOSEmailSubscriptionObserverInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSEmailSubscriptionObserver, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSEmailSubscriptionObserver.class, __md_methods);
	}


	public OneSignalImplementation_OSEmailSubscriptionObserver ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSEmailSubscriptionObserver.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSEmailSubscriptionObserver, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onOSEmailSubscriptionChanged (com.onesignal.OSEmailSubscriptionStateChanges p0)
	{
		n_onOSEmailSubscriptionChanged (p0);
	}

	private native void n_onOSEmailSubscriptionChanged (com.onesignal.OSEmailSubscriptionStateChanges p0);

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
