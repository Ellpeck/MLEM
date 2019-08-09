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
using MonoGame.Extended.TextureAtlases;

namespace Tests {
    public class GameImpl : MlemGame {

        private Texture2D testTexture;
        private NinePatch testPatch;
        private UiSystem uiSystem;

        protected override void LoadContent() {
            base.LoadContent();
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);

            // Ui system tests
            this.uiSystem = new UiSystem(this.Window, this.GraphicsDevice, 5, new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")));

            var root = new Panel(Anchor.BottomLeft, new Vector2(100, 100), new Point(5, 5), this.testPatch);
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, Point.Zero, "This is a test text that is hopefully long enough to cover at least a few lines, otherwise it would be very sad.", 0.2F));
            this.uiSystem.Add("Test", root);
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            // Input tests
            if (Input.IsKeyPressed(Keys.A))
                Console.WriteLine("A was pressed");
            if (Input.IsMouseButtonPressed(MouseButton.Left))
                Console.WriteLine("Left was pressed");
            if (Input.IsGamepadButtonPressed(0, Buttons.A))
                Console.WriteLine("Gamepad A was pressed");

            this.uiSystem.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
            this.GraphicsDevice.Clear(Color.Black);

            this.uiSystem.Draw(gameTime, this.SpriteBatch, samplerState: SamplerState.PointClamp);
        }

    }
}