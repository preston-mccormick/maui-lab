using MauiCustomControls.Views;
using CommunityToolkit.Maui.Views;

namespace MauiCustomControls.Pages;

public partial class PopupMenuPage : ContentPage
{
    public PopupMenuPage()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        if (sender is not View view) return;
        var menu = new MenuPopup()
        {
            Anchor = view,
        };
        this.ShowPopup(menu);
    }
}