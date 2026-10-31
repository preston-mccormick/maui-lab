using System.Collections.ObjectModel;

namespace MauiLab.Usage;

/// <summary>
/// Brings a bound row collection in line with a fresh projection using as few change
/// notifications as possible, so the list keeps its cells instead of rebuilding them.
/// </summary>
public static class TimelineRowDiff
{
    /// <summary>
    /// Patches <paramref name="current"/> in place and returns it, or returns a new collection
    /// when nothing in <paramref name="current"/> survives (first load, or after a reset).
    /// A new collection raises one reset on the view instead of one notification per row.
    /// </summary>
    public static ObservableCollection<TimelineRow> Apply(
        ObservableCollection<TimelineRow> current,
        IReadOnlyList<TimelineRow> latest)
    {
        if (current.Count == 0)
            return latest.Count == 0 ? current : new ObservableCollection<TimelineRow>(latest);

        var existing = new Dictionary<string, TimelineRow>(current.Count);
        foreach (var row in current)
            existing[row.Id] = row;

        var keep = 0;
        var wanted = new HashSet<string>(latest.Count);
        foreach (var row in latest)
        {
            wanted.Add(row.Id);
            if (existing.ContainsKey(row.Id))
                keep++;
        }

        if (keep == 0)
            return new ObservableCollection<TimelineRow>(latest);

        for (var i = current.Count - 1; i >= 0; i--)
        {
            if (!wanted.Contains(current[i].Id))
                current.RemoveAt(i);
        }

        for (var i = 0; i < latest.Count; i++)
        {
            var target = latest[i];
            if (i < current.Count && current[i].Id == target.Id)
            {
                Update(current[i], target);
                continue;
            }

            if (existing.TryGetValue(target.Id, out var moved))
            {
                // Already present further down: a session block moved up because it got a new event.
                var from = current.IndexOf(moved);
                if (from != i)
                    current.Move(from, i);
                Update(moved, target);
                continue;
            }

            current.Insert(i, target);
        }

        return current;
    }

    static void Update(TimelineRow row, TimelineRow target)
    {
        row.Detail = target.Detail;
        row.Rail = target.Rail;
    }
}
