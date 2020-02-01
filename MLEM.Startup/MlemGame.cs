using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui;
using MLEM.Ui.Style;

namespace MLEM.Startup {
    public class MlemGame : Game {

        private static MlemGame instance;
        public static InputHandler Input => instance.InputHandler;

        public readonly GraphicsDeviceManager GraphicsDeviceManager;
        public SpriteBatch SpriteBatch { get; protected set; }
        public InputHandler InputHandler { get; protected set; }
        public UiSystem UiSystem { get; protected set; }

        public GenericCallback OnLoadContent;
        public TimeCallback OnUpdate;
        public TimeCallback OnDraw;

        public MlemGame(int windowWidth = 1280, int windowHeight = 720, bool vsync = false, bool allowResizing = true, string contentDir = "Content") {
            instance = this;
            this.GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = windowWidth,
                PreferredBackBufferHeight = windowHeight,
                SynchronizeWithVerticalRetrace = vsync
            };
            this.Content.RootDirectory = contentDir;
            this.Window.AllowUserResizing = allowResizing;
        }

        protected override void LoadContent() {
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.InputHandler = new InputHandler();
            this.Components.Add(this.InputHandler);
            this.UiSystem = new UiSystem(this.Window, this.GraphicsDevice, new UntexturedStyle(this.SpriteBatch), this.InputHandler);
            this.Components.Add(this.UiSystem);
            this.OnLoadContent?.Invoke(this);
        }

        protected sealed override void Update(GameTime gameTime) {
            this.DoUpdate(gameTime);
            this.OnUpdate?.Invoke(this, gameTime);
            CoroutineHandler.Tick(gameTime.ElapsedGameTime.TotalSeconds);
            CoroutineHandler.RaiseEvent(CoroutineEvents.Update);
        }

        protected sealed override void Draw(GameTime gameTime) {
            this.UiSystem.DrawEarly(gameTime, this.SpriteBatch);
            this.DoDraw(gameTime);
            this.UiSystem.Draw(gameTime, this.SpriteBatch);
            this.OnDraw?.Invoke(this, gameTime);
            CoroutineHandler.RaiseEvent(CoroutineEvents.Draw);
        }

        protected virtual void DoDraw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        protected virtual void DoUpdate(GameTime gameTime) {
            base.Update(gameTime);
        }

        public static T LoadContent<T>(string name) {
            return instance.Content.Load<T>(name);
        }

        public delegate void GenericCallback(MlemGame game);

        public delegate void TimeCallback(MlemGame game, GameTime time);

    }
}