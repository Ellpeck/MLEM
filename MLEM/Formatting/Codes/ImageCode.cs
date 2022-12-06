using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class ImageCode : Code {

        private readonly SpriteAnimation image;
        private readonly bool copyTextColor;

        /// <inheritdoc />
        public ImageCode(Match match, Regex regex, SpriteAnimation image, bool copyTextColor) : base(match, regex) {
            this.image = image;
            this.copyTextColor = copyTextColor;
        }

        /// <inheritdoc />
        public override bool EndsHere(Code other) {
            return true;
        }

        /// <inheritdoc />
        public override float GetSelfWidth(GenericFont font) {
            return font.LineHeight;
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.image.Update(time);
        }

        /// <inheritdoc />
        public override void DrawSelf(GameTime time, SpriteBatch batch, Token token, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            var actualColor = this.copyTextColor ? color : Color.White.CopyAlpha(color);
            batch.Draw(this.image.CurrentRegion, new RectangleF(pos, new Vector2(font.LineHeight * scale)), actualColor);
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            // we don't want to draw the first (space) character (in case it is set to a missing character in FNA)
            return indexInToken == 0;
        }

    }

    /// <summary>
    /// A set of extensions that allow easily adding image formatting codes to a text formatter.
    /// </summary>
    public static class ImageCodeExtensions {

        /// <summary>
        /// Adds a new image formatting code to the given text formatter
        /// </summary>
        /// <param name="formatter">The formatter to add the code to</param>
        /// <param name="name">The name of the formatting code. The regex for this code will be between angle brackets.</param>
        /// <param name="image">The image to render at the code's position</param>
        /// <param name="copyTextColor">Whether or not the image code should use the text's color instead of White</param>
        public static void AddImage(this TextFormatter formatter, string name, TextureRegion image, bool copyTextColor = false) {
            formatter.AddImage(name, new SpriteAnimation(1, image), copyTextColor);
        }

        /// <inheritdoc cref="AddImage(MLEM.Formatting.TextFormatter,string,MLEM.Textures.TextureRegion,bool)"/>
        public static void AddImage(this TextFormatter formatter, string name, SpriteAnimation image, bool copyTextColor = false) {
            formatter.Codes.Add(new Regex($"<i {name}>"), (f, m, r) => new ImageCode(m, r, image, copyTextColor));
        }

    }
}
