using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class EnvironmentalEffectTests
{
    [Fact]
    public void SmokeIsWalkableButBlocksSightBeyondItsCell()
    {
        Map map = BlankLine(10, 14);
        map.AddEnvironmentalEffect(EnvironmentalEffectType.Smoke, 12, 10, 4);

        Assert.True(map.IsWalkable(12, 10));
        Assert.True(map.HasLineOfSight(10, 10, 12, 10));
        Assert.False(map.HasLineOfSight(10, 10, 14, 10));
    }

    [Fact]
    public void ThrownSmokeBombIsConsumedAndCreatesTemporarySmoke()
    {
        Map map = BlankLine(10, 16);
        var player = new Player { X = 10, Y = 10 };
        Item bomb = ItemFactory.Create(ItemType.SmokeBomb);
        player.Inventory.TryAdd(bomb);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.DoesNotContain(bomb, player.Inventory.Items);
        EnvironmentalEffect smoke = map.GetEnvironmentalEffectAt(16, 10, EnvironmentalEffectType.Smoke);
        Assert.NotNull(smoke);
        Assert.Equal(3, smoke.RemainingTurns);
        Assert.Contains("SMOKE SPREAD", game.EventLog.Entries);
    }

    [Fact]
    public void FireBombBurnsNpcAtImpactAndCanKillWithNormalConsequences()
    {
        Map map = BlankLine(10, 16);
        var player = new Player { X = 10, Y = 10 };
        Item bomb = ItemFactory.Create(ItemType.FireBomb);
        player.Inventory.TryAdd(bomb);
        var npc = new Orc(map, 13, 10, null) { State = NPCState.Active, HP = bomb.Power };
        map.NPCs.Add(npc);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.Equal(NPCState.Dead, npc.State);
        Assert.Contains($"{npc.Name} BURNED {bomb.Power}", game.EventLog.Entries);
        Assert.Contains($"{npc.Name} DIED", game.EventLog.Entries);
    }

    [Fact]
    public void PlayerEnteringFireTakesDamage()
    {
        Map map = BlankLine(10, 11);
        var player = new Player { X = 10, Y = 10 };
        map.AddEnvironmentalEffect(EnvironmentalEffectType.Fire, 11, 10, 4, 6);
        var game = new GameState(map, player);
        int health = player.Health;

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal((11, 10), (player.X, player.Y));
        Assert.Equal(health - 6, player.Health);
        Assert.Contains("PLAYER BURNED 6", game.EventLog.Entries);
    }

    [Fact]
    public void NpcPathfindingRoutesAroundFire()
    {
        Map map = Blank();
        Floor(map, (10, 10), (11, 10), (12, 10), (10, 11), (11, 11), (12, 11));
        var npc = new Orc(map, 10, 10, null) { State = NPCState.Active };
        map.AddEnvironmentalEffect(EnvironmentalEffectType.Fire, 11, 10, 4, 6);

        var path = Pathfinding.FindPath(map, 10, 10, 12, 10, npc);

        Assert.Equal((10, 11), path[0]);
        Assert.DoesNotContain((11, 10), path);
    }

    [Fact]
    public void EnvironmentalEffectsExpireAfterTheirDuration()
    {
        Map map = Blank();
        map.AddEnvironmentalEffect(EnvironmentalEffectType.Smoke, 10, 10, 2);

        map.AgeEnvironmentalEffects();
        Assert.NotNull(map.GetEnvironmentalEffectAt(10, 10, EnvironmentalEffectType.Smoke));
        map.AgeEnvironmentalEffects();
        Assert.Empty(map.EnvironmentalEffects);
    }

    private static Map BlankLine(int fromX, int toX)
    {
        Map map = Blank();
        for (int x = fromX; x <= toX; x++) Floor(map, (x, 10));
        return map;
    }

    private static Map Blank()
    {
        var map = new Map(123);
        map.NPCs.Clear(); map.GroundItems.Clear(); map.Doors.Clear();
        map.PlacedTraps.Clear(); map.EnvironmentalEffects.Clear();
        for (int x = 0; x < map.Width; x++)
        for (int y = 0; y < map.Height; y++) map.MapCells[x, y].SetCellType(MapCellType.Wall);
        return map;
    }

    private static void Floor(Map map, params (int X, int Y)[] cells)
    {
        foreach (var cell in cells) map.MapCells[cell.X, cell.Y].SetCellType(MapCellType.Floor);
    }
}
