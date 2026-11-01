using MauiLab.FormControls;

namespace MauiLabTests;

public class FormGalleryTests
{
    readonly FormGallery gallery = new();

    [Fact]
    public void OpensOnEmpty()
    {
        Assert.Equal(FormSamples.Empty, gallery.Current.Name);
        Assert.Equal(0, gallery.CurrentIndex);
        Assert.Equal("1 of 5 · Empty", gallery.Position);
    }

    [Fact]
    public void Next_WrapsFromInvalidToEmpty()
    {
        for (var i = 0; i < 4; i++)
            gallery.NextCommand.Execute(null);
        Assert.Equal(FormSamples.Invalid, gallery.Current.Name);
        Assert.Equal("5 of 5 · Invalid", gallery.Position);

        gallery.NextCommand.Execute(null);

        Assert.Equal(FormSamples.Empty, gallery.Current.Name);
        Assert.Equal("1 of 5 · Empty", gallery.Position);
    }

    [Fact]
    public void Previous_WrapsFromEmptyToInvalid()
    {
        gallery.PreviousCommand.Execute(null);

        Assert.Equal(FormSamples.Invalid, gallery.Current.Name);
        Assert.Equal(4, gallery.CurrentIndex);
        Assert.Equal("5 of 5 · Invalid", gallery.Position);
    }

    [Fact]
    public void Edit_SurvivesARoundTrip()
    {
        gallery.NextCommand.Execute(null);
        Assert.Equal(FormSamples.Short, gallery.Current.Name);
        gallery.Current.FullName = "Ada";

        gallery.NextCommand.Execute(null);
        Assert.Equal(FormSamples.Long, gallery.Current.Name);
        Assert.NotEqual("Ada", gallery.Current.FullName);

        gallery.PreviousCommand.Execute(null);
        Assert.Equal(FormSamples.Short, gallery.Current.Name);
        Assert.Equal("Ada", gallery.Current.FullName);
    }

    [Fact]
    public void ExpanderState_StartsEntryOpenEditorClosed()
    {
        Assert.True(gallery.IsEntryExpanded);
        Assert.False(gallery.IsEditorExpanded);
    }

    [Fact]
    public void Cycling_DoesNotChangeExpanderState()
    {
        gallery.IsEditorExpanded = true;
        gallery.IsEntryExpanded = false;

        gallery.NextCommand.Execute(null);
        gallery.PreviousCommand.Execute(null);
        gallery.PreviousCommand.Execute(null);

        Assert.True(gallery.IsEditorExpanded);
        Assert.False(gallery.IsEntryExpanded);
    }

    [Fact]
    public void Cycling_RaisesCurrentAndPosition()
    {
        var changed = new List<string?>();
        gallery.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        gallery.NextCommand.Execute(null);

        Assert.Contains(nameof(FormGallery.Current), changed);
        Assert.Contains(nameof(FormGallery.Position), changed);
    }

    [Fact]
    public void Position_IsCompleteWhenRaised()
    {
        // A bound label reads Position inside the PropertyChanged handler, so both the index
        // and the sample name must already be updated at that moment.
        var seen = new List<string>();
        gallery.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(FormGallery.Position))
                seen.Add(gallery.Position);
        };

        gallery.NextCommand.Execute(null);
        gallery.PreviousCommand.Execute(null);

        Assert.Equal(["2 of 5 · Short", "1 of 5 · Empty"], seen);
    }
}
