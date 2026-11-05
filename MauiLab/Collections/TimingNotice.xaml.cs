namespace MauiLab.Collections;

public partial class TimingNotice : ContentView
{
    public TimingNotice()
    {
        InitializeComponent();
    }

    void OnTapped(object? sender, TappedEventArgs e) =>
        (BindingContext as CollectionPageModel)?.DismissTiming();
}
