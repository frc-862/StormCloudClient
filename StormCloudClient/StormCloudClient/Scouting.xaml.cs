using StormCloudClient.Classes;
using StormCloudClient.Services;

namespace StormCloudClient;

public class TimerSet
{
    public DateTime track;
    public float seconds;
    public bool enabled;
}

public partial class Scouting : ContentPage
{

    
    public List<string> data = new List<string>();
    public List<string> componentTypes = new List<string>();
    public List<string> extraData = new List<string>();
    public Dictionary<int, List<object>> attachedComponents = new Dictionary<int, List<object>>();
    public Dictionary<int, TimerSet> timers = new Dictionary<int, TimerSet>();
    public string SchemaName;
    public string Environment;
    public DateTime start;

    public int Team;
    public int Number;
    public string Scouter;
    public string AllianceColor;

    MainPage _ref;


    

    public Scouting(string SchemaName, string Environment, MainPage mainMenu)
	{
        this.Environment = Environment;
        this.SchemaName = SchemaName;
        _ref = mainMenu;
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        var Schema = StorageManagement.allSchemas.Find(s => s.Name == SchemaName);


        var defaultScouter = DataManagement.GetValue("default_scouter");
        if (defaultScouter != null)
            Status_PreContent_ScouterName.Text = (string)defaultScouter;

        LoadSchema(Schema.Data);



        Device.StartTimer(TimeSpan.FromMilliseconds(200), () =>
        {
            var now = DateTime.Now;
            foreach (var item in timers)
            {
                if (item.Value.enabled)
                {
                    var time = item.Value.seconds + (now - item.Value.track).TotalSeconds;
                    data[item.Key] = Math.Round(time, 2).ToString();
                    ((Label)attachedComponents[item.Key][2]).Text = Math.Round(time,2).ToString() + "s";
                }
            }

            var timeBetween = (now - start);
            var seconds = timeBetween.Seconds.ToString().PadLeft(2, '0');
            var minutes = timeBetween.Minutes.ToString();
            Match_Time.Text = minutes + ":" + seconds;


            return true;
        });
    }

    private async void Back(object sender, EventArgs e)
    {
        Navigation.PopAsync();
    }

    private async void SaveMatch(object sender, EventArgs e)
    {
        var stringContents = Newtonsoft.Json.JsonConvert.SerializeObject(data);



        StorageManagement.AddData_Match(Number, Team, Scouter, AllianceColor, SchemaName, Environment, stringContents);

        Navigation.PopAsync();
    }

    public void LoadSchema(string schemaJSON)
    {
        try
        {
            dynamic schemaObject = Newtonsoft.Json.JsonConvert.DeserializeObject(schemaJSON);
            // set base form info
            Console.WriteLine(Enumerable.Count(schemaObject.Parts));
            int componentId = 0;
            // go through each part
            foreach(dynamic part in schemaObject.Parts)
            {
               
                // add part title
                Label title = new Label() { Text = part.Name, FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0,20) };
                Form_Content_Fields.Add(title);

                foreach(dynamic component in part.Components)
                {
                    if((string)component.Type == "Label")
                    {

                        if((string)component.Name == "")
                        {
                            Label titleText = new Label() { Text = (string)component.Contents, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(20, 20, 20, 5), HorizontalTextAlignment = TextAlignment.Center };
                            Form_Content_Fields.Add(titleText);
                        }

                        
                        Label mainText = new Label() { Text = (string)component.Contents, FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(20, (string)component.Name == "" ? 20 : 5, 20, 20), HorizontalTextAlignment = TextAlignment.Center };

                        Form_Content_Fields.Add(mainText);

                        continue;
                    }
                    // part for the textbox
                    ColumnDefinition textBox = new ColumnDefinition();
                    textBox.Width = new GridLength(2, GridUnitType.Star);
                    // part for the field content
                    ColumnDefinition content = new ColumnDefinition();
                    content.Width = new GridLength(3, GridUnitType.Star);
                    // actual grid
                    Grid container = new Grid() { Margin = new Thickness(0,5)};
                    container.ColumnDefinitions.Add(textBox);
                    container.ColumnDefinitions.Add(content);

                    data.Add("");
                    extraData.Add("");
                    componentTypes.Add((string)component.Type);

                    switch ((string)component.Type)
                    {
                        case "Step":
                            Grid stepperView = new Grid() { Margin=new Thickness(5,5)};
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

                            Button downButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "-", FontSize=16, ClassId=componentId.ToString(), TextColor = Color.FromHex("#ffffff") };
                            Entry value = new Entry() { Keyboard = Keyboard.Numeric, FontSize = 18, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Text = (string)component.Min, ClassId = componentId.ToString(), TextColor= Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center };
                            Button upButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "+", FontSize = 16, ClassId = componentId.ToString(), TextColor = Color.FromHex("#ffffff") };

                            downButton.Clicked += HandleFormButton;
                            value.TextChanged += HandleFormEntry;
                            upButton.Clicked += HandleFormButton;

                            stepperView.Add(downButton, 0, 0);
                            stepperView.Add(value, 1, 0);
                            stepperView.Add(upButton, 2, 0);
                            container.Add(stepperView, 1, 0);
                            // add initial data as the minimum value for the step component
                            data[componentId] = (string)component.Min;
                            extraData[componentId] = (string)component.Min + ";" + (string)component.Max;
                            // add the initial attached components
                            attachedComponents[componentId] = new List<object>
                            {
                                downButton, value, upButton
                            };

                            break;
                        case "Check":
                            Grid buttonView = new Grid() { Margin = new Thickness(0, 5) };
                            buttonView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            buttonView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            Button offButton = new Button() { BackgroundColor = Color.FromHex("#680991"), Text = (string)component.Off, ClassId=componentId.ToString(), Margin=new Thickness(5,0), TextColor = Color.FromHex("#ffffff") };
                            Button onButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = (string)component.On, ClassId=componentId.ToString(), Margin = new Thickness(5, 0), TextColor = Color.FromHex("#ffffff") };

                            offButton.Clicked += HandleFormButton;
                            onButton.Clicked += HandleFormButton;
                            
                            buttonView.Add(offButton, 0, 0);
                            buttonView.Add(onButton, 1, 0);
                            container.Add(buttonView, 1, 0);

                            data[componentId] = (string)component.Off;
                            attachedComponents[componentId] = new List<object>
                            {
                                offButton, onButton
                            };
                            
                            break;
                        case "Select":
                            Frame pickerFrame = new Frame() { CornerRadius = 8, Margin = new Thickness(5, 5), BackgroundColor = Color.FromHex("#280338"), Padding = new Thickness(5, 0), BorderColor = Color.FromArgb("00ffffff") };
                            Frame borderProtect = new Frame() { CornerRadius = 4, Padding = new Thickness(0), BorderColor = Color.FromHex("#280338") };
                            Picker selection = new Picker() { FontSize = 16, BackgroundColor = Color.FromHex("#280338"), TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, ClassId = componentId.ToString() };

                            borderProtect.Content = selection;
                            pickerFrame.Content = borderProtect;
                            
                            List<string> items = new List<string>();
                            foreach(dynamic option in component.Options)
                            {
                                items.Add((string)option);
                            }
                            selection.ItemsSource = items;

                            selection.SelectedIndexChanged += HandleFormPicker;

                            data[componentId] = "";
                            attachedComponents[componentId] = new List<object>
                            {
                                selection
                            };
                            container.Add(pickerFrame, 1, 0);

                            break;
                        case "Event":
                            Button eventTrigger = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = (string)component.Trigger, ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff") };

                            eventTrigger.Clicked += HandleFormButton;
                            container.Add(eventTrigger, 1, 0);
                            data[componentId] = "";
                            extraData[componentId] = (string)component.Max;
                            attachedComponents[componentId] = new List<object>
                            {
                                eventTrigger
                            };
                            break;
                        case "Timer":
                            Grid timerGrid = new Grid() { Margin = new Thickness(0, 5) };
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                            Button startTimer = new Button() { BackgroundColor = Color.FromHex("#280338"), Text="Start", ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff") };
                            Button resetTimer = new Button() { BackgroundColor = Color.FromHex("#280338"), Text="Reset", ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff") };
                            Label currentTime = new Label() { Text = "0s", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

                            timerGrid.Add(startTimer, 0, 0);
                            timerGrid.Add(resetTimer, 1, 0);
                            timerGrid.Add(currentTime, 2, 0);

                            startTimer.Clicked += HandleFormButton;
                            resetTimer.Clicked += HandleFormButton;

                            container.Add(timerGrid, 1, 0);
                            timers[componentId] = new TimerSet() { enabled = false, seconds = 0, track = DateTime.Now };
                            data[componentId] = "";
                            extraData[componentId] = (string)component.Max;
                            attachedComponents[componentId] = new List<object>
                            {
                                startTimer, resetTimer, currentTime
                            };


                            break;
                    }



                    


                    // create component name label
                    Label componentName = new Label() { Text = component.Name, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin=new Thickness(0,10) };
                    container.Add(componentName, 0, 0);
                    Form_Content_Fields.Add(container);
                    componentId += 1;
                }


            }

            
        }catch(Exception e)
        {
            Console.WriteLine(e);
            DisplayAlert("Oops!", "Something went wrong with the schema on your device... Please contact your scouting administrator", "OK");
        }
        
    }
    private async void HandleFormPicker(object sender, EventArgs e)
    {
        Picker responsible = (Picker)sender as Picker;

        var compId = Int32.Parse(responsible.ClassId);
        var compType = componentTypes[compId];
        var compData = data[compId];

        switch (compType)
        {
            case "Select":
                data[compId] = (string)responsible.SelectedItem;
                break;
        }
    }
    private async void HandleFormEntry(object sender, TextChangedEventArgs e)
    {
        Entry responsible = (Entry)sender as Entry;

        var compId = Int32.Parse(responsible.ClassId);
        var compType = componentTypes[compId];
        var compData = data[compId];

        switch (compType)
        {
            case "Step":

                var bounds = extraData[compId].Split(";");
                try
                {
                    if (Int32.Parse(responsible.Text) > Int32.Parse(bounds[1]))
                        responsible.Text = bounds[1];
                    else if (Int32.Parse(responsible.Text) < Int32.Parse(bounds[0]))
                        responsible.Text = bounds[0];

                    data[compId] = (string)responsible.Text;
                }catch(Exception ex)
                {

                }

                
                break;
        }
    }
    private async void HandleFormButton(object sender, EventArgs e)
    {
        
        Button responsible = (Button)sender as Button;

        var compId = Int32.Parse(responsible.ClassId);
        var compType = componentTypes[compId];
        var compData = data[compId];

        switch (compType)
        {
            case "Step":
                if (data[compId] == "")
                {
                    data[compId] = "0";
                }

                var bounds = extraData[compId].Split(";");

                if(responsible.Text == "+")
                {
                    if ((Int32.Parse(compData) + 1) > Int32.Parse(bounds[1]))
                        break;
                    data[compId] = (Int32.Parse(compData) + 1).ToString();
                    ((Entry)attachedComponents[compId][1]).Text = data[compId];
                }
                else
                {
                    if ((Int32.Parse(compData) - 1) < Int32.Parse(bounds[0]))
                        break;
                    data[compId] = (Int32.Parse(compData)-1).ToString();
                    ((Entry)attachedComponents[compId][1]).Text = data[compId];
                }
                break;
            case "Check":
                if(responsible.Text == ((Button)attachedComponents[compId][0]).Text)
                {
                    // same button
                    ((Button)attachedComponents[compId][0]).BackgroundColor = Color.FromHex("#680991");
                    ((Button)attachedComponents[compId][1]).BackgroundColor = Color.FromHex("#280338");

                }
                else
                {
                    ((Button)attachedComponents[compId][1]).BackgroundColor = Color.FromHex("#680991");
                    ((Button)attachedComponents[compId][0]).BackgroundColor = Color.FromHex("#280338");
                }
                data[compId] = responsible.Text;
                break;
            case "Event":
                var secondsIn = (int)((DateTime.Now - start).TotalSeconds);
                var max = int.Parse(extraData[compId]);

                if (data[compId] == "")
                {
                    data[compId] = secondsIn.ToString();
                }
                else
                {
                    if (data[compId].Split(";").Length < max || max <= 0)
                    {
                        data[compId] = data[compId] + ";" + secondsIn.ToString();
                    }
                    
                }

                responsible.BackgroundColor = Color.FromHex("#680991");
                responsible.IsEnabled = false;

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            responsible.BackgroundColor = Color.FromHex("#280338");
                            responsible.IsEnabled = true;
                        }catch(Exception e)
                        {

                        }
                    });

                    return false;
                });



                break;
            case "Timer":
                var maxSec = int.Parse(extraData[compId]);
                if (responsible.Text == "Reset")
                {
                    // reset button
                    timers[compId].seconds = 0;
                    timers[compId].track = DateTime.Now;
                    ((Label)attachedComponents[compId][2]).Text = Math.Round(timers[compId].seconds, 2).ToString() + "s";
                }
                else
                {
                    // start / stop button
                    if(responsible.Text == "Start")
                    {
                        timers[compId].enabled = true;
                        timers[compId].track = DateTime.Now;
                        responsible.Text = "Pause";
                        ((Button)attachedComponents[compId][1]).IsEnabled = false;
                        //responsible.ImageSource = "pause.png";
                        responsible.BackgroundColor = Color.FromHex("#680991");
                    }
                    else
                    {
                        var secondsToAdd = (DateTime.Now - timers[compId].track).TotalSeconds;

                        timers[compId].seconds += (float)secondsToAdd;
                        timers[compId].enabled = false;
                        ((Label)attachedComponents[compId][2]).Text = Math.Round(timers[compId].seconds, 2).ToString() + "s";
                        responsible.Text = "Start";
                        ((Button)attachedComponents[compId][1]).IsEnabled = true;
                        //responsible.ImageSource = "play.png";
                        responsible.BackgroundColor = Color.FromHex("#280338");

                        data[compId] = Math.Round(timers[compId].seconds, 2).ToString();
                        if (Math.Round(timers[compId].seconds, 2) > maxSec && maxSec > 0)
                        {
                            data[compId] = maxSec.ToString();
                        }
                    }
                }
                break;
        }
    }

    bool _readyTransitionLock;
	private async void Status_MatchReady(object sender, EventArgs e)
	{


		if (_readyTransitionLock)
			return;

        

        // check if fields are valid...

        var matchText = Status_PreContent_MatchNumber.Text;
        int matchNum;
        if (matchText == "" || !Int32.TryParse(matchText, out matchNum))
            return;

        var teamText = Status_PreContent_TeamNumber.Text;
        int teamNumber;
        if (teamText == "" || !Int32.TryParse(teamText, out teamNumber))
            return;

        var allianceColor = (string)Status_PreContent_AllianceColor.SelectedItem;
        if (allianceColor != "Red" && allianceColor != "Blue")
            return;

        var scouterName = Status_PreContent_ScouterName.Text;
        if (scouterName == "")
            return;

        var matchExists = StorageManagement.allMatches.Exists(m => m.Environment == Environment && m.Number == matchNum);
        if (matchExists)
        {
            var res = await DisplayAlert("FYI...", "A match with the same number and environment already exists. Submitting this match will overwrite the match that already exists. Are you sure you want to continue?", "Yes", "No");
            if (!res)
            {
                return;
            }
        }

        Team = teamNumber;
        Number = matchNum;
        AllianceColor = allianceColor;
        Scouter = scouterName;



        Status_PostContent_MatchNumber.Text = "Match " + matchNum.ToString();
        Status_PostContent_TeamNumber.Text = "Team " + teamNumber.ToString();

		_readyTransitionLock = true;
		Status_BottomBar.TranslateTo(0, 500, 500, Easing.CubicInOut);

		Status_PreConfirm.FadeTo(0, 250, Easing.CubicInOut);
		Status_PostConfirm.FadeTo(1, 250, Easing.CubicInOut);

		Status_PreContent.FadeTo(0, 250, Easing.CubicInOut);
		Status_PostContent.IsVisible = true;

        Status_PostContent.FadeTo(1, 250, Easing.CubicInOut);

        ClickBlock.FadeTo(0, 300, Easing.CubicInOut);

        start = DateTime.Now;
        await Task.Delay(350);
        ClickBlock.IsVisible = false;
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

    private void AllianceColorChange(object sender, EventArgs e)
    {
        Picker allianceColorSelector = (Picker)sender as Picker;

        var value = (string)allianceColorSelector.SelectedItem;

        switch (value)
        {
            case "Red":
                allianceColorSelector.BackgroundColor = Color.FromHex("#910929");
                Status_PreContent_AllianceColorFrame.BackgroundColor = Color.FromHex("#910929");
                Status_PreContent_AllianceColorBorderGuard.BorderColor = Color.FromHex("#910929");
                break;
            case "Blue":
                allianceColorSelector.BackgroundColor = Color.FromHex("#290991");
                Status_PreContent_AllianceColorFrame.BackgroundColor = Color.FromHex("#290991");
                Status_PreContent_AllianceColorBorderGuard.BorderColor = Color.FromHex("#290991");
                break;
            default:
                allianceColorSelector.BackgroundColor = Color.FromHex("#3a0e4d");
                Status_PreContent_AllianceColorFrame.BackgroundColor = Color.FromHex("#3a0e4d");
                Status_PreContent_AllianceColorBorderGuard.BorderColor = Color.FromHex("#3a0e4d");
                break;
        }
    }
}