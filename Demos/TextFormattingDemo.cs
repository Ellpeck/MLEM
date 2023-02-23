using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Textures;

namespace Demos {
    public class TextFormattingDemo : Demo {

        private const string Text =
            "MLEM's text formatting system allows for various <b>formatting codes</b> to be applied in the middle of a string. Here's a demonstration of some of them.\n\n" +
            "You can write in <b>bold</i>, <i>italics</i>, <u>with an underline</u>, <st>strikethrough</st>, with a <s #000000 4>drop shadow</s> whose <s #ff0000 4>color</s> and <s #000000 10>offset</s> you can modify in each application of the code, or with various types of <b>combined <c Pink>formatting</c> codes</b>.\n\n" +
            "You can apply <c CornflowerBlue>custom</c> <c Yellow>colors</c> to text, including all default <c Orange>MonoGame colors</c> and <c #aabb00>inline custom colors</c>.\n\n" +
            "You can also use animations like <a wobbly>a wobbly one</a>, as well as create custom ones using the <a wobbly>Code class</a>.\n\n" +
            "You can also display <i grass> icons in your text, and use super<sup>script</sup> or sub<sub>script</sub> formatting!\n\n" +
            "Additionally, the text formatter has various methods for interacting with the text, like custom behaviors when hovering over certain parts, and more.";
        private const float DefaultScale = 0.5F;
        private const float WidthMultiplier = 0.9F;

        private TextFormatter formatter;
        private TokenizedString tokenizedText;
        private GenericFont font;
        private bool drawBounds;
        private float scale = TextFormattingDemo.DefaultScale;

        public TextFormattingDemo(MlemGame game) : base(game) {}

        public override void LoadContent() {
            this.Game.Window.ClientSizeChanged += this.OnResize;

            // creating a new text formatter as well as a generic font to draw with
            this.formatter = new TextFormatter();
            // GenericFont and its subtypes are wrappers around various font classes, including SpriteFont, MonoGame.Extended's BitmapFont and FontStashSharp
            // supplying a bold and italic version of the font here allows for the bold and italic formatting codes to be used
            this.font = new GenericSpriteFont(
                Demo.LoadContent<SpriteFont>("Fonts/Roboto"),
                Demo.LoadContent<SpriteFont>("Fonts/RobotoBold"),
                Demo.LoadContent<SpriteFont>("Fonts/RobotoItalic"));

            // adding the image code used in the example to it
            var testTexture = Demo.LoadContent<Texture2D>("Textures/Test");
            this.formatter.AddImage("grass", new TextureRegion(testTexture, 0, 0, 8, 8));

            // tokenizing our text and splitting it to fit the screen
            // we specify our text alignment here too, so that all data is cached correctly for display
            this.tokenizedText = this.formatter.Tokenize(this.font, TextFormattingDemo.Text, TextAlignment.Center);
            this.tokenizedText.Split(this.font, this.GraphicsDevice.Viewport.Width * TextFormattingDemo.WidthMultiplier, this.scale, TextAlignment.Center);
        }

        public override void DoDraw(GameTime time) {
            this.GraphicsDevice.Clear(Color.DarkSlateGray);
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            // we draw the tokenized text in the center of the screen
            // since the text is already center-aligned, we only need to align it on the y axis here
            var size = this.tokenizedText.GetArea(Vector2.Zero, this.scale).Size;
            var pos = new Vector2(this.GraphicsDevice.Viewport.Width / 2, (this.GraphicsDevice.Viewport.Height - size.Y) / 2);

            // draw bounds, which can be toggled with B in this demo
            if (this.drawBounds) {
                var blank = this.SpriteBatch.GetBlankTexture();
                this.SpriteBatch.Draw(blank, new RectangleF(pos - new Vector2(size.X / 2, 0), size), Color.Red * 0.25F);
                foreach (var token in this.tokenizedText.Tokens) {
                    foreach (var area in token.GetArea(pos, this.scale))
                        this.SpriteBatch.Draw(blank, area, Color.Black * 0.25F);
                }
            }

            // draw the text itself
            this.tokenizedText.Draw(time, this.SpriteBatch, pos, this.font, Color.White, this.scale, 0);

            this.SpriteBatch.End();
        }

        public override void Update(GameTime time) {
            // update our tokenized string to animate the animation codes
            this.tokenizedText.Update(time);
            if (this.InputHandler.IsPressed(Keys.B))
                this.drawBounds = !this.drawBounds;
        }

        public override void Clear() {
            base.Clear();
            this.Game.Window.ClientSizeChanged -= this.OnResize;
        }

        private void OnResize(object sender, EventArgs e) {
            // scale our text based on window size
            var viewport = new Rectangle(0, 0, this.Game.Window.ClientBounds.Width,  this.Game.Window.ClientBounds.Height);
            this.scale = TextFormattingDemo.DefaultScale * Math.Min(viewport.Width / 1280F, viewport.Height / 720F);

            // re-split our text if the window resizes, since it depends on the window size
            // this doesn't require re-tokenization of the text, since TokenizedString also stores the un-split version
            this.tokenizedText.Split(this.font, this.GraphicsDevice.Viewport.Width * TextFormattingDemo.WidthMultiplier, this.scale, TextAlignment.Center);
        }

    }
}
