namespace MauiLab.Usage;

public enum UsageEventKind
{
    Installed,
    HistoryReset,
    SessionStarted,
    SessionEnded,
    Backgrounded,
    Foregrounded,
    PageOpened,
    PageClosed,
    AreaChanged,
    ControlPressed,
    SurfaceOpened,
    SelectionMade,
    Updated,
    PermissionRequested,
    ActionTaken
}

public static class UsageEndReason
{
    public const string Clean = "Clean";
    public const string Terminated = "Terminated";
}

/// <summary>
/// One immutable usage row. <see cref="UploadedUtc"/> stays empty until a future uploader stamps it.
/// </summary>
public sealed record UsageEvent(
    Guid Id,
    long Sequence,
    UsageEventKind Kind,
    DateTimeOffset Timestamp,
    Guid? SessionId,
    Guid? PageVisitId,
    string? Name,
    string? Route,
    TimeSpan? Elapsed,
    TimeSpan? Active,
    string? EndReason,
    DateTimeOffset? UploadedUtc,
    double? Width = null,
    double? Height = null,
    string? Mode = null,
    Guid? ParentId = null,
    string? Detail = null)
{
    public static int Compare(UsageEvent left, UsageEvent right)
    {
        var time = left.Timestamp.CompareTo(right.Timestamp);
        return time != 0 ? time : left.Sequence.CompareTo(right.Sequence);
    }
}
