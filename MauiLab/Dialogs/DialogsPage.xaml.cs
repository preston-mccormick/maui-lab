using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace MauiLab.Dialogs;

public partial class DialogsPage : ContentPage
{
	public DialogsPage()
	{
		InitializeComponent();
    }

    public ICommand ShowAlertCommand => new AsyncRelayCommand(async () =>
    {
        await DisplayAlert("Alert!", "This is a test of the Emergency Broadcast System", "Acknowledge");
    });

    public ICommand ShowPromptCommand => new AsyncRelayCommand(async () =>
    {
        string result = await DisplayPromptAsync("Question 1", "What's your name?");
    });

    public ICommand ShowActionSheetCommand => new AsyncRelayCommand(async () =>
    {
        string action = await DisplayActionSheet("Message", "Cancel", "Destructive", "Button1", "Button2", "Button3");
    });
}