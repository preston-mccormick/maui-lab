using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiLab.FormControls;

/// <summary>
/// Holds the sample forms and the one the page is bound to. Cycling swaps <see cref="Current"/>,
/// which rebinds the whole form in one assignment. Expander state lives here so cycling data
/// does not collapse a group the user opened.
/// </summary>
public partial class FormGallery : ObservableObject
{
    public FormGallery() : this(FormSamples.Create()) { }

    public FormGallery(IReadOnlyList<FormControlsViewModel> samples)
    {
        if (samples.Count == 0)
            throw new ArgumentException("At least one sample is required.", nameof(samples));

        Samples = samples;
        Current = samples[0];
    }

    public IReadOnlyList<FormControlsViewModel> Samples { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Position))]
    public partial int CurrentIndex { get; private set; }

    [ObservableProperty]
    public partial FormControlsViewModel Current { get; private set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EntryGlyph))]
    public partial bool IsEntryExpanded { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditorGlyph))]
    public partial bool IsEditorExpanded { get; set; }

    public string EntryGlyph => Glyph(IsEntryExpanded);

    public string EditorGlyph => Glyph(IsEditorExpanded);

    static string Glyph(bool expanded) => expanded ? "▾" : "▸";

    /// <summary>"1 of 5 · Empty"</summary>
    public string Position => $"{CurrentIndex + 1} of {Samples.Count} · {Current.Name}";

    [RelayCommand]
    void Next() => Select((CurrentIndex + 1) % Samples.Count);

    [RelayCommand]
    void Previous() => Select((CurrentIndex - 1 + Samples.Count) % Samples.Count);

    void Select(int index)
    {
        // Current first. CurrentIndex raises Position, which reads Current.Name, so the
        // bar would otherwise show the new index with the previous sample's name.
        Current = Samples[index];
        CurrentIndex = index;
    }
}
