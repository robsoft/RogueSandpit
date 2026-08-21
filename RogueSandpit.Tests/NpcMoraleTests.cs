using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class NpcMoraleTests
{
    [Fact]
    public void ArchetypesReactDifferentlyToHeavyDamage()
    {
        Map map = CreateBlankMap();
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var skeleton = new Skeleton(map, 11, 10, null) { State = NPCState.Active };
        var troll = new Troll(map, 12, 10, null) { State = NPCState.Active };
        var orc = new Orc(map, 13, 10, null) { State = NPCState.Active };
        var wretch = new Wretch(map, 14, 10, null) { State = NPCState.Active };

        DamageToPercent(goblin, 50);
        DamageToPercent(skeleton, 10);
        DamageToPercent(troll, 50);
        DamageToPercent(orc, 30);
        DamageToPercent(wretch, 40);

        Assert.Equal(NPCMoraleState.Fleeing, goblin.MoraleState);
        Assert.Equal(NPCMoraleState.Fearless, skeleton.MoraleState);
        Assert.Equal(NPCMoraleState.Enraged, troll.MoraleState);
        Assert.Equal(troll.Damage + 5, troll.EffectiveDamage);
        Assert.Equal(NPCMoraleState.Steady, orc.MoraleState);
        Assert.Equal(NPCMoraleState.Fleeing, wretch.MoraleState);

        DamageToPercent(orc, 20);
        Assert.Equal(NPCMoraleState.Fleeing, orc.MoraleState);
    }

    [Fact]
    public void FleeingNpcMovesAwayFromLastReliableThreatPosition()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 4, 15, 10);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 12, Y = 10 };
        DamageToPercent(goblin, 50);

        goblin.Move(player);

        Assert.Equal(NPCMoraleState.Fleeing, goblin.MoraleState);
        Assert.Equal((9, 10), (goblin.X, goblin.Y));
        Assert.NotNull(goblin.RetreatTarget);
        Assert.True(Math.Abs(goblin.RetreatTarget.Value.X - 12) > 2);
    }

    [Fact]
    public void HelpCallSharesRememberedPositionOnlyOnce()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 4, 18, 10);
        AddFloor(map, (12, 9), (12, 11), (11, 9));
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 12, Y = 10 };
        var events = new List<string>();

        goblin.Move(player);
        var ally = new Orc(map, 11, 9, null) { State = NPCState.Active };
        map.NPCs.AddRange([goblin, ally]);
        DamageToPercent(goblin, 50);
        player.X = 40;
        player.Y = 40;

        goblin.Move(player, events.Add);
        goblin.Move(player, events.Add);

        Assert.True(goblin.HasCalledForHelp);
        Assert.Equal(NPCInvestigationSource.AllyAlert, ally.InvestigationSource);
        Assert.True(Math.Abs(ally.InvestigationOrigin.Value.X - 12)
            + Math.Abs(ally.InvestigationOrigin.Value.Y - 10) <= 1);
        Assert.DoesNotContain(ally.InvestigationOrigin.Value, new[] { (40, 40) });
        Assert.Single(events, entry => entry.Contains("CALLED FOR HELP"));
    }

    [Fact]
    public void HelpCallDoesNotDistractPursuingAlly()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 4, 18, 10);
        AddFloor(map, (10, 11), (11, 11), (12, 11));
        var caller = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var ally = new Orc(map, 10, 11, null) { State = NPCState.Active };
        var player = new Player { X = 12, Y = 11 };
        map.NPCs.AddRange([caller, ally]);
        ally.Move(player);
        DamageToPercent(caller, 50);
        caller.Move(player);

        Assert.Equal(NPCAwareness.Pursuing, ally.Awareness);
        Assert.Equal(NPCInvestigationSource.LastSeen, ally.InvestigationSource);
    }

    [Fact]
    public void FurtherDamageDuringSameRetreatDoesNotResetHelpCall()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 4, 18, 10);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 12, Y = 10 };
        var events = new List<string>();
        DamageToPercent(goblin, 50);

        goblin.Move(player, events.Add);
        goblin.TakeDamage(1);
        goblin.Move(player, events.Add);

        Assert.Single(events, entry => entry.Contains("CALLED FOR HELP"));
    }

    [Fact]
    public void ReachingSafetyAllowsLaterDamageToStartNewRetreat()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 4, 18, 10);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 12, Y = 10 };
        DamageToPercent(goblin, 50);

        for (int turn = 0; turn < 10 && goblin.MoraleState == NPCMoraleState.Fleeing; turn++)
            goblin.Move(player);

        Assert.Equal(NPCMoraleState.Shaken, goblin.MoraleState);
        goblin.TakeDamage(1);
        Assert.Equal(NPCMoraleState.Fleeing, goblin.MoraleState);
        Assert.False(goblin.HasCalledForHelp);
    }

    private static void DamageToPercent(BaseNPC npc, int percent)
    {
        int targetHp = Math.Max(1, npc.MaxHP * percent / 100);
        npc.TakeDamage(npc.HP - targetHp);
    }

    private static Map CreateBlankMap()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.Doors.Clear();
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.MapCells[x, y].SetCellType(MapCellType.Wall);
        return map;
    }

    private static void AddHorizontalFloor(Map map, int fromX, int toX, int y)
    {
        for (int x = fromX; x <= toX; x++) AddFloor(map, (x, y));
    }

    private static void AddFloor(Map map, params (int X, int Y)[] positions)
    {
        foreach ((int x, int y) in positions) map.MapCells[x, y].SetCellType(MapCellType.Floor);
    }
}
