using System.Collections.Generic;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class TrapVarietyRangedTests
{
    [Fact]
    public void SnareStunsForTwoActionsWithoutDamage()
    {
        Map map = Blank();
        Floor(map, (10, 10), (11, 10), (12, 10));
        var npc = new Orc(map, 10, 10, null) { State = NPCState.Active, Direction = Direction.Right };
        int hp = npc.HP;
        map.NPCs.Add(npc);
        map.PlaceTrap(11, 10, 0, kind: TrapKind.Snare);

        npc.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(hp, npc.HP);
        TimedStatusEffect effect = Assert.Single(npc.StatusEffects.Effects);
        Assert.Equal(2, effect.RemainingTurns);
        Assert.Contains("Snare", effect.Source);
    }

    [Fact]
    public void AlarmTrapAlertsNearbyNpcWithoutDamageOrStun()
    {
        Map map = Blank();
        Floor(map, (10, 10), (11, 10), (12, 10));
        var victim = new Orc(map, 10, 10, null) { State = NPCState.Active, Direction = Direction.Right };
        var listener = new Goblin(map, 12, 10, null) { State = NPCState.Active };
        int hp = victim.HP;
        map.NPCs.AddRange([victim, listener]);
        map.PlaceTrap(11, 10, 0, kind: TrapKind.Alarm);

        victim.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(hp, victim.HP);
        Assert.False(victim.StatusEffects.Has(StatusEffectType.Stunned));
        Assert.Equal(NPCInvestigationSource.Noise, listener.InvestigationSource);
    }

    [Fact]
    public void BowAutoEquipsAndHitsFirstNpcWithoutBeingConsumed()
    {
        Map map = Blank();
        for (int x = 10; x <= 16; x++) Floor(map, (x, 10));
        var player = new Player { X = 10, Y = 10 };
        Item bow = ItemFactory.Create(ItemType.RangedWeapon);
        Assert.True(player.TryCollectItem(bow, out bool autoEquipped));
        var target = new Orc(map, 13, 10, null) { State = NPCState.Active };
        int hp = target.HP;
        map.NPCs.Add(target);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.FireRangedRight);

        Assert.True(autoEquipped);
        Assert.Same(bow, player.EquippedRangedWeapon);
        Assert.Contains(bow, player.Inventory.Items);
        Assert.Equal(hp - bow.Power, target.HP);
        Assert.Equal((10, 10), (player.X, player.Y));
    }

    [Fact]
    public void FiringWithoutBowDoesNotConsumeTurn()
    {
        Map map = Blank();
        Floor(map, (10, 10), (11, 10));
        map.RecordPlayerMovement(10, 10, 11, 10);
        int age = Assert.Single(map.PlayerTrail).RemainingTurns;
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.FireRangedRight);

        Assert.Equal(age, Assert.Single(map.PlayerTrail).RemainingTurns);
        Assert.Contains("NO RANGED WEAPON EQUIPPED", game.EventLog.Entries);
    }

    private static Map Blank()
    {
        var map = new Map(123);
        map.NPCs.Clear(); map.GroundItems.Clear(); map.Doors.Clear(); map.PlacedTraps.Clear();
        for (int x = 0; x < map.Width; x++)
        for (int y = 0; y < map.Height; y++) map.MapCells[x, y].SetCellType(MapCellType.Wall);
        return map;
    }

    private static void Floor(Map map, params (int X, int Y)[] cells)
    {
        foreach (var cell in cells) map.MapCells[cell.X, cell.Y].SetCellType(MapCellType.Floor);
    }
}
