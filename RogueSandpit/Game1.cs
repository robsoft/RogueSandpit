using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RogueSandpit.Models;

namespace RogueSandpit
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D _renderTarget;
        private Rectangle _renderDestination;
        private bool _isResizing = false;

        private Map map;
        private Player player;
        private GameState gameState;
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;

        private int _nativeWidth = 800;
        private int _nativeHeight = 600;

        public Game1()
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
            map = new Map(GraphicsDevice, _nativeWidth, _nativeHeight, 123);
            KickOffNewGame();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, _nativeWidth, _nativeHeight);
            // initial calculation of the render destination rectangle
            CalculateRenderDestination();
        }

        private void KickOffNewGame()
        {
            map.Initialise();
            player = new Player();
            gameState = new GameState(map, player);

            player.X = map.StartPosX;
            player.Y = map.StartPosY;

            Window.Title = $"Rogue Sandpit - Seed: {RandGen.Seed}";
        }

        // todo: get the basic 'move' stuff implemented - needs to know about path traversal,
        // dealing with barriers and obstacles (can we treat doors with our 'obstacle' class?)
        protected override void Update(GameTime gameTime)
        {
            // deal with the meta stuff - updates that have nothing to do with the actual game itself
            _previousKeyboardState = _currentKeyboardState;

            // Get the new current state
            _currentKeyboardState = Keyboard.GetState();

            if (_currentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            if (_currentKeyboardState.IsKeyUp(Keys.Space) && _previousKeyboardState.IsKeyDown(Keys.Space))
            {
                KickOffNewGame();
            }

            if (_currentKeyboardState.IsKeyUp(Keys.F1) && _previousKeyboardState.IsKeyDown(Keys.F1))
            {
                if (map.RenderMode == RenderMode.Rooms)
                {
                    map.RenderMode = RenderMode.Cells;
                }
                else
                {
                    map.RenderMode = RenderMode.Rooms;
                }
            }
            
            if (!player.Dead)
            {
                // this will take care of the player's turn, and the computer's responses
                gameState.Update(gameTime, _currentKeyboardState, _previousKeyboardState);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // draw to our render target first
            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            map.Display(_spriteBatch);
            _spriteBatch.End();

            // then draw the render target to the screen
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
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
