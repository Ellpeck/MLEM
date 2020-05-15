using System.Text.RegularExpressions;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    public class FontCode : Code {

        private readonly GenericFont font;

        public FontCode(Match match, GenericFont font) : base(match) {
            this.font = font;
        }

        public override GenericFont GetFont() {
            return this.font;
        }

        public override bool EndsHere(Code other) {
            return other is FontCode;
        }

    }
}