using ObjCRuntime;
using UIKit;

namespace StormCloudClient;

public class Program
{
	// This is the main entry point of the application.
	static void Main(string[] args)
	{
        // if you want to use a different Application Delegate class from "AppDelegate"
        // you can specify it here.
        ObjCRuntime.Class.ThrowOnInitFailure = false;
        UIApplication.Main(args, null, typeof(AppDelegate));
	}
}
