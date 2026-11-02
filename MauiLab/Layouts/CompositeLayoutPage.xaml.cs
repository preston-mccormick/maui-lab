namespace MauiLab.Layouts;

public partial class CompositeLayoutPage : ContentPage
{
    public CompositeLayoutPage()
    {
        InitializeComponent();
        var model = new CompositeLayoutPageModel(new TrackerLayoutUsage());
        LayoutPicker.ItemsSource = Enum.GetValues<FieldLayout>();
        BindingContext = model;
        LayoutPicker.SelectedItem = model.Layout;
    }

    void OnLayoutChanged(object? sender, EventArgs e)
    {
        if (LayoutPicker.SelectedItem is FieldLayout layout && BindingContext is CompositeLayoutPageModel model)
            model.SetLayout(layout);
    }

    void OnWidthCommitted(object? sender, EventArgs e) =>
        (BindingContext as CompositeLayoutPageModel)?.CommitWidth();

    void OnDescriptionToggled(object? sender, ToggledEventArgs e) =>
        (BindingContext as CompositeLayoutPageModel)?.SetDescription(e.Value);

    void OnValidationToggled(object? sender, ToggledEventArgs e) =>
        (BindingContext as CompositeLayoutPageModel)?.SetValidation(e.Value);

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is CompositeLayoutPageModel model)
            model.ViewInfo(info.Title);
    }
}
