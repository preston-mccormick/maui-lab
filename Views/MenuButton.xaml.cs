namespace MauiCustomControls.Views;

public partial class MenuButton : ContentView
{
	public MenuButton()
	{
		InitializeComponent();
		BindingContext = this;
	}

	private async Task OnButtonClicked()
    {
        IsMenuVisible = !IsMenuVisible;
        await Task.Delay(1000);
        IsMenuVisible = false;
    }

    public bool IsMenuVisible
    {
        get => (bool)GetValue(MenuIsVisibleProperty);
        set => SetValue(MenuIsVisibleProperty, value);
    }

    public static readonly BindableProperty MenuIsVisibleProperty = BindableProperty.Create(
        nameof(IsMenuVisible),
        typeof(bool),
        typeof(MenuButton),
        false);

	public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(MenuButton),
        string.Empty);

    public Command MenuItemTappedCommand => new Command<string>(async (item) =>
    {
        IsMenuVisible = false;
        await Task.Delay(1000);
        await Shell.Current.CurrentPage.DisplayAlert("Menu Item Tapped", item, "OK");
    });
}