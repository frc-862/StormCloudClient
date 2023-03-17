package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSOutcomeCallback
	extends crc64fddc838597f4fd38.OneSignalImplementation_JavaLaterProxy_1
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.OutcomeCallback
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onSuccess:(Lcom/onesignal/OSOutcomeEvent;)V:GetOnSuccess_Lcom_onesignal_OSOutcomeEvent_Handler:Com.OneSignal.Android.OneSignal/IOutcomeCallbackInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSOutcomeCallback, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSOutcomeCallback.class, __md_methods);
	}


	public OneSignalImplementation_OSOutcomeCallback ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSOutcomeCallback.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSOutcomeCallback, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onSuccess (com.onesignal.OSOutcomeEvent p0)
	{
		n_onSuccess (p0);
	}

	private native void n_onSuccess (com.onesignal.OSOutcomeEvent p0);

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
