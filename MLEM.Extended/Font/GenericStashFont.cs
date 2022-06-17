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
        public override float LineHeight => this.Font.LineHeight;

        /// <summary>
        /// Creates a new generic font using <see cref="SpriteFontBase"/>.
        /// Optionally, a bold and italic version of the font can be supplied.
        /// </summary>
        /// <param name="font">The font to wrap</param>
        /// <param name="bold">A bold version of the font</param>
        /// <param name="italic">An italic version of the font</param>
        public GenericStashFont(SpriteFontBase font, SpriteFontBase bold = null, SpriteFontBase italic = null) {
            this.Font = font;
            this.Bold = bold != null ? new GenericStashFont(bold) : this;
            this.Italic = italic != null ? new GenericStashFont(italic) : this;
        }

        /// <inheritdoc />
        protected override float MeasureChar(char c) {
            return this.Font.MeasureString(c.ToCachedString()).X;
        }

        /// <inheritdoc />
        protected override void DrawChar(SpriteBatch batch, string cString, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth) {
            this.Font.DrawText(batch, cString, position, color, scale, rotation, Vector2.Zero, layerDepth);
        }

    }
}
