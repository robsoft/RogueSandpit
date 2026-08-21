using System;

namespace RogueSandpit.Models;

public sealed class RealtimeTurnTimer
{
    public double IntervalSeconds { get; }
    public double ElapsedSeconds { get; private set; }
    public bool Enabled { get; private set; }
    public double RemainingSeconds => Math.Max(0, IntervalSeconds - ElapsedSeconds);

    public RealtimeTurnTimer(double intervalSeconds, bool enabled = false)
    {
        if (intervalSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(intervalSeconds));
        IntervalSeconds = intervalSeconds;
        Enabled = enabled;
    }

    public void Toggle()
    {
        Enabled = !Enabled;
        Reset();
    }

    public bool Advance(double elapsedSeconds, bool paused)
    {
        if (!Enabled || paused || elapsedSeconds <= 0) return false;
        ElapsedSeconds += elapsedSeconds;
        if (ElapsedSeconds < IntervalSeconds) return false;
        Reset();
        return true;
    }

    public void Reset() => ElapsedSeconds = 0;
}
