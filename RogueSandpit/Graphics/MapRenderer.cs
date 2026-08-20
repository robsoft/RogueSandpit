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

    public void Display(SpriteBatch spriteBatch, Player player, Point? hoveredCell = null)
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

        if (_map.RenderMode == RenderMode.Cells && hoveredCell.HasValue)
        {
            DrawDebugOverlay(spriteBatch, player, hoveredCell.Value);
        }
    }

    private void DrawDebugOverlay(SpriteBatch spriteBatch, Player player, Point hoveredCell)
    {
        BaseNPC npc = _map.GetLivingNPCAt(hoveredCell.X, hoveredCell.Y);
        if (npc != null)
        {
            (int X, int Y)? target = npc.Awareness switch
            {
                NPCAwareness.Pursuing => (player.X, player.Y),
                NPCAwareness.Investigating => npc.InvestigationTarget,
                _ => null
            };

            if (target.HasValue)
            {
                var path = Pathfinding.FindPath(_map, npc.X, npc.Y, target.Value.X, target.Value.Y, npc);
                foreach ((int x, int y) in path)
                {
                    _drawer.DrawFilledRectangle(spriteBatch,
                        new Rectangle(x * _map.CellScale + 2, y * _map.CellScale + 2,
                            _map.CellScale - 4, _map.CellScale - 4), Color.Cyan * 0.65f);
                }
            }

            bool clearSight = _map.HasLineOfSight(npc.X, npc.Y, player.X, player.Y);
            Vector2 npcCenter = CellCenter(npc.X, npc.Y);
            Vector2 playerCenter = CellCenter(player.X, player.Y);
            _drawer.DrawLine(spriteBatch, npcCenter, playerCenter,
                clearSight ? Color.LimeGreen : Color.OrangeRed, 2f);
        }

        int left = hoveredCell.X * _map.CellScale;
        int top = hoveredCell.Y * _map.CellScale;
        int right = left + _map.CellScale;
        int bottom = top + _map.CellScale;
        _drawer.DrawLine(spriteBatch, left, top, right, top, Color.Lime, 2f);
        _drawer.DrawLine(spriteBatch, right, top, right, bottom, Color.Lime, 2f);
        _drawer.DrawLine(spriteBatch, right, bottom, left, bottom, Color.Lime, 2f);
        _drawer.DrawLine(spriteBatch, left, bottom, left, top, Color.Lime, 2f);
    }

    private Vector2 CellCenter(int x, int y)
    {
        float offset = _map.CellScale / 2f;
        return new Vector2(x * _map.CellScale + offset, y * _map.CellScale + offset);
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
                    MapCellType.Door => DoorColor(_map.GetDoorAt(x, y)),
                    MapCellType.Special => Color.Yellow,
                    _ => MapBackgroundColor
                };
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(x * _map.CellScale, y * _map.CellScale, _map.CellScale, _map.CellScale), color);
            }
        }

        DrawGroundItems(spriteBatch, false);

        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            Color npcColor = npc.Awareness switch
            {
                NPCAwareness.Pursuing => Color.OrangeRed,
                NPCAwareness.Investigating => Color.Yellow,
                _ => Color.Black
            };
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

        foreach (Doorway door in _map.Doors)
        {
            if (!IsDoorVisible(door)) continue;
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(door.X1 * _map.CellScale, door.Y1 * _map.CellScale,
                    _map.CellScale, _map.CellScale), DoorColor(door));
        }

        DrawGroundItems(spriteBatch, true);

        BaseContainingElement currentPlayerRoom =
            _map.MapCells[_map.CurrentPlayerX, _map.CurrentPlayerY].ParentElement;
        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            if (currentPlayerRoom != null && npc.CurrentRoom == currentPlayerRoom)
            {
                Color npcColor = npc.Awareness switch
                {
                    NPCAwareness.Pursuing => Color.OrangeRed,
                    NPCAwareness.Investigating => Color.Yellow,
                    _ => Color.Red
                };
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

    private void DrawGroundItems(SpriteBatch spriteBatch, bool visitedOnly)
    {
        foreach (GroundItem groundItem in _map.GroundItems)
        {
            BaseContainingElement parent = _map.MapCells[groundItem.X, groundItem.Y].ParentElement;
            if (visitedOnly && parent != null && !parent.HasVisited) continue;

            Color color = groundItem.Item.Type switch
            {
                ItemType.HealingPotion => Color.LimeGreen,
                ItemType.Weapon => Color.Silver,
                ItemType.Key => Color.Gold,
                ItemType.Armor => Color.SteelBlue,
                _ => Color.White
            };
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(groundItem.X * _map.CellScale + 2, groundItem.Y * _map.CellScale + 2,
                    _map.CellScale - 4, _map.CellScale - 4), color);
        }
    }

    private static Color DoorColor(Doorway door)
    {
        return door?.State switch
        {
            DoorState.Locked => Color.Gold,
            DoorState.Open => Color.SlateGray,
            _ => Color.SaddleBrown
        };
    }

    private bool IsDoorVisible(Doorway door)
    {
        if (_map.MapCells[door.X1, door.Y1].ParentElement?.HasVisited == true) return true;

        foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
        {
            BaseContainingElement adjacent = _map.MapCells[door.X1 + dx, door.Y1 + dy].ParentElement;
            if (adjacent?.HasVisited == true) return true;
        }

        return false;
    }
}
