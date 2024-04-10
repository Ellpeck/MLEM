using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class OutlineCode : Code {

        private readonly Color color;
        private readonly float thickness;
        private readonly bool diagonals;

        /// <inheritdoc />
        public OutlineCode(Match match, Regex regex, Color color, float thickness, bool diagonals) : base(match, regex) {
            this.color = color;
            this.thickness = thickness;
            this.diagonals = diagonals;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            foreach (var dir in this.diagonals ? Direction2Helper.AllExceptNone : Direction2Helper.Adjacent) {
                var offset = Vector2.Normalize(dir.Offset().ToVector2()) * this.thickness;
                var finalPos = font.TransformSingleCharacter(stringPos, charPosOffset + offset, rotation, origin, scale, effects, stringSize, charSize);
                font.DrawCharacter(batch, codePoint, character, finalPos, this.color.CopyAlpha(color), rotation, scale, effects, depth);
            }
            return false;
        }

    }
}
