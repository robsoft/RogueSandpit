using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class PredictivePursuitTrailTests
{
    [Fact]
    public void LosingSightUsesObservedMovementToPredictContinuation()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 20, 10);
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 14, Y = 10 };

        npc.Move(player);
        player.X = 15;
        npc.Move(player);

        Assert.Equal((1, 0), npc.LastObservedPlayerMovement);
        Assert.Equal((19, 10), npc.PredictedInvestigationTarget);

        map.MapCells[13, 10].SetCellType(MapCellType.Wall);
        npc.Move(player);

        Assert.Equal(NPCAwareness.Investigating, npc.Awareness);
        Assert.Equal((15, 10), npc.LastKnownPlayerPosition);
        Assert.Equal((19, 10), npc.InvestigationOrigin);
    }

    [Fact]
    public void LosingSightWithoutObservedMovementFallsBackToLastSeenCell()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 15, 10);
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 15, Y = 10 };

        npc.Move(player);
        map.MapCells[12, 10].SetCellType(MapCellType.Wall);
        npc.Move(player);

        Assert.Null(npc.PredictedInvestigationTarget);
        Assert.Equal((15, 10), npc.InvestigationOrigin);
    }

    [Fact]
    public void SuccessfulPlayerMovementRecordsDirectionalTrail()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10));
        var player = new Player { X = 10, Y = 10 };
        var game = new GameState(map, player);

        game.Update(PlayerCommand.MoveRight);

        PlayerTrailClue clue = Assert.Single(map.PlayerTrail);
        Assert.Equal((10, 10), (clue.X, clue.Y));
        Assert.Equal((11, 10), (clue.NextX, clue.NextY));
    }

    [Fact]
    public void PlayerTrailExpiresAfterTwelveCompletedTurns()
    {
        Map map = CreateBlankMap();
        map.RecordPlayerMovement(10, 10, 11, 10);

        for (int turn = 0; turn < 11; turn++) map.AgePlayerTrail();
        Assert.Single(map.PlayerTrail);

        map.AgePlayerTrail();
        Assert.Empty(map.PlayerTrail);
    }

    [Fact]
    public void InvestigatingNpcDiscoversAndFollowsNearbyTrail()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 15, 10);
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 40, Y = 40 };
        map.RecordPlayerMovement(11, 10, 12, 10);
        npc.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);
        var events = new List<string>();

        npc.Move(player, events.Add);

        Assert.Equal(NPCInvestigationSource.Trail, npc.InvestigationSource);
        Assert.Equal((12, 10), npc.InvestigationOrigin);
        Assert.Equal((11, 10), (npc.X, npc.Y));
        Assert.Contains(events, entry => entry.Contains("FOUND TRAIL"));

        npc.Move(player, events.Add);
        Assert.Single(events, entry => entry.Contains("FOUND TRAIL"));
    }

    [Fact]
    public void InvestigatingNpcDoesNotDiscoverDistantTrail()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 15, 10);
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 40, Y = 40 };
        map.RecordPlayerMovement(13, 10, 14, 10);
        npc.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        npc.Move(player);

        Assert.Equal(NPCInvestigationSource.Noise, npc.InvestigationSource);
        Assert.Equal((15, 10), npc.InvestigationOrigin);
    }

    [Fact]
    public void BlockedPlayerMovementDoesNotLeaveTrail()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var game = new GameState(map, new Player { X = 10, Y = 10 });

        game.Update(PlayerCommand.MoveRight);

        Assert.Empty(map.PlayerTrail);
    }

    [Fact]
    public void AllyAlertAssignsDistinctWalkableSearchCells()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (12, 10), (12, 9), (13, 10), (12, 11), (11, 10));
        var source = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var first = new Orc(map, 10, 11, null) { State = NPCState.Active };
        var second = new Orc(map, 10, 12, null) { State = NPCState.Active };
        var third = new Orc(map, 10, 13, null) { State = NPCState.Active };
        map.NPCs.AddRange([source, first, second, third]);

        Assert.Equal(3, map.AlertNearbyAllies(source, 12, 10));

        Assert.Equal((12, 9), first.InvestigationOrigin);
        Assert.Equal((13, 10), second.InvestigationOrigin);
        Assert.Equal((12, 11), third.InvestigationOrigin);
    }

    [Fact]
    public void TrackingArchetypeFindsTrailBeyondGoblinRange()
    {
        Map wretchMap = CreateBlankMap();
        AddHorizontalFloor(wretchMap, 10, 15, 10);
        var wretch = new Wretch(wretchMap, 10, 10, null) { State = NPCState.Active };
        wretchMap.RecordPlayerMovement(12, 10, 13, 10);
        wretch.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        Map goblinMap = CreateBlankMap();
        AddHorizontalFloor(goblinMap, 10, 15, 10);
        var goblin = new Goblin(goblinMap, 10, 10, null) { State = NPCState.Active };
        goblinMap.RecordPlayerMovement(12, 10, 13, 10);
        goblin.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        wretch.Move(new Player { X = 40, Y = 40 });
        goblin.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(NPCInvestigationSource.Trail, wretch.InvestigationSource);
        Assert.Equal(NPCInvestigationSource.Noise, goblin.InvestigationSource);
    }

    [Fact]
    public void SkeletonCannotInterpretTrailUnderfoot()
    {
        Map map = CreateBlankMap();
        AddHorizontalFloor(map, 10, 15, 10);
        var skeleton = new Skeleton(map, 10, 10, null) { State = NPCState.Active };
        map.RecordPlayerMovement(10, 10, 11, 10);
        skeleton.ReceiveInvestigation((15, 10), NPCInvestigationSource.Noise);

        skeleton.Move(new Player { X = 40, Y = 40 });

        Assert.Equal(NPCInvestigationSource.Noise, skeleton.InvestigationSource);
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

    private static void AddHorizontalFloor(Map map, int fromX, int toX, int y)
    {
        for (int x = fromX; x <= toX; x++) AddFloor(map, (x, y));
    }

    private static void AddFloor(Map map, params (int X, int Y)[] positions)
    {
        foreach ((int x, int y) in positions) map.MapCells[x, y].SetCellType(MapCellType.Floor);
    }
}
