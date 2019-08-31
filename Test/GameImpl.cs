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
            this.UiSystem.GlobalScale = 15;
            this.UiSystem.AutoScaleWithScreen = true;

            var panel = new Panel(Anchor.Center, new Vector2(100, 50), Vector2.Zero, false, true, new Point(5, 10));
            this.UiSystem.Add("Panel", panel);
            panel.AddChild(new Paragraph(Anchor.AutoLeft, 1, "I am Android"));

            var image = new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(tex, 0, 0, 8, 8)) {IsHidden = true};
            panel.AddChild(image);
            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Toggle Image") {
                OnPressed = element => image.IsHidden = !image.IsHidden
            });

            panel.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10)) {PlaceholderText = "Tap to type"});

            panel.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Here is some text that makes it so that the panel is actually long enough for me to try out the scroll behavior."));
            panel.AddChild(new Slider(Anchor.AutoLeft, new Vector2(1, 10), 5, 100));

            panel.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris sapien elit, congue sit amet condimentum in, hendrerit iaculis leo. Phasellus mollis turpis felis, ac finibus elit tincidunt quis. Vestibulum maximus, velit non consequat porttitor, quam diam consequat eros, in cursus nunc mi id dui. Vivamus semper neque at feugiat semper. Nunc ultrices egestas placerat. Proin lectus felis, rutrum quis porta vel, eleifend eget eros. Morbi porttitor massa finibus felis vestibulum, quis faucibus dui volutpat. Nam enim mi, euismod a pharetra vel, suscipit eu tortor. Integer vehicula ligula at consectetur dictum. Etiam fringilla volutpat est, id egestas nunc. Maecenas turpis felis, eleifend non felis a, fringilla lobortis nibh. Morbi rhoncus vestibulum dignissim. Ut posuere nulla ipsum, non condimentum dui posuere sit amet."));
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(gameTime);
        }

    }
}