using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class FogOfWarTests
{
    [Fact]
    public void OpaqueCellIsVisibleButBlocksCellsBeyondIt()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (13, 10));
        map.MapCells[12, 10].SetCellType(MapCellType.Wall);

        map.UpdateVisibility(10, 10);

        Assert.True(map.MapCells[11, 10].IsVisible);
        Assert.True(map.MapCells[12, 10].IsVisible);
        Assert.False(map.MapCells[13, 10].IsVisible);
        Assert.False(map.MapCells[13, 10].IsDiscovered);
    }

    [Fact]
    public void PreviouslyVisibleCellRemainsDiscoveredWhenOutOfSight()
    {
        Map map = CreateBlankMap();
        AddFloor(map, Enumerable.Range(10, 8).Select(x => (x, 10)).ToArray());
        map.UpdateVisibility(10, 10, 2);
        Assert.True(map.MapCells[10, 10].IsVisible);

        map.UpdateVisibility(17, 10, 2);

        Assert.False(map.MapCells[10, 10].IsVisible);
        Assert.True(map.MapCells[10, 10].IsDiscovered);
        Assert.True(map.MapCells[17, 10].IsVisible);
    }

    [Fact]
    public void OpeningDoorRefreshCanRevealTerrainBeyondIt()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var door = new Doorway(11, 10, DoorState.Closed);
        map.Doors.Add(door);
        map.MapCells[11, 10].SetCellType(MapCellType.Door);

        map.UpdateVisibility(10, 10);

        Assert.True(map.MapCells[11, 10].IsVisible);
        Assert.False(map.MapCells[12, 10].IsVisible);

        door.State = DoorState.Open;
        map.UpdateVisibility(10, 10);

        Assert.True(map.MapCells[12, 10].IsVisible);
        Assert.True(map.MapCells[12, 10].IsDiscovered);
    }

    [Fact]
    public void VisibilityIsLimitedBySightRange()
    {
        Map map = CreateBlankMap();
        AddFloor(map, Enumerable.Range(10, 14).Select(x => (x, 10)).ToArray());

        map.UpdateVisibility(10, 10, 12);

        Assert.True(map.MapCells[22, 10].IsVisible);
        Assert.False(map.MapCells[23, 10].IsVisible);
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
                map.MapCells[x, y].IsVisible = false;
                map.MapCells[x, y].IsDiscovered = false;
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
}
