using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Creates a new text formatter with a set of default formatting codes.
        /// </summary>
        public TextFormatter() {
            // font codes
            this.Codes.Add(new Regex("<b>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Bold));
            this.Codes.Add(new Regex("<i>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Italic));
            this.Codes.Add(new Regex(@"<s(?: #([0-9\w]{6,8}) (([+-.0-9]*)))?>"), (f, m, r) => new ShadowCode(m, r, m.Groups[1].Success ? ColorExtensions.FromHex(m.Groups[1].Value) : Color.Black, new Vector2(float.TryParse(m.Groups[2].Value, out var offset) ? offset : 2)));
            this.Codes.Add(new Regex("<u>"), (f, m, r) => new UnderlineCode(m, r, 1 / 16F, 0.85F));
            this.Codes.Add(new Regex("</(b|i|s|u|l)>"), (f, m, r) => new FontCode(m, r, null));

            // color codes
            foreach (var c in typeof(Color).GetProperties()) {
                if (c.GetGetMethod().IsStatic) {
                    var value = (Color) c.GetValue(null);
                    this.Codes.Add(new Regex($"<c {c.Name}>"), (f, m, r) => new ColorCode(m, r, value));
                }
            }
            this.Codes.Add(new Regex(@"<c #([0-9\w]{6,8})>"), (f, m, r) => new ColorCode(m, r, ColorExtensions.FromHex(m.Groups[1].Value)));
            this.Codes.Add(new Regex("</c>"), (f, m, r) => new ColorCode(m, r, null));

            // animation codes
            this.Codes.Add(new Regex(@"<a wobbly(?: ([+-.0-9]*) ([+-.0-9]*))?>"), (f, m, r) => new WobblyCode(m, r, float.TryParse(m.Groups[1].Value, out var mod) ? mod : 5, float.TryParse(m.Groups[2].Value, out var heightMod) ? heightMod : 1 / 8F));
            this.Codes.Add(new Regex("</a>"), (f, m, r) => new AnimatedCode(m, r));
        }

        /// <summary>
        /// Tokenizes a string, returning a tokenized string that is ready for splitting, measuring and drawing.
        /// </summary>
        /// <param name="font">The font to use for tokenization. Note that this font needs to be the same that will later be used for splitting, measuring and/or drawing.</param>
        /// <param name="s">The string to tokenize</param>
        /// <returns></returns>
        public TokenizedString Tokenize(GenericFont font, string s) {
            var tokens = new List<Token>();
            var codes = new List<Code>();
            var rawIndex = 0;
            while (rawIndex < s.Length) {
                var index = StripFormatting(font, s.Substring(0, rawIndex), tokens.SelectMany(t => t.AppliedCodes)).Length;
                var next = this.GetNextCode(s, rawIndex + 1);
                // if we've reached the end of the string
                if (next == null) {
                    var sub = s.Substring(rawIndex, s.Length - rawIndex);
                    tokens.Add(new Token(codes.ToArray(), index, rawIndex, StripFormatting(font, sub, codes), sub));
                    break;
                }

                // create a new token for the content up to the next code
                var ret = s.Substring(rawIndex, next.Match.Index - rawIndex);
                tokens.Add(new Token(codes.ToArray(), index, rawIndex, StripFormatting(font, ret, codes), ret));

                // move to the start of the next code
                rawIndex = next.Match.Index;

                // remove all codes that are incompatible with the next one and apply it
                codes.RemoveAll(c => c.EndsHere(next));
                codes.Add(next);
            }
            return new TokenizedString(font, s, StripFormatting(font, s, tokens.SelectMany(t => t.AppliedCodes)), tokens.ToArray());
        }

        private Code GetNextCode(string s, int index) {
            var (c, m, r) = this.Codes
                .Select(kv => (c: kv.Value, m: kv.Key.Match(s, index), r: kv.Key))
                .Where(kv => kv.m.Success)
                .OrderBy(kv => kv.m.Index)
                .FirstOrDefault();
            return c?.Invoke(this, m, r);
        }

        private static string StripFormatting(GenericFont font, string s, IEnumerable<Code> codes) {
            foreach (var code in codes)
                s = code.Regex.Replace(s, code.GetReplacementString(font));
            return s;
        }

    }
}