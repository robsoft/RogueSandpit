namespace RogueSandpit.Models;

public sealed record NPCAwarenessProfile(
    int SightRange,
    int HearingAdjustment,
    int AllyAlertRadius,
    int PersistenceAdjustment,
    int TrailDetectionRange,
    int TrapDetectionRange)
{
    public static NPCAwarenessProfile Orc { get; } = new(12, 2, 6, 3, 0, 0);
    public static NPCAwarenessProfile Goblin { get; } = new(14, 0, 10, -2, 1, 1);
    public static NPCAwarenessProfile Skeleton { get; } = new(11, -3, 4, 5, -1, 0);
    public static NPCAwarenessProfile Troll { get; } = new(8, 4, 5, 5, 2, 2);
    public static NPCAwarenessProfile Wretch { get; } = new(10, 3, 7, 0, 2, 0);
}
