package crc64fddc838597f4fd38;


public class OneSignalImplementation_OSChangeTagsUpdateHandler
	extends crc64fddc838597f4fd38.OneSignalImplementation_JavaLaterProxy_1
	implements
		mono.android.IGCUserPeer,
		com.onesignal.OneSignal.ChangeTagsUpdateHandler
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFailure:(Lcom/onesignal/OneSignal$SendTagsError;)V:GetOnFailure_Lcom_onesignal_OneSignal_SendTagsError_Handler:Com.OneSignal.Android.OneSignal/IChangeTagsUpdateHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"n_onSuccess:(Lorg/json/JSONObject;)V:GetOnSuccess_Lorg_json_JSONObject_Handler:Com.OneSignal.Android.OneSignal/IChangeTagsUpdateHandlerInvoker, OneSignalSDK.DotNet.Android.Binding\n" +
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSChangeTagsUpdateHandler, OneSignalSDK.DotNet.Android", OneSignalImplementation_OSChangeTagsUpdateHandler.class, __md_methods);
	}


	public OneSignalImplementation_OSChangeTagsUpdateHandler ()
	{
		super ();
		if (getClass () == OneSignalImplementation_OSChangeTagsUpdateHandler.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.OneSignalImplementation+OSChangeTagsUpdateHandler, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}


	public void onFailure (com.onesignal.OneSignal.SendTagsError p0)
	{
		n_onFailure (p0);
	}

	private native void n_onFailure (com.onesignal.OneSignal.SendTagsError p0);


	public void onSuccess (org.json.JSONObject p0)
	{
		n_onSuccess (p0);
	}

	private native void n_onSuccess (org.json.JSONObject p0);

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
