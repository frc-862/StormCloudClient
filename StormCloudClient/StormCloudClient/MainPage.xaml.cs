
using Microsoft.Maui.Platform;
using StormCloudClient.Classes;
using StormCloudClient.Services;
using System.ComponentModel;
using ZXing;

namespace StormCloudClient;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        currentMenu = View_Scout;

       
        
    }

    List<object> settingsComponents;
    protected override void OnAppearing()
    {


        base.OnAppearing();

        


        
        StorageManagement.AddData_Schema("Rapid React Test", "{'Name':'Rapid React Test','Parts':[{'Name':'Autonomous','Time':0,'Components':[{'Type':'Step','Name':'Cargo Low','Min':0,'Max':10},{'Type':'Check','Name':'Off the Tarmac','On':'Yes','Off':'No'},{'Type':'Select','Name':'Level','Options':['A','B','C']},{'Type':'Event','Name':'Balls Shot','Trigger':'Shot Now!','Max':30},{'Type':'Timer','Name':'Playing Defense'}]}]}");

        // get initial settings data


        UpdateSettings();
        ShowMatches();
        ShowPhotos();

        

    }

    private async void ClearSettingsConfig(object sender, EventArgs e)
    {
        DataManagement.SetValue("environment_code", "");
        DataManagement.SetValue("upload_mode", "");
        DataManagement.SetValue("authentication_key", "");
        DataManagement.SetValue("selected_schema", "");
        DataManagement.SetValue("server_address", "");
        DataManagement.SetValue("default_scouter", "");
        DataManagement.SetValue("setup", "");
        if(Navigation.NavigationStack.Where(n => n is Initializer).Count() > 0)
        {
            // already exists
            Navigation.PopAsync();
        }
        else
        {
            Navigation.PushAsync(new Initializer());
        }
        
    }
    public void UpdateSettings()
    {
        var _envCode = DataManagement.GetValue("environment_code");
        var _uploadMode = DataManagement.GetValue("upload_mode");
        var _authKey = DataManagement.GetValue("authentication_key");
        var _selectedSchema = DataManagement.GetValue("selected_schema");
        var _serverAddress = DataManagement.GetValue("server_address");
        var _defaultScouter = DataManagement.GetValue("default_scouter");

        var version = VersionTracking.Default.CurrentVersion.ToString();
        var build = VersionTracking.Default.CurrentBuild.ToString();

        Settings_VersionInfo.Text = "Version: " + version + ", Build: " + build;

        settingsComponents = new List<object>()
        {
            Settings_AuthenticationKey, Settings_EnvironmentCode, Settings_SelectedSchema, Settings_UploadMode, Settings_ServerAddress, Settings_DefaultScouter
        };

        if (_envCode != null)
            Settings_EnvironmentCode.Text = _envCode.ToString();
        if (_uploadMode != null)
            Settings_UploadMode.SelectedIndex = Int32.Parse(_uploadMode.ToString());
        if (_authKey != null)
            Settings_AuthenticationKey.Text = _authKey.ToString();
        if (_serverAddress != null)
            Settings_ServerAddress.Text = _serverAddress.ToString();
        if (_defaultScouter != null)
            Settings_DefaultScouter.Text = _defaultScouter.ToString();

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

        if(!swipe)
            PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);

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

            var id = m.Number.ToString() + ";" + m.Environment;
            Frame outside = new Frame() { BackgroundColor = bgColor, BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, MaximumWidthRequest = 400, Padding = new Thickness(0, 12), ClassId = id };
            
            Grid contentsInside = new Grid() { Margin = new Thickness(5, 0) };
            contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

            Label matchNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Match " + m.Number.ToString(), HorizontalTextAlignment = TextAlignment.Start, TextColor = Color.FromHex("#ffffff"), FontSize = 20, Margin = new Thickness(10, 0, 0, 0) };
            Label teamNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Team " + m.Team.ToString() + " - " + m.Color, HorizontalOptions = LayoutOptions.End, TextColor = Color.FromHex("#ffffff"), FontSize = 14, Margin = new Thickness(0,0,10,0) };

            contentsInside.Add(matchNum, 0, 0);
            contentsInside.Add(teamNum, 1, 0);

            outside.Content = contentsInside;

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += Match_FrameTapped;

            outside.GestureRecognizers.Add(tap);

            Data_Matches.Add(outside);
        }
    }

    public void ShowPhotos()
    {
        var photos = StorageManagement.allPhotos;
        photos.Sort((m1, m2) =>
        {
            return (int)((m1.Taken - m2.Taken).TotalSeconds);
        });

        Data_Photos.Clear();
        // assume 5 images each row
        Data_Photos.ColumnDefinitions = new ColumnDefinitionCollection()
        {
            new ColumnDefinition(){ Width = GridLength.Star},
            new ColumnDefinition(){ Width = GridLength.Star},
            new ColumnDefinition(){ Width = GridLength.Star},
            new ColumnDefinition(){ Width = GridLength.Star}
        };
        int rows = (photos.Count / 4) + 1;
        for(int i = 0; i < rows; i++)
        {
            Data_Photos.RowDefinitions.Add(new RowDefinition() { Height = 50 });
        }

        var number = 0;
        photos.Sort((p1, p2) =>
        {
            if (p1.Team == 0)
                return 1;
            if (p2.Team == 0)
                return -1;
            return p1.Team - p2.Team;

        });
        foreach(var photo in photos)
        {
            Color bgColor = Color.FromHex(photo.Status == UploadStatus.NOT_TRIED ? "#280338" : (photo.Status == UploadStatus.FAILED ? "#60051a" : "#3a0e4d"));

            var id = photo.Path;
            Frame outside = new Frame() { BackgroundColor = bgColor, ClassId = id, BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, WidthRequest = 80, HeightRequest = 50, Margin = new Thickness(10, 0), Padding = new Thickness(5,10) };
            Label teamNum = new Label() { Text = photo.Team == 0 ? "Unknown" : photo.Team.ToString(), FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.Tapped += Photo_FrameTapped;

            outside.Content = teamNum;
            outside.GestureRecognizers.Add(tap);
            Data_Photos.Add(outside, number % 4, number / 4);
            number += 1;
        }
    }
    private async void Photo_FrameTapped(object sender, EventArgs e)
    {
        Frame responsible = (Frame)sender as Frame;
        var path = responsible.ClassId;
        var photo = StorageManagement.allPhotos.Find(p => p.Path == path);

        PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
        var res = "";
        switch (photo.Status)
        {
            case UploadStatus.NOT_TRIED:
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString(), "Never Mind", null, "Submit", "Delete", "Edit Details");
                break;
            case UploadStatus.SUCCEEDED:
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString(), "Never Mind", null, "Resubmit", "Delete", "Edit Details");
                break;
            case UploadStatus.FAILED:
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString(), "Never Mind", null, "Retry Submit", "Delete", "Edit Details");
                break;
        }
        if (res == "Submit" || res == "Resubmit" || res == "Retry Submit")
        {
            await ChangeInfoView(true);
            Task.Run(async () =>
            {




                var response = await APIManager.SendPhotos(new List<Photo>() { photo });
                if (response[0].Status == System.Net.HttpStatusCode.OK)
                {
                    // data is good

                    var content = response[0].Content;

                    photo.Status = UploadStatus.SUCCEEDED;

                }
                else
                {

                    photo.Status = UploadStatus.FAILED;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                        ShowPhotos();
                    });
                }

                StorageManagement._SaveData_Photo();

                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(false);

                    ShowPhotos();

                });
            });


        }
        else if (res == "Delete")
        {
            StorageManagement.RemoveData_Photo(photo.Path);
            ShowPhotos();
        }
        else if(res == "Edit Details")
        {

            Overlay_Content.Clear();
            overlayInputs.Clear();
            Label teamLabel = new Label() { Text = "Team Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry team = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric, Text = photo.Team == 0 ? "" : photo.Team.ToString() };

            var formatString = "";
            if(photo.Matches != null)
            {
                foreach(int m in photo.Matches)
                {
                    formatString += m.ToString() + " ";
                }
                if(formatString.Length > 0)
                    formatString = formatString.Substring(0, formatString.Length - 1);
            }

            Label matchLabel = new Label() { Text = "Match Numbers", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry match = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = formatString };


            Label photoLabel = new Label() { Text = "Photo Type", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            Grid photoButtons = new Grid() { };
            photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

            Button paperButton = new Button() { BackgroundColor = Color.FromHex(photo.Type != "Paper" ? "#3a0e4d" : "#680991"), Text = "Paper", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

            Button otherButton = new Button() { BackgroundColor = Color.FromHex(photo.Type != "Other" ? "#3a0e4d" : "#680991"), Text = "Other", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

            paperButton.Clicked += (s, e) =>
            {
                photoButtons.ClassId = "Paper";
                paperButton.BackgroundColor = Color.FromHex("#680991");
                otherButton.BackgroundColor = Color.FromHex("#3a0e4d");
            };
            otherButton.Clicked += (s, e) =>
            {
                photoButtons.ClassId = "Other";
                otherButton.BackgroundColor = Color.FromHex("#680991");
                paperButton.BackgroundColor = Color.FromHex("#3a0e4d");
            };

            photoButtons.Add(paperButton, 0, 0);
            photoButtons.Add(otherButton, 1, 0);


            Overlay_Title.Text = "Editing Picture";
            Overlay_Content.Add(teamLabel);
            overlayInputs.Add(team);
            Overlay_Content.Add(team);
            Overlay_Content.Add(matchLabel);
            overlayInputs.Add(match);
            Overlay_Content.Add(match);
            Overlay_Content.Add(photoLabel);
            Overlay_Content.Add(photoButtons);
            ShowOverlay();
            overlayFinish = () =>
            {
                int teamNum = 0;

                try
                {
                    var elem = overlayInputs[0];
                    var team = elem.Text;

                    if (team == "")
                    {
                        teamNum = 0;
                    }
                    else
                    {
                        teamNum = Int32.Parse(team);
                    }
                }
                catch (Exception e)
                {

                }

                List<int> matchNums = new List<int>();
                try
                {
                    var matchText = overlayInputs[1].Text;
                    var delimiter = " ";
                    if (matchText != "")
                    {
                        if (matchText.Contains(";"))
                            delimiter = ";";
                        if (matchText.Contains(" "))
                            delimiter = " ";
                        if (matchText.Contains("-"))
                            delimiter = "-";

                        var matches = matchText.Split(delimiter);
                        foreach (string m in matches)
                        {
                            int res = 0;
                            if (Int32.TryParse(m, out res) && !matchNums.Contains(res))
                            {
                                matchNums.Add(res);
                            }
                        }
                    }
                }
                catch (Exception e)
                {

                }

                if (photoButtons.ClassId != "Paper" && photoButtons.ClassId != "Photo")
                {
                    return;
                }
                photo.Type = photoButtons.ClassId;
                photo.Team = teamNum;
                photo.Matches = matchNums;
                

                PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
                
                StorageManagement._SaveData_Photo();

                ShowPhotos();
            };
        }
    }
    private async void Match_FrameTapped(object sender, EventArgs e)
    {
        Frame responsible = (Frame)sender as Frame;
        var details = responsible.ClassId.Split(";");

        var match = StorageManagement.allMatches.Find(m => m.Environment == details[1] && m.Number == Int32.Parse(details[0]));
        var res = "";
        PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
        switch (match.Status)
        {
            case UploadStatus.NOT_TRIED:
                res = await DisplayActionSheet("Match " + match.Number.ToString(), "Never Mind", null, "Submit", "Delete", "Edit Details");
                break;
            case UploadStatus.SUCCEEDED:
                res = await DisplayActionSheet("Match " + match.Number.ToString(), "Never Mind", null, "Resubmit", "Delete", "Edit Details");
                break;
            case UploadStatus.FAILED:
                res = await DisplayActionSheet("Match " + match.Number.ToString(), "Never Mind", null, "Retry Submit", "Delete", "Edit Details");
                break;
        }

        if(res == "Submit" || res == "Resubmit" || res == "Retry Submit")
        {
            await ChangeInfoView(true);
            Task.Run(async () =>
            {
                



                var response = await APIManager.SendMatches(new List<Match>() { match });
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    // data is good

                    var content = response.Content;

                    match.Status = UploadStatus.SUCCEEDED;

                }
                else
                {

                    match.Status = UploadStatus.FAILED;
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                        ShowMatches();
                    });
                }

                StorageManagement._SaveData_Match();

                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(false);

                    ShowMatches();

                });
            });

            
        }
        else if(res == "Delete")
        {
            StorageManagement.RemoveData_Match(match.Number, match.Environment);
            ShowMatches();
        }
        else if(res == "Edit Details")
        {
            Overlay_Content.Clear();
            overlayInputs.Clear();
            Label matchNLabel = new Label() { Text = "Match Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry matchN = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric, Text = match.Number.ToString() };

            Label teamNLabel = new Label() { Text = "Team Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry teamN = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric, Text = match.Team.ToString() };

            Label scoutLabel = new Label() { Text = "Scout Name", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry scout = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = match.Scouter };

            Label colorLabel = new Label() { Text = "Team Color", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry color = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = match.Color };
            Overlay_Title.Text = "Editing Match";
            Overlay_Content.Add(matchNLabel);
            Overlay_Content.Add(matchN);
            Overlay_Content.Add(teamNLabel);
            Overlay_Content.Add(teamN);
            Overlay_Content.Add(colorLabel);
            Overlay_Content.Add(color);
            Overlay_Content.Add(scoutLabel);
            Overlay_Content.Add(scout);
            overlayInputs.Add(matchN);
            overlayInputs.Add(teamN);
            overlayInputs.Add(color);
            overlayInputs.Add(scout);

            
            ShowOverlay();
            overlayFinish = () =>
            {
                int teamNum = 0;

                try
                {
                    var elem = overlayInputs[1];
                    var team = elem.Text;

                    if (team == "")
                    {
                        teamNum = match.Team;
                    }
                    else
                    {
                        teamNum = Int32.Parse(team);
                    }
                }
                catch (Exception e)
                {
                    teamNum = match.Team;
                }

                int matchNum = 0;

                try
                {
                    var elem = overlayInputs[0];
                    var matchN = elem.Text;

                    if (matchN == "")
                    {
                        matchNum = match.Number;
                    }
                    else
                    {
                        matchNum = Int32.Parse(matchN);
                    }
                }
                catch (Exception e)
                {
                    matchNum = match.Number;
                }

                var elemC = overlayInputs[2];
                var color = elemC.Text;
                if (color.ToLower() == "red")
                    color = "Red";
                else if (color.ToLower() == "blue")
                    color = "Blue";
                else
                    color = match.Color;

                var scout = overlayInputs[3].Text;
                if (scout == "")
                    scout = match.Scouter;



                match.Number = matchNum;
                match.Team = teamNum;
                match.Scouter = scout;
                match.Color = color;
                match.Status = UploadStatus.NOT_TRIED;

                PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);

                StorageManagement._SaveData_Match();

                ShowMatches();
            };
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
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
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
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
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
        try
        {
            foreach (object comp in settingsComponents)
            {
                Setting_Unfocused(comp, null);
            }
        }catch(Exception e)
        {

        }
        
    }

    Action overlayFinish;
    List<StormEntry> overlayInputs = new List<StormEntry>();

    public async void ShowOverlay()
    {
        Overlay_Box.TranslationX = -1000;
        Overlay_Backdrop.Opacity = 0;

        Overlay.IsVisible = true;
        Overlay_Backdrop.FadeTo(.4, 400, Easing.CubicInOut);
        Overlay_Box.TranslateTo(0, 0, 400, Easing.CubicInOut);
    }

    public async void HideOverlay(bool success)
    {

        
        Overlay_Backdrop.FadeTo(0, 400, Easing.CubicInOut);
        await Overlay_Box.TranslateTo(success ? 1000 : -1000, 0, 400, Easing.CubicInOut);
        Overlay.IsVisible = false;

        if (success)
        {
            overlayFinish.Invoke();
            ShowPhotos();
        }
    }
    private async void CloseOverlay(object sender, EventArgs e)
    {
        HideOverlay(false);
    }

    private async void DoneOverlay(object sender, EventArgs e)
    {
        HideOverlay(true);
    }

    private async void Info_Back(object sender, EventArgs e)
    {
        ChangeInfoView(false);
    }

    public void DisplayPhotoEdit(FileResult photo)
    {
        Overlay_Content.Clear();
        overlayInputs.Clear();


        /*
         <Frame BorderColor="Transparent" Padding="5,0" Grid.Column="1" BackgroundColor="#3a0e4d" CornerRadius="8">
                            <Frame BorderColor="Transparent" Padding="10,0" BackgroundColor="#3a0e4d" CornerRadius="8" Margin="5">
                                <Frame BackgroundColor="Transparent" Padding="0" BorderColor="#3a0e4d" CornerRadius="4">
                                    <Entry x:Name="Status_PreContent_ScouterName" TextColor="White" BackgroundColor="#3a0e4d" FontSize="16" HorizontalTextAlignment="Center"/>
                                </Frame>

                            </Frame>
                        </Frame>
         */
        Label teamLabel = new Label() { Text = "Team Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
        StormEntry team = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric };

        Label matchLabel = new Label() { Text = "Match Numbers", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
        StormEntry match = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d") };

        Label photoLabel = new Label() { Text = "Photo Type", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
        Grid photoButtons = new Grid() { };
        photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
        photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

        Button paperButton = new Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = "Paper", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10,0) };
        
        Button otherButton = new Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = "Other", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

        paperButton.Clicked += (s, e) =>
        {
            photoButtons.ClassId = "Paper";
            paperButton.BackgroundColor = Color.FromHex("#680991");
            otherButton.BackgroundColor = Color.FromHex("#3a0e4d");
        };
        otherButton.Clicked += (s, e) =>
        {
            photoButtons.ClassId = "Other";
            otherButton.BackgroundColor = Color.FromHex("#680991");
            paperButton.BackgroundColor = Color.FromHex("#3a0e4d");
        };

        photoButtons.Add(paperButton, 0, 0);
        photoButtons.Add(otherButton, 1, 0);

        Overlay_Title.Text = "Adding Picture";
        Overlay_Content.Add(teamLabel);
        overlayInputs.Add(team);
        Overlay_Content.Add(team);
        Overlay_Content.Add(matchLabel);
        overlayInputs.Add(match);
        Overlay_Content.Add(match);
        Overlay_Content.Add(photoLabel);
        Overlay_Content.Add(photoButtons);

        overlayFinish = () =>
        {
            int teamNum = 0;

            try
            {
                var elem = overlayInputs[0];
                var team = elem.Text;

                if (team == "")
                {
                    teamNum = 0;
                }
                else
                {
                    teamNum = Int32.Parse(team);
                }
            }
            catch (Exception e)
            {

            }

            List<int> matchNums = new List<int>();
            try
            {
                var matchText = overlayInputs[1].Text;
                var delimiter = " ";
                if (matchText != "")
                {
                    if (matchText.Contains(";"))
                        delimiter = ";";
                    if (matchText.Contains(" "))
                        delimiter = " ";
                    if (matchText.Contains("-"))
                        delimiter = "-";

                    var matches = matchText.Split(delimiter);
                    foreach (string m in matches)
                    {
                        int res = 0;
                        if (Int32.TryParse(m, out res) && !matchNums.Contains(res))
                        {
                            matchNums.Add(res);
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }

            if(photoButtons.ClassId != "Paper" && photoButtons.ClassId != "Photo")
            {
                return;
            }



            
            PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
            StorageManagement.AddData_Photo(photo, teamNum, matchNums, photoButtons.ClassId);

            Device.StartTimer(TimeSpan.FromSeconds(2), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowPhotos();
                });
                return false;
            });

            
        };

        ShowOverlay();
    }

    private async void Data_RequestTakePhoto(object sender, EventArgs e)
    {
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        if (MediaPicker.Default.IsCaptureSupported)
        {
            FileResult photo = await MediaPicker.Default.CapturePhotoAsync();
            
            if (photo != null)
            {

                DisplayPhotoEdit(photo);





                
            }
        }
        else
        {
            DisplayAlert("Oh no!", "Your device doesn't seem to have a camera that we are able to use. That's OK though, you can still upload a photo...", "Sure");
        }
    }

    private async void Data_RequestFilePick(object sender, EventArgs e)
    {
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        FileResult selected = await MediaPicker.Default.PickPhotoAsync();
        if(selected != null)
        {
            DisplayPhotoEdit(selected);
        }
    }

    private async void Data_StartQRScan(object sender, EventArgs e)
    {

#if IOS
    
#elif ANDROID
    var scanner = new ZXing.Mobile.MobileBarcodeScanner();

        var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
        options.UseNativeScanning = true;
        options.TryHarder = true;

        var result = await scanner.Scan(options);

        if (result != null)
            Console.WriteLine("Scanned Barcode: " + result.Text);


#endif

       




    }


    private async void Data_StartSubmitPaper(object sender, EventArgs e)
    {
        var webServer = DataManagement.GetValue("server_address");
        if (webServer == null)
        {
            return;
        }


        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);



        await ChangeInfoView(true);

        Task.Run(async () =>
        {
            var photosToSubmit = StorageManagement.allPhotos.Where(m => m.Status == UploadStatus.FAILED || m.Status == UploadStatus.NOT_TRIED);
            if (photosToSubmit.Count() == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(false);

                });
                return;
            }



            var responses = await APIManager.SendPhotos(photosToSubmit.ToList());
            foreach(var response in responses)
            {
                var photo = photosToSubmit.First(p => p.Path == response.About);
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    photo.Status = UploadStatus.SUCCEEDED;

                }
                else
                {
                    photo.Status = UploadStatus.FAILED;
                }

            }

            StorageManagement._SaveData_Photo();

            Device.BeginInvokeOnMainThread(() =>
            {
                ChangeInfoView(false);
                ShowPhotos();
                

            });
        });



    }

    private async void Data_StartSubmitMatches(object sender, EventArgs e)
    {
        var webServer = DataManagement.GetValue("server_address");
        if (webServer == null)
        {
            return;
        }


        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);



        await ChangeInfoView(true);

        Task.Run(async () =>
        {
            var matchesToSubmit = StorageManagement.allMatches.Where(m => m.Status == UploadStatus.FAILED || m.Status == UploadStatus.NOT_TRIED);
            if (matchesToSubmit.Count() == 0) 
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(false);

                });
                return;
            }
                


            var response = await APIManager.SendMatches(matchesToSubmit.ToList());
            if (response.Status == System.Net.HttpStatusCode.OK)
            {
                // data is good

                var content = response.Content;

                foreach(Match m in matchesToSubmit)
                {
                    m.Status = UploadStatus.SUCCEEDED;
                }

            }
            else
            {

                foreach (Match m in matchesToSubmit)
                {
                    m.Status = UploadStatus.FAILED;
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                    ShowMatches();
                });
            }

            StorageManagement._SaveData_Match();

            Device.BeginInvokeOnMainThread(() =>
            {
                ChangeInfoView(false);

                ShowMatches();

            });
        });



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

