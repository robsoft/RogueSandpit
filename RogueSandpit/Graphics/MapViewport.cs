using System;
using Microsoft.Xna.Framework;

namespace RogueSandpit.Graphics;

public sealed class MapViewport
{
    public const int TileSize = 32;
    public const int VisibleColumns = 18;
    public const int VisibleRows = 16;
    public const int DeadZoneInset = 4;

    public Rectangle ScreenBounds { get; } =
        new(0, 0, VisibleColumns * TileSize, VisibleRows * TileSize);

    public int WorldX { get; private set; }
    public int WorldY { get; private set; }

    public void Follow(int playerX, int playerY, int mapWidth, int mapHeight)
    {
        int rightThreshold = WorldX + VisibleColumns - DeadZoneInset - 1;
        int bottomThreshold = WorldY + VisibleRows - DeadZoneInset - 1;

        if (playerX < WorldX + DeadZoneInset) WorldX = playerX - DeadZoneInset;
        else if (playerX > rightThreshold) WorldX = playerX - (VisibleColumns - DeadZoneInset - 1);

        if (playerY < WorldY + DeadZoneInset) WorldY = playerY - DeadZoneInset;
        else if (playerY > bottomThreshold) WorldY = playerY - (VisibleRows - DeadZoneInset - 1);

        WorldX = Math.Clamp(WorldX, 0, Math.Max(0, mapWidth - VisibleColumns));
        WorldY = Math.Clamp(WorldY, 0, Math.Max(0, mapHeight - VisibleRows));
    }

    public bool ContainsWorldCell(int worldX, int worldY) =>
        worldX >= WorldX && worldX < WorldX + VisibleColumns
        && worldY >= WorldY && worldY < WorldY + VisibleRows;

    public Rectangle WorldToScreen(int worldX, int worldY) =>
        new(ScreenBounds.X + (worldX - WorldX) * TileSize,
            ScreenBounds.Y + (worldY - WorldY) * TileSize,
            TileSize, TileSize);
}
