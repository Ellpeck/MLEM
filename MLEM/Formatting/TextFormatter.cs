using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting.Codes;

namespace MLEM.Formatting {
    public class TextFormatter {

        public readonly Dictionary<Regex, Code.Constructor> Codes = new Dictionary<Regex, Code.Constructor>();

        public TextFormatter(Func<GenericFont> boldFont = null, Func<GenericFont> italicFont = null) {
            // font codes
            this.Codes.Add(new Regex("<b>"), (f, m) => new FontCode(m, boldFont?.Invoke()));
            this.Codes.Add(new Regex("<i>"), (f, m) => new FontCode(m, italicFont?.Invoke()));
            this.Codes.Add(new Regex("</(b|i)>"), (f, m) => new FontCode(m, null));

            // color codes
            foreach (var c in typeof(Color).GetProperties()) {
                if (c.GetGetMethod().IsStatic) {
                    var value = (Color) c.GetValue(null);
                    this.Codes.Add(new Regex($"<c {c.Name}>"), (f, m) => new ColorCode(m, value));
                }
            }
            this.Codes.Add(new Regex(@"<c #([0-9\w]{6,8})>"), (f, m) => new ColorCode(m, ColorExtensions.FromHex(m.Groups[1].Value)));
            this.Codes.Add(new Regex("</c>"), (f, m) => new ColorCode(m, null));
        }

        public TokenizedString Tokenize(string s) {
            var tokens = new List<Token>();
            var codes = new List<Code>();
            var rawIndex = 0;
            while (rawIndex < s.Length) {
                var index = this.StripFormatting(s.Substring(0, rawIndex)).Length;
                var next = this.GetNextCode(s, rawIndex + 1);
                // if we've reached the end of the string
                if (next == null) {
                    var sub = s.Substring(rawIndex, s.Length - rawIndex);
                    tokens.Add(new Token(codes.ToArray(), index, rawIndex, this.StripFormatting(sub), sub));
                    break;
                }

                // create a new token for the content up to the next code
                var ret = s.Substring(rawIndex, next.Match.Index - rawIndex);
                tokens.Add(new Token(codes.ToArray(), index, rawIndex, this.StripFormatting(ret), ret));

                // move to the start of the next code
                rawIndex = next.Match.Index;

                // remove all codes that are incompatible with the next one and apply it
                codes.RemoveAll(c => c.EndsHere(next));
                codes.Add(next);
            }
            return new TokenizedString(s, this.StripFormatting(s), tokens.ToArray());
        }

        public string StripFormatting(string s) {
            foreach (var regex in this.Codes.Keys)
                s = regex.Replace(s, string.Empty);
            return s;
        }

        private Code GetNextCode(string s, int index) {
            var (c, m) = this.Codes
                .Select(kv => (c: kv.Value, m: kv.Key.Match(s, index)))
                .Where(kv => kv.m.Success)
                .OrderBy(kv => kv.m.Index)
                .FirstOrDefault();
            return c?.Invoke(this, m);
        }

    }
}