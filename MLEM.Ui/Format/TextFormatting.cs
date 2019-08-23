using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace MLEM.Ui.Format {
    public static class TextFormatting {

        private static Regex formatRegex;

        public static readonly Dictionary<string, FormattingCode> FormattingCodes = new Dictionary<string, FormattingCode>();

        static TextFormatting() {
            SetFormatIndicators('[', ']');

            FormattingCodes["regular"] = new FormattingCode(TextStyle.Regular);
            FormattingCodes["italic"] = new FormattingCode(TextStyle.Italic);
            FormattingCodes["bold"] = new FormattingCode(TextStyle.Bold);

            var colors = typeof(Color).GetProperties();
            foreach (var color in colors) {
                if (color.GetGetMethod().IsStatic)
                    FormattingCodes[color.Name.ToLowerInvariant()] = new FormattingCode((Color) color.GetValue(null));
            }
        }

        public static void SetFormatIndicators(char opener, char closer) {
            // escape the opener and closer so that any character can be used
            var op = "\\" + opener;
            var cl = "\\" + closer;
            // find any text that is surrounded by the opener and closer
            formatRegex = new Regex($"{op}[^{op}{cl}]*{cl}");
        }

        public static string RemoveFormatting(this string s) {
            return formatRegex.Replace(s, string.Empty);
        }

        public static Dictionary<int, FormattingCode> GetFormattingCodes(this string s, bool indicesIgnoreCode = true) {
            var codes = new Dictionary<int, FormattingCode>();
            var codeLengths = 0;
            foreach (Match match in formatRegex.Matches(s)) {
                var rawCode = match.Value.Substring(1, match.Value.Length - 2).ToLowerInvariant();
                codes[match.Index - codeLengths] = FormattingCodes[rawCode];
                // if indices of formatting codes should ignore the codes themselves, then the lengths of all
                // of the codes we have sound so far needs to be subtracted from the found code's index
                if (indicesIgnoreCode)
                    codeLengths += match.Length;
            }
            return codes;
        }

    }
}