namespace StormCloudClient;

public partial class StormEntry : ContentView
{
    public Color BackgroundColor;
    public string Text
    {
        get { return Entry.Text; }
        set { Entry.Text = value; }
    }

    public Keyboard Keyboard
    {
        get { return Entry.Keyboard; }
        set { Entry.Keyboard = value; }
    }

    public int MaxLength
    {
        get { return Entry.MaxLength; }
        set { Entry.MaxLength = value; }
    }
    public StormEntry()
	{
		InitializeComponent();
	}
}