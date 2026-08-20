using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class PathfindingTests
{
    [Fact]
    public void LineOfSightIsBlockedByWall()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));

        Assert.True(map.HasLineOfSight(10, 10, 12, 10));

        map.MapCells[11, 10].SetCellType(MapCellType.Wall);

        Assert.False(map.HasLineOfSight(10, 10, 12, 10));
    }

    [Fact]
    public void AStarFindsShortestDetourAroundWall()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (10, 11), (11, 11), (12, 11), (12, 10));

        var path = Pathfinding.FindPath(map, 10, 10, 12, 10);

        Assert.Equal(4, path.Count);
        Assert.Equal((12, 10), path[^1]);
        Assert.DoesNotContain((11, 10), path);
    }

    [Fact]
    public void AStarTreatsLivingNpcAsBlocked()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var blocker = new Orc(map, 11, 10, null) { State = NPCState.Active };
        map.NPCs.Add(blocker);

        var path = Pathfinding.FindPath(map, 10, 10, 12, 10);

        Assert.Empty(path);
    }

    [Fact]
    public void AStarDoesNotPathOntoNpcAtDestination()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var blocker = new Orc(map, 12, 10, null) { State = NPCState.Active };
        map.NPCs.Add(blocker);

        var path = Pathfinding.FindPath(map, 10, 10, 12, 10);

        Assert.Empty(path);
    }

    [Fact]
    public void VisibleNpcTakesOneStepTowardPlayer()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);

        npc.Move(player);

        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.True(npc.HasSeenPlayer);
        Assert.True(npc.IsPursuingPlayer);
    }

    [Fact]
    public void DiagonallyAdjacentNpcMovesInsteadOfAttacking()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (10, 11), (11, 11));
        var npc = new Skeleton(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 11, Y = 11 };
        int startingHealth = player.Health;
        map.NPCs.Add(npc);

        npc.Move(player);

        Assert.Equal(startingHealth, player.Health);
        Assert.Equal(1, Math.Abs(npc.X - player.X) + Math.Abs(npc.Y - player.Y));
        Assert.True(npc.IsPursuingPlayer);
    }

    [Fact]
    public void NpcStopsPursuingWhenLineOfSightIsLost()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);
        npc.Move(player);
        Assert.True(npc.IsPursuingPlayer);

        map.MapCells[12, 10].SetCellType(MapCellType.Wall);
        npc.Move(player);

        Assert.False(npc.IsPursuingPlayer);
        Assert.True(npc.HasSeenPlayer);
        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);
        Assert.Equal((13, 10), npc.LastKnownPlayerPosition);
    }

    [Fact]
    public void NpcForgetsPlayerAfterInvestigatingLastKnownPosition()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);

        npc.Move(player);
        player.X = 20;
        player.Y = 20;
        npc.Move(player);
        npc.Move(player);
        Assert.Equal((13, 10), (npc.X, npc.Y));
        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);

        npc.Move(player);

        Assert.Equal(NPCAwareness.Unaware, npc.Awareness);
        Assert.Null(npc.LastKnownPlayerPosition);
    }

    [Fact]
    public void InvestigatingNpcRetainsTargetWhileAnotherNpcBlocksPath()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);
        npc.Move(player);
        player.X = 20;
        player.Y = 20;
        var blocker = new Orc(map, 12, 10, null) { State = NPCState.Active };
        map.NPCs.Add(blocker);

        npc.Move(player);

        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.Equal((13, 10), npc.LastKnownPlayerPosition);
        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);

        blocker.TakeDamage(blocker.HP);
        npc.Move(player);

        Assert.Equal((12, 10), (npc.X, npc.Y));
        Assert.Equal((13, 10), npc.LastKnownPlayerPosition);
    }

    [Fact]
    public void InvestigatingNpcReturnsToPursuitWhenPlayerIsSeenAgain()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10), (11, 11));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);
        npc.Move(player);
        map.MapCells[12, 10].SetCellType(MapCellType.Wall);
        npc.Move(player);
        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);

        player.X = 11;
        player.Y = 11;
        npc.Move(player);

        Assert.Equal(NPCAwareness.Pursuing, npc.Awareness);
        Assert.Equal((11, 11), npc.LastKnownPlayerPosition);
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

    private static void AddFloor(Map map, params (int X, int Y)[] positions)
    {
        foreach ((int x, int y) in positions)
        {
            map.MapCells[x, y].SetCellType(MapCellType.Floor);
        }
    }
}
