using System;
using System.Linq;
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
        private readonly RealtimeTurnTimer _realtimeTurnTimer;
        private readonly ApplicationScreenCoordinator _screens = new();
        private readonly RuntimeSettings _runtimeSettings;
        private readonly InputBindings _bindings;
        private readonly SettingsStore _settingsStore;
        private int _pauseMenuSelection;
        private int _optionsSelection;
        private int _controlsSelection;
        private int _bindingSlot;
        private bool _capturingBinding;
        private string _controlsMessage = "";
        private readonly int _initialSeed;

        private enum DirectionalAction { None, ToggleDoor, LayFalseTrail, ThrowItem, PlaceTrap, FireRanged }
        private enum PauseMenuItem { Resume, Options, RestartThisSeed, NewRun, Quit }
        private enum OptionsItem { RealtimeInterval, MasterVolume, EffectsVolume, MusicVolume, MuteUnfocused, Controls, Back }

        public const int NativeWidth = 800;
        public const int NativeHeight = 600;

        public GameWrapper(int windowScale = GameOptions.DefaultWindowScale,
            double turnSeconds = GameOptions.DefaultTurnSeconds,
            bool fullscreen = false, bool startRealtime = false, int? seed = null)
        {
            _initialSeed = seed ?? Random.Shared.Next();
            _settingsStore = new SettingsStore(SettingsStore.DefaultPath);
            LoadedSettings loadedSettings = _settingsStore.Load(turnSeconds);
            _runtimeSettings = loadedSettings.Runtime;
            _bindings = loadedSettings.Bindings;
            _realtimeTurnTimer = new RealtimeTurnTimer(
                _runtimeSettings.RealtimeTurnSeconds, startRealtime);
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            if (fullscreen)
            {
                DisplayMode desktop = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                _graphics.HardwareModeSwitch = false;
                _graphics.IsFullScreen = true;
                _graphics.PreferredBackBufferWidth = desktop.Width;
                _graphics.PreferredBackBufferHeight = desktop.Height;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = NativeWidth * windowScale;
                _graphics.PreferredBackBufferHeight = NativeHeight * windowScale;
            }
            _graphics.ApplyChanges();

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += OnResize;
            IsMouseVisible = true;

        }

        protected override void Initialize()
        {
            _map = new Map(_initialSeed);
            KickOffNewGame(false);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderTarget = new RenderTarget2D(GraphicsDevice, NativeWidth, NativeHeight);
            _uiDrawer = new PrimitiveDrawer(GraphicsDevice);
            _pixelFont = new PixelFont(GraphicsDevice);
            Texture2D prototypeAtlasTexture = Content.Load<Texture2D>("sprites/prototype-slice");
            _mapRenderer = new MapRenderer(GraphicsDevice, _map,
                new PrototypeSpriteAtlas(prototypeAtlasTexture));
            // initial calculation of the render destination rectangle
            CalculateRenderDestination();
        }

        private void KickOffNewGame(bool regenerateMap = true, int? newSeed = null)
        {
            _realtimeTurnTimer.Reset();
            _inventoryOpen = false;
            _directionalAction = DirectionalAction.None;
            if (regenerateMap)
            {
                if (newSeed.HasValue) _map.Regenerate(newSeed.Value);
                else _map.Initialise();
            }
            _player = new Player();
            _player.Place(_map, _map.StartPosX, _map.StartPosY);
            _gameState = new GameState(_map, _player);
            _screens.StartPlaying();

            Window.Title = $"Rogue Sandpit - Seed: {_map.Seed}";
        }

        private void StartNewRun()
        {
            int seed;
            do seed = Random.Shared.Next(); while (seed == _map.Seed);
            KickOffNewGame(newSeed: seed);
        }

        protected override void Update(GameTime gameTime)
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            if (WasPressed(Keys.Escape) && HandleEscape())
            {
                base.Update(gameTime);
                return;
            }

            switch (_screens.CurrentScreen)
            {
                case ApplicationScreen.Playing:
                    UpdateLive(gameTime);
                    _screens.SynchronizeOutcome(_gameState.Outcome);
                    break;
                case ApplicationScreen.Paused:
                    UpdatePauseMenu();
                    break;
                case ApplicationScreen.Options:
                    UpdateOptionsMenu();
                    break;
                case ApplicationScreen.Controls:
                    UpdateControlsMenu();
                    break;
                case ApplicationScreen.GameOver:
                case ApplicationScreen.Victory:
                    UpdateTerminalScreen();
                    break;
            }

            base.Update(gameTime);
        }

        private bool HandleEscape()
        {
            if (_directionalAction != DirectionalAction.None)
            {
                _directionalAction = DirectionalAction.None;
                return true;
            }

            if (_inventoryOpen)
            {
                _inventoryOpen = false;
                return true;
            }

            if (_screens.CurrentScreen == ApplicationScreen.Controls)
            {
                if (_capturingBinding)
                {
                    _capturingBinding = false;
                    _controlsMessage = "CAPTURE CANCELLED";
                }
                else
                {
                    _screens.BackFromControls();
                }
                return true;
            }

            if (_screens.CurrentScreen == ApplicationScreen.Options) _screens.BackFromOptions();
            else if (_screens.CurrentScreen == ApplicationScreen.Paused) _screens.Resume();
            else if (_screens.CurrentScreen == ApplicationScreen.Playing) _screens.Pause();
            else return false;
            return true;
        }

        private void UpdateTerminalScreen()
        {
            if (WasPressed(Keys.Space)) StartNewRun();
        }

        private void UpdatePauseMenu()
        {
            int count = Enum.GetValues<PauseMenuItem>().Length;
            if (WasPressed(Keys.Up)) _pauseMenuSelection = (_pauseMenuSelection - 1 + count) % count;
            if (WasPressed(Keys.Down)) _pauseMenuSelection = (_pauseMenuSelection + 1) % count;
            if (!WasPressed(Keys.Enter)) return;

            switch ((PauseMenuItem)_pauseMenuSelection)
            {
                case PauseMenuItem.Resume:
                    _screens.Resume();
                    break;
                case PauseMenuItem.Options:
                    _optionsSelection = 0;
                    _screens.OpenOptions();
                    break;
                case PauseMenuItem.RestartThisSeed:
                    KickOffNewGame();
                    break;
                case PauseMenuItem.NewRun:
                    StartNewRun();
                    break;
                case PauseMenuItem.Quit:
                    Exit();
                    break;
            }
        }

        private void UpdateOptionsMenu()
        {
            int count = Enum.GetValues<OptionsItem>().Length;
            if (WasPressed(Keys.Up)) _optionsSelection = (_optionsSelection - 1 + count) % count;
            if (WasPressed(Keys.Down)) _optionsSelection = (_optionsSelection + 1) % count;

            int direction = WasPressed(Keys.Left) ? -1 : WasPressed(Keys.Right) ? 1 : 0;
            if (direction != 0) AdjustSelectedOption(direction);
            if (!WasPressed(Keys.Enter)) return;
            if ((OptionsItem)_optionsSelection == OptionsItem.Controls)
            {
                _controlsSelection = 0;
                _controlsMessage = "";
                _screens.OpenControls();
            }
            else if ((OptionsItem)_optionsSelection == OptionsItem.Back)
            {
                _screens.BackFromOptions();
            }
        }

        private void AdjustSelectedOption(int direction)
        {
            bool changed = true;
            switch ((OptionsItem)_optionsSelection)
            {
                case OptionsItem.RealtimeInterval:
                    _runtimeSettings.AdjustRealtimeInterval(direction * 0.1);
                    _realtimeTurnTimer.SetInterval(_runtimeSettings.RealtimeTurnSeconds);
                    break;
                case OptionsItem.MasterVolume:
                    _runtimeSettings.AdjustMasterVolume(direction * 10);
                    break;
                case OptionsItem.EffectsVolume:
                    _runtimeSettings.AdjustEffectsVolume(direction * 10);
                    break;
                case OptionsItem.MusicVolume:
                    _runtimeSettings.AdjustMusicVolume(direction * 10);
                    break;
                case OptionsItem.MuteUnfocused:
                    _runtimeSettings.ToggleMuteWhileUnfocused();
                    break;
                default:
                    changed = false;
                    break;
            }
            if (changed) SaveSettings();
        }

        private void UpdateControlsMenu()
        {
            InputAction[] actions = Enum.GetValues<InputAction>();
            int resetRow = actions.Length;
            int backRow = actions.Length + 1;
            int rowCount = actions.Length + 2;

            if (_capturingBinding)
            {
                Keys? captured = NewlyPressedKey();
                if (!captured.HasValue) return;
                InputAction action = actions[_controlsSelection];
                if (InputBindings.IsReserved(captured.Value))
                {
                    _controlsMessage = $"{captured.Value} IS RESERVED";
                }
                else if (_bindings.TrySet(action, _bindingSlot, captured.Value,
                    out InputAction? conflict))
                {
                    _controlsMessage = $"{action} SET TO {captured.Value}";
                    SaveSettings();
                }
                else
                {
                    _controlsMessage = conflict.HasValue
                        ? $"{captured.Value} IS BOUND TO {conflict.Value}"
                        : $"CANNOT BIND {captured.Value}";
                }
                _capturingBinding = false;
                return;
            }

            if (WasPressed(Keys.Up)) _controlsSelection = (_controlsSelection - 1 + rowCount) % rowCount;
            if (WasPressed(Keys.Down)) _controlsSelection = (_controlsSelection + 1) % rowCount;
            if (WasPressed(Keys.Tab)) _bindingSlot = 1 - _bindingSlot;

            if (_controlsSelection < actions.Length && WasPressed(Keys.Back))
            {
                _bindings.Reset(actions[_controlsSelection]);
                _controlsMessage = $"{actions[_controlsSelection]} RESET";
                SaveSettings();
            }
            if (_controlsSelection < actions.Length && _bindingSlot == 1 && WasPressed(Keys.Delete))
            {
                if (_bindings.ClearSecondary(actions[_controlsSelection])) SaveSettings();
                _controlsMessage = "SECONDARY CLEARED";
            }
            if (!WasPressed(Keys.Enter)) return;

            if (_controlsSelection < actions.Length)
            {
                _capturingBinding = true;
                _controlsMessage = "PRESS A KEY   ESC CANCELS";
            }
            else if (_controlsSelection == resetRow)
            {
                _bindings.ResetAll();
                _controlsMessage = "ALL BINDINGS RESET";
                SaveSettings();
            }
            else if (_controlsSelection == backRow)
            {
                _screens.BackFromControls();
            }
        }

        private Keys? NewlyPressedKey()
        {
            foreach (Keys key in _currentKeyboardState.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyUp(key)) return key;
            }
            return null;
        }

        private void SaveSettings() => _settingsStore.Save(_runtimeSettings, _bindings);

        private void UpdateLive(GameTime gameTime)
        {
            if (WasPressed(Keys.F11))
            {
                DeveloperLoadout.Apply(_player);
                _realtimeTurnTimer.Reset();
                _gameState.EventLog.Add("DEVELOPER LOADOUT APPLIED");
            }

            if (WasPressed(Keys.F12))
            {
                _realtimeTurnTimer.Toggle();
                _gameState.EventLog.Add(_realtimeTurnTimer.Enabled
                    ? "REAL-TIME MODE ON"
                    : "REAL-TIME MODE OFF");
            }

            if (WasPressed(InputAction.Inventory))
            {
                _inventoryOpen = !_inventoryOpen;
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
                long previousTurn = _gameState.TurnCount;
                _gameState.Update(GetPlayerCommand());
                if (_gameState.TurnCount != previousTurn)
                {
                    _realtimeTurnTimer.Reset();
                }
                else
                {
                    bool paused = _inventoryOpen
                        || _directionalAction != DirectionalAction.None
                        || !IsActive;
                    if (_realtimeTurnTimer.Advance(gameTime.ElapsedGameTime.TotalSeconds, paused))
                        _gameState.Update(PlayerCommand.Wait, suppressWaitEvent: true);
                }
            }

        }

        private PlayerCommand GetPlayerCommand()
        {
            if (_inventoryOpen)
            {
                int? slot = NewlyPressedInventorySlot();
                if (slot.HasValue)
                {
                    _player.Inventory.SelectIndex(slot.Value);
                    return PlayerCommand.None;
                }
                if (WasPressed(InputAction.MoveUp) || WasPressed(InputAction.MoveLeft)
                    || WasPressed(InputAction.SelectPreviousItem))
                    return PlayerCommand.SelectPreviousItem;
                if (WasPressed(InputAction.MoveDown) || WasPressed(InputAction.MoveRight)
                    || WasPressed(InputAction.SelectNextItem))
                    return PlayerCommand.SelectNextItem;
                if (WasPressed(InputAction.UsePotion)) return PlayerCommand.UsePotion;
                if (WasPressed(InputAction.UseBandage)) return PlayerCommand.UseBandage;
                if (WasPressed(Keys.Enter) || WasPressed(InputAction.Equip))
                    return PlayerCommand.EquipItem;
                if (WasPressed(InputAction.Drop)) return PlayerCommand.DropItem;
                return PlayerCommand.None;
            }

            if (_directionalAction != DirectionalAction.None)
            {
                if (WasPressed(InputAction.MoveUp)) return DirectionalCommand(0, -1);
                if (WasPressed(InputAction.MoveDown)) return DirectionalCommand(0, 1);
                if (WasPressed(InputAction.MoveLeft)) return DirectionalCommand(-1, 0);
                if (WasPressed(InputAction.MoveRight)) return DirectionalCommand(1, 0);
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.ToggleDoor))
            {
                var doors = _map.GetAdjacentOperableDoors(_player.X, _player.Y);
                if (doors.Count == 0)
                {
                    _gameState.EventLog.Add("NO OPERABLE DOOR NEARBY");
                    return PlayerCommand.None;
                }
                if (doors.Count == 1)
                {
                    return ToggleDoorCommand(doors[0].X1 - _player.X, doors[0].Y1 - _player.Y);
                }
                _directionalAction = DirectionalAction.ToggleDoor;
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.LayFalseTrail))
            {
                _directionalAction = DirectionalAction.LayFalseTrail;
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.ThrowItem))
            {
                if (_player.Inventory.SelectedItem == null)
                {
                    _gameState.EventLog.Add("SELECT AN ITEM TO THROW");
                    return PlayerCommand.None;
                }
                _directionalAction = DirectionalAction.ThrowItem;
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.PlaceTrap))
            {
                if (_player.Inventory.SelectedItem?.Type != ItemType.Trap)
                {
                    _gameState.EventLog.Add("SELECT A TRAP");
                    return PlayerCommand.None;
                }
                _directionalAction = DirectionalAction.PlaceTrap;
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.FireRanged))
            {
                if (_player.EquippedRangedWeapon == null)
                {
                    _gameState.EventLog.Add("NO RANGED WEAPON EQUIPPED");
                    return PlayerCommand.None;
                }
                _directionalAction = DirectionalAction.FireRanged;
                return PlayerCommand.None;
            }

            if (WasPressed(InputAction.MoveUp)) return PlayerCommand.MoveUp;
            if (WasPressed(InputAction.MoveDown)) return PlayerCommand.MoveDown;
            if (WasPressed(InputAction.MoveLeft)) return PlayerCommand.MoveLeft;
            if (WasPressed(InputAction.MoveRight)) return PlayerCommand.MoveRight;
            if (WasPressed(InputAction.Wait)) return PlayerCommand.Wait;
            if (WasPressed(InputAction.SelectPreviousItem)) return PlayerCommand.SelectPreviousItem;
            if (WasPressed(InputAction.SelectNextItem)) return PlayerCommand.SelectNextItem;
            if (WasPressed(InputAction.UsePotion)) return PlayerCommand.UsePotion;
            if (WasPressed(InputAction.UseBandage)) return PlayerCommand.UseBandage;
            if (WasPressed(InputAction.Equip)) return PlayerCommand.EquipItem;
            if (WasPressed(InputAction.Drop)) return PlayerCommand.DropItem;
            return PlayerCommand.None;
        }

        private int? NewlyPressedInventorySlot()
        {
            foreach (Keys key in _currentKeyboardState.GetPressedKeys())
            {
                if (_previousKeyboardState.IsKeyDown(key)) continue;
                int? slot = InputBindings.InventorySlotForKey(key);
                if (slot >= 0 && slot < _player.Inventory.Capacity) return slot;
            }
            return null;
        }

        private PlayerCommand DirectionalCommand(int deltaX, int deltaY)
        {
            if (_directionalAction == DirectionalAction.ToggleDoor)
            {
                Doorway door = _map.GetDoorAt(_player.X + deltaX, _player.Y + deltaY);
                if (door != null && door.State != DoorState.Locked
                    && (door.State == DoorState.Closed
                        || !_map.IsOccupiedByLivingNPC(door.X1, door.Y1)))
                    _directionalAction = DirectionalAction.None;
                return ToggleDoorCommand(deltaX, deltaY);
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

            if (_directionalAction == DirectionalAction.FireRanged)
            {
                if (_map.TraceThrow(_player.X, _player.Y, deltaX, deltaY) != null)
                    _directionalAction = DirectionalAction.None;
                return FireRangedCommand(deltaX, deltaY);
            }

            if (_map.CanPlaceTrap(_player.X + deltaX, _player.Y + deltaY, _player))
                _directionalAction = DirectionalAction.None;
            return PlaceTrapCommand(deltaX, deltaY);
        }

        private static PlayerCommand ToggleDoorCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.ToggleDoorUp,
            (0, 1) => PlayerCommand.ToggleDoorDown,
            (-1, 0) => PlayerCommand.ToggleDoorLeft,
            (1, 0) => PlayerCommand.ToggleDoorRight,
            _ => PlayerCommand.None
        };

        private static PlayerCommand FireRangedCommand(int deltaX, int deltaY) => (deltaX, deltaY) switch
        {
            (0, -1) => PlayerCommand.FireRangedUp,
            (0, 1) => PlayerCommand.FireRangedDown,
            (-1, 0) => PlayerCommand.FireRangedLeft,
            (1, 0) => PlayerCommand.FireRangedRight,
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

        private bool WasPressed(InputAction action) =>
            _bindings.IsPressed(action, _currentKeyboardState, _previousKeyboardState);

        protected override void Draw(GameTime gameTime)
        {
            // draw to our render target first
            GraphicsDevice.SetRenderTarget(_renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            DrawPresentationBackground();
            Point? hoveredCell = GetHoveredMapCell();
            _mapRenderer.Display(_spriteBatch, _player, hoveredCell);
            DrawEventLog();
            DrawHud();
            DrawContextStrip();

            if (_directionalAction != DirectionalAction.None) DrawDirectionalActionPrompt();

            if (_inventoryOpen && _gameState.Outcome == GameOutcome.Playing)
            {
                DrawInventoryPanel();
            }

            if (_map.RenderMode == RenderMode.Cells && hoveredCell.HasValue)
            {
                DrawDebugInspection(hoveredCell.Value);
            }

            if (_screens.CurrentScreen == ApplicationScreen.Paused)
            {
                DrawPauseMenu();
            }
            else if (_screens.CurrentScreen == ApplicationScreen.Options)
            {
                DrawOptionsMenu();
            }
            else if (_screens.CurrentScreen == ApplicationScreen.Controls)
            {
                DrawControlsMenu();
            }
            else if (_screens.CurrentScreen is ApplicationScreen.GameOver or ApplicationScreen.Victory)
            {
                DrawEndScreen();
            }
            _spriteBatch.End();

            // then draw the render target to the screen
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(_renderTarget, _renderDestination, Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud()
        {
            if (_map.RenderMode == RenderMode.Cells)
            {
                DrawDebugHud();
                return;
            }

            DrawSidebarHud();
            DrawModeBar();
        }

        private void DrawSidebarHud()
        {
            int x = MapViewport.VisibleColumns * MapViewport.TileSize + 8;
            int contentWidth = NativeWidth - x - 8;

            _pixelFont.DrawText(_spriteBatch, "PLAYER", new Vector2(x, 10), 2, Color.White);
            float healthRatio = _player.MaxHealth <= 0
                ? 0f
                : Math.Clamp((float)_player.Health / _player.MaxHealth, 0f, 1f);
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(x, 32, contentWidth, 12), Color.DarkRed);
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(x, 32, (int)(contentWidth * healthRatio), 12), Color.IndianRed);
            _pixelFont.DrawText(_spriteBatch,
                $"HP {_player.Health}/{_player.MaxHealth}", new Vector2(x + 4, 35), 1, Color.White);
            _pixelFont.DrawText(_spriteBatch,
                $"DAMAGE {_player.Damage}   DEFENCE {_player.Defence}",
                new Vector2(x, 54), 1, Color.LightGray);

            _pixelFont.DrawText(_spriteBatch, "EQUIPMENT", new Vector2(x, 90), 2, Color.White);
            _pixelFont.DrawText(_spriteBatch,
                $"MELEE  {_player.EquippedWeapon?.Name ?? "NONE"}",
                new Vector2(x, 116), 1, Color.LightGray);
            _pixelFont.DrawText(_spriteBatch,
                $"RANGED {_player.EquippedRangedWeapon?.Name ?? "NONE"}",
                new Vector2(x, 134), 1, Color.LightGray);
            _pixelFont.DrawText(_spriteBatch,
                $"ARMOR  {_player.EquippedArmor?.Name ?? "NONE"}",
                new Vector2(x, 152), 1, Color.LightGray);

            _pixelFont.DrawText(_spriteBatch, "INVENTORY", new Vector2(x, 186), 2, Color.White);
            _pixelFont.DrawText(_spriteBatch,
                $"SLOTS {_player.Inventory.Items.Count}/{_player.Inventory.Capacity}",
                new Vector2(x, 212), 1, Color.LightGray);
            Item selected = _player.Inventory.SelectedItem;
            _pixelFont.DrawText(_spriteBatch,
                $"SELECTED {selected?.Name ?? "NONE"}", new Vector2(x, 230), 1,
                selected == null ? Color.DarkGray : Color.White);

            _pixelFont.DrawText(_spriteBatch, "OBJECTIVE", new Vector2(x, 266), 2, Color.White);
            _pixelFont.DrawText(_spriteBatch,
                _player.HasSpecial ? "SPECIAL RECOVERED" : "FIND THE YELLOW SPECIAL",
                new Vector2(x, 292), 1, _player.HasSpecial ? Color.Yellow : Color.LightGray);
            _pixelFont.DrawText(_spriteBatch,
                $"EFFECTS {EffectSummary(_player.StatusEffects)}",
                new Vector2(x, 310), 1,
                _player.StatusEffects.Effects.Count > 0 ? Color.OrangeRed : Color.DarkGray);

            _pixelFont.DrawText(_spriteBatch, "EVENTS", new Vector2(x, 346), 2, Color.White);
        }

        private void DrawModeBar()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(0, 580, NativeWidth, 20), Color.Black);
            string mode = !_realtimeTurnTimer.Enabled
                ? "TURN-BASED"
                : (_inventoryOpen || _directionalAction != DirectionalAction.None
                    || !_screens.SimulationActive || !IsActive)
                    ? "REAL-TIME PAUSED"
                    : "REAL-TIME";
            _pixelFont.DrawText(_spriteBatch,
                $"TURN {_gameState.TurnCount}   MODE {mode}   F1 DEBUG   F12 TOGGLE MODE",
                new Vector2(8, 586), 1, Color.LightGray);
        }

        private void DrawDebugHud()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(0, 552, 300, 14), Color.Black);
            _pixelFont.DrawText(_spriteBatch,
                $"SEED {_map.Seed}  DOORS {_map.Doors.Count}/{_map.DoorCandidateCount}  PRUNED {_map.PrunedDoorwayCount}",
                new Vector2(6, 555), 1, Color.LightGoldenrodYellow);

            if (_realtimeTurnTimer.Enabled && _map.RenderMode == RenderMode.Cells)
            {
                string timerText = (_inventoryOpen || _directionalAction != DirectionalAction.None
                    || !_screens.SimulationActive || !IsActive)
                    ? "REALTIME PAUSED"
                    : $"REALTIME {_realtimeTurnTimer.RemainingSeconds:0.0}s";
                _uiDrawer.DrawFilledRectangle(_spriteBatch,
                    new Rectangle(650, 552, 150, 14), Color.Black);
                _pixelFont.DrawText(_spriteBatch, timerText,
                    new Vector2(656, 555), 1, Color.Yellow);
            }

            if (_player.StatusEffects.Effects.Count > 0)
            {
                _uiDrawer.DrawFilledRectangle(_spriteBatch,
                    new Rectangle(0, 566, NativeWidth, 14), Color.Black);
                _pixelFont.DrawText(_spriteBatch,
                    $"EFFECTS {EffectSummary(_player.StatusEffects)}",
                    new Vector2(6, 569), 1, Color.OrangeRed);
            }
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(0, 580, NativeWidth, 20), Color.Black);
            string specialStatus = _player.HasSpecial ? "YES" : "NO";
            string weaponName = _player.EquippedWeapon?.Name ?? "NONE";
            string armorName = _player.EquippedArmor?.Name ?? "NONE";
            string rangedName = _player.EquippedRangedWeapon?.Name ?? "NONE";
            string selectedName = _player.Inventory.SelectedItem?.Name ?? "NONE";
            _pixelFont.DrawText(_spriteBatch,
                $"HP {_player.Health}/{_player.MaxHealth} DMG {_player.Damage} DEF {_player.Defence} SPECIAL {specialStatus} INV {_player.Inventory.Items.Count} SEL {selectedName} WPN {weaponName} ARM {armorName} RNG {rangedName}",
                new Vector2(6, 585), 1, Color.White);
        }

        private void DrawPresentationBackground()
        {
            if (_map.RenderMode == RenderMode.Cells) return;

            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(MapViewport.VisibleColumns * MapViewport.TileSize, 0,
                    NativeWidth - MapViewport.VisibleColumns * MapViewport.TileSize, 580),
                new Color(18, 18, 24));
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(0, MapViewport.VisibleRows * MapViewport.TileSize,
                    NativeWidth, 580 - MapViewport.VisibleRows * MapViewport.TileSize),
                new Color(12, 12, 18));

            foreach (int y in new[] { 80, 176, 256, 336 })
            {
                int sidebarX = MapViewport.VisibleColumns * MapViewport.TileSize + 8;
                _uiDrawer.DrawLine(_spriteBatch, sidebarX, y, NativeWidth - 8, y,
                    new Color(55, 55, 68));
            }
        }

        private void DrawContextStrip()
        {
            if (_map.RenderMode == RenderMode.Cells || _directionalAction != DirectionalAction.None) return;

            _pixelFont.DrawText(_spriteBatch, "COMMANDS", new Vector2(10, 524), 2, Color.White);
            string hint = _inventoryOpen
                ? "INVENTORY OPEN   ARROWS SELECT   I CLOSE"
                : "ARROWS MOVE   . WAIT   I INVENTORY   C DOOR   F THROW   R FIRE";
            _pixelFont.DrawText(_spriteBatch, hint, new Vector2(10, 554), 1, Color.DarkGray);
        }

        private static string EffectSummary(StatusEffectCollection statusEffects)
        {
            return statusEffects.Effects.Count == 0
                ? "NONE"
                : string.Join(" ", statusEffects.Effects.Select(
                    effect => $"{effect.Type} {effect.RemainingTurns}"));
        }

        private void DrawDirectionalActionPrompt()
        {
            string prompt = _directionalAction switch
            {
                DirectionalAction.ToggleDoor => "OPERATE DOOR: ARROW CHOOSES  ESC CANCELS",
                DirectionalAction.LayFalseTrail => "FALSE TRAIL: ARROW CHOOSES  ESC CANCELS",
                DirectionalAction.ThrowItem => "THROW ITEM: ARROW CHOOSES  ESC CANCELS",
                DirectionalAction.PlaceTrap => "PLACE TRAP: ARROW CHOOSES  ESC CANCELS",
                _ => "FIRE BOW: ARROW CHOOSES  ESC CANCELS"
            };

            if (_map.RenderMode != RenderMode.Cells)
            {
                _pixelFont.DrawText(_spriteBatch, "ACTION", new Vector2(10, 524), 2, Color.Yellow);
                _pixelFont.DrawText(_spriteBatch, prompt,
                    new Vector2(10, 554), 1, Color.White);
                return;
            }

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
                _pixelFont.DrawText(_spriteBatch, $"{(selected ? ">" : " ")}{index + 1} {itemName}",
                    new Vector2(panelX + 18, rowY), 2, itemColor);

                if (item == null) continue;
                string power = item.Power > 0 ? $" {item.Power}" : "";
                string equipped = item == _player.EquippedWeapon ? " [MELEE]"
                    : item == _player.EquippedArmor ? " [ARMOR]"
                    : item == _player.EquippedRangedWeapon ? " [RANGED]"
                    : "";
                _pixelFont.DrawText(_spriteBatch, $"{item.Type}{power}{equipped}",
                    new Vector2(panelX + 245, rowY + 4), 1, Color.LightGray);
            }

            string selectedName = _player.Inventory.SelectedItem?.Name ?? "NONE";
            string selectedAction = InventoryActionHint(_player.Inventory.SelectedItem);
            _pixelFont.DrawText(_spriteBatch, $"SELECTED {selectedName}   {selectedAction}",
                new Vector2(panelX + 14, panelY + panelHeight - 42), 1, Color.Yellow);
            _pixelFont.DrawText(_spriteBatch, "1-8/ARROWS SELECT  ENTER TOGGLE EQUIP  D DROP  I CLOSE",
                new Vector2(panelX + 14, panelY + panelHeight - 22), 1, Color.LightGray);
        }

        private string InventoryActionHint(Item item)
        {
            if (item == null) return "";
            return item.Type switch
            {
                ItemType.Weapon or ItemType.Armor or ItemType.RangedWeapon =>
                    _player.IsEquipped(item) ? "ENTER UNEQUIP  F THROW" : "ENTER EQUIP  F THROW",
                ItemType.HealingPotion => "H USE  F THROW",
                ItemType.Bandage => "B USE  F THROW",
                ItemType.Trap => "P PLACE  F THROW",
                ItemType.SmokeBomb => "F THROW SMOKE",
                ItemType.FireBomb => "F THROW FIRE",
                _ => "F THROW"
            };
        }

        private void DrawPauseMenu()
        {
            const int panelX = 170;
            const int panelY = 75;
            const int panelWidth = 460;
            const int panelHeight = 430;
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.94f);
            const string heading = "PAUSED";
            const int headingScale = 5;
            int headingX = panelX + (panelWidth - _pixelFont.MeasureWidth(heading, headingScale)) / 2;
            _pixelFont.DrawText(_spriteBatch, heading,
                new Vector2(headingX, panelY + 35), headingScale, Color.White);

            PauseMenuItem[] items = Enum.GetValues<PauseMenuItem>();
            for (int index = 0; index < items.Length; index++)
            {
                int y = panelY + 125 + index * 48;
                bool selected = index == _pauseMenuSelection;
                if (selected)
                {
                    _uiDrawer.DrawFilledRectangle(_spriteBatch,
                        new Rectangle(panelX + 35, y - 9, panelWidth - 70, 34), Color.DarkSlateBlue);
                }
                string label = items[index] switch
                {
                    PauseMenuItem.RestartThisSeed => "RESTART THIS SEED",
                    PauseMenuItem.NewRun => "NEW RUN",
                    _ => items[index].ToString().ToUpperInvariant()
                };
                _pixelFont.DrawText(_spriteBatch, label,
                    new Vector2(panelX + 60, y), 3, selected ? Color.White : Color.Gray);
            }

            _pixelFont.DrawText(_spriteBatch, "ARROWS SELECT   ENTER CONFIRM   ESC RESUME",
                new Vector2(panelX + 42, panelY + panelHeight - 30), 1, Color.LightGray);
        }

        private void DrawOptionsMenu()
        {
            const int panelX = 145;
            const int panelY = 65;
            const int panelWidth = 510;
            const int panelHeight = 470;
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.96f);
            _pixelFont.DrawText(_spriteBatch, "OPTIONS",
                new Vector2(panelX + 135, panelY + 28), 5, Color.White);

            string[] values =
            [
                $"REAL-TIME INTERVAL   {_runtimeSettings.RealtimeTurnSeconds:0.0} SECONDS",
                $"MASTER VOLUME        {_runtimeSettings.MasterVolume}%  FUTURE AUDIO",
                $"EFFECTS VOLUME       {_runtimeSettings.EffectsVolume}%  FUTURE AUDIO",
                $"MUSIC VOLUME         {_runtimeSettings.MusicVolume}%  FUTURE AUDIO",
                $"MUTE WHEN UNFOCUSED  {(_runtimeSettings.MuteWhileUnfocused ? "YES" : "NO")}",
                "CONTROLS",
                "BACK"
            ];

            for (int index = 0; index < values.Length; index++)
            {
                int y = panelY + 115 + index * 52;
                bool selected = index == _optionsSelection;
                if (selected)
                {
                    _uiDrawer.DrawFilledRectangle(_spriteBatch,
                        new Rectangle(panelX + 25, y - 10, panelWidth - 50, 34), Color.DarkSlateBlue);
                }
                _pixelFont.DrawText(_spriteBatch, values[index],
                    new Vector2(panelX + 42, y), 2, selected ? Color.White : Color.Gray);
            }

            _pixelFont.DrawText(_spriteBatch, "UP DOWN SELECT   LEFT RIGHT CHANGE   ENTER BACK   ESC BACK",
                new Vector2(panelX + 32, panelY + panelHeight - 27), 1, Color.LightGray);
        }

        private void DrawControlsMenu()
        {
            const int panelX = 80;
            const int panelY = 20;
            const int panelWidth = 640;
            const int panelHeight = 560;
            const int firstRowY = 72;
            const int rowHeight = 25;
            _uiDrawer.DrawFilledRectangle(_spriteBatch,
                new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.97f);
            _pixelFont.DrawText(_spriteBatch, "CONTROLS",
                new Vector2(panelX + 205, panelY + 15), 4, Color.White);
            _pixelFont.DrawText(_spriteBatch, "ACTION",
                new Vector2(panelX + 22, panelY + 47), 1, Color.Gray);
            _pixelFont.DrawText(_spriteBatch, "PRIMARY       SECONDARY",
                new Vector2(panelX + 345, panelY + 47), 1, Color.Gray);

            InputAction[] actions = Enum.GetValues<InputAction>();
            for (int index = 0; index < actions.Length; index++)
            {
                int y = firstRowY + index * rowHeight;
                bool selected = index == _controlsSelection;
                if (selected)
                {
                    _uiDrawer.DrawFilledRectangle(_spriteBatch,
                        new Rectangle(panelX + 12, y - 5, panelWidth - 24, 20), Color.DarkSlateBlue);
                }

                InputAction action = actions[index];
                var keys = _bindings.GetKeys(action);
                string primary = keys.Count > 0 ? FriendlyKeyName(keys[0]) : "NONE";
                string secondary = keys.Count > 1
                    ? string.Join(" / ", keys.Skip(1).Select(FriendlyKeyName))
                    : "NONE";
                _pixelFont.DrawText(_spriteBatch, FriendlyActionName(action),
                    new Vector2(panelX + 22, y), 1, selected ? Color.White : Color.LightGray);
                _pixelFont.DrawText(_spriteBatch, primary,
                    new Vector2(panelX + 345, y), 1,
                    selected && _bindingSlot == 0 ? Color.Yellow : Color.LightGray);
                _pixelFont.DrawText(_spriteBatch, secondary,
                    new Vector2(panelX + 475, y), 1,
                    selected && _bindingSlot == 1 ? Color.Yellow : Color.LightGray);
            }

            int resetY = firstRowY + actions.Length * rowHeight;
            DrawControlCommandRow("RESET ALL", actions.Length, resetY, panelX, panelWidth);
            DrawControlCommandRow("BACK", actions.Length + 1, resetY + rowHeight, panelX, panelWidth);

            string message = string.IsNullOrEmpty(_controlsMessage)
                ? "TAB SLOT  ENTER CHANGE  BACKSPACE RESET  DELETE CLEAR SECONDARY"
                : _controlsMessage;
            _pixelFont.DrawText(_spriteBatch, message,
                new Vector2(panelX + 20, panelY + panelHeight - 20), 1, Color.LightGray);
        }

        private void DrawControlCommandRow(string label, int index, int y, int panelX, int panelWidth)
        {
            bool selected = index == _controlsSelection;
            if (selected)
            {
                _uiDrawer.DrawFilledRectangle(_spriteBatch,
                    new Rectangle(panelX + 12, y - 5, panelWidth - 24, 20), Color.DarkSlateBlue);
            }
            _pixelFont.DrawText(_spriteBatch, label,
                new Vector2(panelX + 22, y), 1, selected ? Color.White : Color.Gray);
        }

        private static string FriendlyActionName(InputAction action) => action switch
        {
            InputAction.SelectPreviousItem => "SELECT PREVIOUS ITEM",
            InputAction.SelectNextItem => "SELECT NEXT ITEM",
            InputAction.UsePotion => "USE POTION",
            InputAction.UseBandage => "USE BANDAGE",
            InputAction.ToggleDoor => "TOGGLE DOOR",
            InputAction.LayFalseTrail => "LAY FALSE TRAIL",
            InputAction.ThrowItem => "THROW ITEM",
            InputAction.PlaceTrap => "PLACE TRAP",
            InputAction.FireRanged => "FIRE RANGED",
            _ => string.Concat(action.ToString().Select((character, index) =>
                index > 0 && char.IsUpper(character) ? $" {character}" : character.ToString())).ToUpperInvariant()
        };

        private static string FriendlyKeyName(Keys key) => key switch
        {
            Keys.OemPeriod => ".",
            Keys.OemOpenBrackets => "[",
            Keys.OemCloseBrackets => "]",
            _ => key.ToString().ToUpperInvariant()
        };

        private void DrawEventLog()
        {
            if (_gameState.EventLog.Entries.Count == 0) return;

            int panelX = _map.RenderMode == RenderMode.Cells
                ? 5
                : MapViewport.VisibleColumns * MapViewport.TileSize + 8;
            int panelY = _map.RenderMode == RenderMode.Cells ? 5 : 374;
            int panelWidth = _map.RenderMode == RenderMode.Cells ? 285 : NativeWidth - panelX - 8;
            if (_map.RenderMode == RenderMode.Cells)
            {
                int panelHeight = 8 + _gameState.EventLog.Entries.Count * 12;
                _uiDrawer.DrawFilledRectangle(_spriteBatch,
                    new Rectangle(panelX, panelY, panelWidth, panelHeight), Color.Black * 0.75f);
            }

            for (int i = 0; i < _gameState.EventLog.Entries.Count; i++)
            {
                _pixelFont.DrawText(_spriteBatch, _gameState.EventLog.Entries[i],
                    new Vector2(panelX + (_map.RenderMode == RenderMode.Cells ? 5 : 0),
                        panelY + i * 18), 1,
                    i == _gameState.EventLog.Entries.Count - 1 ? Color.White : Color.Gray);
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
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(panelX, panelY, 305, 192), Color.Black * 0.9f);

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
            string trapDetails = placedTrap == null ? "NONE" : $"{placedTrap.Kind} DMG {placedTrap.Damage}";
            EnvironmentalEffect environment = _map.EnvironmentalEffects.Find(effect =>
                effect.X == position.X && effect.Y == position.Y);
            string environmentDetails = environment == null
                ? "NONE"
                : $"{environment.Type} {environment.RemainingTurns}";
            _pixelFont.DrawText(_spriteBatch, $"ITEM {itemName} TRAP {trapDetails} ENV {environmentDetails}",
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

            if (position.X == _player.X && position.Y == _player.Y)
            {
                _pixelFont.DrawText(_spriteBatch,
                    $"PLAYER HP {_player.Health}/{_player.MaxHealth} DMG {_player.Damage} DEF {_player.Defence}",
                    new Vector2(panelX + 6, panelY + 75), 1, Color.White);
                _pixelFont.DrawText(_spriteBatch,
                    $"WPN {_player.EquippedWeapon?.Name ?? "NONE"} ARM {_player.EquippedArmor?.Name ?? "NONE"}",
                    new Vector2(panelX + 6, panelY + 91), 1, Color.LightGray);
                _pixelFont.DrawText(_spriteBatch,
                    $"RNG {_player.EquippedRangedWeapon?.Name ?? "NONE"} INV {_player.Inventory.Items.Count}/{_player.Inventory.Capacity}",
                    new Vector2(panelX + 6, panelY + 107), 1, Color.LightGray);
                _pixelFont.DrawText(_spriteBatch,
                    $"FX {EffectSummary(_player.StatusEffects)} SPECIAL {(_player.HasSpecial ? "YES" : "NO")}",
                    new Vector2(panelX + 6, panelY + 123), 1, Color.Violet);
                return;
            }

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
            _pixelFont.DrawText(_spriteBatch, $"FX {EffectSummary(npc.StatusEffects)}",
                new Vector2(panelX + 6, panelY + 139), 1, Color.Violet);
            _pixelFont.DrawText(_spriteBatch,
                $"HAZ {npc.KnownHazards.Count} DEAD {npc.ObservedCasualtyCount} TD {profile.TrapDetectionRange} INIT {_gameState.NextNpcInitiativeOffset}",
                new Vector2(panelX + 6, panelY + 155), 1, Color.LightGoldenrodYellow);
            string ranged = npc.RangedProfile == null
                ? "NONE"
                : $"{npc.RangedProfile.MinimumRange}-{npc.RangedProfile.MaximumRange} DMG {npc.RangedProfile.Damage}";
            _pixelFont.DrawText(_spriteBatch, $"RANGED {ranged}",
                new Vector2(panelX + 6, panelY + 171), 1, Color.SandyBrown);
        }

        private void DrawEndScreen()
        {
            _uiDrawer.DrawFilledRectangle(_spriteBatch, new Rectangle(150, 205, 500, 170), Color.Black * 0.9f);

            string heading = _gameState.Outcome == GameOutcome.Won ? "YOU WIN" : "GAME OVER";
            Color headingColor = _gameState.Outcome == GameOutcome.Won ? Color.Yellow : Color.Red;
            int headingScale = 5;
            int headingX = (NativeWidth - _pixelFont.MeasureWidth(heading, headingScale)) / 2;
            _pixelFont.DrawText(_spriteBatch, heading, new Vector2(headingX, 240), headingScale, headingColor);

            const string restartText = "SPACE FOR NEW RUN";
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
