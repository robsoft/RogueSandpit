using System;

namespace RogueSandpit;

public sealed class RuntimeSettings
{
    public double RealtimeTurnSeconds { get; private set; }
    public int MasterVolume { get; private set; } = 100;
    public int EffectsVolume { get; private set; } = 100;
    public int MusicVolume { get; private set; } = 100;
    public bool MuteWhileUnfocused { get; private set; } = true;

    public RuntimeSettings(double realtimeTurnSeconds)
    {
        RealtimeTurnSeconds = Math.Clamp(realtimeTurnSeconds, 0.1, 10.0);
    }

    public void AdjustRealtimeInterval(double delta) =>
        RealtimeTurnSeconds = Math.Round(Math.Clamp(RealtimeTurnSeconds + delta, 0.1, 10.0), 1);

    public void AdjustMasterVolume(int delta) => MasterVolume = ClampVolume(MasterVolume + delta);
    public void AdjustEffectsVolume(int delta) => EffectsVolume = ClampVolume(EffectsVolume + delta);
    public void AdjustMusicVolume(int delta) => MusicVolume = ClampVolume(MusicVolume + delta);
    public void ToggleMuteWhileUnfocused() => MuteWhileUnfocused = !MuteWhileUnfocused;

    private static int ClampVolume(int value) => Math.Clamp(value, 0, 100);
}
