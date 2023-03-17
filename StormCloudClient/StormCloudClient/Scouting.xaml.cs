using StormCloudClient.Classes;
using StormCloudClient.Services;

namespace StormCloudClient;

public class TimerSet
{
    public DateTime track;
    public float seconds;
    public bool enabled;
}

public class ColorSet
{
    public Color main;
    public Color selected;
    public Color higher;
}

public partial class Scouting : ContentPage
{

    public Match EditingMatch;
    
    public List<string> data = new List<string>();
    public List<string> componentTypes = new List<string>();
    public List<string> extraData = new List<string>();

    public Dictionary<string, List<int>> linkedGrids = new Dictionary<string, List<int>>();

    Dictionary<string, ColorSet> colorSets = new Dictionary<string, ColorSet>()
    {
        
        {"default", new ColorSet(){ selected = Color.FromHex("680991"), higher = Color.FromHex("3a0e4d"), main = Color.FromHex("#280338") } },
        {"red", new ColorSet(){ selected = Color.FromHex("#e80c3f"), higher = Color.FromHex("#910929"), main = Color.FromHex("#60051a") } },
        {"blue", new ColorSet(){ selected = Color.FromHex("#3e0ddb"), higher = Color.FromHex("#290991"), main = Color.FromHex("#1a065d") } },
        {"purple", new ColorSet(){ selected = Color.FromHex("#d9009f"), higher = Color.FromHex("#8f096b"), main = Color.FromHex("#54043f") } },
        {"white", new ColorSet(){ selected = Color.FromHex("#bfbfbf"), higher = Color.FromHex("#5a5a5a"), main = Color.FromHex("#2b2b2b") } }
    };

    public Dictionary<int, Frame> timerSections = new Dictionary<int, Frame>();
    public Frame currentTimerSection;

    public Dictionary<int, List<object>> attachedComponents = new Dictionary<int, List<object>>();
    public Dictionary<int, TimerSet> timers = new Dictionary<int, TimerSet>();
    public TimerSet disabledTimer = new TimerSet() { enabled = false, seconds = 0, track = DateTime.Now };
    public string SchemaName;
    public string Environment;
    public DateTime start;

    public int Team;
    public int Number;
    public string Scouter;
    public string AllianceColor;

    MainPage _ref;


    

    public Scouting(string SchemaName, string Environment, MainPage mainMenu, Match prior)
	{
        this.Environment = Environment;
        this.SchemaName = SchemaName;
        _ref = mainMenu;
        
        
		InitializeComponent();
        if (prior != null)
        {
            EditingMatch = prior;
            try
            {
                data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(prior.Data);

            }
            catch (Exception ex)
            {

            }

            Status_PreContent.IsVisible = false;
            Status_EditContent.IsVisible = true;

            Status_EditContent_Notice.Text = "You are editing Match " + EditingMatch.Number.ToString() + " for Team " + EditingMatch.Team.ToString() + ". Since this was a timed match, you will not be able to modify any Event components. To restart a match, please delete this match and recreate it with the same details.";
            Status_BottomBar.TranslateTo(0, 300, 500, Easing.CubicInOut);

        }
        else
        {
            Status_PreContent.IsVisible = true;
            Status_EditContent.IsVisible = false;
        }
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

            // calculate disabled time
            if(disabledTimer.enabled)
            {
                var time = disabledTimer.seconds + (now - disabledTimer.track).TotalSeconds;
                var seconds = Math.Round(time, 0) % 60;
                var minutes = Math.Floor(Math.Round(time, 0) / 60);
                Match_Disabled_Time.Text = minutes.ToString() + ":" + seconds.ToString().PadLeft(2, '0');
            }


            return true;
        });
    }

    private async void Back(object sender, EventArgs e)
    {
        bool res = false;
        if(EditingMatch != null)
        {
            res = await DisplayAlert("Are You Sure?", "Any unsaved changes that you've made will be lost. The match will revert back to its original state.", "Exit", "Cancel");
        }
        else
        {
            res = await DisplayAlert("Are You Sure?", "Any unsaved changes that you've made to this match will be lost.", "Exit", "Cancel");
        }
        if(res)
            Navigation.PopAsync();
    }

    private async void SaveMatch(object sender, EventArgs e)
    {
        foreach(string group in linkedGrids.Keys.ToList())
        {
            foreach(int compId in linkedGrids[group])
            {
                int width = int.Parse(extraData[compId].Split(";")[0]);
                int height = int.Parse(extraData[compId].Split(";")[1]);
                Grid g = (Grid)attachedComponents[compId][0];
                int[,] values = new int[width, height];
                foreach(var child in g.Children)
                {
                    Label l = child as Label;
                    // this is a label, do not track for the value
                    if (l != null)
                        continue;
                    // we now know that this is a button that we can get
                    Button b = child as Button;
                    int row = g.GetRow(b);
                    int col = g.GetColumn(b) - 1; //subtract 1 since the label is on the 1st column
                    values[col, row] = int.Parse(b.Text);
                }

                string finalResult = "";
                for (int j = 0; j < height; j++)
                {
                    for (int i = 0; i < width; i++)
                    {
                        finalResult += values[i, j].ToString() + ",";
                    }
                    finalResult = finalResult.Substring(0, finalResult.Length - 1) + "*";
                }
                finalResult = finalResult.Substring(0, finalResult.Length - 1);
                data[compId] = finalResult;
            }
        }





        var stringContents = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        StorageManagement.matchesCreated += 1;
        DataManagement.SetValue("matches_created", StorageManagement.matchesCreated.ToString());
        
        




        if(EditingMatch != null)
        {
            StorageManagement.AddData_Match(EditingMatch.Number, EditingMatch.Team, EditingMatch.Scouter, EditingMatch.Color, EditingMatch.Schema, EditingMatch.Environment, stringContents, (int)disabledTimer.seconds);
        }
        else
        {
            StorageManagement.AddData_Match(Number, Team, Scouter, AllianceColor, SchemaName, Environment, stringContents, (int)disabledTimer.seconds);
        }
        

        Navigation.PopAsync();
    }

    public void LoadSchema(string schemaJSON)
    {
        try
        {
            bool isCurrentlyEditing = EditingMatch != null;
            dynamic schemaObject = Newtonsoft.Json.JsonConvert.DeserializeObject(schemaJSON);
            // set base form info
            Console.WriteLine(Enumerable.Count(schemaObject.Parts));
            int componentId = 0;
            // go through each part
            foreach(dynamic part in schemaObject.Parts)
            {

                // add part title
                Frame titleFrame = new Frame() { CornerRadius = 16, Margin = new Thickness(0, 25, 0, 15), BackgroundColor = Color.FromHex("#190024"), Padding = new Thickness(25, 2), BorderColor = Color.FromArgb("00ffffff"), HorizontalOptions=LayoutOptions.Center };

                Label title = new Label() { Text = part.Name, FontSize = 24, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, Margin = new Thickness(0,10) };
                titleFrame.Content = title;
                int startTime = int.Parse((string)part.Time);

                if (!timerSections.ContainsKey(startTime))
                {
                    timerSections[startTime] = titleFrame;
                }

                Form_Content_Fields.Add(titleFrame);

                foreach(dynamic component in part.Components)
                {


                    if((string)component.Type == "Label")
                    {

                        if((string)component.Name != "")
                        {
                            Label titleText = new Label() { Text = (string)component.Name, FontSize = 20, FontAttributes = FontAttributes.Bold, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(20, 20, 20, 5), HorizontalTextAlignment = TextAlignment.Center };
                            Form_Content_Fields.Add(titleText);
                        }

                        
                        Label mainText = new Label() { Text = (string)component.Contents, FontSize = 14, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(20, (string)component.Name == "" ? 0 : 5, 20, 20), HorizontalTextAlignment = TextAlignment.Center };

                        Form_Content_Fields.Add(mainText);

                        continue;
                    }

                    if (!isCurrentlyEditing)
                        data.Add("");
                    extraData.Add("");
                    componentTypes.Add((string)component.Type);

                    if ((string)component.Type == "Grid")
                    {
                        int width = int.Parse((string)component.Width);
                        int height = int.Parse((string)component.Height);
                        string dataString = "";
                        Grid singleContainer = new Grid() { Margin = new Thickness(10, 5) };

                        int[,] prevData = new int[width, height];
                        if (isCurrentlyEditing)
                        {
                            var rows = data[componentId].Split("*");
                            for(int r = 0; r < rows.Length; r++)
                            {
				                var cols = rows[r].Split(",");
				                for(int c = 0; c < cols.Length; c++)
                                {
					                prevData[c,r] = int.Parse(cols[c]);
					            }
				            }
                        }

                        for(int i = 0; i < width; i++)
                        {
                            singleContainer.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                        }
                        
                        for (int i = 0; i < height; i++)
                        {
                            singleContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
                        }
                        // additional column and row for labels; will be first col and last row
                        singleContainer.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
                        singleContainer.RowDefinitions.Add(new RowDefinition(GridLength.Auto));

                        if ((string)component.Group != null)
                        {
                            if (linkedGrids.ContainsKey((string)component.Group))
                            {
                                linkedGrids[(string)component.Group].Add(componentId);
                            }
                            else
                            {
                                linkedGrids[(string)component.Group] = new List<int>() { componentId };
                            }
                        }
                        else
                        {
                            linkedGrids[componentId.ToString()] = new List<int>() { componentId };
                        }

                        List<string> colorscols = new List<string>();
                        foreach (dynamic color in component.ColumnColors)
                        {
                            colorscols.Add((string)color);
                        }

                        for (int i = 0; i < width; i++)
                        {
                            
                            for (int j = 0; j < height; j++)
                            {
                                var useGColor = "default";
                                if(i < colorscols.Count)
                                {
                                    if (colorSets.ContainsKey(colorscols[i].ToLower()))
                                    {
                                        useGColor = colorscols[i].ToLower();
                                    }
                                }
                                Button b = new Button() { BackgroundColor = colorSets[useGColor].main, Text = (isCurrentlyEditing ? prevData[i,j].ToString() : "-1"), FontSize = 10, ClassId = componentId.ToString(), TextColor = Color.FromHex("#280338"), Margin = new Thickness(2,2), StyleId = useGColor };
				                if(isCurrentlyEditing){
					                // should be disabled
					                if(prevData[i,j] == componentId)
                                    {
                                        b.BackgroundColor = colorSets[useGColor].selected;
                                        b.TextColor = colorSets[useGColor].selected;
                                    }
						                
					                else if(prevData[i,j] != -1)
                                    {
                                        b.BackgroundColor = colorSets[useGColor].higher;
                                        b.TextColor = colorSets[useGColor].higher;
                                    }
						                
						
				
				                }
                                b.Clicked += HandleGridPress;
                                singleContainer.Add(b, i+1, j);
                            }
                           
                            

                        }

                        // create labels for grid
                        List<string> collabels = new List<string>();
                        foreach (dynamic label in component.ColumnLabels)
                        {
                            collabels.Add((string)label);
                        }
                        // make sure that we aren't putting too many labels
                        for (int l = 0; l < (collabels.Count() > width ? width : collabels.Count()); l++)
                        {
                            Label newLabel = new Label() { Text = collabels[l], FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(2,6) };
                            singleContainer.Add(newLabel, l+1, height);
                        }

                        List<string> rowlabels = new List<string>();
                        foreach (dynamic label in component.RowLabels)
                        {
                            rowlabels.Add((string)label);
                        }
                        // make sure that we aren't putting too many labels
                        for (int l = 0; l < (rowlabels.Count() > height ? height : rowlabels.Count()); l++)
                        {
                            Label newLabel = new Label() { Text = rowlabels[l], FontSize = 12, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(2) };
                            singleContainer.Add(newLabel, 0, l);
                        }

                        if (!isCurrentlyEditing)
                        {
                            data[componentId] = "";
                        }

                        attachedComponents[componentId] = new List<object>() { singleContainer };

                        extraData[componentId] = width.ToString() + ";" + height.ToString() + ";" + ((string)component.Group == null ? "" : (string)component.Group);

                        Label singleComponentName = new Label() { Text = component.Name, FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Margin = new Thickness(0, 10) };
                        Form_Content_Fields.Add(singleComponentName);
                        Form_Content_Fields.Add(singleContainer);
                        componentId += 1;

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

                    var useColor = "default";
                    try
                    {
                        string color = ((string)component.Color).ToLower();
                        if (colorSets.ContainsKey(color))
                        {
                            useColor = color;
                        }
                    }
                    catch(Exception colorex)
                    {

                    }
                    


                    switch ((string)component.Type)
                    {
                        case "Step":
                            Grid stepperView = new Grid() { Margin=new Thickness(5,5)};
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
                            stepperView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

                            Button downButton = new Button() { BackgroundColor = colorSets[useColor].main, Text = "-", FontSize=16, ClassId=componentId.ToString(), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };
                            Entry value = new Entry() { Keyboard = Keyboard.Numeric, FontSize = 18, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Center, Text = (string)component.Min, ClassId = componentId.ToString(), TextColor= Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, StyleId = useColor };
                            Button upButton = new Button() { BackgroundColor = colorSets[useColor].main, Text = "+", FontSize = 16, ClassId = componentId.ToString(), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };

                            downButton.Clicked += HandleFormButton;
                            value.TextChanged += HandleFormEntry;
                            upButton.Clicked += HandleFormButton;

                            if (isCurrentlyEditing)
                            {
                                value.Text = data[componentId].ToString();
                            }
                            else
                            {
                                data[componentId] = (string)component.Min;
                            }

                            stepperView.Add(downButton, 0, 0);
                            stepperView.Add(value, 1, 0);
                            stepperView.Add(upButton, 2, 0);
                            container.Add(stepperView, 1, 0);
                            // add initial data as the minimum value for the step component
                            
                            extraData[componentId] = (string)component.Min + ";" + (string)component.Max;
                            // add the initial attached components
                            attachedComponents[componentId] = new List<object>
                            {
                                downButton, value, upButton
                            };

                            break;
                        case "Input":
                            Frame entryFrame = new Frame() { CornerRadius = 8, Margin = new Thickness(5, 5), BackgroundColor = colorSets[useColor].main, Padding = new Thickness(5, 0), BorderColor = Color.FromArgb("00ffffff"), StyleId = useColor };
                            Frame borderProtectE = new Frame() { CornerRadius = 4, Padding = new Thickness(0), BorderColor = colorSets[useColor].main, StyleId = useColor };

                            Entry e = new Entry() { FontSize = 14, BackgroundColor = colorSets[useColor].main, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, ClassId = componentId.ToString() };

                            e.TextChanged += HandleFormEntry;

                            borderProtectE.Content = e;
                            entryFrame.Content = borderProtectE;
                            container.Add(entryFrame, 1, 0);

                            attachedComponents[componentId] = new List<object>
                            {
                                e
                            };

                            if (isCurrentlyEditing)
                            {
                                e.Text = data[componentId].ToString();
                            }
                            else
                            {
                                data[componentId] = "";
                            }

                            break;
                        case "Check":
                            Grid buttonView = new Grid() { Margin = new Thickness(0, 5) };
                            buttonView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            buttonView.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                            Button offButton = new Button() { BackgroundColor = colorSets[useColor].selected, Text = (string)component.Off, ClassId=componentId.ToString(), Margin=new Thickness(5,0), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };
                            Button onButton = new Button() { BackgroundColor = colorSets[useColor].main, Text = (string)component.On, ClassId=componentId.ToString(), Margin = new Thickness(5, 0), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };

                            offButton.Clicked += HandleFormButton;
                            onButton.Clicked += HandleFormButton;
                            
                            buttonView.Add(offButton, 0, 0);
                            buttonView.Add(onButton, 1, 0);
                            container.Add(buttonView, 1, 0);

                            attachedComponents[componentId] = new List<object>
                            {
                                offButton, onButton
                            };

                            if (isCurrentlyEditing)
                            {
                                if (data[componentId].ToString() == (string)component.On)
                                {
                                    HandleFormButton(onButton, null);
                                }
                                else
                                {
                                    HandleFormButton(offButton, null);
                                }
                            }
                            else
                            {
                                data[componentId] = (string)component.Off;
                            }

                            
                            
                            
                            break;
                        case "Multi-Select":
                            StackLayout optionsList = new StackLayout() { Margin = new Thickness(0, 5) };
                            attachedComponents[componentId] = new List<object>();
                            foreach (var option in component.Options)
                            {
                                Button b = new Button() { BackgroundColor = colorSets[useColor].main, Text = (string)option.Name, ClassId = componentId.ToString(), Margin = new Thickness(5, 2), Padding = new Thickness(5), TextColor = Color.FromHex("#ffffff"), StyleId = useColor, FontSize = 12 };
                                optionsList.Add(b);

                                b.Clicked += HandleFormButton;

                                attachedComponents[componentId].Add(b);
                            }

                            extraData[componentId] = (string)component.MaxSelect;
                            if (isCurrentlyEditing)
                            {
                                
                            }
                            else
                            {
                                data[componentId] = "";
                            }

                            container.Add(optionsList, 1, 0);

                            
                            break;
                        case "Select":
                            Frame pickerFrame = new Frame() { CornerRadius = 8, Margin = new Thickness(5, 5), BackgroundColor = colorSets[useColor].main, Padding = new Thickness(5, 0), BorderColor = Color.FromArgb("00ffffff"), StyleId = useColor };
                            Frame borderProtect = new Frame() { CornerRadius = 4, Padding = new Thickness(0), BorderColor = colorSets[useColor].main, StyleId = useColor };
                            Picker selection = new Picker() { FontSize = 16, BackgroundColor = colorSets[useColor].main, TextColor = Color.FromHex("#ffffff"), HorizontalTextAlignment = TextAlignment.Center, ClassId = componentId.ToString(), StyleId = useColor };

                            borderProtect.Content = selection;
                            pickerFrame.Content = borderProtect;
                            
                            List<string> items = new List<string>();
                            dynamic options = component.Options;
                            foreach(dynamic option in options)
                            {
                                items.Add(option.Name.ToString());

                            }
                            selection.ItemsSource = items;

                            selection.SelectedIndexChanged += HandleFormPicker;

                            if (isCurrentlyEditing)
                            {
                                int i = items.IndexOf(data[componentId].ToString());
                                if (i < 0)
                                    data[componentId] = "";
                                else
                                    selection.SelectedIndex = i;
                            }
                            else
                            {
                                data[componentId] = "";
                            }
                            
                            attachedComponents[componentId] = new List<object>
                            {
                                selection
                            };
                            container.Add(pickerFrame, 1, 0);

                            break;
                        case "Event":
                            Button eventTrigger = new Button() { BackgroundColor = colorSets[useColor].main, Text = (string)component.Trigger, ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };

                            if (isCurrentlyEditing)
                                eventTrigger.IsEnabled = false;
                            else
                                data[componentId] = "";

                            eventTrigger.Clicked += HandleFormButton;
                            container.Add(eventTrigger, 1, 0);
                            
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
                            Button startTimer = new Button() { BackgroundColor = colorSets[useColor].main, Text="Start", ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };
                            Button resetTimer = new Button() { BackgroundColor = colorSets[useColor].main, Text="Reset", ClassId = componentId.ToString(), Margin = new Thickness(5, 5), TextColor = Color.FromHex("#ffffff"), StyleId = useColor };
                            Label currentTime = new Label() { Text = "0s", FontSize = 16, TextColor = Color.FromHex("#ffffff"), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, StyleId = useColor };

                            timerGrid.Add(startTimer, 0, 0);
                            timerGrid.Add(resetTimer, 1, 0);
                            timerGrid.Add(currentTime, 2, 0);

                            startTimer.Clicked += HandleFormButton;
                            resetTimer.Clicked += HandleFormButton;

                            if (isCurrentlyEditing)
                            {
                                try
                                {
                                    timers[componentId] = new TimerSet() { enabled = false, seconds = float.Parse(data[componentId]), track = DateTime.Now };
                                    currentTime.Text = data[componentId] + "s";
                                }
                                catch(Exception ex)
                                {
                                    timers[componentId] = new TimerSet() { enabled = false, seconds = 0, track = DateTime.Now };
                                    currentTime.Text = "0s";
                                    data[componentId] = "0";
                                }
                                
                            }
                            else
                            {
                                timers[componentId] = new TimerSet() { enabled = false, seconds = 0, track = DateTime.Now };
                                data[componentId] = "0";
                            }

                            container.Add(timerGrid, 1, 0);
                            
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

    private async void ToggleDisabled(object sender, EventArgs e){
        if(!disabledTimer.enabled)
        {
            disabledTimer.enabled = true;
            disabledTimer.track = DateTime.Now;

            ChangeStatusExpansion(true);
            Disabled_Robot.BackgroundColor = Color.FromHex("#680991");
            
        }
        else
        {
            var now = DateTime.Now;
            var secondsToAdd = (now - disabledTimer.track).TotalSeconds;

            ChangeStatusExpansion(false);
            disabledTimer.seconds += (float)secondsToAdd;
            disabledTimer.enabled = false;
            var time = disabledTimer.seconds;
            var seconds = Math.Round(time, 0) % 60;
            var minutes = Math.Floor(Math.Round(time, 0) / 60);
            Match_Disabled_Time.Text = minutes.ToString() + ":" + seconds.ToString().PadLeft(2, '0');
            Disabled_Robot.BackgroundColor = Color.FromHex("#3a0e4d");
            
        }
        
    }

    private async void HandleGridPress(object sender, EventArgs e)
    {
        Button responsible = (Button)sender as Button;
        int componentId = int.Parse(responsible.ClassId);
        Grid g = (Grid)attachedComponents[componentId][0];

        int row = g.GetRow(responsible);
        int col = g.GetColumn(responsible);

        var group = extraData[componentId].Split(";")[2];
        bool selecting = false;

        var useColor = responsible.StyleId;

        if(int.Parse(responsible.Text) == -1)
        {
            // no current selection
            responsible.Text = componentId.ToString();
            responsible.BackgroundColor = colorSets[useColor].selected;
            responsible.TextColor = colorSets[useColor].selected;
            selecting = true;
        }
        else if(int.Parse(responsible.Text) == componentId)
        {
            responsible.Text = "-1";
            responsible.BackgroundColor = colorSets[useColor].main;
            responsible.TextColor = colorSets[useColor].main;
        }
        else
        {
            // current selection is FROM FOREIGN GRID
            return;
        }

        if(group != "")
        {
            foreach(int id in linkedGrids[group])
            {
                if(id != componentId)
                {
                    // give dimmer box
                    var allButtons = ((Grid)attachedComponents[id][0]).Children.ToList();
                    Grid thisGrid = (Grid)attachedComponents[id][0];
                    Button b = (Button)allButtons.Find((x) =>
                    {
                        var xB = (Button)x;
                        return thisGrid.GetRow(xB) == row && thisGrid.GetColumn(xB) == col;
                    });
                    useColor = b.StyleId;
                    if (selecting)
                    {
                        b.TextColor = colorSets[useColor].higher;
                        b.BackgroundColor = colorSets[useColor].higher;
                        b.Text = componentId.ToString();
                    }
                    else
                    {
                        b.TextColor = colorSets[useColor].main;
                        b.BackgroundColor = colorSets[useColor].main;
                        b.Text = "-1";
                    }
                    

                }
            }
        }
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);



    
        
    }

    private async void HandleFormPicker(object sender, EventArgs e)
    {
        Picker responsible = (Picker)sender as Picker;

        var compId = Int32.Parse(responsible.ClassId);
        var compType = componentTypes[compId];
        var compData = data[compId];
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
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
            case "Input":

                data[compId] = (string)responsible.Text;
                break;
        }
    }
    private async void HandleFormButton(object sender, EventArgs e)
    {
        
        Button responsible = (Button)sender as Button;

        var compId = Int32.Parse(responsible.ClassId);
        var compType = componentTypes[compId];
        var compData = data[compId];

        var useColor = responsible.StyleId;
        PhysicalVibrations.TryHaptic(HapticFeedbackType.Click);
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
                    ((Button)attachedComponents[compId][0]).BackgroundColor = colorSets[useColor].selected;
                    ((Button)attachedComponents[compId][1]).BackgroundColor = colorSets[useColor].main;

                }
                else
                {
                    ((Button)attachedComponents[compId][1]).BackgroundColor = colorSets[useColor].selected;
                    ((Button)attachedComponents[compId][0]).BackgroundColor = colorSets[useColor].main;
                }
                data[compId] = responsible.Text;
                break;
            case "Multi-Select":
                var currentlySelectedOptions = data[compId].Split(";").ToList();

                var optionToToggle = responsible.Text;

                if (currentlySelectedOptions.Contains(optionToToggle))
                {
                    // disable option
                    currentlySelectedOptions.Remove(optionToToggle);
                    responsible.BackgroundColor = colorSets[useColor].main;
                }
                else
                {
                    var maxSelect = int.Parse(extraData[compId]);
                    if(currentlySelectedOptions.Count() <= maxSelect || maxSelect <= 0)
                    {
                        // enable option
                        currentlySelectedOptions.Add(optionToToggle);
                        responsible.BackgroundColor = colorSets[useColor].selected;
                    }
                    

                    
                }
                var putBack = "";
                foreach(var option in currentlySelectedOptions)
                {
                    putBack += option + ";";
                }
                if(putBack != "")
                {
                    putBack = putBack.Substring(0, putBack.Length - 1);
                }
                data[compId] = putBack;
                break;
            case "Event":
                if (EditingMatch != null)
                    return;
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

                responsible.BackgroundColor = colorSets[useColor].selected;
                responsible.IsEnabled = false;

                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        try
                        {
                            responsible.BackgroundColor = colorSets[useColor].main;
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
                        responsible.BackgroundColor = colorSets[useColor].selected;
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
                        responsible.BackgroundColor = colorSets[useColor].main;

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

        if(EditingMatch != null)
        {
            Status_PostContent_MatchNumber.Text = "Match " + EditingMatch.Number.ToString();
            Status_PostContent_TeamNumber.Text = "Team " + EditingMatch.Team.ToString();

            _readyTransitionLock = true;
            Status_BottomBar.TranslateTo(0, 500, 500, Easing.CubicInOut);

            Status_PreConfirm.FadeTo(0, 250, Easing.CubicInOut);
            Status_PostConfirm.FadeTo(1, 250, Easing.CubicInOut);

            Status_EditContent.FadeTo(0, 250, Easing.CubicInOut);



            ClickBlock.FadeTo(0, 300, Easing.CubicInOut);

            start = DateTime.Now;
            await Task.Delay(350);
            Status_EditContent.IsVisible = false;
            Status_PostContent.IsVisible = true;

            Status_PostContent.FadeTo(0, 250, Easing.CubicInOut);
            ClickBlock.IsVisible = false;
            return;
        }

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

		

        ClickBlock.FadeTo(0, 300, Easing.CubicInOut);

        start = DateTime.Now;
        await Task.Delay(350);
        Status_PreContent.IsVisible = false;
        Status_PostContent.IsVisible = true;

        Status_PostContent.FadeTo(0, 250, Easing.CubicInOut);
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