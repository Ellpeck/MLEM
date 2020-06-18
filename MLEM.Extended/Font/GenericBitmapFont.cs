using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Font {
    /// <inheritdoc/>
    public class GenericBitmapFont : GenericFont {

        /// <summary>
        /// The <see cref="BitmapFont"/> that is being wrapped by this generic font
        /// </summary>
        public readonly BitmapFont Font;
        /// <inheritdoc/>
        public override GenericFont Bold { get; }
        /// <inheritdoc/>
        public override GenericFont Italic { get; }
        /// <inheritdoc/>
        public override float LineHeight => this.Font.LineHeight;

        /// <summary>
        /// Creates a new generic font using <see cref="BitmapFont"/>.
        /// Optionally, a bold and italic version of the font can be supplied.
        /// </summary>
        /// <param name="font">The font to wrap</param>
        /// <param name="bold">A bold version of the font</param>
        /// <param name="italic">An italic version of the font</param>
        public GenericBitmapFont(BitmapFont font, BitmapFont bold = null, BitmapFont italic = null) {
            this.Font = font;
            this.Bold = bold != null ? new GenericBitmapFont(bold) : this;
            this.Italic = italic != null ? new GenericBitmapFont(italic) : this;
        }

        /// <inheritdoc/>
        public override Vector2 MeasureString(string text) {
            if (text.Length == 1 && this.SingleCharacterWidthFix(text, out var size))
                return size;
            return this.Font.MeasureString(text);
        }

        /// <inheritdoc/>
        public override Vector2 MeasureString(StringBuilder text) {
            if (text.Length == 1 && this.SingleCharacterWidthFix(text.ToString(), out var size))
                return size;
            return this.Font.MeasureString(text);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc/>
        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc />
        public override bool HasCharacter(char c) {
            return this.Font.GetCharacterRegion(c) != null;
        }

        // this fixes an issue with BitmapFonts where, if only given a single character,
        // only the width of the character itself (disregarding spacing) is returned
        private bool SingleCharacterWidthFix(string text, out Vector2 size) {
            var codePoint = char.ConvertToUtf32(text, 0);
            var region = this.Font.GetCharacterRegion(codePoint);
            if (region != null) {
                size = new Vector2(region.XAdvance, region.Height);
                return true;
            }
            size = default;
            return false;
        }

    }
}