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

        private int _nativeWidth = 800;
        private int _nativeHeight = 600;

        public GameWrapper()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = _nativeWidth;
            _graphics.PreferredBackBufferHeight = _nativeHeight;
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
            _renderTarget = new RenderTarget2D(GraphicsDevice, _nativeWidth, _nativeHeight);
            _uiDrawer = new PrimitiveDrawer(GraphicsDevice);
            _pixelFont = new PixelFont(GraphicsDevice);
            _mapRenderer = new MapRenderer(GraphicsDevice, _map);
            // initial calculation of the render destination rectangle
            CalculateRenderDestination();
        }

        private void KickOffNewGame(bool regenerateMap = true)
        {
            if (regenerateMap)
            {
                _map.Initialise();
            }
            _player = new Player();
            _gameState = new GameState(_map, _player);

            _player.X = _map.StartPosX;
            _player.Y = _map.StartPosY;

            Window.Title = $"Rogue Sandpit - Seed: {RandGen.Seed}";
        }

        protected override void Update(GameTime gameTime)
        {
            // deal with the meta stuff - updates that have nothing to do with the actual game itself
            _previousKeyboardState = _currentKeyboardState;

            // Get the new current state
            _currentKeyboardState = Keyboard.GetState();

            if (_currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

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
            if (WasPressed(Keys.Up)) return PlayerCommand.MoveUp;
            if (WasPressed(Keys.Down)) return PlayerCommand.MoveDown;
            if (WasPressed(Keys.Left)) return PlayerCommand.MoveLeft;
            if (WasPressed(Keys.Right)) return PlayerCommand.MoveRight;
            if (WasPressed(Keys.H)) return PlayerCommand.UsePotion;
            if (WasPressed(Keys.E)) return PlayerCommand.EquipWeapon;
            return PlayerCommand.None;
        }

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
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(0, 580, _nativeWidth, 20), Color.Black);
            string specialStatus = _player.HasSpecial ? "YES" : "NO";
            string weaponName = _player.EquippedWeapon?.Name ?? "NONE";
            _pixelFont.DrawText(_spriteBatch,
                $"HP {_player.Health} OF {_player.MaxHealth} DMG {_player.Damage} SPECIAL {specialStatus} INV {_player.Inventory.Items.Count} WPN {weaponName}",
                new Vector2(6, 585), 1, Color.White);
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
                _nativeWidth, _nativeHeight, _map, out Point mapCell)
                ? mapCell
                : null;
        }

        private void DrawDebugInspection(Point position)
        {
            const int panelX = 485;
            const int panelY = 5;
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(panelX, panelY, 305, 128), Color.Black * 0.9f);

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
            _pixelFont.DrawText(_spriteBatch, $"ITEM {itemName}",
                new Vector2(panelX + 6, panelY + 43), 1, Color.LightGray);

            BaseNPC npc = _map.GetLivingNPCAt(position.X, position.Y);
            if (npc == null)
            {
                _pixelFont.DrawText(_spriteBatch, "NPC NONE", new Vector2(panelX + 6, panelY + 59), 1, Color.LightGray);
                return;
            }

            _pixelFont.DrawText(_spriteBatch, $"NPC {npc.Name} HP {npc.HP} DMG {npc.Damage}",
                new Vector2(panelX + 6, panelY + 59), 1, Color.White);
            _pixelFont.DrawText(_spriteBatch, $"AI {npc.Awareness}  SEEN {(npc.HasSeenPlayer ? "YES" : "NO")}",
                new Vector2(panelX + 6, panelY + 75), 1, Color.White);

            string lastKnown = npc.LastKnownPlayerPosition is { } target
                ? $"{target.X} {target.Y}"
                : "NONE";
            _pixelFont.DrawText(_spriteBatch, $"LAST {lastKnown}",
                new Vector2(panelX + 6, panelY + 91), 1, Color.White);

            bool hasLineOfSight = _map.HasLineOfSight(npc.X, npc.Y, _player.X, _player.Y);
            _pixelFont.DrawText(_spriteBatch, $"LOS {(hasLineOfSight ? "CLEAR" : "BLOCKED")}",
                new Vector2(panelX + 6, panelY + 107), 1,
                hasLineOfSight ? Color.LightGreen : Color.OrangeRed);
        }

        private void DrawEndScreen()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(150, 205, 500, 170), Color.Black * 0.9f);

            string heading = _gameState.Outcome == GameOutcome.Won ? "YOU WIN" : "GAME OVER";
            Color headingColor = _gameState.Outcome == GameOutcome.Won ? Color.Yellow : Color.Red;
            int headingScale = 5;
            int headingX = (_nativeWidth - _pixelFont.MeasureWidth(heading, headingScale)) / 2;
            _pixelFont.DrawText(_spriteBatch, heading, new Vector2(headingX, 240), headingScale, headingColor);

            const string restartText = "SPACE TO RESTART";
            int restartScale = 3;
            int restartX = (_nativeWidth - _pixelFont.MeasureWidth(restartText, restartScale)) / 2;
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
            float scaleX = (float)size.X / _nativeWidth;
            float scaleY = (float)size.Y / _nativeHeight;
            float scale = Math.Min(scaleX, scaleY);

            // create a new render destination rectangle
            _renderDestination = new Rectangle(
                (int)((size.X - _nativeWidth * scale) / 2),
                (int)((size.Y - _nativeHeight * scale) / 2),
                (int)(_nativeWidth * scale),
                (int)(_nativeHeight * scale)
            );

        }

    }


}
