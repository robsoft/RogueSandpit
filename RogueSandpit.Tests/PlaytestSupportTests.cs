using System.Text.Json;
using Microsoft.Xna.Framework.Input;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class PlaytestSupportTests
{
    [Fact]
    public void HelpReferenceUsesCurrentBindings()
    {
        var bindings = new InputBindings();
        Assert.True(bindings.TrySet(InputAction.Wait, 0, Keys.W, out _));

        HelpRow wait = HelpReference.Build(bindings).Single(row => row.Action == "WAIT");

        Assert.Contains("W", wait.Keys);
        Assert.DoesNotContain("SPACE", wait.Keys);
    }

    [Fact]
    public void CompletedRunCreatesVersionedUniqueJsonReports()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            GameState game = CreateDefeatedGame(2468);
            var timestamp = new DateTimeOffset(2026, 8, 22, 19, 30, 15, TimeSpan.Zero);
            var store = new RunReportStore(directory, () => timestamp);

            RunReportResult first = store.Save(game, true, 1.5);
            RunReportResult second = store.Save(game, true, 1.5);

            Assert.True(first.Success);
            Assert.True(second.Success);
            Assert.NotEqual(first.FileName, second.FileName);
            CompletedRunReport report = JsonSerializer.Deserialize<CompletedRunReport>(
                File.ReadAllText(Path.Combine(directory, first.FileName)));
            Assert.Equal(CompletedRunReport.CurrentVersion, report.Version);
            Assert.Equal(2468, report.Seed);
            Assert.Equal(GameOutcome.Lost, report.Outcome);
            Assert.True(report.RealtimeMode);
            Assert.Equal(1.5, report.RealtimeTurnSeconds);
            Assert.Equal(game.Statistics.Turns, report.Statistics.Turns);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ActiveRunAndIoFailureAreHandledWithoutThrowing()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            var map = new Map(1357);
            var player = new Player();
            player.Place(map, map.StartPosX, map.StartPosY);
            var active = new GameState(map, player);
            Assert.Equal("RUN STILL ACTIVE", new RunReportStore(directory).Save(active, false, 1).Message);

            string blockingFile = Path.Combine(directory, "not-a-directory");
            File.WriteAllText(blockingFile, "block");
            RunReportResult failed = new RunReportStore(
                Path.Combine(blockingFile, "reports")).Save(CreateDefeatedGame(9753), false, 1);
            Assert.False(failed.Success);
            Assert.Equal("RUN REPORT FAILED", failed.Message);
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static GameState CreateDefeatedGame(int seed)
    {
        var map = new Map(seed);
        var player = new Player();
        player.Place(map, map.StartPosX, map.StartPosY);
        var game = new GameState(map, player);
        player.TakeDamage(player.Health + player.Defence);
        game.Update(PlayerCommand.Wait);
        Assert.Equal(GameOutcome.Lost, game.Outcome);
        return game;
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"RogueSandpitReportTests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }
}
