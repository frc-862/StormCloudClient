using StormCloudClient.Classes;

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
    public Dictionary<int, List<object>> attachedComponents = new Dictionary<int, List<object>>();
    public Dictionary<int, TimerSet> timers = new Dictionary<int, TimerSet>();
    public string SchemaName;
    public string Environment;
    public DateTime start;

    public int Team;
    public int Number;
    public string Scouter;
    public string AllianceColor;


    

    public Scouting(string SchemaName, string Environment)
	{
        this.Environment = Environment;
        this.SchemaName = SchemaName;
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        var Schema = StorageManagement.allSchemas.Find(s => s.Name == SchemaName);



        LoadSchema(Schema.Data);



        Device.StartTimer(TimeSpan.FromMilliseconds(300), () =>
        {
            var now = DateTime.Now;
            foreach (var item in timers)
            {
                if (item.Value.enabled)
                {
                    var time = item.Value.seconds + (now - item.Value.track).TotalSeconds;
                    ((Label)attachedComponents[item.Key][2]).Text = Math.Round(time,2).ToString() + "s";
                }
            }

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
                Label title = new Label() { Text = part.Name, FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold };
                Form_Content_Fields.Add(title);

                foreach(dynamic component in part.Components)
                {
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
                    componentTypes.Add((string)component.Type);

                    switch ((string)component.Type)
                    {
                        case "Step":
                            Grid stepperView = new Grid() { Margin=new Thickness(5,5)};
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

                            Button downButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "-", FontSize=16, ClassId=componentId.ToString() };
                            Entry value = new Entry() { Keyboard = Keyboard.Numeric, FontSize = 18, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Text = (string)component.Min, ClassId = componentId.ToString(), TextColor= Color.FromHex("#ffffff") };
                            Button upButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "+", FontSize = 16, ClassId = componentId.ToString() };

                            downButton.Clicked += HandleFormButton;
                            upButton.Clicked += HandleFormButton;

                            stepperView.Add(downButton, 0, 0);
                            stepperView.Add(value, 1, 0);
                            stepperView.Add(upButton, 2, 0);
                            container.Add(stepperView, 1, 0);
                            // add initial data as the minimum value for the step component
                            data[componentId] = (string)component.Min;
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
                            Button offButton = new Button() { BackgroundColor = Color.FromHex("#680991"), Text = (string)component.Off, ClassId=componentId.ToString(), Margin=new Thickness(5,0) };
                            Button onButton = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = (string)component.On, ClassId=componentId.ToString(), Margin = new Thickness(5, 0) };

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
                            Picker selection = new Picker() { FontSize = 16, BackgroundColor = Color.FromHex("#280338"), TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, ClassId = componentId.ToString() };

                            pickerFrame.Content = selection;
                            
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
                            Button eventTrigger = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = (string)component.Trigger, ClassId = componentId.ToString(), Margin = new Thickness(5, 5) };

                            eventTrigger.Clicked += HandleFormButton;
                            container.Add(eventTrigger, 1, 0);
                            data[componentId] = "";
                            attachedComponents[componentId] = new List<object>
                            {
                                eventTrigger
                            };
                            break;
                        case "Timer":
                            Grid timerGrid = new Grid() { Margin = new Thickness(0, 5) };
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            timerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                            Button startTimer = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "S", ClassId = componentId.ToString(), Margin = new Thickness(5, 5) };
                            Button resetTimer = new Button() { BackgroundColor = Color.FromHex("#280338"), Text = "R", ClassId = componentId.ToString(), Margin = new Thickness(5, 5) };
                            Label currentTime = new Label() { Text = "---", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center };

                            timerGrid.Add(startTimer, 0, 0);
                            timerGrid.Add(resetTimer, 1, 0);
                            timerGrid.Add(currentTime, 2, 0);

                            startTimer.Clicked += HandleFormButton;
                            resetTimer.Clicked += HandleFormButton;

                            container.Add(timerGrid, 1, 0);
                            timers[componentId] = new TimerSet() { enabled = false, seconds = 0, track = DateTime.Now };
                            data[componentId] = "";
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
                if(responsible.Text == "+")
                {
                    data[compId] = (Int32.Parse(compData) + 1).ToString();
                    ((Entry)attachedComponents[compId][1]).Text = data[compId];
                }
                else
                {
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

                if (data[compId] == "")
                {
                    data[compId] = secondsIn.ToString();
                }
                else
                {
                    data[compId] = data[compId] + secondsIn.ToString();
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
                if(responsible.Text == "R")
                {
                    // reset button
                    timers[compId].seconds = 0;
                    timers[compId].track = DateTime.Now;
                    ((Label)attachedComponents[compId][2]).Text = Math.Round(timers[compId].seconds, 2).ToString() + "s";
                }
                else
                {
                    // start / stop button
                    if(responsible.Text == "S")
                    {
                        timers[compId].enabled = true;
                        timers[compId].track = DateTime.Now;
                        responsible.Text = "P";
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
                        responsible.Text = "S";
                        ((Button)attachedComponents[compId][1]).IsEnabled = true;
                        //responsible.ImageSource = "play.png";
                        responsible.BackgroundColor = Color.FromHex("#280338");
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

        start = DateTime.Now;
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
                break;
            case "Blue":
                allianceColorSelector.BackgroundColor = Color.FromHex("#290991");
                Status_PreContent_AllianceColorFrame.BackgroundColor = Color.FromHex("#290991");
                break;
            default:
                allianceColorSelector.BackgroundColor = Color.FromHex("#3a0e4d");
                Status_PreContent_AllianceColorFrame.BackgroundColor = Color.FromHex("#3a0e4d");
                break;
        }
    }
}