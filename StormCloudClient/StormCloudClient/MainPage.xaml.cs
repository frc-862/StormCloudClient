
using StormCloudClient.Classes;
using StormCloudClient.Services;

namespace StormCloudClient;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    List<object> settingsComponents;
    protected override void OnAppearing()
    {


        base.OnAppearing();

        currentMenu = View_Scout;


        
        StorageManagement.AddData_Schema("Rapid React Test", "{'Name':'Rapid React Test','Parts':[{'Name':'Autonomous','Time':0,'Components':[{'Type':'Step','Name':'Cargo Low','Min':0,'Max':10},{'Type':'Check','Name':'Off the Tarmac','On':'Yes','Off':'No'},{'Type':'Select','Name':'Level','Options':['A','B','C']},{'Type':'Event','Name':'Balls Shot','Trigger':'Shot Now!','Max':30},{'Type':'Timer','Name':'Playing Defense'}]}]}");

        // get initial settings data


        UpdateSettings();
        ShowMatches();

        

    }

    public void UpdateSettings()
    {
        var _envCode = DataManagement.GetValue("environment_code");
        var _uploadMode = DataManagement.GetValue("upload_mode");
        var _authKey = DataManagement.GetValue("authentication_key");
        var _selectedSchema = DataManagement.GetValue("selected_schema");
        var _serverAddress = DataManagement.GetValue("server_address");

        var version = VersionTracking.Default.CurrentVersion.ToString();
        var build = VersionTracking.Default.CurrentBuild.ToString();

        Settings_VersionInfo.Text = "Version: " + version + ", Build: " + build;

        settingsComponents = new List<object>()
        {
            Settings_AuthenticationKey, Settings_EnvironmentCode, Settings_SelectedSchema, Settings_UploadMode
        };

        if (_envCode != null)
            Settings_EnvironmentCode.Text = _envCode.ToString();
        if (_uploadMode != null)
            Settings_UploadMode.SelectedIndex = Int32.Parse(_uploadMode.ToString());
        if (_authKey != null)
            Settings_AuthenticationKey.Text = _authKey.ToString();
        if (_serverAddress != null)
            Settings_ServerAddress.Text = _serverAddress.ToString();

        List<string> schemaNames = new List<string>();
        foreach (Schema s in StorageManagement.allSchemas)
        {
            schemaNames.Add(s.Name);
        }
        Settings_SelectedSchema.ItemsSource = schemaNames;

        if (_selectedSchema != null)
        {

            if (!schemaNames.Contains(_selectedSchema.ToString()))
            {
                DataManagement.SetValue("selected_schema", "");
                return;
            }
            Settings_SelectedSchema.SelectedIndex = schemaNames.IndexOf(_selectedSchema.ToString());
        }
    }

    private void CameraBox_Loaded(object sender, EventArgs e)
    {
        Console.WriteLine("Hi");
    }

    bool navExpanded;

    bool _navOpenLock;
    bool _navGoToLock;
    double _navBase;
    bool _first = true;
    ScrollView currentMenu;
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

            ChangeNavigation(goTo, false);
        }





    }
    

    public async void ChangeNavigation(string final, bool swipe)
    {
        if (_navGoToLock)
            return;
        _navGoToLock = true;
        ScrollView goToItem = (ScrollView)FindByName("View_" + final);


        if (goToItem.ClassId == currentMenu.ClassId)
            return;
        if (goToItem.ClassId == "Settings")
            SaveSettings();

        goToItem.TranslationX = -1000;
        goToItem.IsVisible = true;
        currentMenu.TranslateTo(1000, 0, 500, Easing.CubicInOut);
        goToItem.TranslateTo(0, 0, 500, Easing.CubicInOut);

        await Nav_DescriptorPanel.FadeTo(0, 150);
        Nav_Descriptor.Text = final;
        await Nav_DescriptorPanel.FadeTo(1, 150);

        if(!swipe)
            ChangeNavBottomBarExpansion(false);

        await Task.Delay(200);
        currentMenu.IsVisible = false;

        currentMenu = goToItem;
        _navGoToLock = false;

    }

    

    public void ShowMatches()
    {
        var matches = StorageManagement.allMatches;
        matches.Sort((m1, m2) =>
            m1.Number - m2.Number
        );

        Data_Matches.Children.Clear();
        foreach(Match m in matches)
        {
            Color bgColor = Color.FromHex(m.Status == UploadStatus.NOT_TRIED ? "#280338" : (m.Status == UploadStatus.FAILED ? "#60051a" : "#3a0e4d"));


            Frame outside = new Frame() { BackgroundColor = bgColor, BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, MaximumWidthRequest = 400, Padding = new Thickness(0, 12) };
            Grid contentsInside = new Grid() { Margin = new Thickness(5, 0) };
            contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

            Label matchNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Match " + m.Number.ToString(), HorizontalTextAlignment = TextAlignment.Start, TextColor = Color.FromHex("#ffffff"), FontSize = 20, Margin = new Thickness(10, 0, 0, 0) };
            Label teamNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Team " + m.Team.ToString() + " - " + m.Color, HorizontalOptions = LayoutOptions.Center, TextColor = Color.FromHex("#ffffff"), FontSize = 14, Margin = new Thickness(0,0,10,0) };

            contentsInside.Add(matchNum, 0, 0);
            contentsInside.Add(teamNum, 1, 0);

            outside.Content = contentsInside;

            Data_Matches.Add(outside);
        }
    }


    string[] _menuPages = { "Scout", "Data", "Settings" };

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
        else if(e.Direction == SwipeDirection.Left){
            // change to next page

            var currentI = Array.IndexOf(_menuPages,currentMenu.ClassId);
            currentI += 1;

            if(currentI == _menuPages.Length)
            {
                currentI = 0;
            }

            ChangeNavigation(_menuPages[currentI], true);

        }
        else if (e.Direction == SwipeDirection.Right)
        {
            // change to next page

            var currentI = Array.IndexOf(_menuPages, currentMenu.ClassId);
            currentI -= 1;

            if (currentI < 0)
            {
                currentI = _menuPages.Length - 1;
            }

            ChangeNavigation(_menuPages[currentI], true);

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
        var selectedSchema = DataManagement.GetValue("selected_schema");
        if (selectedSchema == null || selectedSchema.ToString() == "")
            return;

        var _envCode = DataManagement.GetValue("environment_code");
        if (_envCode == null)
            _envCode = "";
        Navigation.PushAsync(new Scouting(selectedSchema.ToString(), (string)_envCode, this));

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

            
            if(setting.ClassId == "selected_schema")
            {
                DataManagement.SetValue(setPreference, (string)setting.SelectedItem);
                return;
            }

            DataManagement.SetValue(setPreference, setting.SelectedIndex.ToString());
            return;
        }
        catch (Exception ex)
        {

        }
    }
    public void SaveSettings()
    {
        foreach(object comp in settingsComponents)
        {
            Setting_Unfocused(comp, null);
        }
    }

    private async void Info_Back(object sender, EventArgs e)
    {
        ChangeInfoView(false);
    }

    private async void Data_StartQRScan(object sender, EventArgs e)
    {
        
        await ChangeInfoView(true);
    }
    private async void Data_StartDocumentScan(object sender, EventArgs e)
    {
        var webServer = DataManagement.GetValue("server_address");
        if(webServer == null)
        {
            return;
        }



        


        await ChangeInfoView(true);

        Task.Run(async () =>
        {
            var response = await APIManager.GetSetupData();
            if(response.Status == System.Net.HttpStatusCode.OK)
            {
                // data is good

                var content = response.Content;

                dynamic contentObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var selectedSchema = contentObject["settings"]["selectedSchema"];
                dynamic schemaObject = contentObject["schema"];
                StorageManagement.AddData_Schema((string)schemaObject["Name"], Newtonsoft.Json.JsonConvert.SerializeObject(schemaObject));



                DataManagement.SetValue("selected_schema", (string)selectedSchema);

            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                });
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                ChangeInfoView(false);

                UpdateSettings();

            });
        });
        

        
    }


    bool _infoViewEnabled;
    public async Task<bool> ChangeInfoView(bool newState)
    {
        if (newState == _infoViewEnabled)
            return false;

        _infoViewEnabled = newState;
        if (_infoViewEnabled)
        {
            InfoView.IsVisible = true;
            await InfoView.TranslateTo(0, 0, 500, Easing.CubicInOut);
            //CameraBox.IsEnabled = true;
            //CameraBox.IsDetecting = true;
            //CameraBox.CameraLocation = ZXing.Net.Maui.CameraLocation.Rear;

        }
        else
        {
            await InfoView.TranslateTo(1000, 0, 500, Easing.CubicInOut);
            //CameraView.IsVisible = false;
            //CameraBox.IsEnabled = false;
            //CameraBox.IsDetecting = false;
        }
        return true;
    }

}

