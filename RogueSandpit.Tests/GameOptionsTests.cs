using RogueSandpit;
using Xunit;

namespace RogueSandpit.Tests;

public class GameOptionsTests
{
    [Fact]
    public void DefaultLaunchUsesDoubleWindowScale()
    {
        GameOptions options = GameOptions.Parse([]);

        Assert.Equal(2, options.WindowScale);
        Assert.Equal(1.0, options.TurnSeconds);
        Assert.False(options.Fullscreen);
        Assert.False(options.StartRealtime);
    }

    [Theory]
    [InlineData(new[] { "--scale", "1" }, 1)]
    [InlineData(new[] { "--scale=3" }, 3)]
    [InlineData(new[] { "--scale", "4" }, 4)]
    public void ScaleArgumentSupportsSeparatedAndEqualsForms(string[] args, int expected)
    {
        Assert.Equal(expected, GameOptions.Parse(args).WindowScale);
    }

    [Theory]
    [InlineData("--scale=0")]
    [InlineData("--scale=5")]
    [InlineData("--scale=large")]
    [InlineData("--unknown")]
    public void InvalidArgumentsAreRejected(string argument)
    {
        Assert.Throws<ArgumentException>(() => GameOptions.Parse([argument]));
    }

    [Fact]
    public void MissingScaleValueIsRejected()
    {
        Assert.Throws<ArgumentException>(() => GameOptions.Parse(["--scale"]));
    }

    [Theory]
    [InlineData(new[] { "--turn-seconds", "0.5" }, 0.5)]
    [InlineData(new[] { "--turn-seconds=2.5" }, 2.5)]
    public void TurnSecondsSupportsSeparatedAndEqualsForms(string[] args, double expected)
    {
        Assert.Equal(expected, GameOptions.Parse(args).TurnSeconds);
    }

    [Theory]
    [InlineData("--turn-seconds=0")]
    [InlineData("--turn-seconds=11")]
    [InlineData("--turn-seconds=nope")]
    public void InvalidTurnSecondsAreRejected(string argument)
    {
        Assert.Throws<ArgumentException>(() => GameOptions.Parse([argument]));
    }

    [Fact]
    public void FullscreenAndRealtimeFlagsCanBeCombinedWithValueOptions()
    {
        GameOptions options = GameOptions.Parse([
            "--fullscreen", "--scale", "3", "--realtime", "--turn-seconds=0.75"]);

        Assert.True(options.Fullscreen);
        Assert.True(options.StartRealtime);
        Assert.Equal(3, options.WindowScale);
        Assert.Equal(0.75, options.TurnSeconds);
    }
}
