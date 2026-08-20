using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class ActorArchitectureTests
{
    [Fact]
    public void GeneratedNpcsUseEligibleDistinctRoomCellsAcrossManySeeds()
    {
        for (int seed = 0; seed < 100; seed++)
        {
            var map = new Map(seed);
            int expectedCount = map.RoomList.Sum(room => (int)(room.Area / 35F));

            Assert.Equal(expectedCount, map.NPCs.Count);
            Assert.Equal(map.NPCs.Count, map.NPCs.Select(npc => (npc.X, npc.Y)).Distinct().Count());
            Assert.All(map.NPCs, npc =>
            {
                Assert.True(map.IsWalkable(npc.X, npc.Y));
                Assert.IsType<Room>(map.MapCells[npc.X, npc.Y].ParentElement);
                Assert.False(npc.X == map.StartPosX && npc.Y == map.StartPosY);
            });
        }
    }

    [Fact]
    public void CentralNpcEntryRuleHandlesTerrainActorsAndPlayer()
    {
        Map map = CreateBlankMap();
        map.MapCells[10, 10].SetCellType(MapCellType.Floor);
        map.MapCells[11, 10].SetCellType(MapCellType.Floor);
        var movingNpc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var blockingNpc = new Orc(map, 11, 10, null) { State = NPCState.Active };
        var player = new Player { X = 10, Y = 10 };
        map.NPCs.Add(movingNpc);
        map.NPCs.Add(blockingNpc);

        Assert.False(map.CanNpcEnter(9, 10, movingNpc, player));
        Assert.False(map.CanNpcEnter(10, 10, movingNpc, player));
        Assert.False(map.CanNpcEnter(11, 10, movingNpc, player));

        blockingNpc.TakeDamage(blockingNpc.HP);

        Assert.True(map.CanNpcEnter(11, 10, movingNpc, player));
    }

    [Fact]
    public void PlayerPlacementSynchronizesMapVisitAndVisibilityState()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        (int x, int y) = FindRoomFloor(map);
        var player = new Player();

        player.Place(map, x, y);

        Assert.Equal((x, y), (player.X, player.Y));
        Assert.Equal((x, y), (map.CurrentPlayerX, map.CurrentPlayerY));
        Assert.Same(map.MapCells[x, y].ParentElement, player.CurrentRoom);
        Assert.True(player.CurrentRoom.HasVisited);
        Assert.True(map.MapCells[x, y].IsVisible);
        Assert.True(map.MapCells[x, y].IsDiscovered);
    }

    private static Map CreateBlankMap()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                map.MapCells[x, y].SetCellType(MapCellType.Wall);
            }
        }
        return map;
    }

    private static (int X, int Y) FindRoomFloor(Map map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.MapCells[x, y].CellType == MapCellType.Floor
                    && map.MapCells[x, y].ParentElement is Room)
                {
                    return (x, y);
                }
            }
        }

        throw new InvalidOperationException("No room floor found.");
    }
}
