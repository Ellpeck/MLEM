using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !FNA
using System.Linq;
#endif

namespace MLEM.Font {
    /// <inheritdoc/>
    public class GenericSpriteFont : GenericFont {

        /// <summary>
        /// The <see cref="SpriteFont"/> that is being wrapped by this generic font
        /// </summary>
        public readonly SpriteFont Font;
        /// <inheritdoc/>
        public override GenericFont Bold { get; }
        /// <inheritdoc/>
        public override GenericFont Italic { get; }
        /// <inheritdoc/>
        public override float LineHeight => this.Font.LineSpacing;

        /// <summary>
        /// Creates a new generic font using <see cref="SpriteFont"/>.
        /// Optionally, a bold and italic version of the font can be supplied.
        /// </summary>
        /// <param name="font">The font to wrap</param>
        /// <param name="bold">A bold version of the font</param>
        /// <param name="italic">An italic version of the font</param>
        public GenericSpriteFont(SpriteFont font, SpriteFont bold = null, SpriteFont italic = null) {
            this.Font = GenericSpriteFont.SetDefaults(font);
            this.Bold = bold != null ? new GenericSpriteFont(GenericSpriteFont.SetDefaults(bold)) : this;
            this.Italic = italic != null ? new GenericSpriteFont(GenericSpriteFont.SetDefaults(italic)) : this;
        }

        /// <inheritdoc />
        protected override float MeasureCharacter(int codePoint) {
            return this.Font.MeasureString(char.ConvertFromUtf32(codePoint)).X;
        }

        /// <inheritdoc />
        protected override void DrawCharacter(SpriteBatch batch, int codePoint, string character, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, character, position, color, rotation, Vector2.Zero, scale, effects, layerDepth);
        }

        private static SpriteFont SetDefaults(SpriteFont font) {
            #if FNA
            // none of the copying is available with FNA
            return font;
            #else
            // we copy the font here to set the default character to a space
            return new SpriteFont(
                font.Texture,
                font.Glyphs.Select(g => g.BoundsInTexture).ToList(),
                font.Glyphs.Select(g => g.Cropping).ToList(),
                font.Characters.ToList(),
                font.LineSpacing,
                font.Spacing,
                font.Glyphs.Select(g => new Vector3(g.LeftSideBearing, g.Width, g.RightSideBearing)).ToList(),
                ' ');
            #endif
        }

    }
}
