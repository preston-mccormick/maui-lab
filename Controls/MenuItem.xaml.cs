
namespace MauiCustomControls.Controls;

public partial class MenuItem : ContentView
{
    public MenuItem()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(string),
        typeof(MenuItem),
        string.Empty,
        propertyChanged: OnIconSet
    );

    private static void OnIconSet(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is MenuItem menuItem)
        {
            menuItem.TheIcon.Source = ImageSource.FromFile(menuItem.Icon);
        }
    }

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label),
        typeof(string),
        typeof(MenuItem),
        string.Empty
    );
}