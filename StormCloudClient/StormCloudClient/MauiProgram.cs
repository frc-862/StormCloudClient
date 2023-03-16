

namespace StormCloudClient;
using Plugin.LocalNotification;


public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("Rubix.ttf", "Inter");
			}).ConfigureEssentials(essentials =>
			{
				essentials.UseVersionTracking();
			})
			.UseLocalNotification()
        ;

        return builder.Build();
	}
}
