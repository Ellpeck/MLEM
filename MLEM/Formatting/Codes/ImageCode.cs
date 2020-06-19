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

        /// <inheritdoc />
        public ImageCode(Match match, Regex regex, SpriteAnimation image) : base(match, regex) {
            this.image = image;
        }

        /// <inheritdoc />
        public override bool EndsHere(Code other) {
            return true;
        }

        /// <inheritdoc />
        public override string GetReplacementString(GenericFont font) {
            return GenericFont.OneEmSpace.ToString();
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.image.Update(time);
        }

        /// <inheritdoc />
        public override void DrawSelf(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            batch.Draw(this.image.CurrentRegion, new RectangleF(pos, new Vector2(font.LineHeight * scale)), Color.White.CopyAlpha(color));
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
        public static void AddImage(this TextFormatter formatter, string name, TextureRegion image) {
            formatter.AddImage(name, new SpriteAnimation(1, image));
        }

        /// <inheritdoc cref="AddImage(MLEM.Formatting.TextFormatter,string,MLEM.Textures.TextureRegion)"/>
        public static void AddImage(this TextFormatter formatter, string name, SpriteAnimation image) {
            formatter.Codes.Add(new Regex($"<i {name}>"), (f, m, r) => new ImageCode(m, r, image));
        }

    }
}