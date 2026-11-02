namespace MauiLab.Layouts;

public partial class InfoControl : ContentView
{
    public InfoControl()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(InfoControl), "");

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(InfoControl), "");

    /// <summary>Raised before the text is shown so the page can record the view.</summary>
    public event EventHandler? InfoTapped;

    void OnClicked(object? sender, EventArgs e)
    {
        InfoTapped?.Invoke(this, EventArgs.Empty);
        FindPlayground()?.ShowInfo(Title, Text);
    }

    LayoutPlayground? FindPlayground()
    {
        for (Element? parent = Parent; parent != null; parent = parent.Parent)
        {
            if (parent is LayoutPlayground playground)
                return playground;
        }

        return null;
    }
}
