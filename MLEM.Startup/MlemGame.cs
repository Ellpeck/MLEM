using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui;
using MLEM.Ui.Style;
using MonoGame.Extended;

namespace MLEM.Startup {
    public class MlemGame : Game {

        private static MlemGame instance;
        public static InputHandler Input => instance.InputHandler;

        public readonly GraphicsDeviceManager GraphicsDeviceManager;
        public SpriteBatch SpriteBatch { get; protected set; }
        public InputHandler InputHandler { get; protected set; }
        public UiSystem UiSystem { get; protected set; }

        public MlemGame(int windowWidth = 1280, int windowHeight = 720, bool vsync = false, bool allowResizing = true, string contentDir = "Content") {
            instance = this;
            this.GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = windowWidth,
                PreferredBackBufferHeight = windowHeight,
                SynchronizeWithVerticalRetrace = vsync
            };
            this.Content.RootDirectory = contentDir;

            this.Window.AllowUserResizing = allowResizing;
            this.Window.ClientSizeChanged += (win, args) => this.OnWindowSizeChange(this.GraphicsDevice.Viewport);
            this.Window.TextInput += (win, args) => this.OnTextInput(args.Key, args.Character);
        }

        public virtual void OnWindowSizeChange(Viewport viewport) {
        }

        public virtual void OnTextInput(Keys key, char character) {
        }

        protected override void LoadContent() {
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.InputHandler = new InputHandler();
            this.UiSystem = new UiSystem(this.Window, this.GraphicsDevice, new UntexturedStyle(this.SpriteBatch), this.InputHandler);
        }

        protected override void Initialize() {
            base.Initialize();
            this.OnWindowSizeChange(this.GraphicsDevice.Viewport);
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            this.InputHandler.Update();
            this.UiSystem.Update(gameTime);

            CoroutineHandler.Tick(gameTime.GetElapsedSeconds());
            CoroutineHandler.RaiseEvent(CoroutineEvents.Update);
        }

        protected override void Draw(GameTime gameTime) {
            this.UiSystem.DrawEarly(gameTime, this.SpriteBatch);
            this.DoDraw(gameTime);
            this.UiSystem.Draw(gameTime, this.SpriteBatch);
            CoroutineHandler.RaiseEvent(CoroutineEvents.Draw);
        }

        protected virtual void DoDraw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        public static T LoadContent<T>(string name) {
            return instance.Content.Load<T>(name);
        }

    }
}