using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class ColorCode : Code {

        private readonly Color? color;

        /// <inheritdoc />
        public ColorCode(Match match, Regex regex, Color? color) : base(match, regex) {
            this.color = color;
        }

        /// <inheritdoc />
        public override Color? GetColor(Color defaultPick) {
            return this.color;
        }

    }
}
