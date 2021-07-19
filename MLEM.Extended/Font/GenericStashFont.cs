using System.Text;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;

namespace MLEM.Extended.Font {
    /// <inheritdoc/>
    public class GenericStashFont : GenericFont {

        /// <summary>
        /// The <see cref="SpriteFontBase"/> that is being wrapped by this generic font
        /// </summary>
        public readonly SpriteFontBase Font;
        /// <inheritdoc />
        public override GenericFont Bold { get; }
        /// <inheritdoc />
        public override GenericFont Italic { get; }
        /// <inheritdoc />
        public override float LineHeight { get; }

        /// <summary>
        /// Creates a new generic font using <see cref="SpriteFontBase"/>.
        /// Optionally, a bold and italic version of the font can be supplied.
        /// </summary>
        /// <param name="font">The font to wrap</param>
        /// <param name="bold">A bold version of the font</param>
        /// <param name="italic">An italic version of the font</param>
        public GenericStashFont(SpriteFontBase font, SpriteFontBase bold = null, SpriteFontBase italic = null) {
            this.Font = font;
            this.LineHeight = CalculateLineHeight(font);
            this.Bold = bold != null ? new GenericStashFont(bold) : this;
            this.Italic = italic != null ? new GenericStashFont(italic) : this;
        }

        /// <inheritdoc />
        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            this.Font.DrawText(batch, text, position, color, scale, rotation, origin, layerDepth);
        }

        /// <inheritdoc />
        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            this.Font.DrawText(batch, text, position, color, scale, rotation, origin, layerDepth);
        }

        /// <inheritdoc />
        protected override Vector2 MeasureChar(char c) {
            return this.Font.MeasureString(c.ToCachedString());
        }

        private static float CalculateLineHeight(SpriteFontBase font) {
            if (font is StaticSpriteFont s) {
                // this is the same calculation used internally by StaticSpriteFont
                return s.FontSize + s.LineSpacing;
            } else {
                // Y (min y) just stores the glyph's Y offset, whereas Y2 (max y) stores the glyph's height
                // since we technically want line spacing rather than line height, we calculate it like this
                var bounds = new Bounds();
                font.TextBounds(" ", Vector2.Zero, ref bounds);
                return bounds.Y2 + (bounds.Y2 - bounds.Y);
            }
        }

    }
}