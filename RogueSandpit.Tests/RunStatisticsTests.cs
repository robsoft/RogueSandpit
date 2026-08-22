using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class RunStatisticsTests
{
    [Fact]
    public void InvalidAndSelectionCommandsDoNotAlterStatistics()
    {
        (Map _, Player player, GameState game) = CreateScenario((10, 10));
        player.Inventory.TryAdd(ItemFactory.Create(ItemType.Key));

        game.Update(PlayerCommand.SelectNextItem);
        game.Update(PlayerCommand.UsePotion);

        Assert.Equal(0, game.Statistics.Turns);
        Assert.Equal(0, game.Statistics.ItemsConsumed);
        Assert.Equal(0, game.Statistics.DamageDealt);
    }

    [Fact]
    public void DeliberateAndAutomaticTurnsAreSeparated()
    {
        (_, _, GameState game) = CreateScenario((10, 10));

        game.Update(PlayerCommand.Wait);
        game.Update(PlayerCommand.Wait, suppressWaitEvent: true, automaticRealtimeWait: true);

        Assert.Equal(2, game.Statistics.Turns);
        Assert.Equal(1, game.Statistics.DeliberateTurns);
        Assert.Equal(1, game.Statistics.RealtimeTurns);
        Assert.Equal(game.TurnCount, game.Statistics.Turns);
    }

    [Fact]
    public void MeleeDamageAndDefeatAreReconciledAtTurnEnd()
    {
        (Map map, Player player, GameState game) = CreateScenario((10, 10), (11, 10));
        var target = new Wretch(map, 11, 10, null) { State = NPCState.Active, HP = 5 };
        map.NPCs.Add(target);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(1, game.Statistics.MeleeAttacks);
        Assert.Equal(5, game.Statistics.DamageDealt);
        Assert.Equal(1, game.Statistics.NpcsDefeated);
        Assert.Equal(1, game.Statistics.DefeatsByArchetype[CharacterTypes.Wretch]);
        Assert.Equal((10, 10), (player.X, player.Y));
    }

    [Fact]
    public void HealingConsumptionAndPickupAreRecordedOnSuccess()
    {
        (Map map, Player player, GameState game) = CreateScenario((10, 10), (11, 10));
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        Item key = ItemFactory.Create(ItemType.Key);
        player.Inventory.TryAdd(potion);
        player.TakeDamage(20);
        map.GroundItems.Add(new GroundItem(key, 11, 10));

        game.Update(PlayerCommand.UsePotion);
        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(20, game.Statistics.HealingReceived);
        Assert.Equal(1, game.Statistics.ItemsConsumed);
        Assert.Equal(1, game.Statistics.ItemsCollected);
    }

    [Fact]
    public void DoorObjectiveAndItemActionsRecordSuccessfulOutcomes()
    {
        (Map map, Player player, GameState game) = CreateScenario(
            (10, 10), (11, 10), (12, 10), (13, 10));
        var door = new Doorway(11, 10, DoorState.Locked);
        map.Doors.Add(door);
        map.MapCells[11, 10].SetCellType(MapCellType.Door);
        player.Inventory.TryAdd(ItemFactory.Create(ItemType.Key));

        game.Update(PlayerCommand.MoveRight);
        game.Update(PlayerCommand.MoveRight);
        map.MapCells[12, 10].SetCellType(MapCellType.Special);
        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(1, game.Statistics.DoorsUnlocked);
        Assert.Equal(3, game.Statistics.ObjectiveCollectedTurn);
        Assert.True(player.HasSpecial);
    }

    [Fact]
    public void NpcDamageDetectionAndDefeatCauseAreRecorded()
    {
        (Map map, Player player, GameState game) = CreateScenario((10, 10), (11, 10));
        var attacker = new Orc(map, 11, 10, null)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(attacker);

        game.Update(PlayerCommand.Wait);

        Assert.True(player.Dead);
        Assert.Equal(player.MaxHealth, game.Statistics.DamageReceived);
        Assert.Equal(1, game.Statistics.DetectionEpisodes);
        Assert.Equal(1, game.Statistics.MaximumPursuers);
        Assert.Equal($"SLAIN BY {attacker.Name}", game.Statistics.DefeatCause);
    }

    [Fact]
    public void TriggeredTrapAndPursuitTransitionAreReconciled()
    {
        (Map map, Player player, GameState game) = CreateScenario((10, 10), (11, 10), (12, 10));
        // Wretches do not spot adjacent traps, which keeps this test focused on
        // the statistics reconciliation after a trap is actually consumed.
        var npc = new Wretch(map, 12, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Left
        };
        map.NPCs.Add(npc);
        map.PlaceTrap(11, 10, 1, player);

        game.Update(PlayerCommand.Wait);

        Assert.Equal(1, game.Statistics.TrapsTriggered);
        Assert.Equal(1, game.Statistics.DetectionEpisodes);
        Assert.True(game.Statistics.NpcsAlerted >= 1);
    }

    private static (Map Map, Player Player, GameState Game) CreateScenario(
        params (int X, int Y)[] floor)
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.GroundItems.Clear();
        map.Doors.Clear();
        map.PlacedTraps.Clear();
        for (int x = 0; x < map.Width; x++)
            for (int y = 0; y < map.Height; y++)
                map.MapCells[x, y].SetCellType(MapCellType.Wall);
        foreach ((int x, int y) in floor) map.MapCells[x, y].SetCellType(MapCellType.Floor);

        var player = new Player { X = floor[0].X, Y = floor[0].Y };
        return (map, player, new GameState(map, player));
    }
}
