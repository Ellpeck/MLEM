using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class ShadowCode : Code {

        private readonly Color color;
        private readonly Vector2 offset;

        /// <inheritdoc />
        public ShadowCode(Match match, Regex regex, Color color, Vector2 offset) : base(match, regex) {
            this.color = color;
            this.offset = offset;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            font.DrawString(batch, cString, pos + this.offset * scale, this.color.CopyAlpha(color), 0, Vector2.Zero, scale, SpriteEffects.None, depth);
            // we return false since we still want regular drawing to occur
            return false;
        }

        /// <inheritdoc />
        public override bool EndsHere(Code other) {
            return other is ShadowCode || other is ResetFormattingCode;
        }

    }
}