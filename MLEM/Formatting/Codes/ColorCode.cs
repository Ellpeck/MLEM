using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace MLEM.Formatting.Codes {
    public class ColorCode : Code {

        private readonly Color? color;

        public ColorCode(Match match, Color? color) : base(match) {
            this.color = color;
        }

        public override Color? GetColor() {
            return this.color;
        }

    }
}