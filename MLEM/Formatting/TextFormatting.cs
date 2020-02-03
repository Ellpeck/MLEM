using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Formatting {
    public static class TextFormatting {

        public static readonly Dictionary<Regex, Func<Match, int, FormattingCode>> FormattingCodes = new Dictionary<Regex, Func<Match, int, FormattingCode>>();
        private static readonly Dictionary<IGenericFont, string> OneEmStrings = new Dictionary<IGenericFont, string>();
        private static Regex formatRegex;

        static TextFormatting() {
            SetFormatIndicators('[', ']');

            // style codes
            FormattingCodes[new Regex("regular")] = (m, i) => new FormattingCode(i, TextStyle.Regular);
            FormattingCodes[new Regex("italic")] = (m, i) => new FormattingCode(i, TextStyle.Italic);
            FormattingCodes[new Regex("bold")] = (m, i) => new FormattingCode(i, TextStyle.Bold);
            FormattingCodes[new Regex("shadow")] = (m, i) => new FormattingCode(i, TextStyle.Shadow);

            // color codes
            var colors = typeof(Color).GetProperties();
            foreach (var color in colors) {
                if (color.GetGetMethod().IsStatic)
                    FormattingCodes[new Regex(color.Name.ToLowerInvariant())] = (m, i) => new FormattingCode(i, (Color) color.GetValue(null));
            }

            // animations
            FormattingCodes[new Regex("unanimated")] = (m, i) => new AnimationCode(i, FormatState.DefaultDrawBehavior);
            FormattingCodes[new Regex("wobbly")] = (m, i) => new AnimationCode(i, (state, batch, totalText, index, charSt, position, scale, layerDepth) => {
                var code = (AnimationCode) state.FormattingCodes[i];
                var offset = new Vector2(0, (float) Math.Sin(index + code.Time.TotalSeconds * state.Settings.WobbleModifier) * state.Font.LineHeight * state.Settings.WobbleHeightModifier * scale);
                state.Font.DrawString(batch, charSt, position + offset, state.Color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
            });
            FormattingCodes[new Regex("typing")] = (m, i) => new AnimationCode(i, (state, batch, totalText, index, charSt, position, scale, layerDepth) => {
                var code = (AnimationCode) state.FormattingCodes[i];
                if (code.Time.TotalSeconds * state.Settings.TypingSpeed > index - i + 1)
                    state.Font.DrawString(batch, charSt, position, state.Color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
            });
        }

        public static void SetFormatIndicators(char opener, char closer) {
            // escape the opener and closer so that any character can be used
            var op = "\\" + opener;
            var cl = "\\" + closer;
            // find any text that is surrounded by the opener and closer
            formatRegex = new Regex($"{op}[^{op}{cl}]*{cl}");
        }

        public static string GetOneEmString(IGenericFont font) {
            if (!OneEmStrings.TryGetValue(font, out var strg)) {
                strg = " ";
                while (font.MeasureString(strg + ' ').X < font.LineHeight)
                    strg += ' ';
                OneEmStrings[font] = strg;
            }
            return strg;
        }

        public static string RemoveFormatting(this string s, IGenericFont font) {
            var codeLengths = 0;
            return formatRegex.Replace(s, match => {
                var code = FromMatch(match, match.Index - codeLengths);
                if (code != null) {
                    var replace = code.GetReplacementString(font);
                    codeLengths += match.Length - replace.Length;
                    return replace;
                }
                return match.Value;
            });
        }

        public static FormattingCodeCollection GetFormattingCodes(this string s, IGenericFont font) {
            var codes = new FormattingCodeCollection();
            var codeLengths = 0;
            foreach (Match match in formatRegex.Matches(s)) {
                var index = match.Index - codeLengths;
                var code = FromMatch(match, index);
                if (code == null)
                    continue;
                codes[index] = code;
                codeLengths += match.Length - code.GetReplacementString(font).Length;
            }
            return codes;
        }

        public static void DrawFormattedString(this IGenericFont regularFont, SpriteBatch batch, Vector2 pos, string text, FormattingCodeCollection codes, Color color, float scale, IGenericFont boldFont = null, IGenericFont italicFont = null, float depth = 0, FormatSettings formatSettings = null) {
            var state = new FormatState {
                FormattingCodes = codes,
                RegularFont = regularFont,
                ItalicFont = italicFont,
                BoldFont = boldFont,
                Settings = formatSettings ?? FormatSettings.Default,

                Color = color,
                Font = regularFont,
                DrawBehavior = FormatState.DefaultDrawBehavior
            };

            for (var i = 0; i < text.Length; i++) {
                state.CurrentIndex = i;
                if (codes.TryGetValue(i, out var code))
                    code.ModifyFormatting(ref state);

                var c = text[i];
                var cSt = c.ToString();
                if (c == '\n') {
                    state.InnerOffset = new Vector2(0, state.InnerOffset.Y + regularFont.LineHeight * scale);
                } else {
                    state.DrawBehavior(state, batch, text, i, cSt, pos + state.InnerOffset, scale, depth);
                    state.InnerOffset += new Vector2(regularFont.MeasureString(cSt).X * scale, 0);
                }
            }
        }

        private static FormattingCode FromMatch(Capture match, int index) {
            var rawCode = match.Value.Substring(1, match.Value.Length - 2).ToLowerInvariant();
            foreach (var code in FormattingCodes) {
                var m = code.Key.Match(rawCode);
                if (m.Success)
                    return code.Value(m, index);
            }
            return null;
        }

    }
}