namespace StormCloudClient;

public partial class Scouting : ContentPage
{
	public Scouting()
	{
		InitializeComponent();
	}
	bool _readyTransitionLock;
	private async void Status_MatchReady(object sender, EventArgs e)
	{
		if (_readyTransitionLock)
			return;
		_readyTransitionLock = true;
		Status_BottomBar.TranslateTo(0, 500, 500, Easing.CubicInOut);

		Status_PreConfirm.FadeTo(0, 250, Easing.CubicInOut);
		Status_PostConfirm.FadeTo(1, 250, Easing.CubicInOut);

		Status_PreContent.FadeTo(0, 250, Easing.CubicInOut);
		Status_PostContent.IsVisible = true;
	}
    private async void Status_ToggleBottomBar_Clicked(object sender, EventArgs e)
    {
		ChangeStatusExpansion(!statusExpanded);
    }
    bool statusExpanded;
	bool _statusOpenLock;
	public async void ChangeStatusExpansion(bool _expanded)
	{
		if (_statusOpenLock)
			return;
        _statusOpenLock = true;
        if (_expanded == statusExpanded)
		{
            if (statusExpanded)
            {
                await Status_BottomBar.TranslateTo(0, 340, 100, Easing.CubicInOut);
                await Status_BottomBar.TranslateTo(0, 350, 100, Easing.CubicInOut);
            }
            else
            {
                await Status_BottomBar.TranslateTo(0, 510, 100, Easing.CubicInOut);
                await Status_BottomBar.TranslateTo(0, 500, 100, Easing.CubicInOut);
            }
		}
		else
		{
            statusExpanded = _expanded;
            Status_PostContent.FadeTo(statusExpanded ? 1 : 0, 350, Easing.CubicInOut);
            // Navbar goes up/down :: 500ms (No await)
            Status_BottomBar.TranslateTo(0, (statusExpanded ? 350 : 500), 500, Easing.CubicInOut);

            // Button spins :: 250ms (No await)
            Status_ExtendBar.RotateTo(statusExpanded ? 180 : 0, 250, Easing.CubicInOut);

            // Await for longest anim -> 500ms
            await Task.Delay(500);
        }
		

		_statusOpenLock = false;
	}
}