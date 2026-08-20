using Microsoft.Xna.Framework;
using RogueSandpit.Graphics;
using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class ViewportMapperTests
{
    [Fact]
    public void MapsScaledLetterboxedWindowPositionToCell()
    {
        var map = new Map(123);
        var destination = new Rectangle(200, 100, 400, 300);

        bool mapped = ViewportMapper.TryWindowToMapCell(
            new Point(250, 150), destination, 800, 600, map, out Point cell);

        Assert.True(mapped);
        Assert.Equal(new Point(10, 10), cell);
    }

    [Fact]
    public void RejectsMousePositionInLetterbox()
    {
        var map = new Map(123);
        var destination = new Rectangle(200, 100, 400, 300);

        bool mapped = ViewportMapper.TryWindowToMapCell(
            new Point(100, 150), destination, 800, 600, map, out _);

        Assert.False(mapped);
    }

    [Fact]
    public void RejectsNativeHudAreaBelowMap()
    {
        var map = new Map(123);
        var destination = new Rectangle(0, 0, 800, 600);

        bool mapped = ViewportMapper.TryWindowToMapCell(
            new Point(100, 590), destination, 800, 600, map, out _);

        Assert.False(mapped);
    }
}
