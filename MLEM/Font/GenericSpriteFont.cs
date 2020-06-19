using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        protected override Vector2 CalcCharSize(char c) {
            return this.Font.MeasureString(c.ToString());
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

        private static SpriteFont SetDefaults(SpriteFont font) {
            // so that missing character behavior is in line with MG.Extended's
            // bitmap fonts, we draw nothing as the default character
            font.DefaultCharacter = ' ';
            return font;
        }

    }
}