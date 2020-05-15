using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace MLEM.Formatting.Codes {
    public class ColorCode : Code {

        private readonly Color? color;

        public ColorCode(Match match, Regex regex, Color? color) : base(match, regex) {
            this.color = color;
        }

        public override Color? GetColor() {
            return this.color;
        }

    }
}