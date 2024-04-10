using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class SubSupCode : Code {

        private readonly float offset;

        /// <inheritdoc />
        public SubSupCode(Match match, Regex regex, float offset) : base(match, regex) {
            this.offset = offset;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            charPosOffset.Y += this.offset * font.LineHeight * scale.Y;
            return false;
        }

    }
}
