using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

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
            this.Font = SetDefaults(font);
            this.Bold = bold != null ? new GenericSpriteFont(SetDefaults(bold)) : this;
            this.Italic = italic != null ? new GenericSpriteFont(SetDefaults(italic)) : this;
        }

        /// <inheritdoc />
        protected override float MeasureChar(char c) {
            return this.Font.MeasureString(c.ToCachedString()).X;
        }

        /// <inheritdoc />
        protected override void DrawChar(SpriteBatch batch, char c, string cString, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, cString, position, color, rotation, origin, scale, effects, layerDepth);
        }

        private static SpriteFont SetDefaults(SpriteFont font) {
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
        }

    }
}