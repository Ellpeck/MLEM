using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Input;

namespace MLEM.Startup {
    public class MlemGame : Game {

        private static MlemGame instance;
        public static KeyboardStateExtended Keyboard => instance.keyboardState;
        public static MouseStateExtended Mouse => instance.mouseState;

        public readonly GraphicsDeviceManager GraphicsDeviceManager;
        public SpriteBatch SpriteBatch { get; private set; }

        private KeyboardStateExtended keyboardState;
        private MouseStateExtended mouseState;

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
        }

        protected override void Initialize() {
            base.Initialize();
            this.OnWindowSizeChange(this.GraphicsDevice.Viewport);
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            CoroutineHandler.Tick(gameTime.GetElapsedSeconds());

            this.keyboardState = KeyboardExtended.GetState();
            this.mouseState = MouseExtended.GetState();
        }

        public static T LoadContent<T>(string name) {
            return instance.Content.Load<T>(name);
        }

    }
}