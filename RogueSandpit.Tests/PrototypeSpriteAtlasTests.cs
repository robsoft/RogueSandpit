using Microsoft.Xna.Framework;
using RogueSandpit.Graphics;
using Xunit;

namespace RogueSandpit.Tests;

public class PrototypeSpriteAtlasTests
{
    [Theory]
    [InlineData(PrototypeSprite.Floor, 0, 0)]
    [InlineData(PrototypeSprite.Wall, 32, 0)]
    [InlineData(PrototypeSprite.Player, 64, 0)]
    [InlineData(PrototypeSprite.Orc, 0, 32)]
    [InlineData(PrototypeSprite.HealingPotion, 32, 32)]
    [InlineData(PrototypeSprite.Smoke, 64, 32)]
    [InlineData(PrototypeSprite.OpenDoor, 0, 64)]
    [InlineData(PrototypeSprite.ClosedDoor, 32, 64)]
    [InlineData(PrototypeSprite.Fire, 64, 64)]
    public void SourceRectangleUsesAgreedAtlasPosition(PrototypeSprite sprite, int x, int y)
    {
        Assert.Equal(new Rectangle(x, y, 32, 32),
            PrototypeSpriteAtlas.SourceRectangle(sprite));
    }
}
