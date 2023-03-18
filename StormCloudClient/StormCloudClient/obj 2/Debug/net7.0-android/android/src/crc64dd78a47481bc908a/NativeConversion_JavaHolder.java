package crc64dd78a47481bc908a;


public class NativeConversion_JavaHolder
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"";
		mono.android.Runtime.register ("OneSignalSDK.DotNet.Android.Utilities.NativeConversion+JavaHolder, OneSignalSDK.DotNet.Android", NativeConversion_JavaHolder.class, __md_methods);
	}


	public NativeConversion_JavaHolder ()
	{
		super ();
		if (getClass () == NativeConversion_JavaHolder.class) {
			mono.android.TypeManager.Activate ("OneSignalSDK.DotNet.Android.Utilities.NativeConversion+JavaHolder, OneSignalSDK.DotNet.Android", "", this, new java.lang.Object[] {  });
		}
	}

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
