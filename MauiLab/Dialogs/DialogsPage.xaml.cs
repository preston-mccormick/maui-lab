using System.Windows.Input;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.Input;
using MauiLab.Usage;
using Microsoft.Maui.Controls.Shapes;

namespace MauiLab.Dialogs;

public partial class DialogsPage : ContentPage
{
    static bool detailRouteRegistered;

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

    public ICommand ShowPopupCommand => new AsyncRelayCommand(() =>
        Track("Show Popup", "Popup", async () =>
        {
            var save = new Button { Text = "Save" };
            var discard = new Button { Text = "Don't save" };
            save.Clicked += async (_, _) => await this.ClosePopupAsync("Save");
            discard.Clicked += async (_, _) => await this.ClosePopupAsync("Don't save");

            var lightBackground = ResourceColor("LightBackground", "#FFFFFF");
            var darkBackground = ResourceColor("DarkBackground", "#222222");
            var title = new Label
            {
                Text = "Save changes?",
                HorizontalTextAlignment = TextAlignment.Center,
            };
            title.SetAppThemeColor(
                Label.TextColorProperty,
                ResourceColor("DarkText", "#000000"),
                ResourceColor("LightText", "#F1F1F1"));

            var popup = new VerticalStackLayout
            {
                Padding = 24,
                Spacing = 12,
                WidthRequest = 280,
                Children = { title, save, discard }
            };
            popup.SetAppThemeColor(VisualElement.BackgroundColorProperty, lightBackground, darkBackground);

            var shape = new RoundRectangle { CornerRadius = new CornerRadius(12), StrokeThickness = 0 };
            shape.SetAppTheme<Brush>(
                Shape.FillProperty,
                new SolidColorBrush(lightBackground),
                new SolidColorBrush(darkBackground));

            var result = await this.ShowPopupAsync<string>(popup, new PopupOptions
            {
                CanBeDismissedByTappingOutsideOfPopup = true,
                Shape = shape,
            });
            var choice = result.WasDismissedByTappingOutsideOfPopup ? "Dismissed" : result.Result;
            if (choice is not null)
                await Toast.Make(choice, textSize: 42).Show();
            return choice;
        }));

    public ICommand ShowSnackbarCommand => new AsyncRelayCommand(() =>
        Track("Show Snackbar", "Snackbar", () => ShowSnackbar(null)));

    public ICommand ShowAnchoredSnackbarCommand => new AsyncRelayCommand(() =>
        Track("Snackbar on Button", "Snackbar", () => ShowSnackbar(AnchoredSnackbarButton)));

    public ICommand ShowDetailCommand => new AsyncRelayCommand(() =>
        Track("Show Detail", "Detail", async () =>
        {
            EnsureDetailRoute();
            await Shell.Current.GoToAsync(nameof(DetailPage));
            return null;
        }));

    public ICommand ShowToastCommand => new AsyncRelayCommand(() =>
        Track("Show Toast", "Toast", async () =>
        {
            await Toast.Make("This is a toast", ToastDuration.Long, textSize: 42).Show();
            return null;
        }));

    async Task<string?> ShowSnackbar(IView? anchor)
    {
        string? choice = null;
        var snackbar = Snackbar.Make(
            "Draft saved",
            () => choice = "Undo",
            "Undo",
            TimeSpan.FromSeconds(4),
            SnackbarColors(),
            anchor);

        var dismissed = new TaskCompletionSource();
        void OnDismissed(object? sender, EventArgs e)
        {
            if (sender != snackbar)
                return;

            Snackbar.Dismissed -= OnDismissed;
            dismissed.TrySetResult();
        }

        Snackbar.Dismissed += OnDismissed;
        try
        {
            await snackbar.Show();
            await dismissed.Task;
        }
        finally
        {
            Snackbar.Dismissed -= OnDismissed;
        }

        if (choice is not null)
            await Toast.Make(choice, textSize: 42).Show();
        return choice ?? "Dismissed";
    }

    static SnackbarOptions SnackbarColors()
    {
        var dark = Application.Current?.RequestedTheme == AppTheme.Dark;
        var primary = ResourceColor("Primary", "#78CDD4");
        return new SnackbarOptions
        {
            BackgroundColor = dark
                ? ResourceColor("DarkerGray", "#333333")
                : ResourceColor("LighterGray", "#CCCCCC"),
            TextColor = dark ? primary : ResourceColor("DarkText", "#000000"),
            ActionButtonTextColor = primary,
        };
    }

    static void EnsureDetailRoute()
    {
        if (detailRouteRegistered)
            return;

        Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));
        detailRouteRegistered = true;
    }

    static Color ResourceColor(string key, string fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var value) is true && value is Color color)
            return color;
        return Color.FromArgb(fallback);
    }

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