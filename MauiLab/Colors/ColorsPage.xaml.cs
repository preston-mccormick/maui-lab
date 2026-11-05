namespace MauiLab.Colors;

public partial class ColorsPage : ContentPage
{
    private bool _fontDrag;
    private int _fontEdit;
    private bool _oneRow = true;
    private bool _showFont = true;

    public ColorsPage()
    {
        InitializeComponent();
        BindingContext = new ColorsPageModel();
    }

    private void OnToolBarSizeChanged(object? sender, EventArgs e)
    {
        var width = ToolBar.Width;
        if (width <= 1)
            return;

        var oneRow = width >= 780;
        var showFont = width >= 520;
        if (oneRow == _oneRow && showFont == _showFont)
            return;

        _oneRow = oneRow;
        _showFont = showFont;
        FontGroup.IsVisible = showFont;
        if (oneRow)
        {
            ToolBar.RowDefinitions = [new RowDefinition(GridLength.Auto)];
            ToolBar.ColumnDefinitions =
            [
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Auto)
            ];
            Place(ListGroup, 0, 0);
            Place(SortGroup, 0, 1);
            Place(FontGroup, 0, 2);
            Place(ResetButton, 0, 3);
            return;
        }

        ToolBar.RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)];
        ToolBar.ColumnDefinitions = [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)];
        Place(ListGroup, 0, 0);
        Place(SortGroup, 0, 1);
        Place(FontGroup, 1, 0);
        Place(ResetButton, 1, showFont ? 1 : 0);
    }

    private static void Place(View view, int row, int column)
    {
        Grid.SetRow(view, row);
        Grid.SetColumn(view, column);
    }

    private void OnPickerSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (sender is Picker picker && picker.IsFocused)
            picker.Unfocus();
    }

    private void OnFontDragStarted(object? sender, EventArgs e)
    {
        _fontDrag = true;
        _fontEdit++;
    }

    private void OnFontDragCompleted(object? sender, EventArgs e)
    {
        _fontDrag = false;
        _fontEdit++;
        CommitFont();
    }

    private async void OnFontValueChanged(object? sender, ValueChangedEventArgs e)
    {
        if (BindingContext is not ColorsPageModel model)
            return;

        model.FontSizePreview = e.NewValue;
        if (_fontDrag)
            return;

        var edit = ++_fontEdit;
        await Task.Yield();
        if (edit != _fontEdit || _fontDrag)
            return;

        model.CommitFontSize();
    }

    private void CommitFont()
    {
        if (BindingContext is ColorsPageModel model)
            model.CommitFontSize();
    }

    private void OnResetClicked(object? sender, EventArgs e)
    {
        if (BindingContext is ColorsPageModel model)
            model.Reset();
    }
}
