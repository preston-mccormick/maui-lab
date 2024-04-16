using System.ComponentModel;
using System.Windows.Input;

namespace MauiCustomControls.Controls;

public partial class MenuItem : ContentView
{
    public static readonly BindableProperty AfterTapProperty = BindableProperty.Create(
        nameof(AfterTap),
        typeof(Action),
        typeof(MenuItem),
        null);

    public static readonly BindableProperty BeforeTapProperty = BindableProperty.Create(
        nameof(BeforeTap),
        typeof(Action),
        typeof(MenuItem),
        null);

    public static readonly BindableProperty IconProperty = BindableProperty.Create(
                nameof(Icon),
        typeof(ImageSource),
        typeof(MenuItem),
        default(ImageSource)
    );

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter),
        typeof(object),
        typeof(MenuItem),
        null);

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand), typeof(ICommand), typeof(MenuItem), null);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
                nameof(Text),
        typeof(string),
        typeof(MenuItem),
        string.Empty
    );

    /// <summary>
    /// Action to invoke before the TapCommand is executed. Use with BeforeTapCommand to show
    /// and hide a busy indicator (or whatever).
    /// </summary>
    public Action? AfterTap
    {
        get => (Action)GetValue(AfterTapProperty);
        set => SetValue(AfterTapProperty, value);
    }

    /// <summary>
    /// Action to invoke before the TapCommand is executed. Use with AfterTapCommand to show
    /// and hide a busy indicator (or whatever).
    /// </summary>
    public Action? BeforeTap
    {
        get => (Action)GetValue(BeforeTapProperty);
        set => SetValue(BeforeTapProperty, value);
    }

    /// <summary>
    /// Image source for an icon that appears to the left of the menu item text.
    /// </summary>
    [TypeConverter(typeof(ImageSourceConverter))]
    public ImageSource Icon
    {
        get => (ImageSource)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Command to execute when the menu item is tapped.
    /// </summary>
    public ICommand? TapCommand
    {
        get => (ICommand)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    /// <summary>
    /// Bindable parameter passed to the TapCommand. If not set, the Text property value is
    /// passed.
    /// </summary>
    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    /// <summary>
    /// Menu item text label. Passed to the TapCommand if TapCommandParameter is not set.
    /// </summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Constructor
    /// </summary>
    public MenuItem()
    {
        InitializeComponent();

        // For when it's not bound to a ViewModel
        BindingContext = this;
    }

    /// <summary>
    /// Wrapps the TapCommand execution with BeforeTap and AfterTap actions.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Tapped(object sender, TappedEventArgs e)
    {
        if (TapCommand == null) return;
        if (!TapCommand.CanExecute(TapCommandParameter)) return;

        BeforeTap?.Invoke();

        // If the parameter is not set, pass the Text
        TapCommand.Execute(TapCommandParameter ?? Text);

        AfterTap?.Invoke();
    }
}