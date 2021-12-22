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

        /// <inheritdoc />
        protected override float MeasureChar(char c) {
            var region = this.Font.GetCharacterRegion(c);
            return region != null ? new Vector2(region.XAdvance, region.Height).X : 0;
        }

        /// <inheritdoc />
        protected override void DrawChar(SpriteBatch batch, char c, string cString, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, cString, position, color, rotation, origin, scale, effects, layerDepth);
        }

    }
}