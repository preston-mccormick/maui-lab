using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections;

namespace MauiCustomControls.Views;

public partial class Menu : ContentView
{
	public Menu()
	{
		InitializeComponent();
	}
	public Menu(IEnumerable? items) : this()
	{
		Items = items;
	}

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty) as IEnumerable;
        set => SetValue(ItemsProperty, value);
    }

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(Menu),
		null
    );

    /// <summary>
    /// Action to invoke before the TapCommand is executed.
    /// Use with AfterTapCommand to show and hide a busy indicator (or whatever).
    /// </summary>
    public Action? BeforeItemTap
    {
        get => (Action)GetValue(BeforeItemTappedProperty);
        set => SetValue(BeforeItemTappedProperty, value);
    }

    public static readonly BindableProperty BeforeItemTappedProperty = BindableProperty.Create(
        nameof(BeforeItemTap),
        typeof(Action),
        typeof(MenuItem),
        null);

    /// <summary>
    /// Action to invoke before the TapCommand is executed.
    /// Use with BeforeTapCommand to show and hide a busy indicator (or whatever).
    /// </summary>
    public Action? AfterItemTap
    {
        get => (Action)GetValue(AfterItemTappedProperty);
        set => SetValue(AfterItemTappedProperty, value);
    }

    public static readonly BindableProperty AfterItemTappedProperty = BindableProperty.Create(
        nameof(AfterItemTap),
        typeof(Action),
        typeof(MenuItem),
        null);

}