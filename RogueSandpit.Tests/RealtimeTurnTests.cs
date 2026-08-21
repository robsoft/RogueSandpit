using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class RealtimeTurnTests
{
    [Fact]
    public void EnabledTimerExpiresAtConfiguredIntervalAndResets()
    {
        var timer = new RealtimeTurnTimer(1);
        timer.Toggle();

        Assert.False(timer.Advance(0.4, paused: false));
        Assert.Equal(0.6, timer.RemainingSeconds, 6);
        Assert.True(timer.Advance(0.6, paused: false));
        Assert.Equal(1, timer.RemainingSeconds, 6);
    }

    [Fact]
    public void PausedAndDisabledTimerDoNotAdvance()
    {
        var timer = new RealtimeTurnTimer(1);
        Assert.False(timer.Advance(2, paused: false));
        timer.Toggle();
        Assert.False(timer.Advance(2, paused: true));
        Assert.Equal(1, timer.RemainingSeconds, 6);
    }

    [Fact]
    public void GameStateCountsOnlyConsumedTurns()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        var player = new Player { X = map.StartPosX, Y = map.StartPosY };
        var game = new GameState(map, player);

        game.Update(PlayerCommand.SelectNextItem);
        Assert.Equal(0, game.TurnCount);

        game.Update(PlayerCommand.Wait);
        Assert.Equal(1, game.TurnCount);
    }
}
