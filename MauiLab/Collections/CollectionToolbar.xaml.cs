namespace MauiLab.Collections;

public partial class CollectionToolbar : ContentView
{
    bool _resetting;

    public CollectionToolbar()
    {
        InitializeComponent();
    }

    void OnTimingsToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is CollectionPageModel model)
            model.SetTimings(e.Value);
    }

    void OnTimingsInfo(object? sender, EventArgs e)
    {
        if (sender is MauiLab.Layouts.InfoControl info && BindingContext is CollectionPageModel model)
            model.ViewInfo(info.Title);
    }

    async void OnResetClicked(object? sender, EventArgs e)
    {
        if (_resetting || BindingContext is not CollectionPageModel model)
            return;

        _resetting = true;
        try
        {
            await model.Reset();
        }
        finally
        {
            _resetting = false;
        }
    }
}
