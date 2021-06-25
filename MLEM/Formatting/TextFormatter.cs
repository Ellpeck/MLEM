using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting.Codes;
using MLEM.Misc;

namespace MLEM.Formatting {
    /// <summary>
    /// A text formatter is used for drawing text using <see cref="GenericFont"/> that contains different colors, bold/italic sections and animations.
    /// To format a string of text, use the codes as specified in the constructor. To tokenize and render a formatted string, use <see cref="Tokenize"/>.
    /// </summary>
    public class TextFormatter : GenericDataHolder {

        /// <summary>
        /// The formatting codes that this text formatter uses.
        /// The <see cref="Regex"/> defines how the formatting code should be matched.
        /// </summary>
        public readonly Dictionary<Regex, Code.Constructor> Codes = new Dictionary<Regex, Code.Constructor>();
        /// <summary>
        /// The macros that this text formatter uses.
        /// A macro is a <see cref="Regex"/> that turns a snippet of text into another snippet of text.
        /// Macros can resolve recursively and can resolve into formatting codes.
        /// </summary>
        public readonly Dictionary<Regex, Macro> Macros = new Dictionary<Regex, Macro>();

        /// <summary>
        /// Creates a new text formatter with a set of default formatting codes.
        /// </summary>
        public TextFormatter() {
            // font codes
            this.Codes.Add(new Regex("<b>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Bold));
            this.Codes.Add(new Regex("<i>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Italic));
            this.Codes.Add(new Regex(@"<s(?: #([0-9\w]{6,8}) (([+-.0-9]*)))?>"), (f, m, r) => new ShadowCode(m, r,
                m.Groups[1].Success ? ColorHelper.FromHexString(m.Groups[1].Value) : Color.Black,
                new Vector2(float.TryParse(m.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offset) ? offset : 2)));
            this.Codes.Add(new Regex("<u>"), (f, m, r) => new UnderlineCode(m, r, 1 / 16F, 0.85F));
            this.Codes.Add(new Regex("</(s|u|l)>"), (f, m, r) => new ResetFormattingCode(m, r));
            this.Codes.Add(new Regex("</(b|i)>"), (f, m, r) => new FontCode(m, r, null));

            // color codes
            foreach (var c in typeof(Color).GetProperties()) {
                if (c.GetGetMethod().IsStatic) {
                    var value = (Color) c.GetValue(null);
                    this.Codes.Add(new Regex($"<c {c.Name}>"), (f, m, r) => new ColorCode(m, r, value));
                }
            }
            this.Codes.Add(new Regex(@"<c #([0-9\w]{6,8})>"), (f, m, r) => new ColorCode(m, r, ColorHelper.FromHexString(m.Groups[1].Value)));
            this.Codes.Add(new Regex("</c>"), (f, m, r) => new ColorCode(m, r, null));

            // animation codes
            this.Codes.Add(new Regex(@"<a wobbly(?: ([+-.0-9]*) ([+-.0-9]*))?>"), (f, m, r) => new WobblyCode(m, r,
                float.TryParse(m.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mod) ? mod : 5,
                float.TryParse(m.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var heightMod) ? heightMod : 1 / 8F));
            this.Codes.Add(new Regex("</a>"), (f, m, r) => new AnimatedCode(m, r));

            // macros
            this.Macros.Add(new Regex("~"), (f, m, r) => GenericFont.Nbsp.ToCachedString());
            this.Macros.Add(new Regex("<n>"), (f, m, r) => '\n'.ToCachedString());
        }

        /// <summary>
        /// Tokenizes a string, returning a tokenized string that is ready for splitting, measuring and drawing.
        /// </summary>
        /// <param name="font">The font to use for tokenization. Note that this font needs to be the same that will later be used for splitting, measuring and/or drawing.</param>
        /// <param name="s">The string to tokenize</param>
        /// <param name="alignment">The text alignment that should be used. Note that this alignment needs to be the same that will later be used for splitting, measuring and/or drawing.</param>
        /// <returns></returns>
        public TokenizedString Tokenize(GenericFont font, string s, TextAlignment alignment = TextAlignment.Left) {
            // resolve macros
            s = this.ResolveMacros(s);
            var tokens = new List<Token>();
            var codes = new List<Code>();
            // add the formatting code right at the start of the string
            var firstCode = this.GetNextCode(s, 0, 0);
            if (firstCode != null)
                codes.Add(firstCode);
            var index = 0;
            var rawIndex = 0;
            while (rawIndex < s.Length) {
                var next = this.GetNextCode(s, rawIndex + 1);
                // if we've reached the end of the string
                if (next == null) {
                    var sub = s.Substring(rawIndex, s.Length - rawIndex);
                    tokens.Add(new Token(codes.ToArray(), index, rawIndex, StripFormatting(font, sub, codes), sub));
                    break;
                }

                // create a new token for the content up to the next code
                var ret = s.Substring(rawIndex, next.Match.Index - rawIndex);
                var strippedRet = StripFormatting(font, ret, codes);
                tokens.Add(new Token(codes.ToArray(), index, rawIndex, strippedRet, ret));

                // move to the start of the next code
                rawIndex = next.Match.Index;
                index += strippedRet.Length;

                // remove all codes that are incompatible with the next one and apply it
                codes.RemoveAll(c => c.EndsHere(next));
                codes.Add(next);
            }
            return new TokenizedString(font, alignment, s, StripFormatting(font, s, tokens.SelectMany(t => t.AppliedCodes)), tokens.ToArray());
        }

        /// <summary>
        /// Resolves the macros in the given string recursively, until no more macros can be resolved.
        /// This method is used by <see cref="Tokenize"/>, meaning that it does not explicitly have to be called when using text formatting.
        /// </summary>
        /// <param name="s">The string to resolve macros for</param>
        /// <returns>The final, recursively resolved string</returns>
        public string ResolveMacros(string s) {
            // resolve macros that resolve into macros
            bool matched;
            do {
                matched = false;
                foreach (var macro in this.Macros) {
                    s = macro.Key.Replace(s, m => {
                        // if the match evaluator was queried, then we know we matched something
                        matched = true;
                        return macro.Value(this, m, macro.Key);
                    });
                }
            } while (matched);
            return s;
        }

        private Code GetNextCode(string s, int index, int maxIndex = int.MaxValue) {
            var (c, m, r) = this.Codes
                .Select(kv => (c: kv.Value, m: kv.Key.Match(s, index), r: kv.Key))
                .Where(kv => kv.m.Success && kv.m.Index <= maxIndex)
                .OrderBy(kv => kv.m.Index)
                .FirstOrDefault();
            return c?.Invoke(this, m, r);
        }

        private static string StripFormatting(GenericFont font, string s, IEnumerable<Code> codes) {
            foreach (var code in codes)
                s = code.Regex.Replace(s, code.GetReplacementString(font));
            return s;
        }

        /// <summary>
        /// Represents a text formatting macro. Used by <see cref="TextFormatter.Macros"/>.
        /// </summary>
        /// <param name="formatter">The text formatter that created this macro</param>
        /// <param name="match">The match for the macro's regex</param>
        /// <param name="regex">The regex used to create this macro</param>
        public delegate string Macro(TextFormatter formatter, Match match, Regex regex);

    }
}