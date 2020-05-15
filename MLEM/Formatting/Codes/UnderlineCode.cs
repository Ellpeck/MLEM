using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    public class UnderlineCode : FontCode {

        private readonly float thickness;
        private readonly float yOffset;

        public UnderlineCode(Match match, Regex regex, float thickness, float yOffset) : base(match, regex, null) {
            this.thickness = thickness;
            this.yOffset = yOffset;
        }

        public override bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            // don't underline spaces at the end of lines
            if (c == ' ' && this.Token.Substring.Length > indexInToken + 1 && this.Token.Substring[indexInToken + 1] == '\n')
                return false;
            var size = font.MeasureString(cString) * scale;
            var thicc = size.Y * this.thickness;
            batch.Draw(batch.GetBlankTexture(), new RectangleF(pos.X, pos.Y + this.yOffset * size.Y - thicc, size.X, thicc), color);
            return false;
        }

    }
}