package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSSMSSubscriptionObserver
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OSSMSSubscriptionObserver
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onSMSSubscriptionChanged:(Lcom/onesignal/OSSMSSubscriptionStateChanges;)V:GetOnSMSSubscriptionChanged_Lcom_onesignal_OSSMSSubscriptionStateChanges_Handler:Com.OneSignal.Android.IOSSMSSubscriptionObserverInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSSMSSubscriptionObserver, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSSMSSubscriptionObserver.class, __md_methods);
	}


	public OneSignalImplementation_OSSMSSubscriptionObserver ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSSMSSubscriptionObserver.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSSMSSubscriptionObserver, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onSMSSubscriptionChanged (com.onesignal.OSSMSSubscriptionStateChanges p0)
	{
		n_onSMSSubscriptionChanged (p0);
	}

	private native void n_onSMSSubscriptionChanged (com.onesignal.OSSMSSubscriptionStateChanges p0);

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
