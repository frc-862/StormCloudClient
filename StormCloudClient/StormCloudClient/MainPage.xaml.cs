using StormCloudClient.Services;

namespace StormCloudClient;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        currentMenu = View_Scout;


        // get initial settings data
        var _envCode = DataManagement.GetValue("environment_code");
        var _uploadMode = DataManagement.GetValue("upload_mode");
        var _authKey = DataManagement.GetValue("authentication_key");

        if(_envCode != null)
            Settings_EnvironmentCode.Text = _envCode.ToString();
        if (_uploadMode != null)
            Settings_UploadMode.SelectedIndex = (int)_uploadMode;
        if (_authKey != null)
            Settings_AuthenticationKey.Text = _authKey.ToString();

    }

    bool navExpanded;

    bool _navOpenLock;
    bool _navGoToLock;
    double _navBase;
    bool _first = true;
    StackLayout currentMenu;
    Button currentButtonMenu;
    private async void Nav_ToggleBottomBar_Clicked(object sender, EventArgs e)
    {

        ChangeNavBottomBarExpansion(!navExpanded);

    }

    private async void Nav_GoTo(object sender, EventArgs e)
    {

        Button clicker = (Button)sender as Button;

        var goTo = clicker.ClassId;

        if (goTo != currentMenu.ClassId)
        {
            clicker.BackgroundColor = Color.FromHex("#680991");
            if (currentButtonMenu != null)
                currentButtonMenu.BackgroundColor = Color.FromHex("#3A0E4D");

            currentButtonMenu = clicker;

            ChangeNavigation(goTo);
        }





    }

    public async void ChangeNavigation(string final)
    {
        if (_navGoToLock)
            return;
        _navGoToLock = true;
        StackLayout goToItem = (StackLayout)FindByName("View_" + final);


        if (goToItem.ClassId == currentMenu.ClassId)
            return;

        goToItem.TranslationX = -1000;
        goToItem.IsVisible = true;
        currentMenu.TranslateTo(1000, 0, 500, Easing.CubicInOut);
        goToItem.TranslateTo(0, 0, 500, Easing.CubicInOut);

        await Nav_DescriptorPanel.FadeTo(0, 150);
        Nav_Descriptor.Text = final;
        await Nav_DescriptorPanel.FadeTo(1, 150);

        ChangeNavBottomBarExpansion(false);

        await Task.Delay(200);
        currentMenu.IsVisible = false;

        currentMenu = goToItem;
        _navGoToLock = false;

    }

    private void Nav_SwipeBottomBar(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Up)
        {
            ChangeNavBottomBarExpansion(true);
        }
        else if (e.Direction == SwipeDirection.Down)
        {
            ChangeNavBottomBarExpansion(false);
        }
    }

    private async void ChangeNavBottomBarExpansion(bool _expanded)
    {
        if (_navOpenLock)
            return;
        // check if the navbar status is the same as where it wants to go to
        if (_expanded == navExpanded)
        {
            // already the same lol
            if (navExpanded)
            {
                await Nav_BottomBar.TranslateTo(0, 90, 100, Easing.CubicInOut);
                await Nav_BottomBar.TranslateTo(0, 100, 100, Easing.CubicInOut);
            }
            else
            {
                await Nav_BottomBar.TranslateTo(0, 260, 100, Easing.CubicInOut);
                await Nav_BottomBar.TranslateTo(0, 250, 100, Easing.CubicInOut);
            }
        }
        else
        {



            _navOpenLock = true;

            navExpanded = _expanded;
            Nav_Content.FadeTo(navExpanded ? 1 : 0, 350, Easing.CubicInOut);
            // Navbar goes up/down :: 500ms (No await)
            Nav_BottomBar.TranslateTo(0, (navExpanded ? 100 : 250), 500, Easing.CubicInOut);

            // Button spins :: 250ms (No await)
            Nav_ToggleBottomBar.RotateTo(navExpanded ? 180 : 0, 250, Easing.CubicInOut);

            // Await for longest anim -> 500ms
            await Task.Delay(500);


            _navOpenLock = false;
        }
    }
    private async void GoToMatch(object sender, EventArgs e)
    {
        // need to put in the "Match Details" soon
        Navigation.PushAsync(new Scouting());

    }

    private void Setting_Unfocused(object sender, FocusEventArgs e)
    {
        try
        {
            Entry setting = (Entry)sender as Entry;
            var setPreference = setting.ClassId;

            DataManagement.SetValue(setPreference, setting.Text);
            return;
        }
        catch(Exception ex)
        {

        }
        try
        {
            Picker setting = (Picker)sender as Picker;
            var setPreference = setting.ClassId;

            DataManagement.SetValue(setPreference, setting.SelectedIndex);
            return;
        }
        catch (Exception ex)
        {

        }
    }
}

