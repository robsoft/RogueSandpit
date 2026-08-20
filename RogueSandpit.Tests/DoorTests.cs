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
}
