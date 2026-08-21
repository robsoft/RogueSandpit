using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class DirectionalActionTrailTests
{
    [Fact]
    public void ToggleDoorCommandClosesAdjacentOpenDoorAndMakesNoise()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (10, 11));
        Doorway door = AddDoor(map, 11, 10, DoorState.Open);
        var listener = new Goblin(map, 10, 11, null) { State = NPCState.Active };
        map.NPCs.Add(listener);
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.ToggleDoorRight);

        Assert.Equal(DoorState.Closed, door.State);
        Assert.Contains("CLOSED DOOR", game.EventLog.Entries);
        Assert.Contains("DOOR NOISE DREW 1 NPCS", game.EventLog.Entries);
    }

    [Fact]
    public void InvalidToggleDirectionDoesNotConsumeTurn()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        AddDoor(map, 11, 10, DoorState.Open);
        map.RecordPlayerMovement(10, 10, 11, 10);
        int remainingTurns = Assert.Single(map.PlayerTrail).RemainingTurns;
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.ToggleDoorUp);

        Assert.Equal(remainingTurns, Assert.Single(map.PlayerTrail).RemainingTurns);
        Assert.Contains("NO OPERABLE DOOR THAT WAY", game.EventLog.Entries);
    }

    [Fact]
    public void AdjacentOperableDoorQueryIncludesOpenAndClosedButExcludesLocked()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (9, 10), (10, 9));
        Doorway right = AddDoor(map, 11, 10, DoorState.Open);
        Doorway left = AddDoor(map, 9, 10, DoorState.Closed);
        AddDoor(map, 10, 9, DoorState.Locked);

        Assert.Equal([right, left], map.GetAdjacentOperableDoors(10, 10));
    }

    [Fact]
    public void DoorContainingNpcCannotBeClosed()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        Doorway door = AddDoor(map, 11, 10, DoorState.Open);
        map.NPCs.Add(new Goblin(map, 11, 10, null) { State = NPCState.Active });
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.ToggleDoorRight);

        Assert.Equal(DoorState.Open, door.State);
        Assert.Empty(map.GetAdjacentOperableDoors(10, 10));
        Assert.Contains("DOORWAY BLOCKED", game.EventLog.Entries);
    }

    [Fact]
    public void ToggleDoorCommandOpensAdjacentClosedDoorWithoutMovingPlayer()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        Doorway door = AddDoor(map, 11, 10, DoorState.Closed);
        var player = new Player { X = 10, Y = 10 };
        var game = new GameState(map, player);

        game.Update(PlayerCommand.ToggleDoorRight);

        Assert.Equal(DoorState.Open, door.State);
        Assert.Equal((10, 10), (player.X, player.Y));
        Assert.Contains("OPENED DOOR", game.EventLog.Entries);
    }

    [Fact]
    public void ToggleLockedDoorDoesNotConsumeTurn()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        Doorway door = AddDoor(map, 11, 10, DoorState.Locked);
        map.RecordPlayerMovement(10, 10, 11, 10);
        int remainingTurns = Assert.Single(map.PlayerTrail).RemainingTurns;
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.ToggleDoorRight);

        Assert.Equal(DoorState.Locked, door.State);
        Assert.Equal(remainingTurns, Assert.Single(map.PlayerTrail).RemainingTurns);
        Assert.Contains("NO OPERABLE DOOR THAT WAY", game.EventLog.Entries);
    }

    [Fact]
    public void FalseTrailCommandCreatesShortWeakDecoy()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.LayFalseTrailRight);

        PlayerTrailClue clue = Assert.Single(map.PlayerTrail);
        Assert.False(clue.IsAuthentic);
        Assert.Equal(1, clue.Strength);
        Assert.Equal((11, 10), (clue.NextX, clue.NextY));
        Assert.Equal(5, clue.RemainingTurns);
    }

    [Fact]
    public void DoorwayAndCorridorMovementLeaveStrongerClues()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (20, 20), (21, 20));
        AddDoor(map, 11, 10, DoorState.Open);
        var corridor = new Corridor(20, 20, 21, 20);
        map.MapCells[20, 20] = new MapCell(20, 20, MapCellType.Floor, corridor);

        map.RecordPlayerMovement(10, 10, 11, 10);
        map.RecordPlayerMovement(20, 20, 21, 20);

        Assert.Equal(3, map.PlayerTrail.Single(clue => clue.X == 10).Strength);
        Assert.Equal(18, map.PlayerTrail.Single(clue => clue.X == 10).RemainingTurns);
        Assert.Equal(2, map.PlayerTrail.Single(clue => clue.X == 20).Strength);
        Assert.Equal(16, map.PlayerTrail.Single(clue => clue.X == 20).RemainingTurns);
    }

    [Fact]
    public void StrongTrackerRejectsFalseTrailWhileGoblinFollowsIt()
    {
        Map strongMap = CreateBlankMap();
        AddHorizontalFloor(strongMap, 10, 15, 10);
        var wretch = new Wretch(strongMap, 10, 10, null) { State = NPCState.Active };
        strongMap.RecordFalseTrail(12, 10, 1, 0);
        wretch.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        Map weakMap = CreateBlankMap();
        AddHorizontalFloor(weakMap, 10, 15, 10);
        var goblin = new Goblin(weakMap, 10, 10, null) { State = NPCState.Active };
        weakMap.RecordFalseTrail(11, 10, 1, 0);
        goblin.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        wretch.Move(new Player { X = 40, Y = 40 });
        goblin.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(NPCInvestigationSource.Noise, wretch.InvestigationSource);
        Assert.Equal(NPCInvestigationSource.Trail, goblin.InvestigationSource);
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

    private static Doorway AddDoor(Map map, int x, int y, DoorState state)
    {
        var door = new Doorway(x, y, state);
        map.Doors.Add(door);
        map.MapCells[x, y].SetCellType(MapCellType.Door);
        return door;
    }
}
