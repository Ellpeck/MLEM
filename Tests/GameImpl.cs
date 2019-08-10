using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extended.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Tests {
    public class GameImpl : MlemGame {

        private Texture2D testTexture;
        private NinePatch testPatch;

        public GameImpl() {
            this.IsMouseVisible = true;
        }

        protected override void LoadContent() {
            base.LoadContent();
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);

            Paragraph.DefaultFont = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont"));
            Paragraph.DefaultTextScale = 0.2F;
            Button.DefaultTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4);
            TextField.DefaultTexture = Button.DefaultTexture;
            this.UiSystem.GlobalScale = 5;

            var root = new Panel(Anchor.BottomLeft, new Vector2(100, 100), new Point(5, 5), this.testPatch);
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is a test text that is hopefully long enough to cover at least a few lines, otherwise it would be very sad."));
            var image = root.AddChild(new Image(Anchor.AutoCenter, new Vector2(20, 20), new TextureRegion(this.testTexture, 0, 0, 8, 8)) {IsHidden = true});
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 15), "Test Button") {
                OnClicked = (element, pos, button) => {
                    if (button == MouseButton.Left)
                        image.IsHidden = !image.IsHidden;
                }
            });
            root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 15)));
            this.UiSystem.Add("Test", root);
        }

        protected override void Draw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

    }
}