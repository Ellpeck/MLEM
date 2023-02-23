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
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            foreach (var dir in this.diagonals ? Direction2Helper.AllExceptNone : Direction2Helper.Adjacent) {
                var offset = Vector2.Normalize(dir.Offset().ToVector2()) * (this.thickness * scale);
                font.DrawString(batch, character, pos + offset, this.color.CopyAlpha(color), 0, Vector2.Zero, scale, SpriteEffects.None, depth);
            }
            return false;
        }

    }
}
