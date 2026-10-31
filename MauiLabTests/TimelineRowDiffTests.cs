using System.Collections.ObjectModel;
using System.Collections.Specialized;
using MauiLab.Usage;

namespace MauiLabTests;

public class TimelineRowDiffTests
{
    [Fact]
    public void FirstLoad_ReturnsANewCollection()
    {
        var current = new ObservableCollection<TimelineRow>();
        var latest = Rows("a", "b", "c");

        var result = TimelineRowDiff.Apply(current, latest);

        Assert.NotSame(current, result);
        Assert.Equal(["a", "b", "c"], result.Select(r => r.Id));
    }

    [Fact]
    public void InsertAtTop_KeepsExistingRowsAndRaisesOneAdd()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("b", "c"));
        var kept = current.ToArray();
        var changes = Record(current);

        var result = TimelineRowDiff.Apply(current, Rows("a", "b", "c"));

        Assert.Same(current, result);
        Assert.Equal(["a", "b", "c"], result.Select(r => r.Id));
        Assert.Same(kept[0], result[1]);
        Assert.Same(kept[1], result[2]);
        var change = Assert.Single(changes);
        Assert.Equal(NotifyCollectionChangedAction.Add, change.Action);
        Assert.Equal(0, change.NewStartingIndex);
    }

    [Fact]
    public void InsertInTheMiddle_ShiftsNeighboursWithoutRecreatingThem()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("a", "c"));
        var kept = current.ToArray();

        TimelineRowDiff.Apply(current, Rows("a", "b", "c"));

        Assert.Equal(["a", "b", "c"], current.Select(r => r.Id));
        Assert.Same(kept[0], current[0]);
        Assert.Same(kept[1], current[2]);
    }

    [Fact]
    public void Remove_DropsOnlyTheMissingRows()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("a", "b", "c", "d"));
        var changes = Record(current);

        TimelineRowDiff.Apply(current, Rows("a", "c"));

        Assert.Equal(["a", "c"], current.Select(r => r.Id));
        Assert.Equal(2, changes.Count);
        Assert.All(changes, c => Assert.Equal(NotifyCollectionChangedAction.Remove, c.Action));
    }

    [Fact]
    public void NothingSurvives_ReturnsANewCollectionInsteadOfPatching()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("a", "b"));
        var changes = Record(current);

        var result = TimelineRowDiff.Apply(current, Rows("x", "y", "z"));

        Assert.NotSame(current, result);
        Assert.Empty(changes);
        Assert.Equal(["x", "y", "z"], result.Select(r => r.Id));
    }

    [Fact]
    public void Reorder_MovesTheBlockInsteadOfRecreatingIt()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("a", "b", "c"));
        var kept = current.ToArray();

        TimelineRowDiff.Apply(current, Rows("c", "a", "b"));

        Assert.Equal(["c", "a", "b"], current.Select(r => r.Id));
        Assert.Same(kept[2], current[0]);
        Assert.Same(kept[0], current[1]);
        Assert.Same(kept[1], current[2]);
    }

    [Fact]
    public void SurvivingRows_TakeTheNewDetailAndRail()
    {
        var current = new ObservableCollection<TimelineRow>(Rows("a"));
        var row = current[0];
        var changed = new List<string>();
        row.PropertyChanged += (_, e) => changed.Add(e.PropertyName!);

        var fresh = new TimelineRow("a", 1, "Title", "new detail", "Activity", "now", UsageCategory.Navigation)
        {
            Rail = new TimelineRail(1, UsageCategory.Navigation, true, true, null, [0])
        };
        TimelineRowDiff.Apply(current, [fresh]);

        Assert.Same(row, current[0]);
        Assert.Equal("new detail", row.Detail);
        Assert.True(row.Rail.LineFromTop);
        Assert.Equal([0], row.Rail.VerticalLanes);
        Assert.Equal(["Detail", "Rail"], changed);

        // Applying the same values again is silent.
        changed.Clear();
        TimelineRowDiff.Apply(current, [fresh]);
        Assert.Empty(changed);
    }

    static TimelineRow[] Rows(params string[] ids) =>
        ids.Select(id => new TimelineRow(id, 1, "Title " + id, "", "Activity", "now", UsageCategory.Navigation)).ToArray();

    static List<NotifyCollectionChangedEventArgs> Record(ObservableCollection<TimelineRow> collection)
    {
        var changes = new List<NotifyCollectionChangedEventArgs>();
        collection.CollectionChanged += (_, e) => changes.Add(e);
        return changes;
    }
}
