using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extended.Extensions;
using MLEM.Input;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MonoGame.Extended.TextureAtlases;

namespace Tests {
    public class GameImpl : MlemGame {

        private Texture2D testTexture;
        private TextureRegion testRegion;
        private NinePatch testPatch;
        private NinePatchRegion2D extendedPatch;

        private UiSystem uiSystem;
        private Element testChild;

        protected override void LoadContent() {
            base.LoadContent();
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testRegion = new TextureRegion(this.testTexture, 32, 0, 8, 8);
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);
            this.extendedPatch = this.testPatch.ToExtended();

            // Ui system tests
            this.uiSystem = new UiSystem(this.Window, this.GraphicsDevice, 5);

            var root = new Element(Anchor.BottomLeft, new Point(100, 100), new Point(5, 5));
            for (var i = 0; i < 3; i++)
                root.AddChild(new Element(Anchor.AutoInline, new Point(16, 16), Point.Zero) {
                    Padding = new Point(1, 1)
                });
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

            // Texture region tests
            this.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            this.SpriteBatch.Draw(this.testRegion, new Vector2(10, 10), Color.White);
            this.SpriteBatch.Draw(this.testRegion, new Vector2(30, 10), Color.White, 0, Vector2.Zero, 10, SpriteEffects.None, 0);
            this.SpriteBatch.End();
            this.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(10));
            this.SpriteBatch.Draw(this.testPatch, new Rectangle(20, 20, 40, 20), Color.White);
            this.SpriteBatch.Draw(this.extendedPatch, new Rectangle(80, 20, 40, 20), Color.White);
            this.SpriteBatch.End();

            this.uiSystem.Draw(gameTime, this.SpriteBatch);
        }

    }
}