using Microsoft.Xna.Framework;
using RogueSandpit.Models;

namespace RogueSandpit.Graphics;

public static class ViewportMapper
{
    public static bool TryWindowToMapCell(
        Point windowPosition,
        Rectangle renderDestination,
        int nativeWidth,
        int nativeHeight,
        Map map,
        out Point mapCell)
    {
        mapCell = Point.Zero;
        if (!renderDestination.Contains(windowPosition) || renderDestination.Width <= 0 || renderDestination.Height <= 0)
        {
            return false;
        }

        int nativeX = (windowPosition.X - renderDestination.X) * nativeWidth / renderDestination.Width;
        int nativeY = (windowPosition.Y - renderDestination.Y) * nativeHeight / renderDestination.Height;
        int cellX = nativeX / map.CellScale;
        int cellY = nativeY / map.CellScale;

        if (cellX < 0 || cellX >= map.Width || cellY < 0 || cellY >= map.Height)
        {
            return false;
        }

        mapCell = new Point(cellX, cellY);
        return true;
    }
}
