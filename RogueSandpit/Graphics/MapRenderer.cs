using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSandpit.Models;

namespace RogueSandpit.Graphics;

public class MapRenderer
{
    private static readonly Color MapBackgroundColor = Color.CornflowerBlue;

    private readonly Map _map;
    private readonly PrimitiveDrawer _drawer;

    public MapRenderer(GraphicsDevice graphicsDevice, Map map)
    {
        _map = map;
        _drawer = new PrimitiveDrawer(graphicsDevice);
    }

    public void Display(SpriteBatch spriteBatch)
    {
        if (_map.IsInitialising) return;

        if (_map.RenderMode == RenderMode.Cells)
        {
            RenderMapCells(spriteBatch);
        }
        else
        {
            RenderRooms(spriteBatch);
        }

        _drawer.DrawFilledRectangle(spriteBatch,
            new Rectangle(_map.CurrentPlayerX * _map.CellScale, _map.CurrentPlayerY * _map.CellScale,
                _map.CellScale, _map.CellScale),
            Color.White);

        if (_map.ShowGrid)
        {
            for (int i = 0; i <= _map.Width; i++)
            {
                _drawer.DrawLine(spriteBatch, new Vector2(i * _map.CellScale, 0),
                    new Vector2(i * _map.CellScale, _map.Height * _map.CellScale), Color.Black);
            }
            for (int i = 0; i <= _map.Height; i++)
            {
                _drawer.DrawLine(spriteBatch, new Vector2(0, i * _map.CellScale),
                    new Vector2(_map.Width * _map.CellScale, i * _map.CellScale), Color.Black);
            }
        }
    }

    private void RenderMapCells(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                Color color = _map.MapCells[x, y].CellType switch
                {
                    MapCellType.Wall => Color.DarkGray,
                    MapCellType.Floor => Color.LightGray,
                    MapCellType.Door => Color.Gray,
                    MapCellType.Special => Color.Yellow,
                    _ => MapBackgroundColor
                };
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(x * _map.CellScale, y * _map.CellScale, _map.CellScale, _map.CellScale), color);
            }
        }

        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            Color npcColor = npc.IsPursuingPlayer ? Color.OrangeRed : Color.Black;
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(npc.X * _map.CellScale, npc.Y * _map.CellScale, _map.CellScale, _map.CellScale),
                npcColor);
        }
    }

    private void RenderRooms(SpriteBatch spriteBatch)
    {
        foreach (Room room in _map.RoomList)
        {
            foreach (Corridor corridor in room.HCorridors)
            {
                DrawVisitedCorridor(spriteBatch, corridor);
            }
            foreach (Corridor corridor in room.VCorridors)
            {
                DrawVisitedCorridor(spriteBatch, corridor);
            }

            if (!room.HasVisited) continue;

            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(room.X1 * _map.CellScale, room.Y1 * _map.CellScale,
                    (room.X2 - room.X1) * _map.CellScale, (room.Y2 - room.Y1) * _map.CellScale), room.Color);

            foreach (Obstacle obstacle in room.Obstacles)
            {
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(obstacle.X1 * _map.CellScale, obstacle.Y1 * _map.CellScale,
                        (obstacle.X2 - obstacle.X1) * _map.CellScale,
                        (obstacle.Y2 - obstacle.Y1) * _map.CellScale), MapBackgroundColor);
            }
            foreach (Special special in room.Specials)
            {
                if (_map.MapCells[special.X, special.Y].CellType != MapCellType.Special) continue;
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(special.X * _map.CellScale, special.Y * _map.CellScale,
                        _map.CellScale, _map.CellScale), Color.Yellow);
            }
            foreach (Doorway doorway in room.Doorways)
            {
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(doorway.X1 * _map.CellScale, doorway.Y1 * _map.CellScale,
                        _map.CellScale, _map.CellScale), Color.Blue);
            }
        }

        foreach (Obstacle obstacle in _map.MapObstacles)
        {
            if (!obstacle.HasVisited) continue;
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(obstacle.X1 * _map.CellScale, obstacle.Y1 * _map.CellScale,
                    (obstacle.X2 - obstacle.X1) * _map.CellScale,
                    (obstacle.Y2 - obstacle.Y1) * _map.CellScale), MapBackgroundColor);
        }

        foreach (Corridor corridor in _map.Exits)
        {
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(corridor.X1 * _map.CellScale, corridor.Y1 * _map.CellScale,
                    (1 + corridor.X2 - corridor.X1) * _map.CellScale,
                    (1 + corridor.Y2 - corridor.Y1) * _map.CellScale), corridor.Color);
        }

        BaseContainingElement currentPlayerRoom =
            _map.MapCells[_map.CurrentPlayerX, _map.CurrentPlayerY].ParentElement;
        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            if (currentPlayerRoom != null && npc.CurrentRoom == currentPlayerRoom)
            {
                Color npcColor = npc.IsPursuingPlayer ? Color.OrangeRed : Color.Red;
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(npc.X * _map.CellScale, npc.Y * _map.CellScale,
                        _map.CellScale, _map.CellScale), npcColor);
            }
        }
    }

    private void DrawVisitedCorridor(SpriteBatch spriteBatch, Corridor corridor)
    {
        if (!corridor.HasVisited) return;
        _drawer.DrawFilledRectangle(spriteBatch,
            new Rectangle(corridor.X1 * _map.CellScale, corridor.Y1 * _map.CellScale,
                (1 + corridor.X2 - corridor.X1) * _map.CellScale,
                (1 + corridor.Y2 - corridor.Y1) * _map.CellScale), corridor.Color);
    }
}
