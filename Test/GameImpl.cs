using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace AndroidTests {
    public class GameImpl : MlemGame {

        protected override void LoadContent() {
            base.LoadContent();
            var tex = LoadContent<Texture2D>("Textures/Test");
            var style = new UntexturedStyle(this.SpriteBatch) {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                TextScale = 0.1F,
                PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4),
                TextFieldTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4),
                ScrollBarBackground = new NinePatch(new TextureRegion(tex, 12, 0, 4, 8), 1, 1, 2, 2),
                ScrollBarScrollerTexture = new NinePatch(new TextureRegion(tex, 8, 0, 4, 8), 1, 1, 2, 2),
                CheckboxTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4),
                CheckboxCheckmark = new TextureRegion(tex, 24, 0, 8, 8),
                RadioTexture = new NinePatch(new TextureRegion(tex, 16, 0, 8, 8), 3),
                RadioCheckmark = new TextureRegion(tex, 32, 0, 8, 8)
            };
            this.UiSystem.Style = style;
            this.UiSystem.GlobalScale = 10;
            this.UiSystem.AutoScaleWithScreen = true;

            var panel = new Panel(Anchor.Center, new Vector2(100, 0), Vector2.Zero, true);
            this.UiSystem.Add("Panel", panel);
            panel.AddChild(new Paragraph(Anchor.AutoLeft, 1, "I am Android"));

            var image = new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(tex, 0, 0, 8, 8)) {IsHidden = true};
            panel.AddChild(image);
            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Toggle Image") {
                OnPressed = element => image.IsHidden = !image.IsHidden
            });
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(gameTime);
        }

    }
}