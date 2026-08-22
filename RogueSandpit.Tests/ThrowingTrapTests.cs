using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class ThrowingTrapTests
{
    [Theory]
    [InlineData(ItemType.Armor, 3)]
    [InlineData(ItemType.RangedWeapon, 4)]
    [InlineData(ItemType.Trap, 6)]
    [InlineData(ItemType.HealingPotion, 1)]
    [InlineData(ItemType.Key, 1)]
    [InlineData(ItemType.Bandage, 1)]
    [InlineData(ItemType.SmokeBomb, 0)]
    [InlineData(ItemType.FireBomb, 0)]
    public void OrdinaryAndSpecialItemsHaveDefinedThrownImpactDamage(ItemType type, int expectedDamage)
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 16, 10);
        var player = new Player { X = 10, Y = 10 };
        Item item = ItemFactory.Create(type);
        player.Inventory.TryAdd(item);
        var target = new Troll(map, 13, 10, null) { State = NPCState.Active };
        map.NPCs.Add(target);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        string expectedEvent = expectedDamage > 0
            ? $"{item.Name} HIT {target.Name} {expectedDamage}"
            : $"{item.Name} HIT {target.Name}";
        Assert.Contains(expectedEvent, game.EventLog.Entries);
    }

    [Fact]
    public void ThrowSelectedItemLandsAtRangeAndUnequipsIt()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 18, 10);
        var player = new Player { X = 10, Y = 10 };
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        player.Inventory.TryAdd(weapon);
        player.Equip(weapon);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.Same(weapon, map.GetGroundItemAt(16, 10)?.Item);
        Assert.DoesNotContain(weapon, player.Inventory.Items);
        Assert.Null(player.EquippedWeapon);
        Assert.Contains("THREW IRON SWORD", game.EventLog.Entries);
    }

    [Fact]
    public void TerrainAndLivingNpcStopThrowBeforeObstruction()
    {
        Map wallMap = CreateBlankMap();
        AddFloor(wallMap, (10, 10), (11, 10), (12, 10));
        var wallPlayer = new Player { X = 10, Y = 10 };
        Item armor = ItemFactory.Create(ItemType.Armor);
        wallPlayer.Inventory.TryAdd(armor);
        new GameState(wallMap, wallPlayer).Update(PlayerCommand.ThrowItemRight);

        Map actorMap = CreateBlankMap();
        AddHorizontalFloor(actorMap, 10, 15, 10);
        var actorPlayer = new Player { X = 10, Y = 10 };
        Item key = ItemFactory.Create(ItemType.Key);
        actorPlayer.Inventory.TryAdd(key);
        actorMap.NPCs.Add(new Orc(actorMap, 13, 10, null) { State = NPCState.Active });
        new GameState(actorMap, actorPlayer).Update(PlayerCommand.ThrowItemRight);

        Assert.Same(armor, wallMap.GetGroundItemAt(12, 10)?.Item);
        Assert.Same(key, actorMap.GetGroundItemAt(12, 10)?.Item);
    }

    [Fact]
    public void ThrowImpactCreatesNoiseAtLandingCell()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 20, 10);
        var player = new Player { X = 10, Y = 10 };
        player.Inventory.TryAdd(ItemFactory.Create(ItemType.Key));
        var listener = new Goblin(map, 20, 10, null) { State = NPCState.Active };
        map.NPCs.Add(listener);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.Contains("IMPACT DREW 1 NPCS", game.EventLog.Entries);
    }

    [Fact]
    public void InvalidThrowKeepsItemAndDoesNotConsumeTurn()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var player = new Player { X = 10, Y = 10 };
        Item item = ItemFactory.Create(ItemType.Key);
        player.Inventory.TryAdd(item);
        map.RecordPlayerMovement(10, 10, 10, 11);
        int trailAge = Assert.Single(map.PlayerTrail).RemainingTurns;
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ThrowItemRight);

        Assert.Contains(item, player.Inventory.Items);
        Assert.Equal(trailAge, Assert.Single(map.PlayerTrail).RemainingTurns);
        Assert.Contains("CANNOT THROW THAT WAY", game.EventLog.Entries);
    }

    [Fact]
    public void PlaceTrapConsumesSelectedTrapOnValidAdjacentCell()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        var player = new Player { X = 10, Y = 10 };
        Item trapItem = ItemFactory.Create(ItemType.Trap);
        player.Inventory.TryAdd(trapItem);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.PlaceTrapRight);

        PlacedTrap trap = Assert.Single(map.PlacedTraps);
        Assert.Equal((11, 10), (trap.X, trap.Y));
        Assert.Equal(18, trap.Damage);
        Assert.DoesNotContain(trapItem, player.Inventory.Items);
    }

    [Fact]
    public void NpcEnteringTrapTakesDamageAndConsumesTrap()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var npc = new Orc(map, 10, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Right
        };
        int startingHp = npc.HP;
        map.NPCs.Add(npc);
        map.PlaceTrap(11, 10, 18);
        var events = new List<string>();

        npc.Move(new Player { X = 40, Y = 40 }, events.Add);

        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.Equal(startingHp - 18, npc.HP);
        Assert.Empty(map.PlacedTraps);
        Assert.Contains(events, entry => entry.Contains("TRIGGERED TRAP 18"));
    }

    [Fact]
    public void PlacedTrapDoesNotBlockPathfinding()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 13, 10);
        var npc = new Orc(map, 10, 10, null) { State = NPCState.Active };
        map.PlaceTrap(11, 10, 18);

        List<(int X, int Y)> path = Pathfinding.FindPath(map, 10, 10, 13, 10, npc);

        Assert.Equal((11, 10), path[0]);
    }

    [Fact]
    public void LethalTrapDropsNpcHeldItem()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        Item heldItem = ItemFactory.Create(ItemType.Key);
        var npc = new Wretch(map, 10, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Right,
            HeldItem = heldItem
        };
        map.PlaceTrap(11, 10, 100);

        npc.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(NPCState.Dead, npc.State);
        Assert.Same(heldItem, map.GetGroundItemAt(11, 10)?.Item);
        Assert.Null(npc.HeldItem);
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
