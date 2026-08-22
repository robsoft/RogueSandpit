using System;
using RogueSandpit.Models;

namespace RogueSandpit;

public enum ApplicationScreen
{
    Playing,
    Paused,
    Help,
    Options,
    Controls,
    GameOver,
    Victory
}

public sealed class ApplicationScreenCoordinator
{
    public ApplicationScreen CurrentScreen { get; private set; } = ApplicationScreen.Playing;
    public bool SimulationActive => CurrentScreen == ApplicationScreen.Playing;

    public void Pause()
    {
        if (CurrentScreen == ApplicationScreen.Playing) CurrentScreen = ApplicationScreen.Paused;
    }

    public void Resume()
    {
        if (CurrentScreen == ApplicationScreen.Paused) CurrentScreen = ApplicationScreen.Playing;
    }

    public void OpenOptions()
    {
        if (CurrentScreen == ApplicationScreen.Paused) CurrentScreen = ApplicationScreen.Options;
    }

    public void OpenHelp()
    {
        if (CurrentScreen == ApplicationScreen.Paused) CurrentScreen = ApplicationScreen.Help;
    }

    public void BackFromHelp()
    {
        if (CurrentScreen == ApplicationScreen.Help) CurrentScreen = ApplicationScreen.Paused;
    }

    public void BackFromOptions()
    {
        if (CurrentScreen == ApplicationScreen.Options) CurrentScreen = ApplicationScreen.Paused;
    }

    public void OpenControls()
    {
        if (CurrentScreen == ApplicationScreen.Options) CurrentScreen = ApplicationScreen.Controls;
    }

    public void BackFromControls()
    {
        if (CurrentScreen == ApplicationScreen.Controls) CurrentScreen = ApplicationScreen.Options;
    }

    public void SynchronizeOutcome(GameOutcome outcome)
    {
        if (outcome == GameOutcome.Won) CurrentScreen = ApplicationScreen.Victory;
        else if (outcome == GameOutcome.Lost) CurrentScreen = ApplicationScreen.GameOver;
    }

    public void StartPlaying() => CurrentScreen = ApplicationScreen.Playing;
}
