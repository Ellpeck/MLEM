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
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            // don't underline spaces at the end of lines
            if (codePoint == ' ' && token.DisplayString.Length > indexInToken + 1 && token.DisplayString[indexInToken + 1] == '\n')
                return false;
            var finalPos = font.TransformSingleCharacter(stringPos, charPosOffset + new Vector2(0, (this.yOffset - this.thickness) * charSize.Y), rotation, origin, scale, effects, stringSize, charSize);
            batch.Draw(batch.GetBlankTexture(), new RectangleF(finalPos.X, finalPos.Y, charSize.X * scale.X, this.thickness * charSize.Y * scale.Y), null, color, rotation, Vector2.Zero, SpriteEffects.None, depth);
            return false;
        }

    }
}
