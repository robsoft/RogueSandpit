using System.Text;
using Microsoft.Xna.Framework;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class MapGenerationReproducibilityTests
{
    [Fact]
    public void ReinitialisingMapReproducesCompleteGeneratedRun()
    {
        var map = new Map(8675309);
        string first = GeneratedSignature(map);

        map.Initialise();

        Assert.Equal(8675309, map.Seed);
        Assert.Equal(first, GeneratedSignature(map));
    }

    [Fact]
    public void RegeneratingWithExplicitSeedChangesOwnedSeedAndIsRepeatable()
    {
        var map = new Map(123);
        string first = GeneratedSignature(map);

        map.Regenerate(456);
        string second = GeneratedSignature(map);
        map.Initialise();

        Assert.Equal(456, map.Seed);
        Assert.NotEqual(first, second);
        Assert.Equal(second, GeneratedSignature(map));
    }

    [Fact]
    public void DoorPruningPreservesLockedCandidatesAndSeparatesClosedDoorsAcrossManySeeds()
    {
        int totalPruned = 0;
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            List<Point> candidates = DoorCandidates(map);
            int expectedLocked = (candidates.Count + 3) / 4;

            Assert.Equal(candidates.Count, map.DoorCandidateCount);
            Assert.Equal(candidates.Count, map.Doors.Count + map.PrunedDoorwayCount);
            Assert.Equal(expectedLocked, map.Doors.Count(door => door.State == DoorState.Locked));
            Assert.All(map.Doors, door => Assert.Contains(new Point(door.X1, door.Y1), candidates));

            foreach (Doorway closed in map.Doors.Where(door => door.State == DoorState.Closed))
            {
                bool clustered = map.Doors.Any(other => other != closed
                    && Math.Abs(other.X1 - closed.X1) <= 2
                    && Math.Abs(other.Y1 - closed.Y1) <= 2);
                Assert.False(clustered,
                    $"Seed {seed} retained clustered closed door at ({closed.X1},{closed.Y1}).");
            }

            totalPruned += map.PrunedDoorwayCount;
        }

        Assert.True(totalPruned > 0, "Representative seeds did not exercise doorway pruning.");
    }

    private static List<Point> DoorCandidates(Map map)
    {
        var candidates = new List<Point>();
        for (int x = 1; x < map.Width - 1; x++)
        {
            for (int y = 1; y < map.Height - 1; y++)
            {
                if (map.MapCells[x, y].ParentElement is not Corridor corridor
                    || map.Exits.Contains(corridor)) continue;
                bool touchesRoom = map.MapCells[x - 1, y].ParentElement is Room
                    || map.MapCells[x + 1, y].ParentElement is Room
                    || map.MapCells[x, y - 1].ParentElement is Room
                    || map.MapCells[x, y + 1].ParentElement is Room;
                if (touchesRoom) candidates.Add(new Point(x, y));
            }
        }
        return candidates;
    }

    private static string GeneratedSignature(Map map)
    {
        var signature = new StringBuilder();
        signature.Append($"SEED:{map.Seed};START:{map.StartPosX},{map.StartPosY};");
        for (int y = 0; y < map.Height; y++)
            for (int x = 0; x < map.Width; x++)
                signature.Append((int)map.MapCells[x, y].CellType);

        foreach (Doorway door in map.Doors)
            signature.Append($";D:{door.X1},{door.Y1},{door.State}");
        foreach (BaseNPC npc in map.NPCs)
            signature.Append($";N:{npc.CharacterType},{npc.Name},{npc.X},{npc.Y},{npc.HeldItem?.Name},{npc.HeldItem?.Tier}");
        foreach (GroundItem item in map.GroundItems)
            signature.Append($";I:{item.Item.Name},{item.Item.Power},{item.Item.Tier},{item.X},{item.Y}");
        foreach (Special special in map.RoomList.SelectMany(room => room.Specials))
            signature.Append($";S:{special.X},{special.Y}");
        return signature.ToString();
    }
}
