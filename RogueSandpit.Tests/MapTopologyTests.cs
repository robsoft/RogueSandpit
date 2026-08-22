using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class MapTopologyTests
{
    [Fact]
    public void MapWidthExactlyFillsNativeCanvas()
    {
        var map = new Map(123);

        Assert.Equal(GameWrapper.NativeWidth, map.Width * map.CellScale);
        Assert.Equal(GameWrapper.NativeHeight, map.Height * map.CellScale + 20);
    }

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

    [Fact]
    public void SpecialUsesGreatestEligiblePathDistanceAcrossManySeeds()
    {
        for (int seed = 0; seed < 50; seed++)
        {
            var map = new Map(seed);
            Dictionary<(int X, int Y), int> distances = TerrainDistances(map);
            Special special = map.RoomList.SelectMany(room => room.Specials).Single();

            int greatestEligibleDistance = map.RoomList
                .SelectMany(room => Enumerable.Range(room.X1, room.X2 - room.X1)
                    .SelectMany(x => Enumerable.Range(room.Y1, room.Y2 - room.Y1).Select(y => (X: x, Y: y))))
                .Where(position => map.MapCells[position.X, position.Y].CellType is MapCellType.Floor or MapCellType.Special)
                .Where(distances.ContainsKey)
                .Max(position => distances[position]);

            Assert.Equal(greatestEligibleDistance, distances[(special.X, special.Y)]);
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
        return TerrainDistances(map).Keys.ToHashSet();
    }

    private static Dictionary<(int X, int Y), int> TerrainDistances(Map map)
    {
        var distances = new Dictionary<(int X, int Y), int> { [(map.StartPosX, map.StartPosY)] = 0 };
        var frontier = new Queue<(int X, int Y)>();
        frontier.Enqueue((map.StartPosX, map.StartPosY));

        while (frontier.TryDequeue(out (int X, int Y) current))
        {
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = (X: current.X + dx, Y: current.Y + dy);
                if (next.X < 0 || next.X >= map.Width || next.Y < 0 || next.Y >= map.Height) continue;
                if (map.MapCells[next.X, next.Y].CellType == MapCellType.Wall || distances.ContainsKey(next)) continue;
                distances[next] = distances[current] + 1;
                frontier.Enqueue(next);
            }
        }

        return distances;
    }
}
