namespace MauiLab.Colors;

public partial class ColorsPage : ContentPage
{
    private bool _fontDrag;
    private int _fontEdit;

    public ColorsPage()
    {
        InitializeComponent();
        BindingContext = new ColorsPageModel();
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
}
