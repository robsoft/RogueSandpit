using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSandpit.Models;
using RogueSandpit.Graphics;

namespace RogueSandpit
{
    public class GameWrapper : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private PrimitiveDrawer _uiDrawer;
        private PixelFont _pixelFont;
        private MapRenderer _mapRenderer;
        private Rectangle _renderDestination;
        private bool _isResizing = false;

        private Map _map;
        private Player _player;
        private GameState _gameState;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private bool _inventoryOpen;
        private DirectionalAction _directionalAction;

        private enum DirectionalAction { None, CloseDoor, LayFalseTrail, ThrowItem, PlaceTrap }

        public const int NativeWidth = 800;
        public const int NativeHeight = 600;

        public GameWrapper(int windowScale = GameOptions.DefaultWindowScale)
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = NativeWidth * windowScale;
            _graphics.PreferredBackBufferHeight = NativeHeight * windowScale;
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {
            _map = new Map(123);
            KickOffNewGame(false);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, NativeWidth, NativeHeight);
            _uiDrawer = new PrimitiveDrawer(GraphicsDevice);
            _pixelFont = new PixelFont(GraphicsDevice);
            _mapRenderer = new MapRenderer(GraphicsDevice, _map);
            // initial calculation of the render destination rectangle
            CalculateRenderDestination();
        }

        private void KickOffNewGame(bool regenerateMap = true)
        {
            _inventoryOpen = false;
            _directionalAction = DirectionalAction.None;
            if (regenerateMap)
            {
                _map.Initialise();
            }
            _player = new Player();
            _player.Place(_map, _map.StartPosX, _map.StartPosY);
            _gameState = new GameState(_map, _player);

            Window.Title = $"Rogue Sandpit - Seed: {RandGen.Seed}";
        }

        protected override void Update(GameTime gameTime)
        {
            // deal with the meta stuff - updates that have nothing to do with the actual game itself
            _previousKeyboardState = _currentKeyboardState;

            // Get the new current state
            _currentKeyboardState = Keyboard.GetState();

            if (WasPressed(Keys.Escape))
            {
                if (_directionalAction != DirectionalAction.None)
                {
                    _directionalAction = DirectionalAction.None;
                }
                else
                {
                    Exit();
                }
            }

            if (_gameState.Outcome != GameOutcome.Playing)
            {
                UpdateDead(gameTime);           
            }
            else
            {
                UpdateLive(gameTime);
            }
        }

        private void UpdateDead(GameTime gameTime)
        {
            if (_currentKeyboardState.IsKeyUp(Keys.Space) && _previousKeyboardState.IsKeyDown(Keys.Space))
            {
                KickOffNewGame();
            }
        }


        private void UpdateLive(GameTime gameTime)
        {
            if (WasPressed(Keys.I))
            {
                _inventoryOpen = !_inventoryOpen;
            }

            if (_currentKeyboardState.IsKeyUp(Keys.Space) && _previousKeyboardState.IsKeyDown(Keys.Space))
            {
                KickOffNewGame();
            }

            if (_currentKeyboardState.IsKeyUp(Keys.F1) && _previousKeyboardState.IsKeyDown(Keys.F1))
            {
                if (_map.RenderMode == RenderMode.Rooms)
                {
                    _map.RenderMode = RenderMode.Cells;
                }
                else
                {
                    _map.RenderMode = RenderMode.Rooms;
                }
            }

            if (!_player.Dead)
            {
                // this will take care of the player's turn, and the computer's responses
                _gameState.Update(GetPlayerCommand());
            }

            base.Update(gameTime);
        }

        private PlayerCommand GetPlayerCommand()
        {
            if (_inventoryOpen)
            {
                if (WasPressed(Keys.Up) || WasPressed(Keys.Left) || WasPressed(Keys.OemOpenBrackets))
                    return PlayerCommand.SelectPreviousItem;
                if (WasPressed(Keys.Down) || WasPressed(Keys.Right) || WasPressed(Keys.OemCloseBrackets))
                    return PlayerCommand.SelectNextItem;
                if (WasPressed(Keys.H)) return PlayerCommand.UsePotion;
                if (WasPressed(Keys.E)) return PlayerCommand.EquipItem;
                if (WasPressed(Keys.D)) return PlayerCommand.DropItem;
                return PlayerCommand.None;
            }

            if (_directionalAction != DirectionalAction.None)
            {
                if (WasPressed(Keys.Up)) return DirectionalCommand(0, -1);
                if (WasPressed(Keys.Down)) return DirectionalCommand(0, 1);
                if (WasPressed(Keys.Left)) return DirectionalCommand(-1, 0);
                if (WasPressed(Keys.Right)) return DirectionalCommand(1, 0);
                return PlayerCommand.None;
            }

            if (WasPressed(Keys.C))
            {
                var doors = _map.GetAdjacentOpenDoors(_player.X, _player.Y);
                if (doors.Count == 0)
                {
                    _gameState.EventLog.Add("NO OPEN DOOR NEARBY");
                    return PlayerCommand.None;
                }
                if (doors.Count == 1)
                {
                    return CloseDoorCommand(doors[0].X1 - _player.X, doors[0].Y1 - _player.Y);
                }
                _directionalAction = DirectionalAction.CloseDoor;
                return PlayerCommand.None;
            }

            if (WasPressed(Keys.T))
            {
                _directionalAction = DirectionalAction.LayFalseTrail;
                return PlayerCommand.None;
            }

            if (WasPressed(Keys.F))
            {
                if (_player.Inventory.SelectedItem == null)
                {
                    _gameState.EventLog.Add("SELECT AN ITEM TO THROW");
                    return PlayerCommand.None;
                }
                _directionalAction = DirectionalAction.ThrowItem;
                return PlayerCommand.None;
            }

            if (WasPressed(Keys.P))
            {
                if (_player.Inventory.SelectedItem?.Type != ItemType.Trap)
                {
                    _gameState.EventLog.Add("SELECT A HUNTING TRAP");
                    return PlayerCommand.None;
                }
                _directionalAction = DirectionalAction.PlaceTrap;
                return PlayerCommand.None;
            }

            if (WasPressed(Keys.Up)) return PlayerCommand.MoveUp;
            if (WasPressed(Keys.Down)) return PlayerCommand.MoveDown;
            if (WasPressed(Keys.Left)) return PlayerCommand.MoveLeft;
            if (WasPressed(Keys.Right)) return PlayerCommand.MoveRight;
            if (WasPressed(Keys.OemPeriod) || WasPressed(Keys.NumPad5)) return PlayerCommand.Wait;
            if (WasPressed(Keys.OemOpenBrackets)) return PlayerCommand.SelectPreviousItem;
            if (WasPressed(Keys.OemCloseBrackets)) return PlayerCommand.SelectNextItem;
            if (WasPressed(Keys.H)) return PlayerCommand.UsePotion;
            if (WasPressed(Keys.E)) return PlayerCommand.EquipItem;
            if (WasPressed(Keys.D)) return PlayerCommand.DropItem;
            return PlayerCommand.None;
        }

        private PlayerCommand DirectionalCommand(int deltaX, int deltaY)
        {
            if (_directionalAction == DirectionalAction.CloseDoor)
            {
                Doorway door = _map.GetDoorAt(_player.X + deltaX, _player.Y + deltaY);
                if (door?.State == DoorState.Open
                    && !_map.IsOccupiedByLivingNPC(door.X1, door.Y1))
                    _directionalAction = DirectionalAction.None;
                return CloseDoorCommand(deltaX, deltaY);
            }

            if (_directionalAction == DirectionalAction.LayFalseTrail)
            {
                if (_map.IsWalkable(_player.X + deltaX, _player.Y + deltaY))
                    _directionalAction = DirectionalAction.None;
                return FalseTrailCommand(deltaX, deltaY);
            }

            if (_directionalAction == DirectionalAction.ThrowItem)
            {
                if (_map.FindThrowLanding(_player.X, _player.Y, deltaX, deltaY).HasValue)
                    _directionalAction = DirectionalAction.None;
                return ThrowItemCommand(deltaX, deltaY);
            }

            if (_map.CanPlaceTrap(_player.X + deltaX, _player.Y + deltaY, _player))
                _directionalAction = DirectionalAction.None;
            return PlaceTrapCommand(deltaX, deltaY);
        }

        private static PlayerCommand CloseDoorCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.CloseDoorUp,
            (0, 1) => PlayerCommand.CloseDoorDown,
            (-1, 0) => PlayerCommand.CloseDoorLeft,
            (1, 0) => PlayerCommand.CloseDoorRight,
            _ => PlayerCommand.None
        };

        private static PlayerCommand FalseTrailCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.LayFalseTrailUp,
            (0, 1) => PlayerCommand.LayFalseTrailDown,
            (-1, 0) => PlayerCommand.LayFalseTrailLeft,
            (1, 0) => PlayerCommand.LayFalseTrailRight,
            _ => PlayerCommand.None
        };

        private static PlayerCommand ThrowItemCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.ThrowItemUp,
            (0, 1) => PlayerCommand.ThrowItemDown,
            (-1, 0) => PlayerCommand.ThrowItemLeft,
            (1, 0) => PlayerCommand.ThrowItemRight,
            _ => PlayerCommand.None
        };

        private static PlayerCommand PlaceTrapCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.PlaceTrapUp,
            (0, 1) => PlayerCommand.PlaceTrapDown,
            (-1, 0) => PlayerCommand.PlaceTrapLeft,
            (1, 0) => PlayerCommand.PlaceTrapRight,
            _ => PlayerCommand.None
        };

        private bool WasPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        protected override void Draw(GameTime gameTime)
        {
            // draw to our render target first
            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            Point? hoveredCell = GetHoveredMapCell();
            _mapRenderer.Display(_spriteBatch, _player, hoveredCell);
            DrawEventLog();
            DrawHud();

            if (_directionalAction != DirectionalAction.None) DrawDirectionalActionPrompt();

            if (_inventoryOpen && _gameState.Outcome == GameOutcome.Playing)
            {
                DrawInventoryPanel();
            }

            if (_map.RenderMode == RenderMode.Cells && hoveredCell.HasValue)
            {
                DrawDebugInspection(hoveredCell.Value);
            }

            if (_gameState.Outcome != GameOutcome.Playing)
            {
                DrawEndScreen();
            }
            _spriteBatch.End();

            // then draw the render target to the screen
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(0, 580, NativeWidth, 20), Color.Black);
            string specialStatus = _player.HasSpecial ? "YES" : "NO";
            string weaponName = _player.EquippedWeapon?.Name ?? "NONE";
            string armorName = _player.EquippedArmor?.Name ?? "NONE";
            string selectedName = _player.Inventory.SelectedItem?.Name ?? "NONE";
            _pixelFont.DrawText(_spriteBatch,
                $"HP {_player.Health}/{_player.MaxHealth} DMG {_player.Damage} DEF {_player.Defence} SPECIAL {specialStatus} INV {_player.Inventory.Items.Count} SEL {selectedName} WPN {weaponName} ARM {armorName}",
                new Vector2(6, 585), 1, Color.White);
        }

        private void DrawDirectionalActionPrompt()
        {
            string prompt = _directionalAction switch
            {
                DirectionalAction.CloseDoor => "CLOSE DOOR: ARROW CHOOSES  ESC CANCELS",
                DirectionalAction.LayFalseTrail => "FALSE TRAIL: ARROW CHOOSES  ESC CANCELS",
                DirectionalAction.ThrowItem => "THROW ITEM: ARROW CHOOSES  ESC CANCELS",
                _ => "PLACE TRAP: ARROW CHOOSES  ESC CANCELS"
            };
            const int panelWidth = 330;
            const int panelHeight = 28;
            int panelX = (NativeWidth - panelWidth) / 2;
            const int panelY = 545;
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.9f);
            _pixelFont.DrawText(_spriteBatch, prompt,
                new Vector2(panelX + 10, panelY + 10), 1, Color.White);
        }

        private void DrawInventoryPanel()
        {
            const int panelX = 350;
            const int panelY = 125;
            const int panelWidth = 440;
            const int panelHeight = 330;
            const int firstRowY = panelY + 48;
            const int rowHeight = 28;

            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.94f);
            _pixelFont.DrawText(_spriteBatch, "INVENTORY",
                new Vector2(panelX + 14, panelY + 12), 3, Color.White);

            for (int index = 0; index < _player.Inventory.Capacity; index++)
            {
                int rowY = firstRowY + index * rowHeight;
                bool selected = index == _player.Inventory.SelectedIndex;
                if (selected)
                {
                    _uiDrawer.DrawFilledRectangle(_spriteBatch,
                        new Rectangle(panelX + 10, rowY - 4, panelWidth - 20, rowHeight - 2),
                        Color.DarkSlateBlue);
                }

                Item item = index < _player.Inventory.Items.Count ? _player.Inventory.Items[index] : null;
                string itemName = item?.Name ?? "EMPTY";
                Color itemColor = item == null ? Color.DarkGray : Color.White;
                _pixelFont.DrawText(_spriteBatch, $"{index + 1} {itemName}",
                    new Vector2(panelX + 18, rowY), 2, itemColor);

                if (item == null) continue;
                string power = item.Power > 0 ? $" {item.Power}" : "";
                string equipped = item == _player.EquippedWeapon || item == _player.EquippedArmor
                    ? " EQUIPPED"
                    : "";
                _pixelFont.DrawText(_spriteBatch, $"{item.Type}{power}{equipped}",
                    new Vector2(panelX + 245, rowY + 4), 1, Color.LightGray);
            }

            _pixelFont.DrawText(_spriteBatch, "ARROWS SELECT  H USE  E EQUIP  D DROP  I CLOSE",
                new Vector2(panelX + 14, panelY + panelHeight - 22), 1, Color.LightGray);
        }

        private void DrawEventLog()
        {
            if (_gameState.EventLog.Entries.Count == 0) return;

            const int panelX = 5;
            const int panelY = 5;
            int panelHeight = 8 + _gameState.EventLog.Entries.Count * 12;
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, 285, panelHeight), Color.Black * 0.75f);

            for (int i = 0; i < _gameState.EventLog.Entries.Count; i++)
            {
                _pixelFont.DrawText(_spriteBatch, _gameState.EventLog.Entries[i],
                    new Vector2(panelX + 5, panelY + 5 + i * 12), 1, Color.White);
            }
        }

        private Point? GetHoveredMapCell()
        {
            if (_map.RenderMode != RenderMode.Cells) return null;

            Point mousePosition = Mouse.GetState().Position;
            return ViewportMapper.TryWindowToMapCell(mousePosition, _renderDestination,
                NativeWidth, NativeHeight, _map, out Point mapCell)
                ? mapCell
                : null;
        }

        private void DrawDebugInspection(Point position)
        {
            const int panelX = 485;
            const int panelY = 5;
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(panelX, panelY, 305, 144), Color.Black * 0.9f);

            MapCell cell = _map.MapCells[position.X, position.Y];
            _pixelFont.DrawText(_spriteBatch,
                $"CELL {position.X} {position.Y} {cell.CellType}",
                new Vector2(panelX + 6, panelY + 6), 2, Color.White);

            string parentType = cell.ParentElement?.GetType().Name ?? "NONE";
            string parentName = cell.ParentElement?.Name ?? "";
            _pixelFont.DrawText(_spriteBatch, $"PARENT {parentType} {parentName}",
                new Vector2(panelX + 6, panelY + 27), 1, Color.LightGray);

            GroundItem groundItem = _map.GetGroundItemAt(position.X, position.Y);
            string itemName = groundItem?.Item.Name ?? "NONE";
            PlacedTrap placedTrap = _map.GetTrapAt(position.X, position.Y);
            string trapDetails = placedTrap == null ? "NONE" : $"DMG {placedTrap.Damage}";
            _pixelFont.DrawText(_spriteBatch, $"ITEM {itemName} TRAP {trapDetails}",
                new Vector2(panelX + 6, panelY + 43), 1, Color.LightGray);

            Doorway door = _map.GetDoorAt(position.X, position.Y);
            if (door != null)
            {
                _pixelFont.DrawText(_spriteBatch, $"DOOR {door.State}",
                    new Vector2(panelX + 155, panelY + 43), 1, Color.Gold);
            }

            PlayerTrailClue trail = _map.FindNewestTrailNear(position.X, position.Y, 0, 0);
            string trailDetails = trail == null
                ? "TRACK NONE"
                : $"TRACK {(trail.IsAuthentic ? "REAL" : "FALSE")} S{trail.Strength} T{trail.RemainingTurns} TO {trail.NextX} {trail.NextY}";
            _pixelFont.DrawText(_spriteBatch, trailDetails,
                new Vector2(panelX + 6, panelY + 59), 1, Color.HotPink);

            BaseNPC npc = _map.GetLivingNPCAt(position.X, position.Y);
            if (npc == null)
            {
                _pixelFont.DrawText(_spriteBatch, "NPC NONE", new Vector2(panelX + 6, panelY + 75), 1, Color.LightGray);
                return;
            }

            _pixelFont.DrawText(_spriteBatch, $"NPC {npc.CharacterType} {npc.Name} HP {npc.HP}/{npc.MaxHP} DMG {npc.EffectiveDamage}",
                new Vector2(panelX + 6, panelY + 75), 1, Color.White);
            string retreatTarget = npc.RetreatTarget is { } retreat
                ? $"{retreat.X} {retreat.Y}"
                : "NONE";
            _pixelFont.DrawText(_spriteBatch,
                $"AI {npc.Awareness} M {npc.MoraleState} RT {retreatTarget}",
                new Vector2(panelX + 6, panelY + 91), 1, Color.White);

            string investigationTarget = npc.InvestigationTarget is { } searchTarget
                ? $"{searchTarget.X} {searchTarget.Y}"
                : "NONE";
            string predictedTarget = npc.PredictedInvestigationTarget is { } prediction
                ? $"{prediction.X} {prediction.Y}"
                : "NONE";
            _pixelFont.DrawText(_spriteBatch,
                $"SRC {npc.InvestigationSource} C{npc.InvestigationConfidence} AT {investigationTarget} PR {predictedTarget}",
                new Vector2(panelX + 6, panelY + 107), 1, Color.White);

            bool hasLineOfSight = _map.HasLineOfSight(npc.X, npc.Y, _player.X, _player.Y);
            NPCAwarenessProfile profile = npc.AwarenessProfile;
            _pixelFont.DrawText(_spriteBatch,
                $"LOS {(hasLineOfSight ? "CLEAR" : "BLOCKED")} S{profile.SightRange} H{profile.HearingAdjustment:+#;-#;0} A{profile.AllyAlertRadius} P{profile.PersistenceAdjustment:+#;-#;0} T{profile.TrailDetectionRange}",
                new Vector2(panelX + 6, panelY + 123), 1,
                hasLineOfSight ? Color.LightGreen : Color.OrangeRed);
        }

        private void DrawEndScreen()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(150, 205, 500, 170), Color.Black * 0.9f);

            string heading = _gameState.Outcome == GameOutcome.Won ? "YOU WIN" : "GAME OVER";
            Color headingColor = _gameState.Outcome == GameOutcome.Won ? Color.Yellow : Color.Red;
            int headingScale = 5;
            int headingX = (NativeWidth - _pixelFont.MeasureWidth(heading, headingScale)) / 2;
            _pixelFont.DrawText(_spriteBatch, heading, new Vector2(headingX, 240), headingScale, headingColor);

            const string restartText = "SPACE TO RESTART";
            int restartScale = 3;
            int restartX = (NativeWidth - _pixelFont.MeasureWidth(restartText, restartScale)) / 2;
            _pixelFont.DrawText(_spriteBatch, restartText, new Vector2(restartX, 320), restartScale, Color.White);
        }


        // handle window resizing to maintain aspect ratio and center the game content
        private void OnResize(object sender, EventArgs e)
        {
            // don't do anything is the window is out-of-size scope, or we're already resizing (to avoid recursive calls)
            if (_isResizing || Window.ClientBounds.Width == 0 || Window.ClientBounds.Height == 0) return;

            _isResizing = true;
            CalculateRenderDestination();
            _isResizing = false;
        }

        // recalculate the render destination rectangle to maintain aspect ratio and center the game content
        private void CalculateRenderDestination()
        {
            // figure out the new scale to maintain aspect ratio
            Point size = GraphicsDevice.Viewport.Bounds.Size;
            float scaleX = (float)size.X / NativeWidth;
            float scaleY = (float)size.Y / NativeHeight;
            float scale = Math.Min(scaleX, scaleY);

            // create a new render destination rectangle
            _renderDestination = new Rectangle(
                (int)((size.X - NativeWidth * scale) / 2),
                (int)((size.Y - NativeHeight * scale) / 2),
                (int)(NativeWidth * scale),
                (int)(NativeHeight * scale)
            );

        }

    }


}
