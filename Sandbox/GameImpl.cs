using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace Sandbox {
    public class GameImpl : MlemGame {

        public GameImpl() {
            this.IsMouseVisible = true;
        }

        protected override void LoadContent() {
            base.LoadContent();

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

            var panel = new Panel(Anchor.Center, new Vector2(0, 100), Vector2.Zero) {SetWidthBasedOnChildren = true};
            panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(100, 10)));
            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(80, 10)));
            this.UiSystem.Add("Panel", panel);
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            base.DoDraw(gameTime);
        }

    }
}