
using Microsoft.Maui.Platform;
using Plugin.LocalNotification;
using StormCloudClient.Classes;
using StormCloudClient.Services;
using System.ComponentModel;
using ZXing;
using ZXing.Mobile;
using OneSignalSDK.DotNet;
using OneSignalSDK.DotNet.Core;
using AlohaKit.Controls;
using AlohaKit.Models;
using System.Collections.ObjectModel;

namespace StormCloudClient;

public partial class MainPage : ContentPage
{
    public enum InfoViewStatus
    {
        OPEN,
        SUCCESS,
        FAIL
    }

    public void UpdateStateInformationFields()
    {
        try
        {

            Data_CompetitionName.Text = StorageManagement.compCache.Name;

            Data_CompetitionLocation.Text = "@ "+StorageManagement.compCache.Location;
            if (StorageManagement.compCache.NextMatch > 900)
            {
                Data_NextMatch.Text = "Playoff " + (StorageManagement.compCache.NextMatch - 900).ToString();
            }
            else
            {
                Data_NextMatch.Text = StorageManagement.compCache.NextMatch > 0 ? "Match " + StorageManagement.compCache.NextMatch : "--";
            }
            if (StorageManagement.compCache.OurNextMatch["matchNumber"] != null && int.Parse(StorageManagement.compCache.OurNextMatch["matchNumber"].ToString()) > 900)
            {
                Data_OurNextMatch.Text = "Playoff " + (int.Parse(StorageManagement.compCache.OurNextMatch["matchNumber"].ToString()) - 900).ToString();
            }
            else if (StorageManagement.compCache.OurNextMatch["matchNumber"] == null)
            {
                Data_OurNextMatch.Text = "--";
            }
            else
            {
                Data_OurNextMatch.Text = int.Parse(StorageManagement.compCache.OurNextMatch["matchNumber"].ToString()) > 0 ? "Match " + StorageManagement.compCache.OurNextMatch["matchNumber"].ToString() : "--";
            }

            var ourColor = "";
            foreach(var team in StorageManagement.compCache.OurNextMatch["teams"])
            {
                if(team.team.ToString() == StorageManagement.compCache.TeamNumber.ToString())
                {
                    ourColor = team.color.ToString();
                }
            }
            

            Data_OurNextMatchFrame.BackgroundColor = ourColor == "Red" ? Color.FromHex("#910929") : Color.FromHex("#290991");

        }
        catch (Exception e) {

        }
    }
    public async void UpdateStateInformation(){


        UpdateStateInformationFields();
        APIResponse response = await APIManager.GetCurrentState();
        dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);


        if (response.Status == System.Net.HttpStatusCode.OK)
        {

            StorageManagement.compCache.Name = data["competitionName"].ToString();
            StorageManagement.compCache.Location = data["location"].ToString();
            StorageManagement.compCache.MatchType = data["matchType"].ToString();
            int nextMatch = 0;

            bool isNextMatchAvailable = int.TryParse(data["currentMatch"].ToString(), out nextMatch);
            bool currentlyRunning = (bool)data["currentlyRunning"];
            if (isNextMatchAvailable && currentlyRunning) {
                StorageManagement.compCache.NextMatch = nextMatch;
            }
            else
            {
                StorageManagement.compCache.NextMatch = -1;
            }

            StorageManagement.compCache.OurNextMatch = data["ourNextMatch"];

            StorageManagement.compCache.Matches = data["matches"];

            var matchCount = 0;
            foreach(var match in StorageManagement.compCache.Matches)
            {
                matchCount += 1;
            }

            if(matchCount == 0)
            {
                Data_CompetitionNotStarted.IsVisible = true;
                Data_CompetitionStarted.IsVisible = false;
            }
            else
            {
                Data_CompetitionNotStarted.IsVisible = false;
                Data_CompetitionStarted.IsVisible = true;
            }


            
            StorageManagement.compCache.Teams = data["teams"];

            StorageManagement.compCache.Rankings = data["rankings"];

            StorageManagement.compCache.TeamNumber = int.Parse(data["teamNumber"].ToString());

            UpdateStateInformationFields();
            StorageManagement._SaveData_Comp();

        }
        

    }


    public void UpdateMatchesNotification()
    {
        LocalNotificationCenter.Current.CancelAll();
        // create initial notification based on matches
        var matchesLeftToSubmit = StorageManagement.allMatches.Where(m => m.Status != UploadStatus.SUCCEEDED);

        if(matchesLeftToSubmit.Count() == 0)
        {
            var matchesNotif = new NotificationRequest()
            {
                NotificationId = 102,
                Title = "Matches All Submitted",
                Description = "Nice job! You've submitted all of your matches to the server!",
                ReturningData = "SUBMIT",
                Silent = true,
                CategoryType = NotificationCategoryType.Reminder,
                Group = "Submit",
                Image = new NotificationImage() { ResourceName = "check.png" }
            };
            LocalNotificationCenter.Current.Show(matchesNotif);
        }
        else
        {
            var matchesNotif = new NotificationRequest()
            {
                NotificationId = 102,
                Title = "Matches to Submit",
                Description = "Don't forget... You still have " + matchesLeftToSubmit.Count().ToString() + " left to submit to the server!",
                ReturningData = "SUBMIT",
                Silent = true,
                CategoryType = NotificationCategoryType.Reminder,
                Group = "Submit",
                Image = new NotificationImage() { ResourceName = "statsreport.png" }
            };
            LocalNotificationCenter.Current.Show(matchesNotif);
        }
        

        
    }

    public MainPage()
    {
        InitializeComponent();
        currentMenu = View_Scout;

       



    }

    Dictionary<string, string[]> viewDirections = new Dictionary<string, string[]>()
    {
        { "Scout", new string[]{ "Settings", "Data"} },
        { "Settings", new string[]{ "Data", "Scout" } },
        { "Data", new string[]{ "Scout", "Settings" } }
    };

    static bool setup;
    List<object> settingsComponents;
    protected override void OnAppearing()
    {


        base.OnAppearing();

        if (!setup)
        {
            OneSignal.Default.PromptForPushNotificationsWithUserResponse();
            MessagingCenter.Subscribe<NavigationPage, MessageContent>((NavigationPage)Application.Current.MainPage, "REFRESH", (s, m) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ShowMatches();
                    ShowPhotos();
                    
                });
            });


            Device.StartTimer(TimeSpan.FromSeconds(30), () =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    UpdateStateInformation();
                });
                return true;
            });

            setup = true;





            
        }

        UpdateMatchesNotification();
        UpdateStateInformation();


        
        //StorageManagement.AddData_Schema("Rapid React Test", "{'Name':'Rapid React Test','Parts':[{'Name':'Autonomous','Time':0,'Components':[{'Type':'Step','Name':'Cargo Low','Min':0,'Max':10},{'Type':'Check','Name':'Off the Tarmac','On':'Yes','Off':'No'},{'Type':'Select','Name':'Level','Options':['A','B','C']},{'Type':'Event','Name':'Balls Shot','Trigger':'Shot Now!','Max':30},{'Type':'Timer','Name':'Playing Defense'}]}]}");

        // get initial settings data


        UpdateSettings();
        ShowMatches();
        ShowPhotos();

        ButtonMenu = Button_Scout;
        currentMenu = View_Scout;




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
        var _matchesScouted = DataManagement.GetValue("matches_created");
        var _notifications = DataManagement.GetValue("notifications");

        var deviceId = (string)DataManagement.GetValue("deviceId");
        if(deviceId == "")
        {
            deviceId = DataManagement.GenerateRandomCharacters(8);
            DataManagement.SetValue("deviceId", deviceId);
        }
        Settings_DeviceID.Text = "Device " + deviceId;

        var version = VersionTracking.Default.CurrentVersion.ToString();
        var build = VersionTracking.Default.CurrentBuild.ToString();

        Settings_VersionInfo.Text = "Version: " + version + ", Build: " + build;

        settingsComponents = new List<object>()
        {
            Settings_AuthenticationKey, Settings_EnvironmentCode, Settings_SelectedSchema, Settings_UploadMode, Settings_ServerAddress, Settings_DefaultScouter, Settings_Notifications
        };

        if (_envCode != null)
            Settings_EnvironmentCode.Text = _envCode.ToString();
        if (_uploadMode != null)
            Settings_UploadMode.SelectedIndex = Int32.Parse(_uploadMode.ToString());
        else
            Settings_UploadMode.SelectedIndex = 0;
        if (_authKey != null)
            Settings_AuthenticationKey.Text = _authKey.ToString();
        if (_serverAddress != null)
            Settings_ServerAddress.Text = _serverAddress.ToString();
        if (_defaultScouter != null)
            Settings_DefaultScouter.Text = _defaultScouter.ToString();
        if (_matchesScouted != null)
            StorageManagement.matchesCreated = Int32.Parse(_matchesScouted.ToString());
        if (_notifications != null)
            Settings_Notifications.SelectedIndex = Int32.Parse(_notifications.ToString());
        else
            Settings_Notifications.SelectedIndex = 0;

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

        if(StorageManagement.matchesCreated >= 2 && (string)Settings_UploadMode.SelectedItem == "Using Wireless")
        {
            StorageManagement.matchesCreated = 0;
            DataManagement.SetValue("matches_created", "0");

            Data_StartSubmitMatches(null, null);
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
    Microsoft.Maui.Controls.Button ButtonMenu;
    private async void Nav_ToggleBottomBar_Clicked(object sender, EventArgs e)
    {

        ChangeNavBottomBarExpansion(!navExpanded);

    }

    private async void Nav_GoTo(object sender, EventArgs e)
    {

        Microsoft.Maui.Controls.Button clicker = (Microsoft.Maui.Controls.Button)sender as Microsoft.Maui.Controls.Button;

        var goTo = clicker.ClassId;

        if (goTo != currentMenu.ClassId)
        {
            

            ButtonMenu = clicker;

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

        Button_Scout.BackgroundColor = Color.FromHex("#3A0E4D");
        Button_Data.BackgroundColor = Color.FromHex("#3A0E4D");
        Button_Settings.BackgroundColor = Color.FromHex("#3A0E4D");
        ((Microsoft.Maui.Controls.Button)FindByName("Button_" + final)).BackgroundColor = Color.FromHex("#680991");

        if (goToItem.ClassId == currentMenu.ClassId)
            return;
        if (goToItem.ClassId == "Settings")
            SaveSettings();

        
        var origName = currentMenu.ClassId;
        var newName = final;
        var associations = viewDirections[origName];
        var directionRight = associations[1] != newName;


        goToItem.TranslationX = directionRight ? -1000 : 1000;
        goToItem.IsVisible = true;
        currentMenu.TranslateTo(directionRight ? 1000 : -1000, 0, 500, Easing.CubicInOut);
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

        if(matches.Count == 0)
        {
            Data_SomeMatches.IsVisible = false;
            Data_NoMatches.IsVisible = true;
        }
        else
        {
            Data_SomeMatches.IsVisible = true;
            Data_NoMatches.IsVisible = false;
        }

        Data_Matches.Children.Clear();
        foreach(Match m in matches)
        {
            Color bgColor = Color.FromHex(m.Status == UploadStatus.NOT_TRIED ? "#280338" : (m.Status == UploadStatus.FAILED ? "#60051a" : "#3a0e4d"));

            var id = m.Number.ToString() + ";" + m.Environment;
            Frame outside = new Frame() { BackgroundColor = bgColor, BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, MaximumWidthRequest = 400, Padding = new Thickness(0, 12), ClassId = id };
            
            Grid contentsInside = new Grid() { Margin = new Thickness(5, 0) };

            if(m.Number > 0)
            {
                // General Document
                contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                Label matchNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Match " + m.Number.ToString(), HorizontalTextAlignment = TextAlignment.Start, TextColor = Color.FromHex("#ffffff"), FontSize = 20, Margin = new Thickness(10, 0, 0, 0) };
                Label teamNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Team " + m.Team.ToString() + " - " + m.Color, HorizontalOptions = LayoutOptions.End, TextColor = Color.FromHex("#ffffff"), FontSize = 14, Margin = new Thickness(0, 0, 10, 0) };

                contentsInside.Add(matchNum, 0, 0);
                contentsInside.Add(teamNum, 1, 0);

                outside.Content = contentsInside;
            }
            else
            {
                contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                contentsInside.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
                

                Label teamNum = new Label() { VerticalOptions = LayoutOptions.Center, Text = "Team " + m.Team.ToString(), HorizontalTextAlignment = TextAlignment.Start, TextColor = Color.FromHex("#ffffff"), FontSize = 20, Margin = new Thickness(10, 0, 0, 0) };
                Label dateEdited = new Label() { VerticalOptions = LayoutOptions.Center, Text = m.Created.ToShortTimeString(), HorizontalOptions = LayoutOptions.End, TextColor = Color.FromHex("#ffffff"), FontSize = 14, Margin = new Thickness(0, 0, 10, 0) };

                contentsInside.Add(teamNum, 0, 0);
                contentsInside.Add(dateEdited, 1, 0);

                outside.Content = contentsInside;
            }
            

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
            Data_Photos.RowDefinitions.Add(new RowDefinition() { Height = 58 });
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
            Frame outside = new Frame() { BackgroundColor = bgColor, ClassId = id, BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, WidthRequest = 80, HeightRequest = 50, Margin = new Thickness(8, 4), Padding = new Thickness(4,8), VerticalOptions = LayoutOptions.Center };
            Label teamNum = new Label() { Text = photo.Team == 0 ? "Unknown" : photo.Team.ToString(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
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
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString(), "Never Mind", "Delete", "Submit", "Edit Details");
                break;
            case UploadStatus.SUCCEEDED:
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString() + " (Submitted)", "Never Mind", "Delete", "Mark as 'Not Submitted'", "Edit Details");
                break;
            case UploadStatus.FAILED:
                res = await DisplayActionSheet(photo.Team == 0 ? "Photo" : "Photo for " + photo.Team.ToString(), "Never Mind", "Delete", "Retry Submit", "Edit Details");
                break;
        }
        if (res == "Submit" || res == "Resubmit" || res == "Retry Submit")
        {
            Info_Label.Text = "Submitting Photo";
            await ChangeInfoView(InfoViewStatus.OPEN);
            Task.Run(async () =>
            {




                var response = await APIManager.SendPhotos(new List<Photo>() { photo });
                if (response[0].Status == System.Net.HttpStatusCode.OK)
                {
                    // data is good

                    var content = response[0].Content;

                    photo.Status = UploadStatus.SUCCEEDED;
                    StorageManagement._SaveData_Photo();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChangeInfoView(InfoViewStatus.SUCCESS);

                        ShowPhotos();

                    });

                }
                else
                {

                    photo.Status = UploadStatus.FAILED;
                    StorageManagement._SaveData_Photo();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                        ShowPhotos();
                    });
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChangeInfoView(InfoViewStatus.FAIL);

                        ShowPhotos();

                    });
                }

                StorageManagement._SaveData_Photo();

                
            });


        }
        else if (res == "Mark as 'Not Submitted'")
        {
            photo.Status = UploadStatus.NOT_TRIED;
            StorageManagement._SaveData_Match();
            ShowPhotos();
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

            try
            {

                Image i = new Image() { Source = ImageSource.FromFile(StorageManagement.GetPath(photo.Path)), Margin = new Thickness(5, 5), HeightRequest = 400, Rotation = (photo.JustTaken ? 90 : 0) };
                Overlay_Content.Add(i);

            }
            catch(Exception ex)
            {

            }
            

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
            Grid photoButtons = new Grid() { Margin = new Thickness(10,0,10,10) };
            photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
            photoButtons.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });

            Microsoft.Maui.Controls.Button paperButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex(photo.Type != "Paper" ? "#3a0e4d" : "#680991"), Text = "Paper", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

            Microsoft.Maui.Controls.Button otherButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex(photo.Type != "Other" ? "#3a0e4d" : "#680991"), Text = "Other", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

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

                if (photoButtons.ClassId != "Paper" && photoButtons.ClassId != "Other")
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

        var topText = "Match " + match.Number.ToString();
        if(match.Number <= 0)
        {
            topText = "General Document";
        }

        PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
        switch (match.Status)
        {
            case UploadStatus.NOT_TRIED:
                res = await DisplayActionSheet(topText, "Never Mind", "Delete", "Submit", "Edit Details", "Edit Data");
                break;
            case UploadStatus.SUCCEEDED:
                res = await DisplayActionSheet(topText + " (Submitted)", "Never Mind", "Delete", "Mark as 'Not Submitted'", "Edit Details", "Edit Data");
                break;
            case UploadStatus.FAILED:
                res = await DisplayActionSheet(topText, "Never Mind", "Delete", "Retry Submit", "Edit Details", "Edit Data");
                break;
        }

        if(res == "Submit" || res == "Resubmit" || res == "Retry Submit")
        {
            Info_Label.Text = "Submitting Match " + match.Number.ToString();
            await ChangeInfoView(InfoViewStatus.OPEN);
            Task.Run(async () =>
            {
                



                var response = await APIManager.SendMatches(new List<Match>() { match });
                if (response.Status == System.Net.HttpStatusCode.OK)
                {
                    // data is good

                    var content = response.Content;

                    match.Status = UploadStatus.SUCCEEDED;
                    StorageManagement._SaveData_Match();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChangeInfoView(InfoViewStatus.SUCCESS);

                        ShowMatches();

                    });

                }
                else
                {

                    match.Status = UploadStatus.FAILED;
                    StorageManagement._SaveData_Match();
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                        ShowMatches();
                    });
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        ChangeInfoView(InfoViewStatus.FAIL);

                        ShowMatches();

                    });
                }

                StorageManagement._SaveData_Match();
                UpdateMatchesNotification();


            });

            
        }
        else if(res == "Edit Data")
        {
            Navigation.PushAsync(new Scouting(match.Schema, match.Schema, this, match));
        }
        else if(res == "Mark as 'Not Submitted'")
        {
            match.Status = UploadStatus.NOT_TRIED;
            StorageManagement._SaveData_Match();
            ShowMatches();
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

            var generalDocument = match.Number <= 0;
            Label matchNLabel = new Label() { Text = "Match Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry matchN = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric, Text = generalDocument ? "N/A" : match.Number.ToString(), IsEnabled=!generalDocument, Opacity= generalDocument ? 0.7 : 1 };

            Label teamNLabel = new Label() { Text = "Team Number", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry teamN = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Keyboard = Keyboard.Numeric, Text = match.Team.ToString() };

            Label scoutLabel = new Label() { Text = "Scout Name", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry scout = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = match.Scouter };

            Label colorLabel = new Label() { Text = "Team Color", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0, 10) };
            StormEntry color = new StormEntry() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = generalDocument ? "N/A" : match.Color, IsEnabled = !generalDocument, Opacity = generalDocument ? 0.7 : 1 };
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

                if (!generalDocument)
                {
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

                    match.Number = matchNum;
                    match.Color = color;
                }
                

                var scout = overlayInputs[3].Text;
                if (scout == "")
                    scout = match.Scouter;



                
                match.Team = teamNum;
                match.Scouter = scout;
                
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

            // Microsoft.Maui.Controls.Button spins :: 250ms (No await)
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
        Navigation.PushAsync(new Scouting(selectedSchema.ToString(), (string)_envCode, this, null));

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
        ChangeInfoView(InfoViewStatus.FAIL);
    }

    public void DisplayPhotoEdit(FileResult photo, bool taken)
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

        Microsoft.Maui.Controls.Button paperButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = "Paper", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10,0) };
        
        Microsoft.Maui.Controls.Button otherButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = "Other", FontSize = 16, TextColor = Color.FromHex("#ffffff"), Margin = new Thickness(10, 0) };

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

            if(photoButtons.ClassId != "Paper" && photoButtons.ClassId != "Other")
            {
                return;
            }



            
            PhysicalVibrations.TryHaptic(HapticFeedbackType.LongPress);
            StorageManagement.AddData_Photo(photo, teamNum, matchNums, photoButtons.ClassId, taken);

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

                DisplayPhotoEdit(photo, true);





                
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
            DisplayPhotoEdit(selected, false);
        }
    }

    private async void Data_StartQRScan(object sender, EventArgs e)
    {

#if IOS
    DisplayAlert("Oops", "QR Code Scanning currently isn't supported on this platform...", "OK");
#elif ANDROID
        
        var scanner = new ZXing.Mobile.MobileBarcodeScanner();

        var options = new ZXing.Mobile.MobileBarcodeScanningOptions();
        options.UseNativeScanning = true;
        options.TryHarder = true;

        var result = await scanner.Scan(options);

        if (result != null)
        {
            try
            {

                dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(result.Text);

                switch ((string)obj.type)
                {
                    case "config":
                        string server = (string)obj.serverAddress;
                        Settings_ServerAddress.Text = server;
                        SaveSettings();
                        break;
                    default:
                        DisplayAlert("Oops", "That QR Code isn't accepted here...", "OK");
                        break;

                }
            }
            catch(Exception ex)
            {
                DisplayAlert("Oops", "That QR Code isn't accepted here...", "OK");
            }
            

        }

            


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

        var photosToSubmit = StorageManagement.allPhotos.Where(m => m.Status == UploadStatus.FAILED || m.Status == UploadStatus.NOT_TRIED);

        Info_Label.Text = "Submitting " + photosToSubmit.Count() + " Photos";
        await ChangeInfoView(InfoViewStatus.OPEN);

        Task.Run(async () =>
        {
            
            if (photosToSubmit.Count() == 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(InfoViewStatus.FAIL);

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
                ChangeInfoView(InfoViewStatus.SUCCESS);
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

        var matchesToSubmit = StorageManagement.allMatches.Where(m => m.Status == UploadStatus.FAILED || m.Status == UploadStatus.NOT_TRIED);
        Info_Label.Text = "Submitting " + matchesToSubmit.Count().ToString() + " Matches";
        await ChangeInfoView(InfoViewStatus.OPEN);

        Task.Run(async () =>
        {
            
            if (matchesToSubmit.Count() == 0) 
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(InfoViewStatus.FAIL);

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
            UpdateMatchesNotification();

            Device.BeginInvokeOnMainThread(() =>
            {
                ChangeInfoView(InfoViewStatus.SUCCESS);

                ShowMatches();

            });
        });



    }

    private async void Data_StartDownload(object sender, EventArgs e)
    {
        var webServer = DataManagement.GetValue("server_address");
        if(webServer == null)
        {
            return;
        }




        Info_Label.Text = "Downloading Data";

        await ChangeInfoView(InfoViewStatus.OPEN);

        Task.Run(async () =>
        {
            var schemasRes = await APIManager.GetSchemas();
            if(schemasRes.Status == System.Net.HttpStatusCode.OK){
                var content = schemasRes.Content;
                dynamic contentObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                foreach(var schema in contentObject.schemas){
                    StorageManagement.AddData_Schema((string)schema["Name"], Newtonsoft.Json.JsonConvert.SerializeObject(schema), (dynamic)schema["Settings"]);
                }
            }

            var response = await APIManager.GetSetupData();
            if(response.Status == System.Net.HttpStatusCode.OK)
            {
                // data is good

                var content = response.Content;

                dynamic contentObject = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
                var selectedSchema = contentObject["settings"]["selectedSchema"];
                var version = contentObject["settings"]["minBuild"];
                dynamic schemaObject = contentObject["schema"];
                StorageManagement.AddData_Schema((string)schemaObject["Name"], Newtonsoft.Json.JsonConvert.SerializeObject(schemaObject), (dynamic)schemaObject["Settings"]);



                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(InfoViewStatus.SUCCESS);

                    var build = VersionTracking.Default.CurrentBuild.ToString();
                    if(int.Parse(build) < int.Parse((string)version)){
                        DisplayAlert("Warning", "We have detected that you are on an older version of StormCloud. The website that you are using requies a version newer than yours to properly run. Please advise upgrading your app version.", "OK");
                    }
                    

                    UpdateSettings();

                });
                DataManagement.SetValue("selected_schema", (string)selectedSchema);

            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    ChangeInfoView(InfoViewStatus.FAIL);

                    UpdateSettings();
                    DisplayAlert("Oops", "Something went wrong connecting to the server. Please ensure that you have a connection and that the server address is correct", "OK");
                });
                Device.BeginInvokeOnMainThread(() =>
                {
                    

                });
            }


            
        });
        

        
    }


    InfoViewStatus _infoViewStatus = InfoViewStatus.SUCCESS;

    public async Task<bool> ChangeInfoView(InfoViewStatus newState)
    {
        if (newState == _infoViewStatus)
            return false;

        _infoViewStatus = newState;
        if (_infoViewStatus == InfoViewStatus.OPEN)
        {
            InfoView.IsVisible = true;
            InfoFinished.Opacity = 0;

            InfoWaiting.Opacity = 0;
            InfoWaiting.FadeTo(1, 500, Easing.CubicInOut);
            await InfoView.TranslateTo(0, 0, 500, Easing.CubicInOut);

            //CameraBox.IsEnabled = true;
            //CameraBox.IsDetecting = true;
            //CameraBox.CameraLocation = ZXing.Net.Maui.CameraLocation.Rear;

        }
        else if(_infoViewStatus == InfoViewStatus.SUCCESS)
        {
            InfoWaiting.FadeTo(0, 500, Easing.CubicInOut);
            await Task.Delay(200);
            InfoFinished.FadeTo(1, 500, Easing.CubicInOut);
            await Task.Delay(2000);
            InfoFinished.FadeTo(0, 500, Easing.CubicInOut);

            await InfoView.TranslateTo(1000, 0, 500, Easing.CubicInOut);
            //CameraView.IsVisible = false;
            //CameraBox.IsEnabled = false;
            //CameraBox.IsDetecting = false;
        }
        else
        {
            await InfoView.TranslateTo(1000, 0, 500, Easing.CubicInOut);
        }
        return true;
    }

    private async void Search_Exit(object sender, SwipedEventArgs e)
    {
        ChangeSearchView(false);
        
    }

    private async void Search_Exit(object sender, EventArgs e)
    {
        ChangeSearchView(false);

    }

    private async void Info_Exit(object sender, SwipedEventArgs e)
    {
        ChangeInfoView(false);

    }

    private async void Info_Show(object sender, EventArgs e)
    {
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        await ChangeInfoView(true);
        Label sendL = sender as Label;
        if(sendL != null)
        {
            PutInfoOnInfoView(sendL.ClassId);
            return;
        }

        Microsoft.Maui.Controls.Button sendB = sender as Microsoft.Maui.Controls.Button;
        if (sendB != null)
        {
            PutInfoOnInfoView(sendB.ClassId);
            return;
        }

        Frame sendF = sender as Frame;
        if (sendF != null)
        {
            PutInfoOnInfoView(sendF.ClassId);
            return;
        }


    }

    private async void Search_StartDocumentSearch(object sender, EventArgs e)
    {
        ChangeSearchView(true);
    }

    bool _searchViewVisible;
    bool _infoViewVisible;
    Dictionary<string, string> infoData = new Dictionary<string, string>();

    public async Task<bool> ChangeInfoView(bool show)
    {
        if (show == _infoViewVisible)
        {
            return false;
        }

        _infoViewVisible = show;
        if (show)
        {
            Info_View_Result_Detail.IsVisible = false;
            Info_View_Result_Loading.IsVisible = true;
            Info_View_Result_Main.IsVisible = false;
            Info_View_Backdrop.Opacity = 0;
            Info_View_Backdrop.IsVisible = true;
            Info_View.IsVisible = true;
            Info_View_Result_Box.TranslationY = 1800;
            Info_View_Backdrop.FadeTo(.5, 250, Easing.CubicInOut);

            
            await Info_View_Result_Box.TranslateTo(0, 300, 500, Easing.CubicInOut);


        }
        else
        {
            Info_View_Backdrop.FadeTo(0, 250, Easing.CubicInOut);
            await Info_View_Result_Box.TranslateTo(0, 1800, 500, Easing.CubicInOut);
            Info_View_Backdrop.IsVisible = false;
            Info_View.IsVisible = false;
        }
        return true;
    }

    public async void PutInfoOnInfoView(string infoEvent){

        Info_View_Result_Main_Content.Children.Clear();
        
        try
        {
            infoData["event"] = infoEvent;
            switch (infoEvent)
            {
                case "teams":
                    StackLayout teamsContent = Info_View_Result_Main_Content;
                    Label headerTeams = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 24, Text = "Teams Competing", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5, 0, 5) };
                    Label subheaderTeams = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 16, Text = "Please click on a team to view its most recent analysis. Some teams may have incomplete or missing documents due to scouter error. Please visit your StormCloud portal for document management.", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(10, 5, 10, 20) };
                    teamsContent.Add(headerTeams);
                    teamsContent.Add(subheaderTeams);

                    Grid allTeams = new Grid() { Margin = new Thickness(10, 5) };
                    allTeams.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    allTeams.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    allTeams.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    allTeams.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    int teamCount = 0;
                    foreach(dynamic team in StorageManagement.compCache.Teams){
                        if(teamCount % 4 == 0)
                        {
                            allTeams.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                        }
                        Frame teamFrame = new Frame() { CornerRadius = 8, BackgroundColor = Color.FromHex("#3a0e4d"), Margin = new Thickness(5), Padding = new Thickness(5), HasShadow = false, BorderColor = (string)team.teamNumber == StorageManagement.compCache.TeamNumber.ToString() ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), ClassId = (string)team.teamNumber };
                        Label teamLabel = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 20, Text = (string)team.teamNumber, HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(5), ClassId = (string)team.teamNumber };
                        
                        teamFrame.Content = teamLabel;

                        TapGestureRecognizer teamTap = new TapGestureRecognizer();
                        teamTap.Tapped += (s,e) => {
                            Frame teamFrameTapped = s as Frame;

                            PutInfoOnDetailView("team",teamFrameTapped.ClassId,true);
                        };

                        teamFrame.GestureRecognizers.Add(teamTap);

                        allTeams.Add(teamFrame, teamCount % 4, teamCount / 4);
                        teamCount++;
                    }

                    teamsContent.Add(allTeams);

                   



                    break;

                case "rankings":
                    StackLayout rankingsContent = Info_View_Result_Main_Content;
                    Label header = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 24, Text = "Rankings", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5, 0, 20) };
                    rankingsContent.Add(header);

                    foreach(dynamic ranking in StorageManagement.compCache.Rankings)
                    {
                        Frame rankingContainer = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = (string)ranking.team == StorageManagement.compCache.TeamNumber.ToString() ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                        Grid rankingContent = new Grid();
                        rankingContent.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                        rankingContent.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
                        rankingContent.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));
                        rankingContent.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(2, GridUnitType.Star)));

                        Label rank = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 16, Text = "#" + (string)ranking.rank, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };
                        Label team = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 20, Text = (string)ranking.team, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };
                        Label record = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 16, Text = (string)ranking.record.wins + "-" + (string)ranking.record.losses + "-" + (string)ranking.record.ties, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };
                        Label RP = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 16, Text = (string)ranking.rankingPoints + " RP", VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };

                        rankingContent.Add(rank, 0, 0);
                        rankingContent.Add(team, 1, 0);
                        rankingContent.Add(record, 2, 0);
                        rankingContent.Add(RP, 3, 0);

                        rankingContainer.Content = rankingContent;

                        rankingsContent.Add(rankingContainer);

                    }

                    break;
                case "matches":
                    StackLayout matchesContent = Info_View_Result_Main_Content;
                    foreach (dynamic match in StorageManagement.compCache.Matches)
                    {
                        Label matchNum = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 18, Text = "#" + match["matchNumber"].ToString(), VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };


                        Grid matchDetails = new Grid() { Margin = new Thickness(0,5), ClassId = match.matchNumber.ToString()};
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(4, GridUnitType.Star)));
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

                        

                        Grid teamGrid = new Grid() { Margin = new Thickness(0, 10) };

                        if(match.matchNumber.ToString() == StorageManagement.compCache.NextMatch.ToString())
                        {
                            matchDetails.BackgroundColor = Color.FromArgb("20ffffff");
                        }

                        TapGestureRecognizer tgr = new TapGestureRecognizer();
                        tgr.Tapped += (s, e) => {

                            Grid send = s as Grid;
                            var matchNumber = send.ClassId;
                            Info_View_Result_Detail.IsVisible = false;
                            Info_View_Result_Loading.IsVisible = true;
                            Info_View_Result_Main.IsVisible = false;
                            PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
                            PutInfoOnDetailView("match", matchNumber, true);
                        };

                        matchDetails.GestureRecognizers.Add(tgr);

                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        for (int i = 0; i < match.teams.Count / 2; i++)
                        {
                            teamGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        var blueCount = 0;
                        var redCount = 0;
                        foreach (var team in match.teams)
                        {
                            Frame f = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = (string)team.team == StorageManagement.compCache.TeamNumber.ToString() ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false };
                            Label teamLabel = new Label() { Text = (string)team.team, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            f.Content = teamLabel;
                            if ((string)team.color == "Red")
                            {
                                f.BackgroundColor = Color.FromHex("#910929");
                                teamGrid.Add(f, redCount, 0);
                                redCount++;
                            }
                            else if ((string)team.color == "Blue")
                            {
                                f.BackgroundColor = Color.FromHex("#290991");
                                teamGrid.Add(f, blueCount, 1);
                                blueCount++;
                            }
                        }

                        StackLayout resultsStack = new StackLayout() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                        if ((bool)match.results.finished)
                        {
                            var redScore = int.Parse(match.results.red.ToString());
                            var blueScore = int.Parse(match.results.blue.ToString());
                            // show the blue and red scores
                            Frame resultFrameRed = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = redScore > blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#910929"), Margin=new Thickness(2) };
                            Label redScoreLabel = new Label() { Text = redScore.ToString(), FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            Frame resultFrameBlue = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = redScore < blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#290991"), Margin = new Thickness(2) };
                            Label blueScoreLabel = new Label() { Text = blueScore.ToString(), FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            resultFrameRed.Content = redScoreLabel;
                            resultFrameBlue.Content = blueScoreLabel;
                            resultsStack.Add(resultFrameRed);
                            resultsStack.Add(resultFrameBlue);
                        }
                        else
                        {
                            DateTime planned = DateTime.Parse(match.planned.ToString());
                            Frame plannedFrame = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#5a5a5a") };
                            Label plannedLabel = new Label() { Text = planned.ToShortTimeString(), FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            plannedFrame.Content = plannedLabel;
                            resultsStack.Add(plannedFrame);
                        }


                        

                        matchDetails.Add(matchNum, 0, 0);
                        matchDetails.Add(teamGrid, 1, 0);
                        matchDetails.Add(resultsStack, 2, 0);



                        matchesContent.Add(matchDetails);
                    }
                   
                    break;
                case "our_matches":
                    StackLayout ourMatchesContent = Info_View_Result_Main_Content;
                    foreach (dynamic match in StorageManagement.compCache.Matches)
                    {

                        bool weAreIn = false;
                        foreach(var team in match.teams)
                        {
                            if (team.team.ToString() == StorageManagement.compCache.TeamNumber.ToString())
                            {
                                weAreIn = true;
                                break;
                            }
                                

                        }

                        if (!weAreIn)
                        {
                            continue;
                        }

                        Label matchNum = new Label() { TextColor = Color.FromHex("#ffffff"), FontSize = 18, Text = "#" + match["matchNumber"].ToString(), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };


                        Grid matchDetails = new Grid() { Margin = new Thickness(0, 5), ClassId = match.matchNumber.ToString() };
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(4, GridUnitType.Star)));
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));


                        try
                        {
                            if (StorageManagement.compCache.OurNextMatch["matchNumber"] != null && match.matchNumber.ToString() == StorageManagement.compCache.OurNextMatch["matchNumber"].ToString())
                            {
                                matchDetails.BackgroundColor = Color.FromArgb("20ffffff");
                            }
                        }catch(Exception cex)
                        {

                        }
                        

                        TapGestureRecognizer tgr = new TapGestureRecognizer();
                        tgr.Tapped += (s, e) => {

                            Grid send = s as Grid;
                            var matchNumber = send.ClassId;
                            Info_View_Result_Detail.IsVisible = false;
                            Info_View_Result_Loading.IsVisible = true;
                            Info_View_Result_Main.IsVisible = false;
                            PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
                            PutInfoOnDetailView("match", matchNumber, true);

                        };

                        matchDetails.GestureRecognizers.Add(tgr);


                        Grid teamGrid = new Grid() { Margin = new Thickness(0, 10) };
                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        for (int i = 0; i < match.teams.Count / 2; i++)
                        {
                            teamGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        var blueCount = 0;
                        var redCount = 0;
                        foreach (var team in match.teams)
                        {
                            Frame f = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(4), BorderColor = (string)team.team == StorageManagement.compCache.TeamNumber.ToString() ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false };
                            Label teamLabel = new Label() { Text = (string)team.team, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            f.Content = teamLabel;
                            if ((string)team.color == "Red")
                            {
                                f.BackgroundColor = Color.FromHex("#910929");
                                teamGrid.Add(f, redCount, 0);
                                redCount++;
                            }
                            else if ((string)team.color == "Blue")
                            {
                                f.BackgroundColor = Color.FromHex("#290991");
                                teamGrid.Add(f, blueCount, 1);
                                blueCount++;
                            }
                        }

                        StackLayout resultsStack = new StackLayout() { HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };
                        if ((bool)match.results.finished)
                        {
                            var redScore = int.Parse(match.results.red.ToString());
                            var blueScore = int.Parse(match.results.blue.ToString());
                            // show the blue and red scores
                            Frame resultFrameRed = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = redScore > blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#910929"), Margin = new Thickness(2) };
                            Label redScoreLabel = new Label() { Text = redScore.ToString(), FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            Frame resultFrameBlue = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = redScore < blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#290991"), Margin = new Thickness(2) };
                            Label blueScoreLabel = new Label() { Text = blueScore.ToString(), FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            resultFrameRed.Content = redScoreLabel;
                            resultFrameBlue.Content = blueScoreLabel;
                            resultsStack.Add(resultFrameRed);
                            resultsStack.Add(resultFrameBlue);
                        }
                        else
                        {
                            DateTime planned = DateTime.Parse(match.planned.ToString());
                            Frame plannedFrame = new Frame() { Padding = new Thickness(10, 5), CornerRadius = 4, BorderColor = Color.FromArgb("00ffffff"), HasShadow = false, BackgroundColor = Color.FromHex("#5a5a5a") };
                            Label plannedLabel = new Label() { Text = planned.ToShortTimeString(), FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            plannedFrame.Content = plannedLabel;
                            resultsStack.Add(plannedFrame);
                        }




                        matchDetails.Add(matchNum, 0, 0);
                        matchDetails.Add(teamGrid, 1, 0);
                        matchDetails.Add(resultsStack, 2, 0);



                        ourMatchesContent.Add(matchDetails);


                    }
                    break;
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }

        Info_View_Result_Detail.IsVisible = false;
        Info_View_Result_Loading.IsVisible = false;
        Info_View_Result_Main.IsVisible = true;

        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);


    }

    public async void PutInfoOnDetailView(string detailEvent, string detailData, bool canGoBack)
    {

        Info_View_Result_Detail_Content.Children.Clear();


        Info_View_Result_Detail.IsVisible = true;
        Info_View_Result_Loading.IsVisible = false;
        Info_View_Result_Main.IsVisible = false;

        StackLayout detailContent = Info_View_Result_Detail_Content;
        

        if (canGoBack){
            Microsoft.Maui.Controls.Button backButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = "Go Back", Margin=new Thickness(0,10,0,30), TextColor = Color.FromHex("#ffffff") };
            backButton.Clicked += (sender, e) =>
            {
                Info_View_Result_Detail.IsVisible = false;
                Info_View_Result_Loading.IsVisible = false;
                Info_View_Result_Main.IsVisible = true;
                PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
                //PutInfoOnInfoView(infoData["event"]);
            };
            detailContent.Add(backButton);
        } 

        try{
            switch(detailEvent){
                case "team":

                    dynamic applicableTeam = null;
                    foreach(var team in StorageManagement.compCache.Teams){
                        if(team.teamNumber.ToString() == detailData){
                            applicableTeam = team;
                            break;
                        }
                    }
                    if(applicableTeam == null){
                        // technically go back silently
                        Info_View_Result_Detail.IsVisible = false;
                        Info_View_Result_Loading.IsVisible = false;
                        Info_View_Result_Main.IsVisible = true;
                        break;
                    }

                    Label teamNumberLabel = new Label() { Text = "Team " + applicableTeam.teamNumber.ToString(), FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10, 0, 10) };
                    detailContent.Add(teamNumberLabel);
                    Label teamName = new Label() { Text = applicableTeam.teamName.ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 15) };
                    detailContent.Add(teamName);


                    Label teamMatchesLabel = new Label() { Text = "In Matches", FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
                    detailContent.Add(teamMatchesLabel);

                    ScrollView inMatchesParent = new ScrollView() { Orientation = ScrollOrientation.Horizontal };
                    StackLayout inMatches = new StackLayout() { Orientation = StackOrientation.Horizontal, Margin = new Thickness(10, 5,10, 20) };


                    foreach(var searchmatch in StorageManagement.compCache.Matches)
                    {
                        bool inMatch = false;
                        bool matchFinished = false;
                        foreach(var mT in searchmatch.teams)
                        {
                            if((string)mT.team == applicableTeam.teamNumber.ToString())
                            {
                                inMatch = true;
                                matchFinished = (bool)searchmatch.results.finished;
                                break;
                            }
                        }

                        if (inMatch)
                        {
                            Frame matchTeamFrame = new Frame() { Padding = new Thickness(4), Margin = new Thickness(4), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false, BackgroundColor = matchFinished ? Color.FromHex("#08503b") : Color.FromHex("#3a0e4d") };
                            Label matchTeamNumber = new Label() { Text = (string)searchmatch.matchNumber, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            matchTeamFrame.Content = matchTeamNumber;

                            inMatches.Add(matchTeamFrame);
                        }
                    }

                    inMatchesParent.Content = inMatches;

                    detailContent.Add(inMatchesParent);


                    StackLayout analysisTeam = new StackLayout() { Margin = new Thickness(0, 5, 0, 10) };



                    List<int> teamsToAnalyzeT = new List<int>(){int.Parse(detailData)};
                            

                    ActivityIndicator loadingIndiactorT = new ActivityIndicator() { WidthRequest = 50, HeightRequest = 50, Color = Color.FromHex("#ffffff"), IsRunning = true };


                    Label analysisLabelT = new Label() { Text = "Team Analysis", Margin = new Thickness(5, 15, 0, 0), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                    detailContent.Add(analysisLabelT);

                    analysisTeam.Add(loadingIndiactorT);


                    

                    Task.Run(async () =>
                    {
                        APIResponse response = await APIManager.GetAnalysis(teamsToAnalyzeT.ToArray());
                        if (response.Status == System.Net.HttpStatusCode.OK)
                        {
                            dynamic responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                            var authenticated = (bool)responseData.auth;
                            if (authenticated)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {

                                    Dictionary<string, dynamic> analysisData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseData.analysis.ToString());

                                    Dictionary<string, StackLayout> analysisViews = new Dictionary<string, StackLayout>();

                                    analysisTeam.Children.Clear();



                                    foreach (string key in analysisData.Keys.ToList())
                                    {

                                        analysisViews[key] = new StackLayout();
                                        analysisViews[key].IsVisible = true;

                                        

                                        analysisTeam.Add(analysisViews[key]);

                                        


                                        // the actual analysis part
                                        foreach (dynamic part in analysisData[key])
                                        {
                                            switch ((string)part.type)
                                            {
                                                case "Number":
                                                    Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                    analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                    analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                    Label analysisNumberLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                    Label analysisNumberValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    analysisNumberHolder.Content = analysisNumberValue;
                                                    analysisNumber.Add(analysisNumberLabel, 0, 0);
                                                    analysisNumber.Add(analysisNumberHolder, 1, 0);
                                                    analysisViews[key].Add(analysisNumber);

                                                    break;
                                                case "Custom":
                                                    Grid analysisCustom = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                    analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                    analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                    Label analysisCustomLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    Frame analysisCustomHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                    Label analysisCustomValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    analysisCustomHolder.Content = analysisCustomValue;
                                                    analysisCustom.Add(analysisCustomLabel, 0, 0);
                                                    analysisCustom.Add(analysisCustomHolder, 1, 0);
                                                    analysisViews[key].Add(analysisCustom);

                                                    break;
                                                case "FIRST":
                                                    Grid analysisFIRST = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                    analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                    analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                    Label analysisFIRSTLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    Frame analysisFIRSTHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                    Label analysisFIRSTValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    analysisFIRSTHolder.Content = analysisFIRSTValue;
                                                    analysisFIRST.Add(analysisFIRSTLabel, 0, 0);
                                                    analysisFIRST.Add(analysisFIRSTHolder, 1, 0);
                                                    analysisViews[key].Add(analysisFIRST);

                                                    break;
                                                case "Graph":

                                                    StackLayout analysisGraph = new StackLayout() { Margin = new Thickness(0, 5) };
                                                    Label analysisGraphLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
                                                    Label analysisGraphAverage = new Label() { Text = "Average: " + (string)part.average, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };

                                                    LineChart analysisGraphChart = new LineChart() { AxisLinesColor = Color.FromArgb("#60ffffff"), ChartStyle = AlohaKit.Enums.ChartEnums.LineChartStyle.Line, HeightRequest = 200, DisplayValueLabelsOnTop = true, FontColor = Color.FromHex("#ffffff"), FontSize = 16, DisplayHorizontalAxisLines = true, DisplayVerticalAxisLines = true, DisplayHeaderValues = true, LineColor = Color.FromHex("#680991"), PointColor = Color.FromHex("#680991") };


                                                    Grid dataPoints = new Grid() { Margin = new Thickness(15, 0) };

                                                    List<ChartItem> itemsToShow = new List<ChartItem>();
                                                    int i = 0;
                                                    foreach (var label in part.matches)
                                                    {
                                                        itemsToShow.Add(new ChartItem()
                                                        {
                                                            Label = "#" + (string)label,
                                                            Value = int.Parse((string)part.data[i]),
                                                            IsLabelBold = true
                                                        });
                                                        

                                                        dataPoints.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                                                        Label dataPoint = new Label() { Text = (string)part.data[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5), Opacity=0.7 };
                                                        dataPoints.Add(dataPoint, dataPoints.ColumnDefinitions.Count() - 1, 0);

                                                        i++;
                                                    }

                                                    analysisGraphChart.Entries = new ObservableCollection<ChartItem>(itemsToShow);

                                                    analysisGraph.Add(analysisGraphLabel);
                                                    analysisGraph.Add(analysisGraphAverage);
                                                    
                                                    analysisGraph.Add(analysisGraphChart);
                                                    analysisGraph.Add(dataPoints);
                                                    analysisViews[key].Add(analysisGraph);


                                                    break;
                                                case "Frequency":
                                                    Grid analysisFrequency = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                    analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                    analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                    Label analysisFrequencyLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                    Frame analysisFrequencyHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };

                                                    StackLayout analysisFrequencyValues = new StackLayout();
                                                    int j = 0;
                                                    foreach (var field in part.fields)
                                                    {
                                                        var value = (string)part.values[j];

                                                        Label analysisFrequencyValue = new Label() { Text = (string)field + " - " + value, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                        analysisFrequencyValues.Add(analysisFrequencyValue);
                                                        j++;
                                                    }


                                                    analysisFrequencyHolder.Content = analysisFrequencyValues;
                                                    analysisFrequency.Add(analysisFrequencyLabel, 0, 0);
                                                    analysisFrequency.Add(analysisFrequencyHolder, 1, 0);
                                                    analysisViews[key].Add(analysisFrequency);

                                                    break;



                                                    

                                            }
                                        }
                                    }








                                });
                            }
                            else
                            {
                                Device.BeginInvokeOnMainThread(() =>
                                {
                                    analysisTeam.Children.Clear();
                                    Label noAuth = new Label() { Text = "Not Authenticated", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                    analysisTeam.Add(noAuth);
                                });
                            }

                        }
                        else
                        {
                            // show error
                            Device.BeginInvokeOnMainThread(() =>
                            {
                                analysisTeam.Children.Clear();
                                Label noLoad = new Label() { Text = "Failed Loading", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                analysisTeam.Add(noLoad);
                            });
                        }
                    });

                    detailContent.Add(analysisTeam);






                    break;
                case "match":
                    // format of detailData should JUST be matchNumber

                    var matchNumber = int.Parse(detailData);

                    var match = (new List<dynamic>(StorageManagement.compCache.Matches)).Find(m => m.matchNumber.ToString() == detailData);

                    if(match != null)
                    {
                        Label matchLabel = new Label() { Text = "Match " + matchNumber.ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 15) };
                        detailContent.Add(matchLabel);

                        Grid teamGrid = new Grid() { Margin = new Thickness(0, 0, 0, 5) };
                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        for (int i = 0; i < match.teams.Count / 2; i++)
                        {
                            teamGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        }

                        

                        var blueCount = 0;
                        var redCount = 0;
                        foreach (var team in match.teams)
                        {
                            Frame f = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false };
                            Label teamLabel = new Label() { Text = (string)team.team, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            f.Content = teamLabel;
                            if ((string)team.color == "Red")
                            {
                                f.BackgroundColor = Color.FromHex("#910929");
                                teamGrid.Add(f, redCount, 0);
                                redCount++;
                            }
                            else if ((string)team.color == "Blue")
                            {
                                f.BackgroundColor = Color.FromHex("#290991");
                                teamGrid.Add(f, blueCount, 1);
                                blueCount++;
                            }
                        }

                        detailContent.Add(teamGrid);


                        Frame planned = new Frame() { Padding = new Thickness(20, 3), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#5a5a5a"), HorizontalOptions = LayoutOptions.Center };
                        Label plannedLabel = new Label() { Text = DateTime.Parse((string)match.planned).ToShortTimeString(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                        if ((bool)(match.results.finished))
                        {
                            planned.BackgroundColor = Color.FromHex("#08503b");
                            plannedLabel.Text = "Match Complete";



                        }
                        planned.Content = plannedLabel;

                        detailContent.Add(planned);


                        if ((bool)(match.results.finished))
                        {
                            Grid scoreGrid = new Grid() { Margin = new Thickness(20, 10, 20, 0) };
                            scoreGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                            scoreGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                            scoreGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
                            scoreGrid.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));

                            int redScore = int.Parse((string)match.results.red);
                            int blueScore = int.Parse((string)match.results.blue);

                            int redRP = int.Parse((string)match.results.redStats.rp);
                            int blueRP = int.Parse((string)match.results.blueStats.rp);

                            Frame redScoreFrame = new Frame() { Padding = new Thickness(10, 5), BorderColor = redScore > blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("#00ffffff"), CornerRadius = 8, BackgroundColor = Color.FromHex("#910929"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(4) };
                            Label redScoreLabel = new Label() { Text = redScore.ToString(), FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            redScoreFrame.Content = redScoreLabel;
                            Frame blueScoreFrame = new Frame() { Padding = new Thickness(10, 5), BorderColor = redScore < blueScore ? Color.FromHex("#ffffff") : Color.FromArgb("#00ffffff"), CornerRadius = 8, BackgroundColor = Color.FromHex("#290991"), HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(4) };
                            Label blueScoreLabel = new Label() { Text = blueScore.ToString(), FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            blueScoreFrame.Content = blueScoreLabel;
                            Frame redRPFrame = new Frame() { Padding = new Thickness(8, 4), BorderColor = Color.FromArgb("#00ffffff"), CornerRadius = 4, BackgroundColor = Color.FromHex("#910929"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(4) };
                            Label redRPLabel = new Label() { Text = redRP.ToString() + " RP", FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            redRPFrame.Content = redRPLabel;
                            Frame blueRPFrame = new Frame() { Padding = new Thickness(8, 4), BorderColor = Color.FromArgb("#00ffffff"), CornerRadius = 4, BackgroundColor = Color.FromHex("#290991"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(4) };
                            Label blueRPLabel = new Label() { Text = blueRP.ToString() + " RP", FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            blueRPFrame.Content = blueRPLabel;


                            scoreGrid.Add(redRPFrame, 0, 0);
                            scoreGrid.Add(redScoreFrame, 1, 0);
                            scoreGrid.Add(blueScoreFrame, 2, 0);
                            scoreGrid.Add(blueRPFrame, 3, 0);

                            detailContent.Add(scoreGrid);


                            Grid analysis = new Grid() { Margin = new Thickness(0, 15, 0, 10) };

                            List<int> teamsToAnalyze = new List<int>();
                            foreach (var team in match.teams)
                            {
                                teamsToAnalyze.Add(int.Parse((string)team.team));
                            }

                            ActivityIndicator loadingIndiactor = new ActivityIndicator() { WidthRequest = 50, HeightRequest = 50, Color = Color.FromHex("#ffffff"), IsRunning = true };


                            Label analysisLabel = new Label() { Text = "Match Analysis", Margin = new Thickness(5, 15, 0, 0), FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                            detailContent.Add(analysisLabel);

                            analysis.Add(loadingIndiactor, 0, 0);


                            Grid teamButtons = new Grid() { Margin = new Thickness(10, 10, 10, 0) };
                            detailContent.Add(teamButtons);

                            Task.Run(async () =>
                            {
                                APIResponse response = await APIManager.GetAnalysis(teamsToAnalyze.ToArray());
                                if (response.Status == System.Net.HttpStatusCode.OK)
                                {
                                    dynamic responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                                    var authenticated = (bool)responseData.auth;
                                    if (authenticated)
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {

                                            Dictionary<string, dynamic> analysisData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseData.analysis.ToString());

                                            Dictionary<string, StackLayout> analysisViews = new Dictionary<string, StackLayout>();

                                            analysis.Children.Clear();



                                            foreach (string key in analysisData.Keys.ToList())
                                            {

                                                analysisViews[key] = new StackLayout();
                                                analysisViews[key].IsVisible = false;

                                                Label teamNum = new Label() { Text = "Team " + key, Margin = new Thickness(5), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                                analysisViews[key].Add(teamNum);

                                                analysis.Add(analysisViews[key], 0, 0);

                                                teamButtons.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                                                Microsoft.Maui.Controls.Button teamButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = key, ClassId = key, Margin = new Thickness(5, 2), Padding = new Thickness(5), TextColor = Color.FromHex("#ffffff"), FontSize = 14 };
                                                teamButtons.Add(teamButton, teamButtons.ColumnDefinitions.Count() - 1, 0);

                                                teamButton.Clicked += (s, e) =>
                                                {
                                                    Microsoft.Maui.Controls.Button sender = (Microsoft.Maui.Controls.Button)s;
                                                    foreach (StackLayout other in analysisViews.Values.ToList())
                                                    {
                                                        other.IsVisible = false;
                                                    }

                                                    analysisViews[sender.ClassId].IsVisible = true;

                                                };


                                                // the actual analysis part
                                                foreach (dynamic part in analysisData[key])
                                                {
                                                    switch ((string)part.type)
                                                    {
                                                        case "Number":
                                                            Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisNumberLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisNumberValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisNumberHolder.Content = analysisNumberValue;
                                                            analysisNumber.Add(analysisNumberLabel, 0, 0);
                                                            analysisNumber.Add(analysisNumberHolder, 1, 0);
                                                            analysisViews[key].Add(analysisNumber);

                                                            break;
                                                        case "Custom":
                                                            Grid analysisCustom = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisCustomLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisCustomHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisCustomValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisCustomHolder.Content = analysisCustomValue;
                                                            analysisCustom.Add(analysisCustomLabel, 0, 0);
                                                            analysisCustom.Add(analysisCustomHolder, 1, 0);
                                                            analysisViews[key].Add(analysisCustom);

                                                            break;
                                                        case "FIRST":
                                                            Grid analysisFIRST = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisFIRSTLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisFIRSTHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisFIRSTValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisFIRSTHolder.Content = analysisFIRSTValue;
                                                            analysisFIRST.Add(analysisFIRSTLabel, 0, 0);
                                                            analysisFIRST.Add(analysisFIRSTHolder, 1, 0);
                                                            analysisViews[key].Add(analysisFIRST);

                                                            break;
                                                        case "Graph":

                                                            StackLayout analysisGraph = new StackLayout() { Margin = new Thickness(0, 5) };
                                                            Label analysisGraphLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
                                                            Label analysisGraphAverage = new Label() { Text = "Average: " + (string)part.average, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };

                                                            LineChart analysisGraphChart = new LineChart() { AxisLinesColor = Color.FromArgb("#60ffffff"), ChartStyle = AlohaKit.Enums.ChartEnums.LineChartStyle.Line, HeightRequest = 200, DisplayValueLabelsOnTop = true, FontColor = Color.FromHex("#ffffff"), FontSize = 16, DisplayHorizontalAxisLines = true, DisplayVerticalAxisLines = true, DisplayHeaderValues = true, LineColor = Color.FromHex("#680991"), PointColor = Color.FromHex("#680991") };


                                                            Grid dataPoints = new Grid() { Margin = new Thickness(15, 0) };

                                                            List<ChartItem> itemsToShow = new List<ChartItem>();
                                                            int i = 0;
                                                            foreach (var label in part.matches)
                                                            {
                                                                itemsToShow.Add(new ChartItem()
                                                                {
                                                                    Label = "#" + (string)label,
                                                                    Value = int.Parse((string)part.data[i]),
                                                                    IsLabelBold = true
                                                                });
                                                                

                                                                dataPoints.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                                                                Label dataPoint = new Label() { Text = (string)part.data[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5), Opacity=0.7 };
                                                                dataPoints.Add(dataPoint, dataPoints.ColumnDefinitions.Count() - 1, 0);

                                                                i++;
                                                            }

                                                            analysisGraphChart.Entries = new ObservableCollection<ChartItem>(itemsToShow);

                                                            analysisGraph.Add(analysisGraphLabel);
                                                            analysisGraph.Add(analysisGraphAverage);
                                                            
                                                            analysisGraph.Add(analysisGraphChart);
                                                            analysisGraph.Add(dataPoints);
                                                            analysisViews[key].Add(analysisGraph);


                                                            break;
                                                        case "Frequency":
                                                            Grid analysisFrequency = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisFrequencyLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisFrequencyHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };

                                                            StackLayout analysisFrequencyValues = new StackLayout();
                                                            int j = 0;
                                                            foreach (var field in part.fields)
                                                            {
                                                                var value = (string)part.values[j];

                                                                Label analysisFrequencyValue = new Label() { Text = (string)field + " - " + value, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                                analysisFrequencyValues.Add(analysisFrequencyValue);
                                                                j++;
                                                            }


                                                            analysisFrequencyHolder.Content = analysisFrequencyValues;
                                                            analysisFrequency.Add(analysisFrequencyLabel, 0, 0);
                                                            analysisFrequency.Add(analysisFrequencyHolder, 1, 0);
                                                            analysisViews[key].Add(analysisFrequency);

                                                            break;



                                                            

                                                    }
                                                }
                                            }








                                        });
                                    }
                                    else
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            analysis.Children.Clear();
                                            Label noAuth = new Label() { Text = "Not Authenticated", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                            analysis.Add(noAuth);
                                        });
                                    }

                                }
                                else
                                {
                                    // show error
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        analysis.Children.Clear();
                                        Label noLoad = new Label() { Text = "Failed Loading", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                        analysis.Add(noLoad);
                                    });
                                }
                            });

                            detailContent.Add(analysis);


                        }
                        else
                        {
                            

                            Grid analysis = new Grid() { Margin = new Thickness(0, 15, 0, 10) };

                            List<int> teamsToAnalyze = new List<int>();
                            foreach (var team in match.teams)
                            {
                                teamsToAnalyze.Add(int.Parse((string)team.team));
                            }

                            ActivityIndicator loadingIndiactor = new ActivityIndicator() { WidthRequest = 50, HeightRequest = 50, Color = Color.FromHex("#ffffff"), IsRunning = true };


                            Label analysisLabel = new Label() { Text = "Match Analysis", Margin = new Thickness(5, 15, 0, 0), FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                            detailContent.Add(analysisLabel);

                            analysis.Add(loadingIndiactor, 0, 0);


                            Grid teamButtons = new Grid() { Margin = new Thickness(10, 10, 10, 0) };
                            detailContent.Add(teamButtons);

                            Task.Run(async () =>
                            {
                                APIResponse response = await APIManager.GetAnalysis(teamsToAnalyze.ToArray());
                                if (response.Status == System.Net.HttpStatusCode.OK)
                                {
                                    dynamic responseData = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                                    var authenticated = (bool)responseData.auth;
                                    if (authenticated)
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {

                                            Dictionary<string, dynamic> analysisData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(responseData.analysis.ToString());

                                            Dictionary<string, StackLayout> analysisViews = new Dictionary<string, StackLayout>();

                                            analysis.Children.Clear();



                                            foreach (string key in analysisData.Keys.ToList())
                                            {

                                                analysisViews[key] = new StackLayout();
                                                analysisViews[key].IsVisible = false;

                                                Label teamNum = new Label() { Text = "Team " + key, Margin = new Thickness(5), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                                analysisViews[key].Add(teamNum);

                                                analysis.Add(analysisViews[key], 0, 0);

                                                teamButtons.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                                                Microsoft.Maui.Controls.Button teamButton = new Microsoft.Maui.Controls.Button() { BackgroundColor = Color.FromHex("#3a0e4d"), Text = key, ClassId = key, Margin = new Thickness(5, 2), Padding = new Thickness(5), TextColor = Color.FromHex("#ffffff"), FontSize = 14 };
                                                teamButtons.Add(teamButton, teamButtons.ColumnDefinitions.Count() - 1, 0);

                                                teamButton.Clicked += (s, e) =>
                                                {
                                                    Microsoft.Maui.Controls.Button sender = (Microsoft.Maui.Controls.Button)s;
                                                    foreach (StackLayout other in analysisViews.Values.ToList())
                                                    {
                                                        other.IsVisible = false;
                                                    }

                                                    analysisViews[sender.ClassId].IsVisible = true;

                                                };


                                                // the actual analysis part
                                                foreach (dynamic part in analysisData[key])
                                                {
                                                    switch ((string)part.type)
                                                    {
                                                        case "Number":
                                                            Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisNumberLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisNumberValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisNumberHolder.Content = analysisNumberValue;
                                                            analysisNumber.Add(analysisNumberLabel, 0, 0);
                                                            analysisNumber.Add(analysisNumberHolder, 1, 0);
                                                            analysisViews[key].Add(analysisNumber);

                                                            break;
                                                        case "Custom":
                                                            Grid analysisCustom = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisCustomLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisCustomHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisCustomValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisCustomHolder.Content = analysisCustomValue;
                                                            analysisCustom.Add(analysisCustomLabel, 0, 0);
                                                            analysisCustom.Add(analysisCustomHolder, 1, 0);
                                                            analysisViews[key].Add(analysisCustom);

                                                            break;
                                                        case "FIRST":
                                                            Grid analysisFIRST = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisFIRSTLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisFIRSTHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                                            Label analysisFIRSTValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            analysisFIRSTHolder.Content = analysisFIRSTValue;
                                                            analysisFIRST.Add(analysisFIRSTLabel, 0, 0);
                                                            analysisFIRST.Add(analysisFIRSTHolder, 1, 0);
                                                            analysisViews[key].Add(analysisFIRST);

                                                            break;
                                                        case "Graph":

                                                            StackLayout analysisGraph = new StackLayout() { Margin = new Thickness(0, 5) };
                                                            Label analysisGraphLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
                                                            Label analysisGraphAverage = new Label() { Text = "Average: " + (string)part.average, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };

                                                            LineChart analysisGraphChart = new LineChart() { AxisLinesColor = Color.FromArgb("#60ffffff"), ChartStyle = AlohaKit.Enums.ChartEnums.LineChartStyle.Line, HeightRequest = 200, DisplayValueLabelsOnTop = true, FontColor = Color.FromHex("#ffffff"), FontSize = 16, DisplayHorizontalAxisLines = true, DisplayVerticalAxisLines = true, DisplayHeaderValues = true, LineColor = Color.FromHex("#680991"), PointColor = Color.FromHex("#680991") };


                                                            Grid dataPoints = new Grid() { Margin = new Thickness(15, 0) };

                                                            List<ChartItem> itemsToShow = new List<ChartItem>();
                                                            int i = 0;
                                                            foreach (var label in part.matches)
                                                            {
                                                                itemsToShow.Add(new ChartItem()
                                                                {
                                                                    Label = "#" + (string)label,
                                                                    Value = int.Parse((string)part.data[i]),
                                                                    IsLabelBold = true
                                                                });
                                                                

                                                                dataPoints.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                                                                Label dataPoint = new Label() { Text = (string)part.data[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5), Opacity=0.7 };
                                                                dataPoints.Add(dataPoint, dataPoints.ColumnDefinitions.Count() - 1, 0);

                                                                i++;
                                                            }

                                                            analysisGraphChart.Entries = new ObservableCollection<ChartItem>(itemsToShow);

                                                            analysisGraph.Add(analysisGraphLabel);
                                                            analysisGraph.Add(analysisGraphAverage);
                                                            
                                                            analysisGraph.Add(analysisGraphChart);
                                                            analysisGraph.Add(dataPoints);
                                                            analysisViews[key].Add(analysisGraph);


                                                            break;
                                                        case "Frequency":
                                                            Grid analysisFrequency = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                                            analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                                            analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                                            Label analysisFrequencyLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                            Frame analysisFrequencyHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };

                                                            StackLayout analysisFrequencyValues = new StackLayout();
                                                            int j = 0;
                                                            foreach (var field in part.fields)
                                                            {
                                                                var value = (string)part.values[j];

                                                                Label analysisFrequencyValue = new Label() { Text = (string)field + " - " + value, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                                                analysisFrequencyValues.Add(analysisFrequencyValue);
                                                                j++;
                                                            }


                                                            analysisFrequencyHolder.Content = analysisFrequencyValues;
                                                            analysisFrequency.Add(analysisFrequencyLabel, 0, 0);
                                                            analysisFrequency.Add(analysisFrequencyHolder, 1, 0);
                                                            analysisViews[key].Add(analysisFrequency);

                                                            break;



                                                            

                                                    }
                                                }
                                            }








                                        });
                                    }
                                    else
                                    {
                                        Device.BeginInvokeOnMainThread(() =>
                                        {
                                            analysis.Children.Clear();
                                            Label noAuth = new Label() { Text = "Not Authenticated", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                            analysis.Add(noAuth);
                                        });
                                    }

                                }
                                else
                                {
                                    // show error
                                    Device.BeginInvokeOnMainThread(() =>
                                    {
                                        analysis.Children.Clear();
                                        Label noLoad = new Label() { Text = "Failed Loading", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                                        analysis.Add(noLoad);
                                    });
                                }
                            });

                            detailContent.Add(analysis);





                            // if (data.team.analysis != null)
                            // {

                            //     Label analysisLabel = new Label() { Text = "Team Summary", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 25, 0, 0) };
                            //     // there is an analysis attached; loop through
                            //     details.Add(analysisLabel);
                            //     // foreach (var analysisPoint in data.team.analysis)
                            //     // {
                            //     //     if (analysisPoint.value == null)
                            //     //     {
                            //     //         continue;
                            //     //     }
                            //     //     switch ((string)analysisPoint.type)
                            //     //     {
                            //     //         case "Number":

                            //     //             Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                            //     //             analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                            //     //             analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            //     //             Label analysisNumberLabel = new Label() { Text = (string)analysisPoint.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            //     //             Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                            //     //             Label analysisNumberValue = new Label() { Text = Math.Round((double)analysisPoint.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            //     //             analysisNumberHolder.Content = analysisNumberValue;
                            //     //             analysisNumber.Add(analysisNumberLabel, 0, 0);
                            //     //             analysisNumber.Add(analysisNumberHolder, 1, 0);
                            //     //             analysis.Add(analysisNumber);
                            //     //             break;
                            //     //         case "Grid":
                            //     //             Label gridNameLabel = new Label() { Text = (string)analysisPoint.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            //     //             var rows = analysisPoint.value;


                            //     //             Grid gridGrid = new Grid { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5, 0, 15) };
                            //     //             for (int rn = 0; rn < rows.Count; rn++)
                            //     //             {
                            //     //                 gridGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            //     //             }
                            //     //             var demoCols = rows[0];
                            //     //             for (int cn = 0; cn < demoCols.Count; cn++)
                            //     //             {
                            //     //                 gridGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                            //     //             }

                            //     //             for (int r = 0; r < rows.Count; r++)
                            //     //             {
                            //     //                 var cols = rows[r];
                            //     //                 for (int c = 0; c < cols.Count; c++)
                            //     //                 {
                            //     //                     Label itemNumber = new Label() { Text = (string)cols[c], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            //     //                     Frame item = new Frame() { BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HeightRequest = 30, Margin = new Thickness(2), Padding = new Thickness(0) };
                            //     //                     item.Content = itemNumber;
                            //     //                     gridGrid.Add(item, c, r);
                            //     //                 }
                            //     //             }

                            //     //             analysis.Add(gridNameLabel);
                            //     //             analysis.Add(gridGrid);


                            //     //             break;
                            //     //     }
                            //     // }


                            //     WebView w = new WebView(){
                            //         Source = "https://scout.robosmrt.com/analysis",
                            //         Margin = new Thickness(0, 20)
                            //     };
                            //     w.HeightRequest = 400;


                            //     details.Add(w);

                            //     details.Add(analysis);
                            // }
                        }


                    }
                    else
                    {
                        Label matchLabel = new Label() { Text = "Match Not Available", FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 20) };
                        detailContent.Add(matchLabel);
                    }

                    

                    break;
            }
        }catch(Exception e){

        }

        
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        



        // assuming that infoData is set as well
    }

    public async void ChangeSearchView(bool show)
    {
        if(show == _searchViewVisible)
        {
            return;
        }

        _searchViewVisible = show;
        if (show)
        {
            Search_Docs_Backdrop.Opacity = 0;
            Search_Docs_Backdrop.IsVisible = true;
            Search_Docs.IsVisible = true;
            Search_Docs_Box.IsVisible = true;
            Search_Docs_Box.TranslationY = -1000;
            Search_Docs_Result_Box.TranslationY = 1000;
            Search_Docs_Backdrop.FadeTo(.5, 250, Easing.CubicInOut);

            ShowSearchResultScreen("Start");
            Search_Docs_Box.TranslateTo(0, -550, 500, Easing.CubicInOut);
            Search_Docs_Result_Box.TranslateTo(0, 550, 500, Easing.CubicInOut);


        }
        else
        {
            Search_Docs_Backdrop.FadeTo(0, 250, Easing.CubicInOut);
            Search_Docs_Result_Box.TranslateTo(0, 1500, 500, Easing.CubicInOut);
            await Search_Docs_Box.TranslateTo(0, -1500, 500, Easing.CubicInOut);
            Search_Docs_Backdrop.IsVisible = false;
            Search_Docs.IsVisible = false;
        }
    }

    public async void ShowSearchResultScreen(string name)
    {
        Search_Docs_Result_Failed.IsVisible = false;
        Search_Docs_Result_Start.IsVisible = false;
        Search_Docs_Result_Loading.IsVisible = false;
        Search_Docs_Result_Info.IsVisible = false;

        ((StackLayout)FindByName("Search_Docs_Result_" + name)).IsVisible = true;
    }

    Microsoft.Maui.Controls.Button currentSearchPage;

    private async void Search_Info_ChangePage(string page)
    {
        Search_Docs_Result_Info_Document.IsVisible = false;
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        if (currentSearchPage != null)
        {
            currentSearchPage.BackgroundColor = Color.FromHex("#3a0e4d");
            ((ScrollView)FindByName("Search_Docs_Result_Info_" + currentSearchPage.ClassId)).IsVisible = false;
        }
        currentSearchPage = null;
        ((ScrollView)FindByName("Search_Docs_Result_Info_" + page)).IsVisible = true;
    }
    private async void Search_Info_ChangePage(object sender, EventArgs e)
    {
        Search_Docs_Result_Info_Document.IsVisible = false;
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        if (currentSearchPage != null)
        {
            currentSearchPage.BackgroundColor = Color.FromHex("#3a0e4d");
            ((ScrollView)FindByName("Search_Docs_Result_Info_" + currentSearchPage.ClassId)).IsVisible = false;
        }
        currentSearchPage = (Microsoft.Maui.Controls.Button)sender;
        currentSearchPage.BackgroundColor = Color.FromHex("#680991");
        ((ScrollView)FindByName("Search_Docs_Result_Info_" + currentSearchPage.ClassId)).IsVisible = true;
    }
    private async void Search_Info_ChangePage_Clear()
    {
        Search_Docs_Result_Info_Document.IsVisible = false;
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        if (currentSearchPage != null)
        {
            currentSearchPage.BackgroundColor = Color.FromHex("#3a0e4d");
            ((ScrollView)FindByName("Search_Docs_Result_Info_" + currentSearchPage.ClassId)).IsVisible = false;
        }
        currentSearchPage = null;
    }


    dynamic _currentSearchItem;
    private async void Search_StartAPISearch(object sender, EventArgs e)
    {
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
        Search_Docs_Result_Info_Document.IsVisible = false;
        Search_Docs_Result_Box.TranslateTo(0, 600, 500, Easing.CubicInOut);
        ShowSearchResultScreen("Loading");
        var typeofdoc = (string)Search_Docs_Type.SelectedItem;
        var value = Search_Docs_Filter.Text;
        APIResponse response;
        try
        {
            if (typeofdoc == "Match")
            {
                response = await APIManager.GetMatchInformation(int.Parse(value));
                if (response.Status == System.Net.HttpStatusCode.NotFound)
                {
                    ShowSearchResultScreen("Failed");
                    PhysicalVibrations.TryVibrate(1000);
                    Search_Docs_Result_Box.TranslateTo(0, 650, 500, Easing.CubicInOut);
                    return;
                }
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                _currentSearchItem = data;
                ShowSearchResultScreen("Info");
                Search_Docs_Result_InfoTitle.Text = "Match " + (string)data.match.matchNumber;
                Search_Docs_Result_Box.TranslateTo(0, 300, 500, Easing.CubicInOut);

                Search_Docs_Result_Info_Details.Children.Clear();

                Grid teamGrid = new Grid() { Margin = new Thickness(0, 10) };
                teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                teamGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                for (int i = 0; i < data.match.teams.Count / 2; i++)
                {
                    teamGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }

                bool auth = (bool)data.auth;

                var blueCount = 0;
                var redCount = 0;
                foreach (var team in data.match.teams)
                {
                    Frame f = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false };
                    Label teamLabel = new Label() { Text = (string)team.team, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                    f.Content = teamLabel;
                    if ((string)team.color == "Red")
                    {
                        f.BackgroundColor = Color.FromHex("#910929");
                        teamGrid.Add(f, redCount, 0);
                        redCount++;
                    }
                    else if ((string)team.color == "Blue")
                    {
                        f.BackgroundColor = Color.FromHex("#290991");
                        teamGrid.Add(f, blueCount, 1);
                        blueCount++;
                    }
                }
                Search_Docs_Result_Info_Details.Add(teamGrid);
                

                if (data.match.results == null || !((bool)data.match.results.finished))
                {
                    // match has NOT completed
                    Label status = new Label() { Text = "Match Not Complete", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 0.7 };
                    Search_Docs_Result_Info_Details.Add(status);
                }
                else
                {
                    // match has completed
                    Label status = new Label() { Text = "Completed Match Stats", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10) };
                    Search_Docs_Result_Info_Details.Add(status);
                    Frame winnerFrame = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 4, HasShadow = false };
                    if (int.Parse((string)data.match.results.red) > int.Parse((string)data.match.results.blue))
                    {
                        // red won
                        Label winner = new Label() { Text = "Red Won " + ((string)data.match.results.red) + " - " + ((string)data.match.results.blue), FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10) };
                        winnerFrame.BackgroundColor = Color.FromHex("#910929");
                        winnerFrame.Content = winner;
                    }
                    else if (int.Parse((string)data.match.results.red) < int.Parse((string)data.match.results.blue))
                    {
                        // blue won
                        Label winner = new Label() { Text = "Blue Won " + ((string)data.match.results.blue) + " - " + ((string)data.match.results.red), FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10) };
                        winnerFrame.BackgroundColor = Color.FromHex("#290991");
                        winnerFrame.Content = winner;
                    }
                    else
                    {
                        // tie
                        Label winner = new Label() { Text = "Tie " + ((string)data.match.results.red) + " - " + ((string)data.match.results.blue), FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 10) };
                        winnerFrame.BackgroundColor = Color.FromHex("#5a5a5a");
                        winnerFrame.Content = winner;
                    }
                    Search_Docs_Result_Info_Details.Add(winnerFrame);

                    if (data.match.results.redStats != null)
                    {
                        Dictionary<string, object> red = data.match.results.redStats.ToObject<Dictionary<string, object>>();
                        Dictionary<string, object> blue = data.match.results.redStats.ToObject<Dictionary<string, object>>();
                        Grid matchStats = new Grid() { Margin = new Thickness(0, 10) };
                        matchStats.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
                        matchStats.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        matchStats.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        foreach (var key in red.Keys)
                        {
                            matchStats.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                            Label keyLabel = new Label() { Text = key, FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };

                            Frame redFrame = new Frame() { Padding = new Thickness(5, 5), Margin = new Thickness(0), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 0, HasShadow = false, BackgroundColor = Color.FromHex("#910929") };
                            Frame blueFrame = new Frame() { Padding = new Thickness(5, 5), Margin = new Thickness(0), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 0, HasShadow = false, BackgroundColor = Color.FromHex("#290991") };

                            Label redLabel = new Label() { Text = red[key].ToString(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            Label blueLabel = new Label() { Text = blue[key].ToString(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };

                            redFrame.Content = redLabel;
                            blueFrame.Content = blueLabel;

                            matchStats.Add(keyLabel, 0, matchStats.RowDefinitions.Count - 1);
                            matchStats.Add(redFrame, 1, matchStats.RowDefinitions.Count - 1);
                            matchStats.Add(blueFrame, 2, matchStats.RowDefinitions.Count - 1);
                        }


                        Search_Docs_Result_Info_Details.Add(matchStats);
                    }




                    Search_Docs_Result_Info_Documents.Children.Clear();
                    if (auth)
                    {
                        if (data.match.documents.Count == 0)
                        {
                            Label noDocuments = new Label() { Text = "No Documents", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 0.7 };
                            Search_Docs_Result_Info_Documents.Add(noDocuments);
                        }
                        foreach (var doc in data.match.documents)
                        {

                            bool flagged = (bool)doc.flagged;

                            Frame docFrame = new Frame() { Padding = new Thickness(5), Margin = new Thickness(5), BorderColor = flagged ? Color.FromHex("#ffff00") : Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                            dynamic docData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)doc.json);

                            Grid docInfo = new Grid() { Margin = new Thickness(10, 5) };

                            docInfo.ClassId = (string)doc._id;
                            docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                            docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                            docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                            var docNameText = "Unknown Team";
                            if (docData.team != null)
                            {
                                docNameText = "Team " + (string)docData.team;
                            }
                            Label docName = new Label() { Text = docNameText, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                            Label docType = new Label() { Text = ((string)docData.type).ToUpper(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };
                            Microsoft.Maui.Controls.Button docView = new Microsoft.Maui.Controls.Button() { Text = "View", FontSize = 12, Padding = new Thickness(2), TextColor = Color.FromHex("#ffffff"), BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, CornerRadius = 4, Margin = new Thickness(0, 0, 0, 0), ClassId = (string)doc._id };
                            docView.Clicked += GoToDocumentView;
                            docInfo.Add(docName, 0, 0);
                            docInfo.Add(docType, 1, 0);
                            docInfo.Add(docView, 2, 0);

                            docFrame.Content = docInfo;

                            Search_Docs_Result_Info_Documents.Add(docFrame);
                        }
                    }
                    else
                    {
                        Label noAuth = new Label() { Text = "Not Authenticated", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                        Search_Docs_Result_Info_Documents.Add(noAuth);
                    }


                    




                }


                Search_Info_ChangePage_Clear();
            }
            else if (typeofdoc == "Team")
            {
                response = await APIManager.GetTeamInformation(int.Parse(value));
                if (response.Status == System.Net.HttpStatusCode.NotFound)
                {
                    ShowSearchResultScreen("Failed");
                    PhysicalVibrations.TryVibrate(1000);
                    Search_Docs_Result_Box.TranslateTo(0, 650, 500, Easing.CubicInOut);

                    return;
                }
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(response.Content);
                _currentSearchItem = data;
                ShowSearchResultScreen("Info");
                Search_Docs_Result_InfoTitle.Text = "Team " + (string)data.team.teamNumber;
                Search_Docs_Result_Box.TranslateTo(0, 300, 500, Easing.CubicInOut);


                Search_Docs_Result_Info_Details.Children.Clear();
                Label teamName = new Label() { Text = (string)data.team.name, FontSize = 32, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 5) };
                Search_Docs_Result_Info_Details.Add(teamName);

                bool auth = (bool)data.auth;
                Label matchesLabel = new Label() { Text = "Matches This Competition", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 15, 0, 10) };

                StackLayout details_matches = new StackLayout();

                foreach (var match in data.team.matches)
                {
                    if ((bool)match.finished)
                    {
                        // match is done
                        Frame matchFrame = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                        Grid matchDetails = new Grid();
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

                        var result = "T";
                        var scoreString = "";
                        if (int.Parse((string)match.score.red) > int.Parse((string)match.score.blue))
                        {
                            if (((string)match.color).ToLower() == "red")
                                result = "W";
                            else
                                result = "L";
                            scoreString = (string)match.score.red + " - " + (string)match.score.blue;
                        }
                        else if (int.Parse((string)match.score.red) < int.Parse((string)match.score.blue))
                        {
                            if (((string)match.color).ToLower() == "blue")
                                result = "W";
                            else
                                result = "L";
                            scoreString = (string)match.score.blue + " - " + (string)match.score.red;
                        }

                        Label matchNumber = new Label() { Text = "Match " + (string)match.matchNumber, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                        Label matchStatus = new Label() { Text = result, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                        Label matchScore = new Label() { Text = scoreString, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };
                        matchDetails.Add(matchNumber, 0, 0);
                        matchDetails.Add(matchStatus, 1, 0);
                        matchDetails.Add(matchScore, 2, 0);
                        matchFrame.Content = matchDetails;
                        details_matches.Add(matchFrame);
                    }
                    else
                    {
                        // match is not done
                        Frame matchFrame = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                        Grid matchDetails = new Grid();
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        matchDetails.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        Label matchNumber = new Label() { Text = "Match " + (string)match.matchNumber, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                        Label matchStatus = new Label() { Text = "Not Done", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };

                        matchDetails.Add(matchNumber, 0, 0);
                        matchDetails.Add(matchStatus, 1, 0);
                        matchFrame.Content = matchDetails;
                        details_matches.Add(matchFrame);
                    }

                }



                Search_Docs_Result_Info_Details.Add(matchesLabel);
                Search_Docs_Result_Info_Details.Add(details_matches);


                StackLayout analysis = new StackLayout() { Margin = new Thickness(0, 15, 0, 10) };

                if (data.team.analysis != null)
                {

                    Label analysisLabel = new Label() { Text = "Team Summary", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 25, 0, 0) };
                    // there is an analysis attached; loop through
                    Search_Docs_Result_Info_Details.Add(analysisLabel);
                    Dictionary<string, dynamic> analysisData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(data.team.analysis.ToString());
                    foreach (dynamic part in analysisData[(string)data.team.teamNumber])
                    {
                        switch ((string)part.type)
                        {
                            case "Number":
                                Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                Label analysisNumberLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                Label analysisNumberValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                analysisNumberHolder.Content = analysisNumberValue;
                                analysisNumber.Add(analysisNumberLabel, 0, 0);
                                analysisNumber.Add(analysisNumberHolder, 1, 0);
                                analysis.Add(analysisNumber);

                                break;
                            case "Custom":
                                Grid analysisCustom = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                analysisCustom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                Label analysisCustomLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                Frame analysisCustomHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                Label analysisCustomValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                analysisCustomHolder.Content = analysisCustomValue;
                                analysisCustom.Add(analysisCustomLabel, 0, 0);
                                analysisCustom.Add(analysisCustomHolder, 1, 0);
                                analysis.Add(analysisCustom);

                                break;
                            case "FIRST":
                                Grid analysisFIRST = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                analysisFIRST.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                Label analysisFIRSTLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                Frame analysisFIRSTHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                                Label analysisFIRSTValue = new Label() { Text = Math.Round((double)part.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                analysisFIRSTHolder.Content = analysisFIRSTValue;
                                analysisFIRST.Add(analysisFIRSTLabel, 0, 0);
                                analysisFIRST.Add(analysisFIRSTHolder, 1, 0);
                                analysis.Add(analysisFIRST);

                                break;
                            case "Graph":

                                StackLayout analysisGraph = new StackLayout() { Margin = new Thickness(0, 5) };
                                Label analysisGraphLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5) };
                                Label analysisGraphAverage = new Label() { Text = "Average: " + (string)part.average, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 10) };

                                LineChart analysisGraphChart = new LineChart() { AxisLinesColor = Color.FromArgb("#60ffffff"), ChartStyle = AlohaKit.Enums.ChartEnums.LineChartStyle.Line, HeightRequest = 200, DisplayValueLabelsOnTop = true, FontColor = Color.FromHex("#ffffff"), FontSize = 16, DisplayHorizontalAxisLines = true, DisplayVerticalAxisLines = true, DisplayHeaderValues = true, LineColor = Color.FromHex("#680991"), PointColor = Color.FromHex("#680991") };


                                Grid dataPoints = new Grid() { Margin = new Thickness(15, 0) };

                                List<ChartItem> itemsToShow = new List<ChartItem>();
                                int i = 0;
                                foreach (var label in part.matches)
                                {
                                    itemsToShow.Add(new ChartItem()
                                    {
                                        Label = "#" + (string)label,
                                        Value = int.Parse((string)part.data[i]),
                                        IsLabelBold = true
                                    });


                                    dataPoints.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

                                    Label dataPoint = new Label() { Text = (string)part.data[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 5), Opacity = 0.7 };
                                    dataPoints.Add(dataPoint, dataPoints.ColumnDefinitions.Count() - 1, 0);

                                    i++;
                                }

                                analysisGraphChart.Entries = new ObservableCollection<ChartItem>(itemsToShow);

                                analysisGraph.Add(analysisGraphLabel);
                                analysisGraph.Add(analysisGraphAverage);

                                analysisGraph.Add(analysisGraphChart);
                                analysisGraph.Add(dataPoints);
                                analysis.Add(analysisGraph);


                                break;
                            case "Frequency":
                                Grid analysisFrequency = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                                analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                                analysisFrequency.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                                Label analysisFrequencyLabel = new Label() { Text = (string)part.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                Frame analysisFrequencyHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };

                                StackLayout analysisFrequencyValues = new StackLayout();
                                int j = 0;
                                foreach (var field in part.fields)
                                {
                                    var val = (string)part.values[j];

                                    Label analysisFrequencyValue = new Label() { Text = (string)field + " - " + val, FontSize = 18, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                    analysisFrequencyValues.Add(analysisFrequencyValue);
                                    j++;
                                }


                                analysisFrequencyHolder.Content = analysisFrequencyValues;
                                analysisFrequency.Add(analysisFrequencyLabel, 0, 0);
                                analysisFrequency.Add(analysisFrequencyHolder, 1, 0);
                                analysis.Add(analysisFrequency);

                                break;



                               

                        }
                    }
                    // foreach (var analysisPoint in data.team.analysis)
                    // {
                    //     if (analysisPoint.value == null)
                    //     {
                    //         continue;
                    //     }
                    //     switch ((string)analysisPoint.type)
                    //     {
                    //         case "Number":

                    //             Grid analysisNumber = new Grid() { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5) };
                    //             analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    //             analysisNumber.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    //             Label analysisNumberLabel = new Label() { Text = (string)analysisPoint.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                    //             Frame analysisNumberHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.Center };
                    //             Label analysisNumberValue = new Label() { Text = Math.Round((double)analysisPoint.value, 2).ToString(), FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                    //             analysisNumberHolder.Content = analysisNumberValue;
                    //             analysisNumber.Add(analysisNumberLabel, 0, 0);
                    //             analysisNumber.Add(analysisNumberHolder, 1, 0);
                    //             analysis.Add(analysisNumber);
                    //             break;
                    //         case "Grid":
                    //             Label gridNameLabel = new Label() { Text = (string)analysisPoint.name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                    //             var rows = analysisPoint.value;


                    //             Grid gridGrid = new Grid { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0, 5, 0, 15) };
                    //             for (int rn = 0; rn < rows.Count; rn++)
                    //             {
                    //                 gridGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                    //             }
                    //             var demoCols = rows[0];
                    //             for (int cn = 0; cn < demoCols.Count; cn++)
                    //             {
                    //                 gridGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                    //             }

                    //             for (int r = 0; r < rows.Count; r++)
                    //             {
                    //                 var cols = rows[r];
                    //                 for (int c = 0; c < cols.Count; c++)
                    //                 {
                    //                     Label itemNumber = new Label() { Text = (string)cols[c], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                    //                     Frame item = new Frame() { BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HeightRequest = 30, Margin = new Thickness(2), Padding = new Thickness(0) };
                    //                     item.Content = itemNumber;
                    //                     gridGrid.Add(item, c, r);
                    //                 }
                    //             }

                    //             analysis.Add(gridNameLabel);
                    //             analysis.Add(gridGrid);


                    //             break;
                    //     }
                    // }


                    //WebView w = new WebView(){
                    //    Source = "https://scout.robosmrt.com/analysis",
                    //    Margin = new Thickness(0, 20)
                    //};
                    //w.HeightRequest = 400;


                    //details.Add(w);

                    Search_Docs_Result_Info_Details.Add(analysis);
                }




                Search_Docs_Result_Info_Documents.Children.Clear();

                if (auth)
                {

                    if (data.team.documents.Count == 0)
                    {
                        Label noDocuments = new Label() { Text = "No Documents", FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 0.7 };
                        Search_Docs_Result_Info_Documents.Add(noDocuments);
                    }
                    foreach (var doc in data.team.documents)
                    {




                        bool flagged = doc.flagged == null ? false : (bool)doc.flagged;

                        Frame docFrame = new Frame() { Padding = new Thickness(5), Margin = new Thickness(5), BorderColor = flagged ? Color.FromHex("#ffff00") : Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                        dynamic docData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)doc.json);

                        Grid docInfo = new Grid() { Margin = new Thickness(10, 5) };

                        docInfo.ClassId = (string)doc._id;
                        docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        docInfo.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var docNameText = "Unknown Match";
                        if (docData.match != null && (string)docData.match != "undefined")
                        {
                            if(docData.general != null && (bool)docData.general == true)
                            {
                                docNameText = "General Document";
                            }
                            else
                            {
                                docNameText = "Match " + (string)docData.match;
                            }
                            
                        }
                        Label docName = new Label() { Text = docNameText, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                        Label docType = new Label() { Text = ((string)docData.type).ToUpper(), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center };

                        docInfo.Add(docName, 0, 0);
                        docInfo.Add(docType, 1, 0);

                        if ((string)docData.type == "note" || (string)docData.type == "tablet")
                        {
                            Microsoft.Maui.Controls.Button docView = new Microsoft.Maui.Controls.Button() { Text = "View", FontSize = 12, Padding = new Thickness(2), TextColor = Color.FromHex("#ffffff"), BackgroundColor = Color.FromHex("#680991"), HorizontalOptions = LayoutOptions.End, VerticalOptions = LayoutOptions.Center, CornerRadius = 4, Margin = new Thickness(0, 0, 0, 0), ClassId = (string)doc._id };
                            docView.Clicked += GoToDocumentView;
                            docInfo.Add(docView, 2, 0);
                        }


                        docFrame.Content = docInfo;

                        Search_Docs_Result_Info_Documents.Add(docFrame);
                    }
                }
                else
                {
                    Label noAuth = new Label() { Text = "Not Authenticated", Margin = new Thickness(5), FontSize = 28, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                    Search_Docs_Result_Info_Documents.Add(noAuth);
                }



                



                Search_Info_ChangePage_Clear();
            }
            else if (typeofdoc == "Scouter")
            {
                ShowSearchResultScreen("Failed");
                PhysicalVibrations.TryVibrate(1000);
                Search_Docs_Result_Box.TranslateTo(0, 650, 500, Easing.CubicInOut);
            }
        }
        catch(Exception searchex)
        {
            ShowSearchResultScreen("Failed");
            PhysicalVibrations.TryVibrate(1000);
            Search_Docs_Result_Box.TranslateTo(0, 650, 500, Easing.CubicInOut);
        }
        
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);

    }

    public async void GoToDocumentView(object sender, EventArgs e){
        Search_Info_ChangePage("Document");

        Microsoft.Maui.Controls.Button doc = (Microsoft.Maui.Controls.Button)sender;
        var findDocID = doc.ClassId;
        Search_Docs_Result_Info_Document.Children.Clear();


        dynamic document = null;
        if(_currentSearchItem.match == null){
            // must be from teams
            foreach(var d in _currentSearchItem.team.documents){
                if((string)d._id == (string)findDocID){
                    document = d;
                    break;
                }
            }

        }else{
            // must be from matches
            foreach(var d in _currentSearchItem.match.documents){
                if((string)d._id == (string)findDocID){
                    document = d;
                    break;
                }
            }
        }

        if(document != null){
            dynamic docData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)document.json);
            var name = "Team " + (string)docData.team + " " + ((string)docData.type).ToUpper();
            if(document.name != null && (string)document.name != ""){
                name = (string)document.name;
            }

            var flagged = document.flagged == null ? false : (bool)document.flagged;


            Label documentTitle = new Label() { Text = name, FontSize = 28, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
            Search_Docs_Result_Info_Document.Add(documentTitle);

            Grid actionGrid = new Grid() { Margin = new Thickness(10,10, 10, 20)};
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            actionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Microsoft.Maui.Controls.Button flaggedIndicator = new Microsoft.Maui.Controls.Button() { Text = flagged ? "Remove Flag" : "Add Flag", FontSize = 16, TextColor = Color.FromHex("#ffffff"), BackgroundColor = flagged ? Color.FromHex("#680991") : Color.FromHex("#3a0e4d"), Margin = new Thickness(5), VerticalOptions = LayoutOptions.Center, ClassId = (string)document._id };
            Microsoft.Maui.Controls.Button deleteButton = new Microsoft.Maui.Controls.Button() { Text = "Delete", FontSize = 16, TextColor = Color.FromHex("#ffffff"), BackgroundColor = Color.FromHex("#910929"), Margin = new Thickness(5), VerticalOptions = LayoutOptions.Center, ClassId = (string)document._id };
            
            flaggedIndicator.Clicked += async (object s, EventArgs e) => {
                flaggedIndicator.IsEnabled = false;
                APIResponse response = await APIManager.FlagDocument((string)document._id, flaggedIndicator.Text == "Flagged" ? false : true);
                
                if (response.Status == System.Net.HttpStatusCode.OK){
                    flagged = flaggedIndicator.Text == "Remove Flag" ? false : true;
                    flaggedIndicator.Text = flagged ? "Remove Flag" : "Add Flag";
                    flaggedIndicator.BackgroundColor = flagged ? Color.FromHex("#680991") : Color.FromHex("#3a0e4d");
                }else{
                    await DisplayAlert("Oops!", "There was an error changing the flag status on this document. Please try again later.", "OK");
                }
                flaggedIndicator.IsEnabled = true;
            };
            deleteButton.Clicked += async (object s, EventArgs e) => {
                APIResponse response = await APIManager.DeleteDocument((string)document._id);
                if(response.Status == System.Net.HttpStatusCode.OK){
                    // just reload the entire page
                    Search_StartAPISearch(null, null);
                }else{
                    await DisplayAlert("Oops!", "There was an error deleting this document. Please try again later.", "OK");
                }
            };
            
            actionGrid.Add(flaggedIndicator, 0, 0);
            actionGrid.Add(deleteButton, 1, 0);
            Search_Docs_Result_Info_Document.Add(actionGrid);

            try{
                StackLayout documentContents = new StackLayout(){Margin = new Thickness(0,10,0,0)};
                 switch((string)docData.type){
                    case "note":
                        Frame noteFrame = new Frame() { BackgroundColor = Color.FromHex("#190024"), Padding = new Thickness(5, 10), Margin = new Thickness(5), CornerRadius = 8, HasShadow = false, BorderColor = Color.FromArgb("00ffffff") };
                        Label noteText = new Label() { Text = (string)docData.contents, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                        noteFrame.Content = noteText;
                        Label author = new Label() { Text = "by " + (docData.author == null || (string)docData.author == "" ? "???" : (string)docData.author), FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.End, VerticalTextAlignment = TextAlignment.Center, Opacity = 0.5 };
                        documentContents.Add(noteFrame);
                        documentContents.Add(author);
                        break;
                    case "tablet":
                        var selectedSchema = StorageManagement.allSchemas.Find(s => s.Name == (string)docData.schema);
                        if(selectedSchema == null)
                        {
                            Label noSchema = new Label() { Text = "There is no appropriate schema on this device to view this document...", FontSize = 20, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            documentContents.Add(noSchema);
                        }
                        else
                        {
                            Label useSchema = new Label() { Text = "Using " + (string)docData.schema, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                            documentContents.Add(useSchema);

                            dynamic schemaObject = Newtonsoft.Json.JsonConvert.DeserializeObject(selectedSchema.Data);
                            dynamic tabletData = Newtonsoft.Json.JsonConvert.DeserializeObject((string)docData.data);
                            var i = 0;
                            foreach(var part in schemaObject.Parts)
                            {
                                Label sectionLabel = new Label() { Text = (string)part.Name, FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Margin = new Thickness(0, 20, 0, 10) };
                                documentContents.Add(sectionLabel);
                                foreach (var comp in part.Components)
                                {
                                    if((string)comp.Type == "Label")
                                    {
                                        continue;
                                    }

                                    if((string)comp.Type == "Grid")
                                    {


                                        var rows = ((string)tabletData[i]).Split("*");
                                        Label gridLabel = new Label() { Text = (string)comp.Name, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                        documentContents.Add(gridLabel);

                                        Grid gridGrid = new Grid { ColumnSpacing = 0, RowSpacing = 0, Margin = new Thickness(0,5,0,15) };
                                        for(int rn = 0; rn < rows.Length; rn++)
                                        {
                                            gridGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                                        }
                                        var demoCols = rows[0].Split(",");
                                        for(int cn = 0; cn < demoCols.Length; cn++)
                                        {
                                            gridGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
                                        }

                                        for(int r = 0; r < rows.Length; r++)
                                        {
                                            var cols = rows[r].Split(",");
                                            for(int c = 0; c < cols.Length; c++)
                                            {
                                                if (int.Parse(cols[c]) == i)
                                                {
                                                    Frame item = new Frame() { BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#680991"), HeightRequest = 30, Margin = new Thickness(2) };
                                                    gridGrid.Add(item, c, r);
                                                }
                                                else
                                                {
                                                    Frame item = new Frame() { BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HeightRequest = 30, Margin = new Thickness(2) };
                                                    gridGrid.Add(item, c, r);
                                                }
                                            }
                                        }

                                        documentContents.Add(gridGrid);


                                        i += 1;
                                        continue;
                                    }else if((string)comp.Type == "Multi-Select")
                                    {
                                        Label multiSelectLabel = new Label() { Text = (string)comp.Name, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                                        documentContents.Add(multiSelectLabel);

                                        

                                        StackLayout multiSelectData = new StackLayout() { Orientation = StackOrientation.Vertical };
                                        foreach (string item in ((string)tabletData[i]).Split(";"))
                                        {
                                            Frame multiSelectDataHolder = new Frame() { Padding = new Thickness(10, 4), Margin = new Thickness(4), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d") };
                                            multiSelectDataHolder.Content = new Label() { Text = item, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.End };
                                            multiSelectData.Children.Add(multiSelectDataHolder);
                                        }

                                        documentContents.Add(multiSelectData);

                                        i += 1;
                                        continue;
                                    }

                                    Grid docDataGrid = new Grid() { ColumnSpacing = 0, RowSpacing = 0 };
                                    docDataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
                                    docDataGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

                                    Label nameLabel = new Label() { Text = (string)comp.Name, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Start, VerticalTextAlignment = TextAlignment.Center };
                                    docDataGrid.Add(nameLabel, 0, 0);

                                    switch ((string)comp.Type)
                                    {
                                        case "Step":

                                            Frame stepDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label stepData = new Label() { Text = (string)tabletData[i], FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                            stepDataHolder.Content = stepData;
                                            docDataGrid.Add(stepDataHolder, 1, 0);
                                            break;
                                        case "Timer":
                                            Frame timerDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label timerData = new Label() { Text = (string)tabletData[i] + "s", FontSize = 20, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                            timerDataHolder.Content = timerData;
                                            docDataGrid.Add(timerDataHolder, 1, 0);
                                            break;
                                        case "Check":
                                            Frame checkDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = (string)tabletData[i] == (string)comp.On ? Color.FromHex("#680991") : Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label checkData = new Label() { Text = (string)tabletData[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                                            checkDataHolder.Content = checkData;
                                            docDataGrid.Add(checkDataHolder, 1, 0);
                                            break;
                                        case "Select":
                                            Frame selectDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label selectData = new Label() { Text = (string)tabletData[i] == "" ? "N/A" : (string)tabletData[i], FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.End };
                                            selectDataHolder.Content = selectData;
                                            docDataGrid.Add(selectDataHolder, 1, 0);
                                            break;
                                        
                                        case "Input":
                                            Frame inputDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label inputData = new Label() { Text = (string)tabletData[i] == "" ? "N/A" : (string)tabletData[i], FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.End };
                                            inputDataHolder.Content = inputData;
                                            docDataGrid.Add(inputDataHolder, 1, 0);
                                            break;
                                        case "Event":
                                            Frame eventDataHolder = new Frame() { Padding = new Thickness(10, 5), Margin = new Thickness(5), BorderColor = Color.FromArgb("00ffffff"), CornerRadius = 8, HasShadow = false, BackgroundColor = Color.FromHex("#3a0e4d"), HorizontalOptions = LayoutOptions.End };
                                            Label eventData = new Label() { Text = ((string)tabletData[i]).Split(";").Length + " event(s)", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.End };
                                            eventDataHolder.Content = eventData;
                                            docDataGrid.Add(eventDataHolder, 1, 0);
                                            break;

                                    }

                                    documentContents.Add(docDataGrid);

                                    i += 1;
                                }
                            }

                        }
                        break;
                 }
                Search_Docs_Result_Info_Document.Add(documentContents);
            
            }catch(Exception dex){
                Label noAuth = new Label() { Text = "Cannot Load Data", Margin=new Thickness(5), FontSize = 18, TextColor = Color.FromHex("#910929"), HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center, Opacity = 1 };
                Search_Docs_Result_Info_Document.Add(noAuth);
            }
           


           
        }

        

    }


}