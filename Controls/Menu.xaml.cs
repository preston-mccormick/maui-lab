using System.Collections;

namespace MauiCustomControls.Controls;

public partial class Menu : ContentView
{
    public static readonly BindableProperty AfterItemTappedProperty = BindableProperty.Create(
        nameof(AfterItemTap), typeof(Action), typeof(Menu),
        defaultValue: null);

    public static readonly BindableProperty BeforeItemTappedProperty = BindableProperty.Create(
        nameof(BeforeItemTap),
        typeof(Action),
        typeof(Menu), defaultValue: null);

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items), typeof(IEnumerable), typeof(Menu),
        defaultValue: null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            if (bindable is not Menu menu) return;
            if (newValue is not IEnumerable items) return;

            // Setup the menu item template
            var menuItemTemplate = new DataTemplate(() =>
            {
                var menuItem = new MenuItem();
                menuItem.SetBinding(MenuItem.IconProperty, new Binding("Icon"));
                menuItem.SetBinding(MenuItem.TextProperty, new Binding("Text"));
                menuItem.SetBinding(MenuItem.TapCommandProperty, new Binding("TapCommand"));
                menuItem.SetValue(MenuItem.BeforeTapProperty, menu.BeforeItemTap);
                menuItem.SetValue(MenuItem.AfterTapProperty, menu.AfterItemTap);
                return menuItem;
            });
            BindableLayout.SetItemTemplate(menu.MenuLayout, menuItemTemplate);

            BindableLayout.SetItemsSource(menu.MenuLayout, items);
        });

    /// <summary>
    /// Action to invoke before the TapCommand is executed. Use with BeforeTapCommand to show
    /// and hide a busy indicator (or whatever).
    /// </summary>
    public Action? AfterItemTap
    {
        get => (Action)GetValue(AfterItemTappedProperty);
        set => SetValue(AfterItemTappedProperty, value);
    }

    /// <summary>
    /// Action to invoke before the TapCommand is executed. Use with AfterTapCommand to show
    /// and hide a busy indicator (or whatever).
    /// </summary>
    public Action? BeforeItemTap
    {
        get => (Action)GetValue(BeforeItemTappedProperty);
        set => SetValue(BeforeItemTappedProperty, value);
    }

    public IEnumerable? Items
    {
        get => GetValue(ItemsProperty) as IEnumerable;
        set => SetValue(ItemsProperty, value);
    }

    /// <summary>
    /// Constructor
    /// </summary>
	public Menu()
    {
        InitializeComponent();
    }

    public Menu(IEnumerable? items) : this()
    {
        Items = items;
    }
}