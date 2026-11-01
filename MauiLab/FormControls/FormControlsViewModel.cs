using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.FormControls;

/// <summary>
/// One sample record behind the Form Controls page. Every value the form shows or edits is a
/// property here, so swapping the bound instance rebinds the whole form.
/// </summary>
public partial class FormControlsViewModel : ObservableObject
{
    /// <summary>Picker items. An array so XAML can hand it to <c>Picker.ItemsSource</c> directly.</summary>
    public static readonly string[] Choices = ["Red", "Green", "Blue"];

    public const string RadioOne = "One";
    public const string RadioTwo = "Two";
    public const string RadioThree = "Three";

    public FormControlsViewModel(string name)
    {
        Name = name;
        Today = DateTime.Today;
    }

    /// <summary>Sample name shown in the gallery bar.</summary>
    public string Name { get; }

    /// <summary>Upper bound for the constrained date picker.</summary>
    public DateTime Today { get; }

    // Entry
    [ObservableProperty] public partial string? FullName { get; set; }
    [ObservableProperty] public partial string? Password { get; set; }
    [ObservableProperty] public partial string? ReadOnlyText { get; set; }
    [ObservableProperty] public partial string? DisabledText { get; set; }
    [ObservableProperty] public partial string? Required { get; set; }
    [ObservableProperty] public partial string? Email { get; set; }
    [ObservableProperty] public partial string? Number { get; set; }
    [ObservableProperty] public partial string? Exact { get; set; }
    [ObservableProperty] public partial string? Digit { get; set; }
    [ObservableProperty] public partial string? Url { get; set; }
    [ObservableProperty] public partial string? Letters { get; set; }
    [ObservableProperty] public partial string? Phone { get; set; }

    // Editor
    [ObservableProperty] public partial string? Notes { get; set; }
    [ObservableProperty] public partial string? GrowingNotes { get; set; }
    [ObservableProperty] public partial string? DisabledNotes { get; set; }

    // SearchBar
    [ObservableProperty] public partial string? Search { get; set; }
    [ObservableProperty] public partial string? DisabledSearch { get; set; }

    // Picker
    [ObservableProperty] public partial string? Choice { get; set; }
    [ObservableProperty] public partial string? SelectedChoice { get; set; }
    [ObservableProperty] public partial string? DisabledChoice { get; set; }

    // DatePicker
    [ObservableProperty] public partial DateTime? Date { get; set; }
    [ObservableProperty] public partial DateTime? ConstrainedDate { get; set; }
    [ObservableProperty] public partial DateTime? DisabledDate { get; set; }

    // TimePicker
    [ObservableProperty] public partial TimeSpan? Time12 { get; set; }
    [ObservableProperty] public partial TimeSpan? Time24 { get; set; }
    [ObservableProperty] public partial TimeSpan? DisabledTime { get; set; }

    // CheckBox
    [ObservableProperty] public partial bool Box1 { get; set; }
    [ObservableProperty] public partial bool Box2 { get; set; }
    [ObservableProperty] public partial bool DisabledBox { get; set; }

    // Switch
    [ObservableProperty] public partial bool Switch1 { get; set; }
    [ObservableProperty] public partial bool Switch2 { get; set; }
    [ObservableProperty] public partial bool DisabledSwitch { get; set; }

    // RadioButton
    [ObservableProperty] public partial string? RadioChoice { get; set; }

    // Slider
    [ObservableProperty] public partial double SliderValue { get; set; }
    [ObservableProperty] public partial double DisabledSliderValue { get; set; }

    // Stepper
    [ObservableProperty] public partial double StepperValue { get; set; }
    [ObservableProperty] public partial double DisabledStepperValue { get; set; }

    // Button
    [ObservableProperty] public partial int PressCount { get; set; }

    // RatingView
    [ObservableProperty] public partial double Stars { get; set; }
    [ObservableProperty] public partial double Hearts { get; set; }
    [ObservableProperty] public partial double ReadOnlyRating { get; set; }

    [RelayCommand]
    void Press() => PressCount++;
}
