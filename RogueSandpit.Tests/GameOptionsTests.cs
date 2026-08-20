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
}
