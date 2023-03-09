package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSLanguageUpdateHandler
	extends crc64fddc838597f4fd38.OneSignalImplementation_JavaLaterProxy_1
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.OSSetLanguageCompletionHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFailure:(Lcom/onesignal/OneSignal$OSLanguageError;)V:GetOnFailure_Lcom_onesignal_OneSignal_OSLanguageError_Handler:Com.OneSignal.Android.OneSignal/IOSSetLanguageCompletionHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"n_onSuccess:(Ljava/lang/String;)V:GetOnSuccess_Ljava_lang_String_Handler:Com.OneSignal.Android.OneSignal/IOSSetLanguageCompletionHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSLanguageUpdateHandler, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSLanguageUpdateHandler.class, __md_methods);
	}


	public OneSignalImplementation_OSLanguageUpdateHandler ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSLanguageUpdateHandler.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSLanguageUpdateHandler, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onFailure (com.onesignal.OneSignal.OSLanguageError p0)
	{
		n_onFailure (p0);
	}

	private native void n_onFailure (com.onesignal.OneSignal.OSLanguageError p0);


	public void onSuccess (java.lang.String p0)
	{
		n_onSuccess (p0);
	}

	private native void n_onSuccess (java.lang.String p0);

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
