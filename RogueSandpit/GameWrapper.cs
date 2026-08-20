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
            _map = new Map(GraphicsDevice, _nativeWidth, _nativeHeight, 123);
            KickOffNewGame(false);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _nativeWidth, _nativeHeight);
            _uiDrawer = new PrimitiveDrawer(GraphicsDevice);
            _pixelFont = new PixelFont(GraphicsDevice);
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
                _gameState.Update(gameTime, _currentKeyboardState, _previousKeyboardState);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // draw to our render target first
            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            _map.Display(_spriteBatch);
            DrawHud();

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
            _pixelFont.DrawText(_spriteBatch,
                $"HP {_player.Health}  DMG {_player.Damage}  SPECIAL {specialStatus}",
                new Vector2(6, 583), 2, Color.White);
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
