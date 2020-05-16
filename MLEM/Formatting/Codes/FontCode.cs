using System;
using System.Text.RegularExpressions;
using MLEM.Font;

namespace MLEM.Formatting.Codes {
    public class FontCode : Code {

        private readonly Func<GenericFont, GenericFont> font;

        public FontCode(Match match, Regex regex, Func<GenericFont, GenericFont> font) : base(match, regex) {
            this.font = font;
        }

        public override GenericFont GetFont(GenericFont defaultPick) {
            return this.font?.Invoke(defaultPick);
        }

        public override bool EndsHere(Code other) {
            return other is FontCode;
        }

    }
}