using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RogueSandpit.Models;

namespace RogueSandpit.Graphics;

public class MapRenderer
{
    private static readonly Color MapBackgroundColor = Color.CornflowerBlue;

    private readonly Map _map;
    private readonly PrimitiveDrawer _drawer;
    private readonly PrototypeSpriteAtlas _atlas;
    private readonly MapViewport _viewport = new();

    public MapRenderer(GraphicsDevice graphicsDevice, Map map, PrototypeSpriteAtlas atlas)
    {
        _map = map;
        _drawer = new PrimitiveDrawer(graphicsDevice);
        _atlas = atlas;
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
            _viewport.Follow(player.X, player.Y, _map.Width, _map.Height);
            RenderViewport(spriteBatch);
        }

        _atlas.Draw(spriteBatch, PrototypeSprite.Player,
            CellDestination(_map.CurrentPlayerX, _map.CurrentPlayerY));

        if (_map.ShowGrid)
        {
            DrawGrid(spriteBatch);
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
        Rectangle destination = CellDestination(x, y);
        return new Vector2(destination.Center.X, destination.Center.Y);
    }

    private void RenderMapCells(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                DrawMapCell(spriteBatch, x, y);
            }
        }

        DrawPlayerTrail(spriteBatch);
        DrawEnvironmentalEffects(spriteBatch, false);
        DrawGroundItems(spriteBatch, false);
        DrawPlacedTraps(spriteBatch, false);

        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            DrawNpc(spriteBatch, npc);
        }

    }

    private void DrawPlayerTrail(SpriteBatch spriteBatch)
    {
        foreach (PlayerTrailClue clue in _map.PlayerTrail)
        {
            if (!IsWorldCellDrawable(clue.X, clue.Y)) continue;
            float ageOpacity = Math.Clamp(clue.RemainingTurns / 18f, 0.2f, 1f);
            Color clueColor = clue.IsAuthentic ? Color.HotPink : Color.MediumPurple;
            Vector2 start = CellCenter(clue.X, clue.Y);
            Vector2 end = CellCenter(clue.NextX, clue.NextY);
            Rectangle destination = CellDestination(clue.X, clue.Y);
            int inset = Math.Max(2, ActiveCellScale / 4);
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(destination.X + inset, destination.Y + inset,
                    destination.Width - inset * 2, destination.Height - inset * 2),
                clueColor * ageOpacity);
            _drawer.DrawLine(spriteBatch, start, end, clueColor * ageOpacity, clue.Strength);
        }
    }

    private void RenderViewport(SpriteBatch spriteBatch)
    {
        int right = Math.Min(_map.Width, _viewport.WorldX + MapViewport.VisibleColumns);
        int bottom = Math.Min(_map.Height, _viewport.WorldY + MapViewport.VisibleRows);

        for (int x = _viewport.WorldX; x < right; x++)
        {
            for (int y = _viewport.WorldY; y < bottom; y++)
            {
                DrawMapCell(spriteBatch, x, y);
            }
        }

        DrawPlayerTrail(spriteBatch);
        DrawEnvironmentalEffects(spriteBatch, true);
        DrawGroundItems(spriteBatch, true);
        DrawPlacedTraps(spriteBatch, true);

        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead || !IsWorldCellDrawable(npc.X, npc.Y)) continue;
            if (_map.MapCells[npc.X, npc.Y].IsVisible) DrawNpc(spriteBatch, npc);
        }

        DrawViewportFog(spriteBatch);
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

            if (!room.HasVisited && !IsElementDiscovered(room)) continue;

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

        DrawDiscoveredAtlasTerrain(spriteBatch);

        foreach (Doorway door in _map.Doors)
        {
            if (!IsDoorVisible(door)) continue;
            DrawDoor(spriteBatch, door);
        }

        DrawGroundItems(spriteBatch, true);
        DrawEnvironmentalEffects(spriteBatch, true);
        DrawPlacedTraps(spriteBatch, true);

        foreach (BaseNPC npc in _map.NPCs)
        {
            if (npc.State == NPCState.Dead) continue;
            if (_map.MapCells[npc.X, npc.Y].IsVisible)
            {
                DrawNpc(spriteBatch, npc);
            }
        }

        DrawFogOfWar(spriteBatch);
    }

    private void DrawVisitedCorridor(SpriteBatch spriteBatch, Corridor corridor)
    {
        if (!corridor.HasVisited && !IsElementDiscovered(corridor)) return;
        _drawer.DrawFilledRectangle(spriteBatch,
            new Rectangle(corridor.X1 * _map.CellScale, corridor.Y1 * _map.CellScale,
                (1 + corridor.X2 - corridor.X1) * _map.CellScale,
                (1 + corridor.Y2 - corridor.Y1) * _map.CellScale), corridor.Color);
    }

    private void DrawGroundItems(SpriteBatch spriteBatch, bool visibleOnly)
    {
        foreach (GroundItem groundItem in _map.GroundItems)
        {
            if (!IsWorldCellDrawable(groundItem.X, groundItem.Y)) continue;
            if (visibleOnly && !_map.MapCells[groundItem.X, groundItem.Y].IsVisible) continue;

            if (groundItem.Item.Type == ItemType.HealingPotion)
            {
                _atlas.Draw(spriteBatch, PrototypeSprite.HealingPotion,
                    CellDestination(groundItem.X, groundItem.Y));
                continue;
            }

            Color color = groundItem.Item.Type switch
            {
                ItemType.Weapon => Color.Silver,
                ItemType.Key => Color.Gold,
                ItemType.Armor => Color.SteelBlue,
                ItemType.Trap => Color.Orange,
                ItemType.RangedWeapon => Color.SandyBrown,
                ItemType.Bandage => Color.AntiqueWhite,
                ItemType.SmokeBomb => Color.LightGray,
                ItemType.FireBomb => Color.OrangeRed,
                _ => Color.White
            };
            Rectangle destination = CellDestination(groundItem.X, groundItem.Y);
            int inset = Math.Max(2, ActiveCellScale / 5);
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(destination.X + inset, destination.Y + inset,
                    destination.Width - inset * 2, destination.Height - inset * 2), color);
        }
    }

    private void DrawGrid(SpriteBatch spriteBatch)
    {
        int columns = _map.RenderMode == RenderMode.Cells ? _map.Width : MapViewport.VisibleColumns;
        int rows = _map.RenderMode == RenderMode.Cells ? _map.Height : MapViewport.VisibleRows;
        int scale = ActiveCellScale;

        for (int i = 0; i <= columns; i++)
        {
            _drawer.DrawLine(spriteBatch, new Vector2(i * scale, 0),
                new Vector2(i * scale, rows * scale), Color.Black);
        }
        for (int i = 0; i <= rows; i++)
        {
            _drawer.DrawLine(spriteBatch, new Vector2(0, i * scale),
                new Vector2(columns * scale, i * scale), Color.Black);
        }
    }

    private void DrawPlacedTraps(SpriteBatch spriteBatch, bool visibleOnly)
    {
        foreach (PlacedTrap trap in _map.PlacedTraps)
        {
            if (!IsWorldCellDrawable(trap.X, trap.Y)) continue;
            if (visibleOnly && !_map.MapCells[trap.X, trap.Y].IsVisible) continue;
            Rectangle destination = CellDestination(trap.X, trap.Y);
            int horizontalInset = Math.Max(2, ActiveCellScale / 5);
            int verticalInset = Math.Max(3, ActiveCellScale / 4);
            _drawer.DrawFilledRectangle(spriteBatch,
                new Rectangle(destination.X + horizontalInset, destination.Y + verticalInset,
                    destination.Width - horizontalInset * 2,
                    destination.Height - verticalInset * 2), Color.OrangeRed);
        }
    }

    private void DrawEnvironmentalEffects(SpriteBatch spriteBatch, bool visibleOnly)
    {
        foreach (EnvironmentalEffect effect in _map.EnvironmentalEffects)
        {
            if (!IsWorldCellDrawable(effect.X, effect.Y)) continue;
            if (visibleOnly && !_map.MapCells[effect.X, effect.Y].IsVisible) continue;
            PrototypeSprite sprite = effect.Type == EnvironmentalEffectType.Smoke
                ? PrototypeSprite.Smoke
                : PrototypeSprite.Fire;
            _atlas.Draw(spriteBatch, sprite, CellDestination(effect.X, effect.Y));
        }
    }

    private void DrawDiscoveredAtlasTerrain(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                if (!_map.MapCells[x, y].IsDiscovered) continue;
                DrawMapCell(spriteBatch, x, y);
            }
        }
    }

    private void DrawMapCell(SpriteBatch spriteBatch, int x, int y)
    {
        MapCellType cellType = _map.MapCells[x, y].CellType;
        PrototypeSprite baseSprite = cellType == MapCellType.Wall
            ? PrototypeSprite.Wall
            : PrototypeSprite.Floor;
        _atlas.Draw(spriteBatch, baseSprite, CellDestination(x, y));

        if (cellType == MapCellType.Door)
        {
            DrawDoor(spriteBatch, _map.GetDoorAt(x, y));
        }
        else if (cellType == MapCellType.Special)
        {
            _drawer.DrawFilledRectangle(spriteBatch, CellDestination(x, y), Color.Yellow);
        }
    }

    private void DrawDoor(SpriteBatch spriteBatch, Doorway door)
    {
        if (door?.State == DoorState.Open)
        {
            _atlas.Draw(spriteBatch, PrototypeSprite.OpenDoor,
                CellDestination(door.X1, door.Y1));
        }
        else if (door?.State == DoorState.Closed)
        {
            _atlas.Draw(spriteBatch, PrototypeSprite.ClosedDoor,
                CellDestination(door.X1, door.Y1));
        }
        else if (door != null)
        {
            _drawer.DrawFilledRectangle(spriteBatch,
                CellDestination(door.X1, door.Y1), DoorColor(door));
        }
    }

    private void DrawNpc(SpriteBatch spriteBatch, BaseNPC npc)
    {
        if (npc.CharacterType == CharacterTypes.Orc)
        {
            Color tint = npc.StatusEffects.Has(StatusEffectType.Stunned)
                || npc.MoraleState is NPCMoraleState.Fleeing or NPCMoraleState.Enraged
                || npc.Awareness != NPCAwareness.Unaware
                    ? NpcColor(npc)
                    : Color.White;
            _atlas.Draw(spriteBatch, PrototypeSprite.Orc,
                CellDestination(npc.X, npc.Y), tint);
            return;
        }

        _drawer.DrawFilledRectangle(spriteBatch,
            CellDestination(npc.X, npc.Y), NpcColor(npc));
    }

    private int ActiveCellScale => _map.RenderMode == RenderMode.Cells
        ? _map.CellScale
        : MapViewport.TileSize;

    private bool IsWorldCellDrawable(int x, int y) =>
        _map.RenderMode == RenderMode.Cells || _viewport.ContainsWorldCell(x, y);

    private Rectangle CellDestination(int x, int y) =>
        _map.RenderMode == RenderMode.Cells
            ? new Rectangle(x * _map.CellScale, y * _map.CellScale,
                _map.CellScale, _map.CellScale)
            : _viewport.WorldToScreen(x, y);

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
        if (_map.MapCells[door.X1, door.Y1].IsDiscovered) return true;
        if (_map.MapCells[door.X1, door.Y1].ParentElement?.HasVisited == true) return true;

        foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
        {
            BaseContainingElement adjacent = _map.MapCells[door.X1 + dx, door.Y1 + dy].ParentElement;
            if (adjacent?.HasVisited == true) return true;
        }

        return false;
    }

    private bool IsElementDiscovered(BaseContainingElement element)
    {
        for (int x = Math.Max(0, element.X1); x <= Math.Min(_map.Width - 1, element.X2); x++)
        {
            for (int y = Math.Max(0, element.Y1); y <= Math.Min(_map.Height - 1, element.Y2); y++)
            {
                if (_map.MapCells[x, y].ParentElement == element && _map.MapCells[x, y].IsDiscovered)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void DrawViewportFog(SpriteBatch spriteBatch)
    {
        int right = Math.Min(_map.Width, _viewport.WorldX + MapViewport.VisibleColumns);
        int bottom = Math.Min(_map.Height, _viewport.WorldY + MapViewport.VisibleRows);

        for (int x = _viewport.WorldX; x < right; x++)
        {
            for (int y = _viewport.WorldY; y < bottom; y++)
            {
                MapCell cell = _map.MapCells[x, y];
                if (cell.IsVisible) continue;

                Color fog = cell.IsDiscovered ? Color.Black * 0.65f : Color.Black;
                _drawer.DrawFilledRectangle(spriteBatch, CellDestination(x, y), fog);
            }
        }
    }

    private void DrawFogOfWar(SpriteBatch spriteBatch)
    {
        for (int x = 0; x < _map.Width; x++)
        {
            for (int y = 0; y < _map.Height; y++)
            {
                MapCell cell = _map.MapCells[x, y];
                if (cell.IsVisible) continue;

                Color fog = cell.IsDiscovered ? Color.Black * 0.65f : Color.Black;
                _drawer.DrawFilledRectangle(spriteBatch,
                    new Rectangle(x * _map.CellScale, y * _map.CellScale,
                        _map.CellScale, _map.CellScale), fog);
            }
        }
    }

    private static Color NpcColor(BaseNPC npc)
    {
        if (npc.StatusEffects.Has(StatusEffectType.Stunned)) return Color.Violet;
        if (npc.MoraleState == NPCMoraleState.Fleeing) return Color.DeepSkyBlue;
        if (npc.MoraleState == NPCMoraleState.Enraged) return Color.Red;
        if (npc.Awareness == NPCAwareness.Pursuing) return Color.OrangeRed;
        if (npc.Awareness == NPCAwareness.Investigating) return Color.Yellow;

        return npc.CharacterType switch
        {
            CharacterTypes.Orc => Color.DarkOliveGreen,
            CharacterTypes.Goblin => Color.LimeGreen,
            CharacterTypes.Skeleton => Color.AntiqueWhite,
            CharacterTypes.Troll => Color.DarkSlateBlue,
            CharacterTypes.Wretch => Color.MediumPurple,
            _ => Color.Red
        };
    }
}
