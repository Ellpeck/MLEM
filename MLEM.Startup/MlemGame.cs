using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Input;
using MLEM.Ui;
using MLEM.Ui.Style;

namespace MLEM.Startup {
    /// <summary>
    /// MlemGame is an extension of MonoGame's default <see cref="Game"/> class.
    /// It features the default template setup, as well as an <see cref="InputHandler"/>, a <see cref="UiSystem"/> and some additional callback events.
    /// It also runs all of the <see cref="CoroutineHandler"/> callbacks which can be used through <see cref="CoroutineEvents"/>.
    /// </summary>
    public class MlemGame : Game {

        private static MlemGame instance;
        /// <summary>
        /// The static game instance's input handler
        /// </summary>
        public static InputHandler Input => MlemGame.instance.InputHandler;

        /// <summary>
        /// This game's graphics device manager, initialized in the constructor
        /// </summary>
        public readonly GraphicsDeviceManager GraphicsDeviceManager;
        /// <summary>
        /// This game's sprite batch
        /// </summary>
        public SpriteBatch SpriteBatch { get; protected set; }
        /// <summary>
        /// This game's input handler. This can easily be accessed through <see cref="Input"/>.
        /// </summary>
        public InputHandler InputHandler { get; protected set; }
        /// <summary>
        /// This game's ui system
        /// </summary>
        public UiSystem UiSystem { get; protected set; }

        /// <summary>
        /// An event that is invoked in <see cref="LoadContent"/>.
        /// </summary>
        public event GenericCallback OnLoadContent;
        /// <summary>
        /// An event that is invoked in <see cref="Update"/>, before <see cref="DoUpdate"/> is called.
        /// </summary>
        public event TimeCallback PreUpdate;
        /// <summary>
        /// An event that is invoked in <see cref="Update"/>, after <see cref="DoUpdate"/> is called.
        /// </summary>
        public event TimeCallback OnUpdate;
        /// <summary>
        /// An event that is invoked in <see cref="Draw"/>, before <see cref="DoDraw"/> is called.
        /// </summary>
        public event TimeCallback PreDraw;
        /// <summary>
        /// An event that is invoked in <see cref="Draw"/>, after <see cref="DoDraw"/> is called.
        /// </summary>
        public event TimeCallback OnDraw;

        /// <summary>
        /// Creates a new MlemGame instance with some default settings
        /// </summary>
        /// <param name="windowWidth">The default window width</param>
        /// <param name="windowHeight">The default window height</param>
        public MlemGame(int windowWidth = 1280, int windowHeight = 720) {
            MlemGame.instance = this;

            this.GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = windowWidth,
                PreferredBackBufferHeight = windowHeight,
                HardwareModeSwitch = false
            };
            this.Window.AllowUserResizing = true;
            this.Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Override this to load graphical resources required by the game.
        /// </summary>
        protected override void LoadContent() {
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            this.InputHandler = new InputHandler(this);
            this.Components.Add(this.InputHandler);
            this.UiSystem = new UiSystem(this, this.InitializeDefaultUiStyle(this.SpriteBatch), this.InputHandler);
            this.Components.Add(this.UiSystem);
            this.OnLoadContent?.Invoke(this);
        }

        /// <summary>
        /// Called when the game should update.
        /// Updates the <see cref="T:Microsoft.Xna.Framework.GameComponent" /> instances attached to this game.
        /// Override <see cref="DoUpdate"/> to update your game.
        /// </summary>
        /// <param name="gameTime">The elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Update(Microsoft.Xna.Framework.GameTime)" />.</param>
        protected sealed override void Update(GameTime gameTime) {
            this.PreUpdate?.Invoke(this, gameTime);
            CoroutineHandler.RaiseEvent(CoroutineEvents.PreUpdate);

            this.DoUpdate(gameTime);

            this.OnUpdate?.Invoke(this, gameTime);
            CoroutineHandler.Tick(gameTime.ElapsedGameTime.TotalSeconds);
            CoroutineHandler.RaiseEvent(CoroutineEvents.Update);
        }

        /// <summary>
        /// Called when the game should draw a frame.
        /// Draws the <see cref="T:Microsoft.Xna.Framework.DrawableGameComponent" /> instances attached to this game.
        /// Override <see cref="DoDraw"/> to render your game.
        /// </summary>
        /// <param name="gameTime">A <see cref="T:Microsoft.Xna.Framework.GameTime" /> instance containing the elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.Game.Draw(Microsoft.Xna.Framework.GameTime)" /> and the total time elapsed since the game started.</param>
        protected sealed override void Draw(GameTime gameTime) {
            this.PreDraw?.Invoke(this, gameTime);
            CoroutineHandler.RaiseEvent(CoroutineEvents.PreDraw);

            #pragma warning disable CS0618
            this.UiSystem.DrawEarly(gameTime, this.SpriteBatch);
            #pragma warning restore CS0618
            this.DoDraw(gameTime);
            this.UiSystem.Draw(gameTime, this.SpriteBatch);

            this.OnDraw?.Invoke(this, gameTime);
            CoroutineHandler.RaiseEvent(CoroutineEvents.Draw);
        }

        /// <summary>
        /// This method is called in <see cref="Draw"/>.
        /// It is the version that should be overridden by implementors to draw game content.
        /// </summary>
        /// <param name="gameTime">The game's time</param>
        protected virtual void DoDraw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        /// <summary>
        /// This method is called in <see cref="Update"/>.
        /// It is the version that should be overridden by implementors to update game content.
        /// </summary>
        /// <param name="gameTime">The game's time</param>
        protected virtual void DoUpdate(GameTime gameTime) {
            base.Update(gameTime);
        }

        /// <summary>
        /// This method is called in <see cref="LoadContent"/> when the <see cref="UiSystem"/> is initialized.
        /// Override this method to easily modify or create a new default <see cref="UiStyle"/> for this game's <see cref="UiSystem"/>.
        /// </summary>
        /// <param name="batch">The sprite batch to use</param>
        /// <returns>The <see cref="UiStyle"/> to use for this game's <see cref="UiSystem"/>.</returns>
        protected virtual UiStyle InitializeDefaultUiStyle(SpriteBatch batch) {
            return new UntexturedStyle(batch);
        }

        /// <summary>
        /// Static helper method for <see cref="ContentManager.Load{T}"/>.
        /// This just invokes the game instance's load method.
        /// </summary>
        /// <param name="name">The name of the content file to load</param>
        /// <typeparam name="T">The type of content to load</typeparam>
        /// <returns>The loaded content</returns>
        public static T LoadContent<T>(string name) {
            return MlemGame.instance.Content.Load<T>(name);
        }

        /// <summary>
        /// A delegate method used by <see cref="MlemGame.OnLoadContent"/>.
        /// </summary>
        /// <param name="game">The game in question</param>
        public delegate void GenericCallback(MlemGame game);

        /// <summary>
        /// A delegate method used by <see cref="MlemGame.OnUpdate"/> and <see cref="MlemGame.OnDraw"/>.
        /// </summary>
        /// <param name="game">The game in question</param>
        /// <param name="time">The game's current time</param>
        public delegate void TimeCallback(MlemGame game, GameTime time);

    }
}
