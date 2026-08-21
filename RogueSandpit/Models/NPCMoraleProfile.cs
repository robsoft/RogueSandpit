namespace RogueSandpit.Models;

public sealed record NPCMoraleProfile(
    int FleeHealthPercent,
    bool IsFearless,
    bool EnragesWhenWounded,
    int EnrageDamageBonus,
    int HelpCallRadius)
{
    public static NPCMoraleProfile Orc { get; } = new(20, false, false, 0, 7);
    public static NPCMoraleProfile Goblin { get; } = new(50, false, false, 0, 10);
    public static NPCMoraleProfile Skeleton { get; } = new(0, true, false, 0, 0);
    public static NPCMoraleProfile Troll { get; } = new(50, false, true, 5, 6);
    public static NPCMoraleProfile Wretch { get; } = new(40, false, false, 0, 8);
}
