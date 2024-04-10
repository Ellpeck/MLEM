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
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            var finalPos = font.TransformSingleCharacter(stringPos, charPosOffset + this.offset, rotation, origin, scale, effects, stringSize, charSize);
            font.DrawString(batch, character, finalPos, this.color.CopyAlpha(color), rotation, Vector2.Zero, scale, effects, depth);
            // we return false since we still want regular drawing to occur
            return false;
        }

    }
}
