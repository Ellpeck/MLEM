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
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            pos.Y += this.offset * font.LineHeight * scale;
            return false;
        }

    }
}
