using System.Linq;
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
        /// <remarks>
        /// <see cref="DynamicSpriteFont"/> doesn't expose a text-independent line height (https://github.com/rds1983/FontStashSharp/blob/main/src/FontStashSharp/DynamicSpriteFont.cs#L130).
        /// Since <see cref="GenericFont"/> exposes <see cref="LineHeight"/>, there is somewhat of an incompatibility between the two.
        /// Because of this, <see cref="GenericStashFont"/> uses a heuristic to determine a text-independent line height based on the tallest character out of a set of predetermined characters (spaces, numbers and uppercase and lowercase A through Z).
        /// Because this heuristic is just that, and because it excludes non-latin characters, the desired line height can be specified using <paramref name="lineHeight"/>, overriding the default heuristic.
        /// </remarks>
        /// <param name="font">The font to wrap</param>
        /// <param name="bold">A bold version of the font</param>
        /// <param name="italic">An italic version of the font</param>
        /// <param name="lineHeight">The line height that should be used for <see cref="LineHeight"/> instead of the heuristic described in the remarks</param>
        public GenericStashFont(SpriteFontBase font, SpriteFontBase bold = null, SpriteFontBase italic = null, float? lineHeight = null) {
            this.Font = font;
            this.LineHeight = lineHeight ?? CalculateLineHeight(font);
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
                // use a heuristic to determine the text-independent line heights as described in the constructor remarks
                return new[] {' ', '\n', OneEmSpace, Zwsp, Nbsp}
                    .Concat(Enumerable.Range('a', 'z' - 'a' + 1).SelectMany(c => new[] {(char) c, char.ToUpper((char) c)}))
                    .Concat(Enumerable.Range('0', '9' - '0' + 1).Select(c => (char) c))
                    .Select(c => font.MeasureString(c.ToString()).Y).Max();
            }
        }

    }
}