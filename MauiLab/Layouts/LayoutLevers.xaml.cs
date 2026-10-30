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

	public bool IsExpanded
	{
		get => (bool)GetValue(IsExpandedProperty);
		set => SetValue(IsExpandedProperty, value);
	}

	public static readonly BindableProperty IsExpandedProperty =
		BindableProperty.Create(nameof(IsExpanded), typeof(bool), typeof(LayoutLevers), false, propertyChanged: OnIsExpandedChanged);

	public string ExpandGlyph => IsExpanded ? "▾" : "▸";

	static void OnIsExpandedChanged(BindableObject bindable, object oldValue, object newValue) =>
		((LayoutLevers)bindable).OnPropertyChanged(nameof(ExpandGlyph));

	void OnHeaderTapped(object? sender, TappedEventArgs e) => IsExpanded = !IsExpanded;
}
