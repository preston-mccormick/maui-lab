using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.Dialogs;

public partial class DialogsPage : ContentPage
{
    public DialogsPage()
    {
        InitializeComponent();
    }

    public ICommand ShowAlertCommand => new AsyncRelayCommand(async () =>
    {
        await DisplayAlertAsync("Alert!", "This is a test of the Emergency Broadcast System", "Acknowledge");
        await Toast.Make("Acknowleged", textSize: 42).Show();
    });

    public ICommand ShowPromptCommand => new AsyncRelayCommand(async () =>
    {
        string result = await DisplayPromptAsync("Question 1", "What's your name?");
        if (result is not null)
            await Toast.Make($"Nice to meet you {result}!", textSize: 42).Show();
    });

    public ICommand ShowActionSheetCommand => new AsyncRelayCommand(async () =>
    {
        string action = await DisplayActionSheetAsync("This is a long message that will be truncated if it's too long.",
            "Cancel", "Delete", "Copy", "Move", "Archive", "Share", "Cancel");
        if (action is not null)
            await Toast.Make(action, textSize: 42).Show();
    });
}