using System.ComponentModel;

namespace MauiCustomControls.Controls;

public partial class MenuItem : ContentView
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(MenuItem),
        default(ImageSource)
    );

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label),
        typeof(string),
        typeof(MenuItem),
        string.Empty
    );

    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public MenuItem()
    {
        InitializeComponent();
        BindingContext = this;
    }
}