using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class WobblyCode : AnimatedCode {

        private readonly float modifier;
        private readonly float heightModifier;
        /// <summary>
        /// The time that this wobbly animation has been running for.
        /// To reset its animation progress, reset this value.
        /// </summary>
        public TimeSpan TimeIntoAnimation;

        /// <inheritdoc />
        public WobblyCode(Match match, Regex regex, float modifier, float heightModifier) : base(match, regex) {
            this.modifier = modifier;
            this.heightModifier = heightModifier;
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.TimeIntoAnimation += time.ElapsedGameTime;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            charPosOffset += new Vector2(0, (float) Math.Sin(token.Index + indexInToken + this.TimeIntoAnimation.TotalSeconds * this.modifier) * font.LineHeight * this.heightModifier * scale.Y);
            // we return false since we still want regular drawing to occur, we just changed the position
            return false;
        }

    }
}
