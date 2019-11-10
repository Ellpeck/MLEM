using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Cameras;
using MLEM.Extended.Extensions;
using MLEM.Extended.Tiled;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MonoGame.Extended.Tiled;

namespace Sandbox {
    public class GameImpl : MlemGame {

        private Camera camera;
        private TiledMap map;
        private IndividualTiledMapRenderer mapRenderer;

        public GameImpl() {
            this.IsMouseVisible = true;
            this.Window.ClientSizeChanged += (o, args) => {
                Console.WriteLine("Size changed");
            };
        }

        protected override void LoadContent() {
            base.LoadContent();

            this.map = LoadContent<TiledMap>("Tiled/Map");
            this.mapRenderer = new IndividualTiledMapRenderer(this.map);

            this.camera = new Camera(this.GraphicsDevice) {
                AutoScaleWithScreen = true,
                Scale = 2,
                LookingPosition = new Vector2(25, 25) * this.map.GetTileSize(),
                MinScale = 0.25F,
                MaxScale = 4
            };

            /*var tex = LoadContent<Texture2D>("Textures/Test");
            this.UiSystem.Style = new UntexturedStyle(this.SpriteBatch) {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                TextScale = 0.1F,
                PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4)
            };
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            var panel = new Panel(Anchor.Center, new Vector2(0, 100), Vector2.Zero) {SetWidthBasedOnChildren = true};
            panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(100, 10)));
            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(80, 10)));
            this.UiSystem.Add("Panel", panel);*/
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
            if (this.InputHandler.IsKeyPressed(Keys.F11))
                this.GraphicsDeviceManager.SetFullscreen(!this.GraphicsDeviceManager.IsFullScreen);

            var delta = this.InputHandler.ScrollWheel - this.InputHandler.LastScrollWheel;
            if (delta != 0) {
                this.camera.Zoom(0.1F * Math.Sign(delta), this.InputHandler.MousePosition.ToVector2());
            }
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, this.camera.ViewMatrix);
            this.mapRenderer.Draw(this.SpriteBatch, this.camera.GetVisibleRectangle().ToExtended());
            this.SpriteBatch.End();
            base.DoDraw(gameTime);
        }

    }
}