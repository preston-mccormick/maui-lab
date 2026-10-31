using System.Windows.Input;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.Input;
using MauiLab.Usage;

namespace MauiLab.Dialogs;

public partial class DialogsPage : ContentPage
{
    public DialogsPage()
    {
        InitializeComponent();
    }

    public ICommand ShowAlertCommand => new AsyncRelayCommand(() =>
        Track("Show Alert", "Alert", async () =>
        {
            await DisplayAlertAsync("Alert!", "This is a test of the Emergency Broadcast System", "Acknowledge");
            await Toast.Make("Acknowleged", textSize: 42).Show();
            return "Acknowledge";
        }));

    public ICommand ShowPromptCommand => new AsyncRelayCommand(() =>
        Track("Show Prompt", "Prompt", async () =>
        {
            var result = await DisplayPromptAsync("Question 1", "What's your name?");
            if (result is not null)
                await Toast.Make($"Nice to meet you {result}!", textSize: 42).Show();
            return result;
        }));

    public ICommand ShowActionSheetCommand => new AsyncRelayCommand(() =>
        Track("Show ActionSheet", "Action sheet", async () =>
        {
            var action = await DisplayActionSheetAsync(
                "This is a long message that will be truncated if it's too long.",
                "Cancel",
                "Delete",
                "Copy",
                "Move",
                "Archive",
                "Share",
                "Cancel");
            if (action is not null)
                await Toast.Make(action, textSize: 42).Show();
            return action;
        }));

    static async Task Track(string button, string surface, Func<Task<string?>> action)
    {
        var usage = UsageServices.Current;
        usage?.BeginInteraction(button, surface);
        try
        {
            var choice = await action();
            if (choice != null)
                usage?.NoteChoice(choice);
        }
        finally
        {
            usage?.CompleteInteraction();
        }
    }
}