using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RogueSandpit.Graphics;

public enum PrototypeSprite
{
    Floor,
    Wall,
    Player,
    Orc,
    HealingPotion,
    Smoke,
    OpenDoor,
    ClosedDoor,
    Fire
}

public sealed class PrototypeSpriteAtlas
{
    public const int TileSize = 32;

    private readonly Texture2D _texture;

    public PrototypeSpriteAtlas(Texture2D texture)
    {
        _texture = texture;
    }

    public void Draw(SpriteBatch spriteBatch, PrototypeSprite sprite, Rectangle destination,
        Color? tint = null)
    {
        spriteBatch.Draw(_texture, destination, SourceRectangle(sprite), tint ?? Color.White);
    }

    public static Rectangle SourceRectangle(PrototypeSprite sprite)
    {
        (int column, int row) = sprite switch
        {
            PrototypeSprite.Floor => (0, 0),
            PrototypeSprite.Wall => (1, 0),
            PrototypeSprite.Player => (2, 0),
            PrototypeSprite.Orc => (0, 1),
            PrototypeSprite.HealingPotion => (1, 1),
            PrototypeSprite.Smoke => (2, 1),
            PrototypeSprite.OpenDoor => (0, 2),
            PrototypeSprite.ClosedDoor => (1, 2),
            PrototypeSprite.Fire => (2, 2),
            _ => throw new System.ArgumentOutOfRangeException(nameof(sprite))
        };

        return new Rectangle(column * TileSize, row * TileSize, TileSize, TileSize);
    }
}
