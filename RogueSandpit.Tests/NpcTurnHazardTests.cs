using System.Collections.Generic;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class NpcTurnHazardTests
{
    [Fact]
    public void ContestedCellPriorityRotatesBetweenNpcTurns()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var first = new Orc(map, 10, 10, null) { State = NPCState.Active };
        var second = new Goblin(map, 12, 10, null) { State = NPCState.Active };
        first.ReceiveInvestigation((11, 10), NPCInvestigationSource.Noise);
        second.ReceiveInvestigation((11, 10), NPCInvestigationSource.Noise);
        map.NPCs.AddRange([first, second]);
        var game = new GameState(map, new Player { X = 40, Y = 40 });

        game.Update(PlayerCommand.Wait);

        Assert.Equal((11, 10), (first.X, first.Y));
        Assert.Equal((12, 10), (second.X, second.Y));

        first.X = 10;
        first.Y = 10;
        second.X = 12;
        second.Y = 10;
        game.Update(PlayerCommand.Wait);

        Assert.Equal((10, 10), (first.X, first.Y));
        Assert.Equal((11, 10), (second.X, second.Y));
    }

    [Fact]
    public void ActiveNpcInvestigatesNewlySeenCasualty()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10));
        var observer = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var casualty = new Orc(map, 12, 10, null) { State = NPCState.Dead };
        map.NPCs.AddRange([observer, casualty]);
        var events = new List<string>();

        observer.Move(new Player { X = 40, Y = 40 }, events.Add);

        Assert.Equal(NPCInvestigationSource.Casualty, observer.InvestigationSource);
        Assert.Equal((12, 10), observer.InvestigationOrigin);
        Assert.Equal(1, observer.ObservedCasualtyCount);
        Assert.Contains(events, entry => entry == $"{observer.Name} FOUND {casualty.Name} DEAD");
    }

    [Fact]
    public void SeeingPlayerOverridesCasualtyEvidence()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10), (13, 10));
        var observer = new Goblin(map, 10, 10, null) { State = NPCState.Active };
        var casualty = new Orc(map, 11, 10, null) { State = NPCState.Dead };
        map.NPCs.AddRange([observer, casualty]);

        observer.Move(new Player { X = 13, Y = 10 });

        Assert.Equal(NPCAwareness.Pursuing, observer.Awareness);
        Assert.Equal(NPCInvestigationSource.LastSeen, observer.InvestigationSource);
        Assert.Equal((13, 10), observer.InvestigationOrigin);
    }

    [Fact]
    public void SkilledNpcSpotsAndRoutesAroundNearbyTrap()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10),
            (10, 11), (11, 11), (12, 11));
        var troll = new Troll(map, 10, 10, null) { State = NPCState.Active };
        troll.ReceiveInvestigation((12, 10), NPCInvestigationSource.Noise);
        map.NPCs.Add(troll);
        map.PlaceTrap(11, 10, 18);
        var events = new List<string>();

        troll.Move(new Player { X = 40, Y = 40 }, events.Add);

        Assert.True(troll.IsKnownHazard(11, 10));
        Assert.Equal((10, 11), (troll.X, troll.Y));
        Assert.NotNull(map.GetTrapAt(11, 10));
        Assert.Contains(events, entry => entry == $"{troll.Name} SPOTTED TRAP");
    }

    [Fact]
    public void NpcClearsTrapKnowledgeAfterSeeingHazardIsGone()
    {
        Map map = CreateBlankMap();
        AddFloor(map, (10, 10), (11, 10), (12, 10),
            (10, 11), (11, 11), (12, 11));
        var troll = new Troll(map, 10, 10, null) { State = NPCState.Active };
        troll.ReceiveInvestigation((12, 10), NPCInvestigationSource.Noise);
        map.NPCs.Add(troll);
        map.PlaceTrap(11, 10, 18);

        troll.Move(new Player { X = 40, Y = 40 });
        map.RemoveTrap(map.GetTrapAt(11, 10));
        troll.Move(new Player { X = 40, Y = 40 });

        Assert.False(troll.IsKnownHazard(11, 10));
    }

    private static Map CreateBlankMap()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.GroundItems.Clear();
        map.Doors.Clear();
        map.PlacedTraps.Clear();
        for (int x = 0; x < map.Width; x++)
        for (int y = 0; y < map.Height; y++)
            map.MapCells[x, y].SetCellType(MapCellType.Wall);
        return map;
    }

    private static void AddFloor(Map map, params (int X, int Y)[] cells)
    {
        foreach ((int x, int y) in cells)
            map.MapCells[x, y].SetCellType(MapCellType.Floor);
    }
}
