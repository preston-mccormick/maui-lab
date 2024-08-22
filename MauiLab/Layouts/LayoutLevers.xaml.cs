namespace MauiLab.Layouts;

public partial class LayoutLevers : ContentView
{
	public LayoutLevers()
	{
		InitializeComponent();
	}

	public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

	public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(LayoutLevers), default(string));
}