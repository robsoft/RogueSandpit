using System.Collections.Generic;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class NpcRangedTacticsTests
{
    [Fact]
    public void GoblinShootsVisiblePlayerInsidePreferredRange()
    {
        Map map = Blank();
        FloorLine(map, 10, 14);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        int health = player.Health;
        map.NPCs.Add(goblin);
        var events = new List<string>();

        goblin.Move(player, events.Add);

        Assert.Equal(health - goblin.RangedProfile.Damage, player.Health);
        Assert.Equal((10, 10), (goblin.X, goblin.Y));
        Assert.Contains(events, entry => entry == $"{goblin.Name} SHOT PLAYER {goblin.RangedProfile.Damage}");
    }

    [Fact]
    public void GoblinRangedDamageRespectsArmor()
    {
        Map map = Blank();
        FloorLine(map, 10, 14);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        Item armor = ItemFactory.CreateEquipment(ItemType.Armor, 1);
        player.Inventory.TryAdd(armor);
        player.EquipArmor(armor);
        int health = player.Health;

        goblin.Move(player);

        Assert.Equal(health - 1, player.Health);
    }

    [Fact]
    public void ClosedDoorBlocksShotAndGoblinSpendsTurnOpeningIt()
    {
        Map map = Blank();
        FloorLine(map, 10, 14);
        var door = new Doorway(11, 10, DoorState.Closed);
        map.Doors.Add(door);
        map.MapCells[11, 10].SetCellType(MapCellType.Door);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        int health = player.Health;
        goblin.ReceiveInvestigation((13, 10), NPCInvestigationSource.LastSeen);

        goblin.Move(player);

        Assert.Equal(health, player.Health);
        Assert.Equal(DoorState.Open, door.State);
    }

    [Fact]
    public void AdjacentGoblinStepsAwayBeforeFiringOnLaterTurn()
    {
        Map map = Blank();
        FloorLine(map, 10, 13);
        var player = new Player { X = 10, Y = 10 };
        var goblin = new Goblin(map, 11, 10, null) { State = NPCState.Active };
        map.NPCs.Add(goblin);

        goblin.Move(player);

        Assert.Equal((12, 10), (goblin.X, goblin.Y));
        int health = player.Health;
        goblin.Move(player);
        Assert.Equal(health - goblin.RangedProfile.Damage, player.Health);
    }

    [Fact]
    public void TrappedAdjacentGoblinFallsBackToMelee()
    {
        Map map = Blank();
        map.MapCells[10, 10].SetCellType(MapCellType.Floor);
        map.MapCells[11, 10].SetCellType(MapCellType.Floor);
        var player = new Player { X = 10, Y = 10 };
        var goblin = new Goblin(map, 11, 10, null) { State = NPCState.Active };
        int health = player.Health;
        var events = new List<string>();

        goblin.Move(player, events.Add);

        Assert.True(player.Health < health);
        Assert.Contains(events, entry => entry.StartsWith($"{goblin.Name} HIT PLAYER"));
        Assert.DoesNotContain(events, entry => entry.Contains("SHOT PLAYER"));
    }

    [Fact]
    public void GoblinOutsideMaximumRangeAdvancesNormally()
    {
        Map map = Blank();
        FloorLine(map, 10, 20);
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 18, Y = 10 };
        map.NPCs.Add(goblin);

        goblin.Move(player);

        Assert.Equal((11, 10), (goblin.X, goblin.Y));
    }

    private static Map Blank()
    {
        var map = new Map(123);
        map.NPCs.Clear(); map.GroundItems.Clear(); map.Doors.Clear(); map.PlacedTraps.Clear();
        for (int x = 0; x < map.Width; x++)
        for (int y = 0; y < map.Height; y++) map.MapCells[x, y].SetCellType(MapCellType.Wall);
        return map;
    }

    private static void FloorLine(Map map, int fromX, int toX)
    {
        for (int x = fromX; x <= toX; x++) map.MapCells[x, 10].SetCellType(MapCellType.Floor);
    }
}
