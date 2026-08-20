using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class GameStateTests
{
    [Fact]
    public void BumpingLivingNpcDealsDamageWithoutMovingPlayer()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        var npc = new Orc(map, x + 1, y, map.MapCells[x, y].ParentElement)
        {
            State = NPCState.Active,
            HP = player.Damage + 1
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Equal(x, player.X);
        Assert.Equal(y, player.Y);
        Assert.Equal(1, npc.HP);
        Assert.Contains($"PLAYER HIT {npc.Name} {player.Damage}", gameState.EventLog.Entries);
    }

    [Fact]
    public void KilledNpcStopsOccupyingItsCell()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        var npc = new Goblin(map, x + 1, y, map.MapCells[x, y].ParentElement)
        {
            State = NPCState.Active,
            HP = player.Damage
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Equal(NPCState.Dead, npc.State);
        Assert.False(map.IsOccupiedByLivingNPC(x + 1, y));
        Assert.Equal(NPCAwareness.Unaware, npc.Awareness);
        Assert.Null(npc.LastKnownPlayerPosition);
        Assert.Contains($"{npc.Name} DIED", gameState.EventLog.Entries);
    }

    [Fact]
    public void MovingOntoSpecialCollectsIt()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        map.MapCells[x + 1, y].SetCellType(MapCellType.Special);

        gameState.Update(PlayerCommand.MoveRight);

        Assert.True(player.HasSpecial);
        Assert.Equal(MapCellType.Floor, map.MapCells[x + 1, y].CellType);
        Assert.Equal(x + 1, player.X);
        Assert.Contains("SPECIAL COLLECTED", gameState.EventLog.Entries);
    }

    [Fact]
    public void ReturningToEntranceWithSpecialWinsGame()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        var player = new Player
        {
            X = map.StartPosX,
            Y = map.StartPosY - 1
        };
        player.CollectSpecial();
        var gameState = new GameState(map, player);

        gameState.Update(PlayerCommand.MoveDown);

        Assert.Equal(GameOutcome.Won, gameState.Outcome);
        Assert.Equal(map.StartPosX, player.X);
        Assert.Equal(map.StartPosY, player.Y);
        Assert.Contains("YOU ESCAPED WITH SPECIAL", gameState.EventLog.Entries);
    }

    [Fact]
    public void NpcDoesNotMoveOntoAnotherLivingNpc()
    {
        (Map map, _, _, int x, int y) = CreateGameOnOpenFloor();
        var room = map.MapCells[x, y].ParentElement;
        var movingNpc = new Orc(map, x, y, room)
        {
            State = NPCState.Active,
            Direction = Direction.Right
        };
        var blockingNpc = new Goblin(map, x + 1, y, room)
        {
            State = NPCState.Active
        };
        map.NPCs.Add(movingNpc);
        map.NPCs.Add(blockingNpc);
        var distantPlayer = new Player { X = map.StartPosX, Y = map.StartPosY };

        movingNpc.Move(distantPlayer);

        Assert.False(movingNpc.X == blockingNpc.X && movingNpc.Y == blockingNpc.Y);
    }

    [Fact]
    public void GeneratedSpecialIsReachableFromEntrance()
    {
        var map = new Map(123);
        var special = map.RoomList.SelectMany(room => room.Specials).Single();
        var visited = new HashSet<(int X, int Y)> { (map.StartPosX, map.StartPosY) };
        var frontier = new Queue<(int X, int Y)>();
        frontier.Enqueue((map.StartPosX, map.StartPosY));

        while (frontier.Count > 0)
        {
            (int x, int y) = frontier.Dequeue();
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = (X: x + dx, Y: y + dy);
                bool traversableTerrain = next.X >= 0 && next.X < map.Width
                    && next.Y >= 0 && next.Y < map.Height
                    && map.MapCells[next.X, next.Y].CellType != MapCellType.Wall;
                if (traversableTerrain && visited.Add(next))
                {
                    frontier.Enqueue(next);
                }
            }
        }

        Assert.Contains((special.X, special.Y), visited);
    }

    [Fact]
    public void NoCommandDoesNotAdvanceNpcTurn()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        var npc = new Orc(map, x, y + 1, map.MapCells[x, y].ParentElement)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.None);

        Assert.False(player.Dead);
        Assert.Equal(GameOutcome.Playing, gameState.Outcome);
    }

    [Fact]
    public void BlockedMoveConsumesTurnAndCanEndGame()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        map.MapCells[x + 1, y].SetCellType(MapCellType.Wall);
        var npc = new Orc(map, x, y + 1, map.MapCells[x, y].ParentElement)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Equal(x, player.X);
        Assert.Equal(y, player.Y);
        Assert.True(player.Dead);
        Assert.Equal(GameOutcome.Lost, gameState.Outcome);
        Assert.Contains($"{npc.Name} HIT PLAYER {npc.Damage}", gameState.EventLog.Entries);
        Assert.Contains("PLAYER DIED", gameState.EventLog.Entries);
    }

    private static (Map Map, Player Player, GameState GameState, int X, int Y) CreateGameOnOpenFloor()
    {
        var map = new Map(123);
        map.NPCs.Clear();

        for (int y = 1; y < map.Height - 1; y++)
        {
            for (int x = 1; x < map.Width - 2; x++)
            {
                if (map.IsWalkable(x, y) && map.IsWalkable(x + 1, y))
                {
                    var player = new Player { X = x, Y = y };
                    map.CurrentPlayerX = x;
                    map.CurrentPlayerY = y;
                    return (map, player, new GameState(map, player), x, y);
                }
            }
        }

        throw new InvalidOperationException("Generated map contained no adjacent floor cells.");
    }
}
