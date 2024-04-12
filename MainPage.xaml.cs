using System.Diagnostics;
using System.Windows.Input;

namespace MauiCustomControls;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    public ICommand ShowAlertCommand => new Command(() =>
        {
            DisplayAlert("Alert!", "This is a test of the Emergency Broadcast System", "Acknowledge");
        });

    public ICommand ShowPromptCommand => new Command(async () =>
        {
            string result = await DisplayPromptAsync("Question 1", "What's your name?");
        });

    public ICommand ShowActionSheetCommand => new Command(async () =>
        {
            string action = await DisplayActionSheet("Message", "Cancel", "Destructive", "Button1", "Button2", "Button3");
        });
}

