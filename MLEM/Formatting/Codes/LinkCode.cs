using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class LinkCode : UnderlineCode {

        private readonly Func<Token, bool> isSelected;
        private readonly Color? color;

        /// <inheritdoc />
        public LinkCode(Match match, Regex regex, float thickness, float yOffset, Func<Token, bool> isSelected, Color? color = null) : base(match, regex, thickness, yOffset) {
            this.isSelected = isSelected;
            this.color = color;
        }

        /// <summary>
        /// Returns true if this link formatting code is currently selected or hovered over, based on the selection function.
        /// </summary>
        /// <returns>True if this code is currently selected</returns>
        public virtual bool IsSelected() {
            foreach (var token in this.Tokens) {
                if (this.isSelected(token))
                    return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override Color? GetColor(Color defaultPick) {
            return this.color;
        }

        /// <inheritdoc />
        public override bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, Token token, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            // since we inherit from UnderlineCode, we can just call base if selected
            return this.IsSelected() && base.DrawCharacter(time, batch, c, cString, token, indexInToken, ref pos, font, ref color, ref scale, depth);
        }

    }
}