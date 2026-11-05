namespace MauiLab.Collections;

public partial class SwatchView : ContentView
{
    public SwatchView()
    {
        InitializeComponent();
    }

    /// <summary>Set when this swatch sits in a ListView cell, whose view is not parented to the page.</summary>
    public CollectionPageModel? PaintHost { get; set; }

    void OnTapped(object? sender, TappedEventArgs e)
    {
        if (BindingContext is not SwatchItem item)
            return;

        (PaintHost ?? FindModel())?.Paint(item);
    }

    CollectionPageModel? FindModel()
    {
        for (Element? parent = this; parent != null; parent = parent.Parent)
        {
            if (parent.BindingContext is CollectionPageModel model)
                return model;
        }

        return null;
    }
}
