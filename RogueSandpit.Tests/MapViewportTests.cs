using Microsoft.Xna.Framework;
using RogueSandpit.Graphics;
using Xunit;

namespace RogueSandpit.Tests;

public class MapViewportTests
{
    [Fact]
    public void FollowPositionsDistantPlayerInsideLowerDeadZoneEdge()
    {
        var viewport = new MapViewport();

        viewport.Follow(40, 30, 80, 58);

        Assert.Equal(27, viewport.WorldX);
        Assert.Equal(19, viewport.WorldY);
        Assert.Equal(new Rectangle(416, 352, 32, 32), viewport.WorldToScreen(40, 30));
    }

    [Fact]
    public void FollowDoesNotMoveWithinDeadZone()
    {
        var viewport = new MapViewport();
        viewport.Follow(40, 30, 80, 58);

        viewport.Follow(36, 23, 80, 58);

        Assert.Equal(27, viewport.WorldX);
        Assert.Equal(19, viewport.WorldY);
    }

    [Fact]
    public void FollowMovesOneCellAfterCrossingDeadZone()
    {
        var viewport = new MapViewport();
        viewport.Follow(40, 30, 80, 58);

        viewport.Follow(41, 31, 80, 58);

        Assert.Equal(28, viewport.WorldX);
        Assert.Equal(20, viewport.WorldY);
    }

    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(79, 57, 62, 42)]
    public void FollowClampsAtWorldEdges(int playerX, int playerY, int expectedX, int expectedY)
    {
        var viewport = new MapViewport();

        viewport.Follow(playerX, playerY, 80, 58);

        Assert.Equal(expectedX, viewport.WorldX);
        Assert.Equal(expectedY, viewport.WorldY);
    }

    [Fact]
    public void ContainsWorldCellUsesHalfOpenCameraBounds()
    {
        var viewport = new MapViewport();
        viewport.Follow(40, 30, 80, 58);

        Assert.True(viewport.ContainsWorldCell(27, 19));
        Assert.True(viewport.ContainsWorldCell(44, 34));
        Assert.False(viewport.ContainsWorldCell(26, 19));
        Assert.False(viewport.ContainsWorldCell(45, 35));
    }
}
