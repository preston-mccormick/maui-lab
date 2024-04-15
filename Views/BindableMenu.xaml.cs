using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using System.Collections;

namespace MauiCustomControls.Views;

public partial class BindableMenu : ContentView
{
	public BindableMenu()
	{
		InitializeComponent();
	}
	public BindableMenu(IEnumerable? items) : this()
	{
		Items = items;
	}

	public IEnumerable? Items { 
		get => (IEnumerable)GetValue(ItemsProperty); 
		set => SetValue(ItemsProperty, value); 
	}

	public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(BindableMenu),
		null
    );
}