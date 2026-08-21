using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class AwarenessSimulationTests
{
    [Fact]
    public void NoiseReachesOnlyActiveNpcsInsideManhattanRadius()
    {
        Map map = CreateBlankMap();
        var nearNpc = new Goblin(map, 13, 10, null) { State = NPCState.Active };
        var farNpc = new Orc(map, 17, 10, null) { State = NPCState.Active };
        var inactiveNpc = new Wretch(map, 11, 10, null) { State = NPCState.InActive };
        map.NPCs.AddRange([nearNpc, farNpc, inactiveNpc]);

        int listeners = map.NotifyNoise(10, 10, 4);

        Assert.Equal(1, listeners);
        Assert.Equal(NPCAwareness.Investigating, nearNpc.Awareness);
        Assert.Equal(NPCInvestigationSource.Noise, nearNpc.InvestigationSource);
        Assert.Equal((10, 10), nearNpc.InvestigationOrigin);
        Assert.Null(nearNpc.LastKnownPlayerPosition);
        Assert.Equal(NPCAwareness.Unaware, farNpc.Awareness);
        Assert.Equal(NPCAwareness.Unaware, inactiveNpc.Awareness);
    }

    [Fact]
    public void StrongerInvestigationEvidenceIsNotOverwrittenByNoise()
    {
        Map map = CreateBlankMap();
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };

        Assert.True(npc.ReceiveInvestigation((20, 20), NPCInvestigationSource.AllyAlert));
        Assert.False(npc.ReceiveInvestigation((11, 10), NPCInvestigationSource.Noise));

        Assert.Equal(NPCInvestigationSource.AllyAlert, npc.InvestigationSource);
        Assert.Equal((20, 20), npc.InvestigationOrigin);
    }

    [Fact]
    public void PursuingNpcIgnoresNoise()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        map.NPCs.Add(npc);
        npc.Move(player);

        int listeners = map.NotifyNoise(30, 30, 50);

        Assert.Equal(0, listeners);
        Assert.Equal(NPCAwareness.Pursuing, npc.Awareness);
        Assert.Equal(NPCInvestigationSource.LastSeen, npc.InvestigationSource);
        Assert.Equal((13, 10), npc.InvestigationOrigin);
    }

    [Fact]
    public void FirstSightingAlertsNearbyAllyOnceWithoutLiveTracking()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (10, 11), (30, 30));
        var spotter = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var nearbyAlly = new Orc(map, 10, 11, null) { State = NPCState.Active };
        var distantAlly = new Troll(map, 30, 30, null) { State = NPCState.Active };
        var player = new Player { X = 13, Y = 10 };
        var events = new List<string>();
        map.NPCs.AddRange([spotter, nearbyAlly, distantAlly]);

        spotter.Move(player, events.Add);

        Assert.Equal(NPCInvestigationSource.AllyAlert, nearbyAlly.InvestigationSource);
        Assert.Equal((13, 10), nearbyAlly.InvestigationOrigin);
        Assert.Equal(NPCAwareness.Unaware, distantAlly.Awareness);
        Assert.Contains(events, entry => entry.Contains("ALERTED 1 ALLIES"));

        player.X = 14;
        spotter.Move(player, events.Add);

        Assert.Equal((13, 10), nearbyAlly.InvestigationOrigin);
        Assert.Single(events, entry => entry.Contains("ALERTED"));
    }

    [Fact]
    public void OpeningDoorReportsNoiseListenersInEventLog()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (10, 11));
        var door = new Doorway(11, 10, DoorState.Closed);
        map.Doors.Add(door);
        map.MapCells[11, 10].SetCellType(MapCellType.Door);
        var player = new Player { X = 10, Y = 10 };
        var listener = new Goblin(map, 10, 11, null) { State = NPCState.Active };
        map.NPCs.Add(listener);
        var game = new GameState(map, player);

        game.Update(PlayerCommand.MoveRight);

        Assert.Equal(DoorState.Open, door.State);
        Assert.Contains("DOOR NOISE DREW 1 NPCS", game.EventLog.Entries);
    }

    [Fact]
    public void InvestigationConfidenceDecaysUntilNpcAbandonsSearch()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 40, Y = 40 };

        npc.ReceiveInvestigation((30, 30), NPCInvestigationSource.Noise);
        int startingConfidence = npc.InvestigationConfidence;

        for (int turn = 0; turn < startingConfidence; turn++) npc.Move(player);

        Assert.Equal(0, npc.InvestigationConfidence);
        Assert.Equal(NPCAwareness.Unaware, npc.Awareness);
        Assert.Equal(NPCInvestigationSource.None, npc.InvestigationSource);
        Assert.Null(npc.InvestigationOrigin);
    }

    [Fact]
    public void FreshEqualEvidenceRefreshesConfidenceAndTarget()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10));
        var npc = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var player = new Player { X = 40, Y = 40 };

        npc.ReceiveInvestigation((30, 30), NPCInvestigationSource.Noise);
        int fullConfidence = npc.InvestigationConfidence;
        npc.Move(player);

        Assert.True(npc.ReceiveInvestigation((25, 25), NPCInvestigationSource.Noise));
        Assert.Equal(fullConfidence, npc.InvestigationConfidence);
        Assert.Equal((25, 25), npc.InvestigationOrigin);
    }

    [Fact]
    public void ArchetypeHearingChangesWhoReceivesSameNoise()
    {
        Map map = CreateBlankMap();
        var troll = new Troll(map, 18, 10, null) { State = NPCState.Active };
        var skeleton = new Skeleton(map, 10, 18, null) { State = NPCState.Active };
        map.NPCs.AddRange([troll, skeleton]);

        int listeners = map.NotifyNoise(10, 10, 6);

        Assert.Equal(1, listeners);
        Assert.Equal(NPCInvestigationSource.Noise, troll.InvestigationSource);
        Assert.Equal(NPCAwareness.Unaware, skeleton.Awareness);
    }

    [Fact]
    public void ArchetypePersistenceChangesStartingConfidence()
    {
        Map map = CreateBlankMap();
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var skeleton = new Skeleton(map, 20, 20, null) { State = NPCState.Active };

        goblin.ReceiveInvestigation((30, 30), NPCInvestigationSource.AllyAlert);
        skeleton.ReceiveInvestigation((30, 30), NPCInvestigationSource.AllyAlert);

        Assert.True(skeleton.InvestigationConfidence > goblin.InvestigationConfidence);
    }

    [Fact]
    public void ArchetypeSightRangeChangesWhoSpotsPlayer()
    {
        Map map = CreateBlankMap();
        for (int x = 10; x <= 23; x++) AddFloor(map, (x, 10));
        for (int x = 10; x <= 19; x++) AddFloor(map, (x, 20));
        var goblin = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var troll = new Troll(map, 10, 20, null) { State = NPCState.Active };

        goblin.Move(new Player { X = 23, Y = 10 });
        troll.Move(new Player { X = 19, Y = 20 });

        Assert.Equal(NPCAwareness.Pursuing, goblin.Awareness);
        Assert.Equal(NPCAwareness.Unaware, troll.Awareness);
    }

    [Fact]
    public void ArchetypeAlertRadiusChangesHowFarSightingSpreads()
    {
        Map goblinMap = CreateBlankMap();
        var goblin = new Goblin(goblinMap, 10, 10, null) { State = NPCState.Active };
        var goblinAlly = new Orc(goblinMap, 19, 10, null) { State = NPCState.Active };
        goblinMap.NPCs.AddRange([goblin, goblinAlly]);

        Map skeletonMap = CreateBlankMap();
        var skeleton = new Skeleton(skeletonMap, 10, 10, null) { State = NPCState.Active };
        var skeletonAlly = new Orc(skeletonMap, 19, 10, null) { State = NPCState.Active };
        skeletonMap.NPCs.AddRange([skeleton, skeletonAlly]);

        Assert.Equal(1, goblinMap.AlertNearbyAllies(goblin, 20, 20));
        Assert.Equal(0, skeletonMap.AlertNearbyAllies(skeleton, 20, 20));
        Assert.Equal(NPCInvestigationSource.AllyAlert, goblinAlly.InvestigationSource);
        Assert.Equal(NPCAwareness.Unaware, skeletonAlly.Awareness);
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
}
