namespace RogueSandpit.Models;

public sealed record NPCRangedProfile(int MinimumRange, int MaximumRange, int Damage)
{
    public static NPCRangedProfile Goblin { get; } = new(2, 6, 6);
}
