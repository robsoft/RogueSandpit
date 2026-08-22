using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class ApplicationScreenCoordinatorTests
{
    [Fact]
    public void PauseOptionsBackAndResumeFollowExpectedRoute()
    {
        var coordinator = new ApplicationScreenCoordinator();

        coordinator.Pause();
        Assert.Equal(ApplicationScreen.Paused, coordinator.CurrentScreen);
        Assert.False(coordinator.SimulationActive);

        coordinator.OpenOptions();
        Assert.Equal(ApplicationScreen.Options, coordinator.CurrentScreen);
        coordinator.BackFromOptions();
        Assert.Equal(ApplicationScreen.Paused, coordinator.CurrentScreen);
        coordinator.Resume();

        Assert.Equal(ApplicationScreen.Playing, coordinator.CurrentScreen);
        Assert.True(coordinator.SimulationActive);
    }

    [Theory]
    [InlineData(GameOutcome.Won, ApplicationScreen.Victory)]
    [InlineData(GameOutcome.Lost, ApplicationScreen.GameOver)]
    public void OutcomeCreatesTerminalScreen(GameOutcome outcome, ApplicationScreen expected)
    {
        var coordinator = new ApplicationScreenCoordinator();

        coordinator.SynchronizeOutcome(outcome);
        coordinator.Resume();

        Assert.Equal(expected, coordinator.CurrentScreen);
        Assert.False(coordinator.SimulationActive);
    }

    [Fact]
    public void StartPlayingResetsTerminalScreen()
    {
        var coordinator = new ApplicationScreenCoordinator();
        coordinator.SynchronizeOutcome(GameOutcome.Lost);

        coordinator.StartPlaying();

        Assert.Equal(ApplicationScreen.Playing, coordinator.CurrentScreen);
    }

    [Fact]
    public void ControlsIsAnOptionsOffshoot()
    {
        var coordinator = new ApplicationScreenCoordinator();
        coordinator.Pause();
        coordinator.OpenOptions();

        coordinator.OpenControls();
        Assert.Equal(ApplicationScreen.Controls, coordinator.CurrentScreen);
        coordinator.BackFromControls();

        Assert.Equal(ApplicationScreen.Options, coordinator.CurrentScreen);
    }

    [Fact]
    public void HelpIsAPauseOffshoot()
    {
        var coordinator = new ApplicationScreenCoordinator();
        coordinator.Pause();

        coordinator.OpenHelp();
        Assert.Equal(ApplicationScreen.Help, coordinator.CurrentScreen);
        Assert.False(coordinator.SimulationActive);
        coordinator.BackFromHelp();

        Assert.Equal(ApplicationScreen.Paused, coordinator.CurrentScreen);
    }
}
