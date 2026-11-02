namespace MauiLab.Layouts;

public partial class LeversSection : ContentView
{
    public LeversSection()
    {
        InitializeComponent();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(LeversSection), "");

    public View? Body
    {
        get => (View?)GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    public static readonly BindableProperty BodyProperty =
        BindableProperty.Create(nameof(Body), typeof(View), typeof(LeversSection),
            propertyChanged: (bindable, _, value) => ((LeversSection)bindable).BodyHost.Content = value as View);

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly BindableProperty IsExpandedProperty =
        BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(LeversSection), true, propertyChanged: OnIsExpandedChanged);

    public string ExpandGlyph => IsExpanded ? "▾" : "▸";

    static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((LeversSection)bindable).OnPropertyChanged(nameof(ExpandGlyph));

    void OnHeaderTapped(object? sender, TappedEventArgs e) => IsExpanded = !IsExpanded;
}
