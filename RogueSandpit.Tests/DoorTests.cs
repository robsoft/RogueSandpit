using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class DoorTests
{
    [Fact]
    public void GeneratedMapHasUniqueDoorsAndReachableEntranceKey()
    {
        var map = new Map(123);

        Assert.NotEmpty(map.Doors);
        Assert.Contains(map.Doors, door => door.State == DoorState.Locked);
        Assert.Equal(map.Doors.Count, map.Doors.Select(door => (door.X1, door.Y1)).Distinct().Count());
        Assert.Contains(map.GroundItems, item =>
            item.Item.Type == ItemType.Key
            && item.X == map.StartPosX
            && item.Y == map.StartPosY - 1);
    }

    [Fact]
    public void ClosedDoorOpensOnFirstBumpAndIsCrossedOnSecond()
    {
        (Map map, Player player, GameState game, int doorX, int doorY) = CreateDoorScenario(DoorState.Closed);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(DoorState.Open, map.GetDoorAt(doorX, doorY).State);
        Assert.Equal(doorX - 1, player.X);
        Assert.Contains("OPENED DOOR", game.EventLog.Entries);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(doorX, player.X);
    }

    [Fact]
    public void LockedDoorWithoutKeyStaysLocked()
    {
        (Map map, Player player, GameState game, int doorX, int doorY) = CreateDoorScenario(DoorState.Locked);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(DoorState.Locked, map.GetDoorAt(doorX, doorY).State);
        Assert.Equal(doorX - 1, player.X);
        Assert.Contains("DOOR LOCKED", game.EventLog.Entries);
    }

    [Fact]
    public void KeyUnlocksDoorAndIsRetained()
    {
        (Map map, Player player, GameState game, int doorX, int doorY) = CreateDoorScenario(DoorState.Locked);
        Item key = ItemFactory.Create(ItemType.Key);
        player.Inventory.TryAdd(key);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(DoorState.Open, map.GetDoorAt(doorX, doorY).State);
        Assert.Contains(key, player.Inventory.Items);
        Assert.Equal(doorX - 1, player.X);
        Assert.Contains("UNLOCKED DOOR", game.EventLog.Entries);
    }

    [Fact]
    public void ClosedDoorBlocksLineOfSightAndPathUntilOpened()
    {
        (Map map, _, _, int doorX, int doorY) = CreateDoorScenario(DoorState.Closed);

        Assert.False(map.HasLineOfSight(doorX - 1, doorY, doorX + 1, doorY));
        Assert.Empty(Pathfinding.FindPath(map, doorX - 1, doorY, doorX, doorY));

        map.GetDoorAt(doorX, doorY).State = DoorState.Open;

        Assert.True(map.HasLineOfSight(doorX - 1, doorY, doorX + 1, doorY));
        Assert.NotEmpty(Pathfinding.FindPath(map, doorX - 1, doorY, doorX, doorY));
    }

    [Fact]
    public void InvestigatingNpcSpendsTurnOpeningClosedDoorThenCrossesIt()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        var events = new List<string>();
        map.NPCs.Add(npc);
        npc.Move(player);
        player.X = 20;
        player.Y = 20;
        Doorway door = AddDoor(map, 12, 10, DoorState.Closed);

        npc.Move(player, events.Add);

        Assert.Equal(DoorState.Open, door.State);
        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.Contains($"{npc.Name} OPENED DOOR", events);

        npc.Move(player, events.Add);

        Assert.Equal((12, 10), (npc.X, npc.Y));
    }

    [Fact]
    public void InvestigatingNpcCannotOpenLockedDoor()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);
        npc.Move(player);
        player.X = 20;
        player.Y = 20;
        Doorway door = AddDoor(map, 12, 10, DoorState.Locked);

        npc.Move(player);

        Assert.Equal(DoorState.Locked, door.State);
        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);
    }

    [Fact]
    public void UnawareNpcDoesNotOpenClosedDoorWhileWandering()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        Doorway door = AddDoor(map, 11, 10, DoorState.Closed);
        var npc = new Goblin(map, 10, 10, null)
        {
            State = NPCState.Active,
            Direction = Direction.Right
        };
        map.NPCs.Add(npc);
        var player = new Player { X = 20, Y = 20 };

        npc.Move(player);

        Assert.Equal(DoorState.Closed, door.State);
        Assert.Equal((10, 10), (npc.X, npc.Y));
    }

    private static (Map Map, Player Player, GameState Game, int DoorX, int DoorY) CreateDoorScenario(DoorState state)
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.GroundItems.Clear();

        for (int y = 1; y < map.Height - 1; y++)
        {
            for (int x = 1; x < map.Width - 2; x++)
            {
                if (!map.IsWalkable(x - 1, y) || !map.IsWalkable(x, y) || !map.IsWalkable(x + 1, y)) continue;

                map.Doors.RemoveAll(door => door.X1 == x && door.Y1 == y);
                map.Doors.Add(new Doorway(x, y, state));
                map.MapCells[x, y].SetCellType(MapCellType.Door);
                var player = new Player { X = x - 1, Y = y };
                map.CurrentPlayerX = player.X;
                map.CurrentPlayerY = player.Y;
                return (map, player, new GameState(map, player), x, y);
            }
        }

        throw new InvalidOperationException("Generated map contained no three adjacent open cells.");
    }

    private static Map CreateBlankMap()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.Doors.Clear();
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                map.MapCells[x, y].SetCellType(MapCellType.Wall);
            }
        }
        return map;
    }

    private static void AddFloor(Map map, params (int X, int Y)[] positions)
    {
        foreach ((int x, int y) in positions)
        {
            map.MapCells[x, y].SetCellType(MapCellType.Floor);
        }
    }

    private static Doorway AddDoor(Map map, int x, int y, DoorState state)
    {
        var door = new Doorway(x, y, state);
        map.Doors.Add(door);
        map.MapCells[x, y].SetCellType(MapCellType.Door);
        return door;
    }
}
