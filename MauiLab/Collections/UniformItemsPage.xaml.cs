using CommunityToolkit.Maui.Layouts;
using MauiLab.Layouts;

namespace MauiLab.Collections;

public partial class UniformItemsPage : ContentPage
{
    static readonly string[] Caps = ["Fit", "1", "2", "3", "4", "6", "8"];

    readonly DataTemplate _template = new(() => new ContentView
    {
        IsClippedToBounds = true,
        Content = new SwatchView()
    });
    UniformItemsLayout? _layout;
    ScrollView? _scroll;
    bool _syncing;
    int _generation;

    public UniformItemsPage()
    {
        InitializeComponent();
        ColumnsPicker.ItemsSource = Caps;
        RowsPicker.ItemsSource = Caps;
        LayoutHost.SizeChanged += (_, _) => FitToCanvas();
    }

    bool _loaded;

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded)
            return;

        // This bottom tab is created when it is shown. Skip a stray appear while another page is current.
        if (Shell.Current?.CurrentPage is Page current && current != this)
            return;

        _loaded = true;
        var model = new UniformItemsPageModel(new TrackerLayoutUsage());
        BindingContext = model;
        CollectionTimings.Attach(model);
        _syncing = true;
        ColumnsPicker.SelectedItem = "Fit";
        RowsPicker.SelectedItem = "Fit";
        _syncing = false;
        model.Restored += (_, _) => Sync(model);

        model.CardChanged += Rebuild;
        model.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(UniformItemsPageModel.AppliedMaxColumns)
                or nameof(UniformItemsPageModel.AppliedMaxRows)
                or nameof(CollectionPageModel.Items))
                Apply();
        };
        Apply();
        FitToCanvas();
    }

    void Sync(UniformItemsPageModel model)
    {
        _syncing = true;
        ColumnsPicker.SelectedItem = model.MaxColumns?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Fit";
        RowsPicker.SelectedItem = model.MaxRows?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "Fit";
        _syncing = false;
        FitToCanvas();
    }

    void FitToCanvas()
    {
        if (BindingContext is not UniformItemsPageModel model || LayoutHost.Width <= 1)
            return;

        var fit = SwatchFit.Measure(LayoutHost.Width);
        if (model.ApplyCanvasWidth(LayoutHost.Width))
            _ = Rebuild();
        if (_scroll != null)
            _scroll.Padding = new Thickness(fit.Padding, fit.Margin);
    }

    void Apply()
    {
        if (BindingContext is not UniformItemsPageModel model)
            return;

        _layout ??= NewLayout();
        _layout.MaxColumns = model.AppliedMaxColumns;
        _layout.MaxRows = model.AppliedMaxRows;
        BindableLayout.SetItemsSource(_layout, model.Items);
    }

    async Task Rebuild()
    {
        if (BindingContext is not UniformItemsPageModel model)
            return;

        if (!model.LoadsInBatches)
        {
            _generation++;
            _layout = NewLayout();
            _layout.MaxColumns = model.AppliedMaxColumns;
            _layout.MaxRows = model.AppliedMaxRows;
            BindableLayout.SetItemsSource(_layout, model.Items);
            return;
        }

        var generation = ++_generation;
        var pending = model.Items.ToList();
        model.Items.Clear();
        _layout = NewLayout();
        _layout.MaxColumns = model.AppliedMaxColumns;
        _layout.MaxRows = model.AppliedMaxRows;
        BindableLayout.SetItemsSource(_layout, model.Items);
        for (var start = 0; start < pending.Count; start += CollectionPageModel.FlexBatchSize)
        {
            if (generation != _generation)
                return;

            var end = Math.Min(start + CollectionPageModel.FlexBatchSize, pending.Count);
            for (var i = start; i < end; i++)
                model.Items.Add(pending[i]);
            await Task.Delay(1);
        }
    }

    UniformItemsLayout NewLayout()
    {
        var layout = new UniformItemsLayout { IsClippedToBounds = true };
        BindableLayout.SetItemTemplate(layout, _template);
        _scroll = new ScrollView { Content = layout };
        LayoutHost.Content = _scroll;
        return layout;
    }

    void OnBatchesToggled(object? sender, ToggledEventArgs e)
    {
        if (BindingContext is CollectionPageModel model)
            model.SetLoadsInBatches(e.Value);
    }

    void OnColumnsChanged(object? sender, EventArgs e) => ApplyCap(ColumnsPicker, (model, value) => model.SetMaxColumns(value));

    void OnRowsChanged(object? sender, EventArgs e) => ApplyCap(RowsPicker, (model, value) => model.SetMaxRows(value));

    void ApplyCap(Picker picker, Action<UniformItemsPageModel, int?> assign)
    {
        if (_syncing || BindingContext is not UniformItemsPageModel model || picker.SelectedItem is not string text)
            return;

        assign(model, text == "Fit" ? null : int.Parse(text));
    }

    void OnAboutTapped(object? sender, EventArgs e)
    {
        if (BindingContext is CollectionPageModel model)
            model.ViewInfo(model.KindTitle);
    }

    void OnInfoTapped(object? sender, EventArgs e)
    {
        if (sender is InfoControl info && BindingContext is CollectionPageModel model)
            model.ViewInfo(info.Title);
    }
}
