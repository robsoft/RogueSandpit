using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class StatusEffectImpactTests
{
    [Fact]
    public void ReapplyingEffectKeepsLongerDurationAndStrongerPower()
    {
        var effects = new StatusEffectCollection();

        effects.Apply(StatusEffectType.Bleeding, 2, 1, "FIRST");
        effects.Apply(StatusEffectType.Bleeding, 3, 2, "SECOND");

        TimedStatusEffect effect = Assert.Single(effects.Effects);
        Assert.Equal(3, effect.RemainingTurns);
        Assert.Equal(2, effect.Power);
        Assert.Equal("SECOND", effect.Source);
    }

    [Fact]
    public void StunnedNpcSkipsExactlyOneAction()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var npc = new Orc(map, 10, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Right
        };
        npc.ApplyStatus(StatusEffectType.Stunned, 1, source: "TEST");

        npc.Move(new Player { X = 40, Y = 40 });
        Assert.Equal((10, 10), (npc.X, npc.Y));
        Assert.Empty(npc.StatusEffects.Effects);

        npc.Move(new Player { X = 40, Y = 40 });
        Assert.Equal((11, 10), (npc.X, npc.Y));
    }

    [Fact]
    public void BleedingDamagesNpcForConfiguredActorTurns()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var npc = new Orc(map, 10, 10, null) { State = NPCState.Active };
        int startingHp = npc.HP;
        npc.ApplyStatus(StatusEffectType.Bleeding, 3, 2, "TEST");

        for (int turn = 0; turn < 3; turn++) npc.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(startingHp - 6, npc.HP);
        Assert.Empty(npc.StatusEffects.Effects);
    }

    [Fact]
    public void StunnedPlayerLosesNextAction()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        var player = new Player { X = 10, Y = 10 };
        player.ApplyStatus(StatusEffectType.Stunned, 1, source: "TEST");
        var game = new GameState(map, player);

        game.Update(PlayerCommand.MoveRight);
        Assert.Equal((10, 10), (player.X, player.Y));
        Assert.Contains("PLAYER STUNNED", game.EventLog.Entries);

        game.Update(PlayerCommand.MoveRight);
        Assert.Equal((11, 10), (player.X, player.Y));
    }

    [Fact]
    public void PlayerCanDieFromBleedingAtTurnBoundary()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var player = new Player { X = 10, Y = 10 };
        player.TakeDamage(player.Health - 1);
        player.ApplyStatus(StatusEffectType.Bleeding, 1, 2, "TEST");
        var game = new GameState(map, player);

        game.Update(PlayerCommand.Wait);

        Assert.True(player.Dead);
        Assert.Equal(GameOutcome.Lost, game.Outcome);
        Assert.Contains("PLAYER BLED 2", game.EventLog.Entries);
    }

    [Fact]
    public void ThrownWeaponHitsFirstNpcAndAppliesBleeding()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 16, 10);
        var player = new Player { X = 10, Y = 10 };
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        player.Inventory.TryAdd(weapon);
        var npc = new Orc(map, 13, 10, null) { State = NPCState.Active };
        int startingHp = npc.HP;
        map.NPCs.Add(npc);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.Equal(startingHp - weapon.Power - 2, npc.HP);
        TimedStatusEffect bleeding = Assert.Single(npc.StatusEffects.Effects);
        Assert.Equal(StatusEffectType.Bleeding, bleeding.Type);
        Assert.Equal(2, bleeding.RemainingTurns);
        Assert.Same(weapon, map.GetGroundItemAt(12, 10)?.Item);
    }

    [Fact]
    public void ThrownHealingPotionShattersInsteadOfLanding()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 16, 10);
        var player = new Player { X = 10, Y = 10 };
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        player.Inventory.TryAdd(potion);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.DoesNotContain(potion, player.Inventory.Items);
        Assert.DoesNotContain(map.GroundItems, groundItem => groundItem.Item == potion);
        Assert.Contains("HEALING POTION SHATTERED", game.EventLog.Entries);
    }

    [Fact]
    public void HuntingTrapStunsSurvivingNpcNextTurn()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var npc = new Orc(map, 10, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Right
        };
        map.PlaceTrap(11, 10, 18);

        npc.Move(new Player { X = 40, Y = 40 });
        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.True(npc.StatusEffects.Has(StatusEffectType.Stunned));

        npc.Move(new Player { X = 40, Y = 40 });
        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.False(npc.StatusEffects.Has(StatusEffectType.Stunned));
    }

    [Fact]
    public void BleedingDeathDropsNpcHeldItem()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        Item heldItem = ItemFactory.Create(ItemType.Key);
        var npc = new Wretch(map, 10, 10, null)
        {
            State = NPCState.Active,
            HeldItem = heldItem
        };
        npc.ApplyStatus(StatusEffectType.Bleeding, 1, npc.HP, "TEST");

        npc.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(NPCState.Dead, npc.State);
        Assert.Same(heldItem, map.GetGroundItemAt(10, 10)?.Item);
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
