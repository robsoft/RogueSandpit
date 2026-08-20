using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class MapTopologyTests
{
    [Fact]
    public void DistinctRoomsNeverShareDirectlyWalkableEdgeAcrossManySeeds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            for (int x = 0; x < map.Width - 1; x++)
            {
                for (int y = 0; y < map.Height - 1; y++)
                {
                    AssertNotDifferentRooms(map, seed, x, y, x + 1, y);
                    AssertNotDifferentRooms(map, seed, x, y, x, y + 1);
                }
            }
        }
    }

    [Fact]
    public void GeneratedContentRemainsReachableAcrossManySeeds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            HashSet<(int X, int Y)> reachable = ReachableTerrain(map);

            Assert.All(map.RoomList, room => Assert.Contains(reachable, position =>
                map.MapCells[position.X, position.Y].ParentElement == room));
            Assert.All(map.GroundItems, item => Assert.Contains((item.X, item.Y), reachable));
            Special special = map.RoomList.SelectMany(room => room.Specials).Single();
            Assert.Contains((special.X, special.Y), reachable);
        }
    }

    private static void AssertNotDifferentRooms(Map map, int seed, int x1, int y1, int x2, int y2)
    {
        if (map.MapCells[x1, y1].ParentElement is not Room firstRoom
            || map.MapCells[x2, y2].ParentElement is not Room secondRoom
            || firstRoom == secondRoom)
        {
            return;
        }

        Assert.Fail($"Seed {seed} has directly adjacent rooms at ({x1},{y1}) and ({x2},{y2}).");
    }

    private static HashSet<(int X, int Y)> ReachableTerrain(Map map)
    {
        var reachable = new HashSet<(int X, int Y)> { (map.StartPosX, map.StartPosY) };
        var frontier = new Queue<(int X, int Y)>();
        frontier.Enqueue((map.StartPosX, map.StartPosY));

        while (frontier.TryDequeue(out (int X, int Y) current))
        {
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = (X: current.X + dx, Y: current.Y + dy);
                if (next.X < 0 || next.X >= map.Width || next.Y < 0 || next.Y >= map.Height) continue;
                if (map.MapCells[next.X, next.Y].CellType == MapCellType.Wall || !reachable.Add(next)) continue;
                frontier.Enqueue(next);
            }
        }

        return reachable;
    }
}
