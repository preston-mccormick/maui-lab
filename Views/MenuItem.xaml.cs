using System.ComponentModel;
using System.Windows.Input;

namespace MauiCustomControls.Views;

public partial class MenuItem : ContentView
{
    public static readonly BindableProperty IconProperty = BindableProperty.Create(
        nameof(Icon),
        typeof(ImageSource),
        typeof(MenuItem),
        default(ImageSource)
    );

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MenuItem),
        string.Empty
    );

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand), typeof(ICommand), typeof(MenuItem), null);

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter), typeof(object), typeof(MenuItem), null);

    public ICommand TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public MenuItem()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnPointerEntered(object sender, EventArgs e)
    {
        if (Application.Current?.PlatformAppTheme == AppTheme.Dark)
        {
            BackgroundColor = Colors.DarkBlue;
        }
        else
        {
            BackgroundColor = Colors.AliceBlue;
        }
    }
    private void OnPointerExited(object sender, EventArgs e)
    {
        BackgroundColor = null;
    }

    private void OnPointerEntered(object sender, PointerEventArgs e)
    {

    }
}