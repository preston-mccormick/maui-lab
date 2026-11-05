namespace MauiLab.Collections;

/// <summary>One timings switch for every Collections page.</summary>
public static class CollectionTimings
{
    static readonly List<WeakReference<CollectionPageModel>> Live = [];

    public static bool Enabled { get; private set; }

    public static void Attach(CollectionPageModel model)
    {
        Prune();
        Live.Add(new WeakReference<CollectionPageModel>(model));
        model.AdoptTimings(Enabled);
    }

    public static void Publish(bool on)
    {
        Enabled = on;
        Prune();
        foreach (var weak in Live)
        {
            if (weak.TryGetTarget(out var model))
                model.AdoptTimings(on);
        }
    }

    internal static void Reset()
    {
        Enabled = false;
        Live.Clear();
    }

    static void Prune()
    {
        Live.RemoveAll(weak => !weak.TryGetTarget(out _));
    }
}
