using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Cameras;
using MLEM.Extended.Extensions;
using MLEM.Extended.Tiled;
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
        }

        protected override void LoadContent() {
            base.LoadContent();

            this.map = LoadContent<TiledMap>("Tiled/Map");
            this.mapRenderer = new IndividualTiledMapRenderer(this.map);

            this.camera = new Camera(this.GraphicsDevice) {
                AutoScaleWithScreen = true,
                Scale = 2,
                LookingPosition = new Vector2(25, 25) * this.map.GetTileSize()
            };

            var tex = LoadContent<Texture2D>("Textures/Test");
            this.UiSystem.Style = new UntexturedStyle(this.SpriteBatch) {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                TextScale = 0.1F,
                PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4)
            };
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            var root = new Panel(Anchor.Center, new Vector2(50, 50), Vector2.Zero);
            this.UiSystem.Add("Root", root);
            var group = root.AddChild(new CustomDrawGroup(Anchor.AutoLeft, new Vector2(1, 10)));
            group.AddChild(new Button(Anchor.AutoLeft, Vector2.One, "Test text"));
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, this.camera.ViewMatrix);
            this.mapRenderer.Draw(this.SpriteBatch, this.camera.GetVisibleRectangle());
            this.SpriteBatch.End();
            base.DoDraw(gameTime);
        }

    }
}