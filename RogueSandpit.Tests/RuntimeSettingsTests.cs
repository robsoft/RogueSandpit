using Xunit;

namespace RogueSandpit.Tests;

public class RuntimeSettingsTests
{
    [Fact]
    public void ValuesAdjustAndClamp()
    {
        var settings = new RuntimeSettings(1.0);

        settings.AdjustRealtimeInterval(-20);
        settings.AdjustMasterVolume(-150);
        settings.AdjustEffectsVolume(-10);
        settings.AdjustMusicVolume(10);
        settings.ToggleMuteWhileUnfocused();

        Assert.Equal(0.1, settings.RealtimeTurnSeconds);
        Assert.Equal(0, settings.MasterVolume);
        Assert.Equal(90, settings.EffectsVolume);
        Assert.Equal(100, settings.MusicVolume);
        Assert.False(settings.MuteWhileUnfocused);
    }
}
