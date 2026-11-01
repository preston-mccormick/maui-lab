namespace MauiLab.FormControls;

/// <summary>
/// The five sample records the Form Controls page cycles through. Same shape, different data,
/// so the same control can be seen with empty, short, long, valid, and invalid values.
/// </summary>
public static class FormSamples
{
    public const string Empty = "Empty";
    public const string Short = "Short";
    public const string Long = "Long";
    public const string Valid = "Valid";
    public const string Invalid = "Invalid";

    public const string LongParagraph =
        "The quick brown fox jumps over the lazy dog while the five boxing wizards jump quickly. " +
        "Pack my box with five dozen liquor jugs. How vexingly quick daft zebras jump. " +
        "Sphinx of black quartz, judge my vow. The jay, pig, fox, zebra and my wolves quack. " +
        "Jackdaws love my big sphinx of quartz, and waltz, nymph, for quick jigs vex Bud.";

    public static readonly string LongLetters = new string('a', 120) + new string('b', 120) + new string('c', 60);

    public static readonly string LongEmail = new string('a', 200) + "@example.com";

    public static readonly string LongUrl = "https://example.com/" + string.Join('/', Enumerable.Repeat("segment", 40));

    public static readonly string LongDigits = string.Concat(Enumerable.Repeat("abc123", 50));

    public static IReadOnlyList<FormControlsViewModel> Create() =>
    [
        CreateEmpty(),
        CreateShort(),
        CreateLong(),
        CreateValid(),
        CreateInvalid(),
    ];

    static FormControlsViewModel CreateEmpty() => new(Empty);

    static FormControlsViewModel CreateShort()
    {
        var today = DateTime.Today;
        return new FormControlsViewModel(Short)
        {
            FullName = "Al",
            Password = "pw",
            ReadOnlyText = "ro",
            DisabledText = "no",
            Required = "a",
            Email = "a@b.co",
            Number = "5",
            Exact = "yes",
            Digit = "a1",
            Url = "https://a.co",
            Letters = "ab",
            Phone = "(555) 1",
            Notes = "Hi",
            GrowingNotes = "Hi",
            DisabledNotes = "Hi",
            Search = "ab",
            DisabledSearch = "ab",
            Choice = "Red",
            SelectedChoice = "Red",
            DisabledChoice = "Red",
            Date = today.AddDays(-1),
            ConstrainedDate = today.AddDays(-1),
            DisabledDate = today.AddDays(-1),
            Time12 = new TimeSpan(9, 5, 0),
            Time24 = new TimeSpan(9, 5, 0),
            DisabledTime = new TimeSpan(9, 5, 0),
            Box1 = true,
            Switch1 = true,
            RadioChoice = FormControlsViewModel.RadioOne,
            SliderValue = 10,
            DisabledSliderValue = 10,
            StepperValue = 1,
            DisabledStepperValue = 1,
            Stars = 1,
            Hearts = 1,
            ReadOnlyRating = 1,
        };
    }

    static FormControlsViewModel CreateLong()
    {
        var today = DateTime.Today;
        return new FormControlsViewModel(Long)
        {
            FullName = LongParagraph,
            Password = LongLetters,
            ReadOnlyText = LongParagraph,
            DisabledText = LongParagraph,
            Required = LongParagraph,
            Email = LongEmail,
            Number = "10",
            Exact = "yes",
            Digit = LongDigits,
            Url = LongUrl,
            Letters = LongLetters,
            Phone = "(555) 123-4567",
            Notes = LongParagraph,
            GrowingNotes = LongParagraph + "\n\n" + LongParagraph,
            DisabledNotes = LongParagraph,
            Search = LongParagraph,
            DisabledSearch = LongParagraph,
            Choice = "Blue",
            SelectedChoice = "Blue",
            DisabledChoice = "Blue",
            Date = today.AddDays(-3650),
            ConstrainedDate = today.AddDays(-3650),
            DisabledDate = today.AddDays(-3650),
            Time12 = new TimeSpan(23, 59, 0),
            Time24 = new TimeSpan(23, 59, 0),
            DisabledTime = new TimeSpan(23, 59, 0),
            Box1 = true,
            Box2 = true,
            DisabledBox = true,
            Switch1 = true,
            Switch2 = true,
            DisabledSwitch = true,
            RadioChoice = FormControlsViewModel.RadioTwo,
            SliderValue = 100,
            DisabledSliderValue = 100,
            StepperValue = 10,
            DisabledStepperValue = 10,
            Stars = 5,
            Hearts = 5,
            ReadOnlyRating = 5,
        };
    }

    static FormControlsViewModel CreateValid()
    {
        var today = DateTime.Today;
        return new FormControlsViewModel(Valid)
        {
            FullName = "Ada Lovelace",
            Password = "correct horse",
            ReadOnlyText = "Read only text",
            DisabledText = "Disabled text",
            Required = "Present",
            Email = "ada@example.com",
            Number = "7",
            Exact = "yes",
            Digit = "abc123",
            Url = "https://example.com",
            Letters = "letters",
            Phone = "(555) 123-4567",
            Notes = "A short note about the analytical engine.",
            GrowingNotes = "First line.\nSecond line.\nThird line.",
            DisabledNotes = "Cannot be edited.",
            Search = "analytical engine",
            DisabledSearch = "analytical engine",
            Choice = "Green",
            SelectedChoice = "Green",
            DisabledChoice = "Green",
            Date = today.AddDays(-30),
            ConstrainedDate = today.AddDays(-30),
            DisabledDate = today.AddDays(-30),
            Time12 = new TimeSpan(14, 30, 0),
            Time24 = new TimeSpan(14, 30, 0),
            DisabledTime = new TimeSpan(14, 30, 0),
            Box1 = false,
            Box2 = true,
            DisabledBox = true,
            Switch1 = false,
            Switch2 = true,
            DisabledSwitch = true,
            RadioChoice = FormControlsViewModel.RadioTwo,
            SliderValue = 40,
            DisabledSliderValue = 60,
            StepperValue = 2,
            DisabledStepperValue = 5,
            Stars = 4,
            Hearts = 3,
            ReadOnlyRating = 5,
        };
    }

    static FormControlsViewModel CreateInvalid()
    {
        var today = DateTime.Today;
        return new FormControlsViewModel(Invalid)
        {
            FullName = "A name that is fine",
            Password = "x",
            ReadOnlyText = "Cannot edit",
            DisabledText = "Disabled",
            Required = "   ",
            Email = "not-an-email",
            Number = "0",
            Exact = "Yes",
            Digit = "abcdef",
            Url = "not a url",
            Letters = "abc123",
            Phone = "(555) 12",
            Notes = "Notes on an invalid record.",
            GrowingNotes = "Only one line.",
            DisabledNotes = "Disabled notes.",
            Search = "invalid",
            DisabledSearch = "invalid",
            Choice = null,
            SelectedChoice = "Red",
            DisabledChoice = "Red",
            Date = today.AddDays(-7),
            ConstrainedDate = today.AddDays(-7),
            DisabledDate = today.AddDays(-7),
            Time12 = TimeSpan.Zero,
            Time24 = TimeSpan.Zero,
            DisabledTime = TimeSpan.Zero,
            DisabledBox = true,
            DisabledSwitch = true,
            RadioChoice = FormControlsViewModel.RadioOne,
            SliderValue = 0,
            DisabledSliderValue = 100,
            StepperValue = 0,
            DisabledStepperValue = 10,
            Stars = 0,
            Hearts = 0,
            ReadOnlyRating = 2,
        };
    }
}
