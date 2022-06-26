using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class UnderlineCode : Code {

        private readonly float thickness;
        private readonly float yOffset;

        /// <inheritdoc />
        public UnderlineCode(Match match, Regex regex, float thickness, float yOffset) : base(match, regex) {
            this.thickness = thickness;
            this.yOffset = yOffset;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            // don't underline spaces at the end of lines
            if (c == ' ' && token.DisplayString.Length > indexInToken + 1 && token.DisplayString[indexInToken + 1] == '\n')
                return false;
            var size = font.MeasureString(cString) * scale;
            var t = size.Y * this.thickness;
            batch.Draw(batch.GetBlankTexture(), new RectangleF(pos.X, pos.Y + this.yOffset * size.Y - t, size.X, t), color);
            return false;
        }

    }
}
