using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Formatting {
    public static class TextFormatting {

        public static readonly Dictionary<string, FormattingCode> FormattingCodes = new Dictionary<string, FormattingCode>();
        private static Regex formatRegex;

        // Unicode suggests that a space should be 1/4em in length, so this string should be
        // 1em in length for most fonts. If it's not for the used font, the user can just
        // change the value of this string to something that is in fact 1em long
        public static string OneEmString = "    ";

        static TextFormatting() {
            SetFormatIndicators('[', ']');

            // style codes
            FormattingCodes["regular"] = new FormattingCode(TextStyle.Regular);
            FormattingCodes["italic"] = new FormattingCode(TextStyle.Italic);
            FormattingCodes["bold"] = new FormattingCode(TextStyle.Bold);

            // color codes
            var colors = typeof(Color).GetProperties();
            foreach (var color in colors) {
                if (color.GetGetMethod().IsStatic)
                    FormattingCodes[color.Name.ToLowerInvariant()] = new FormattingCode((Color) color.GetValue(null));
            }

            // animations
            FormattingCodes["unanimated"] = new FormattingCode(TextAnimation.Default);
            FormattingCodes["wobbly"] = new FormattingCode(TextAnimation.Wobbly);
            FormattingCodes["typing"] = new FormattingCode(TextAnimation.Typing);
        }

        public static void SetFormatIndicators(char opener, char closer) {
            // escape the opener and closer so that any character can be used
            var op = "\\" + opener;
            var cl = "\\" + closer;
            // find any text that is surrounded by the opener and closer
            formatRegex = new Regex($"{op}[^{op}{cl}]*{cl}");
        }

        public static string RemoveFormatting(this string s) {
            return formatRegex.Replace(s, match => {
                var code = FromMatch(match);
                return code != null ? code.GetReplacementString() : string.Empty;
            });
        }

        public static Dictionary<int, FormattingCode> GetFormattingCodes(this string s) {
            var codes = new Dictionary<int, FormattingCode>();
            var codeLengths = 0;
            foreach (Match match in formatRegex.Matches(s)) {
                var code = FromMatch(match);
                if (code == null)
                    continue;
                codes[match.Index - codeLengths] = code;
                codeLengths += match.Length - code.GetReplacementString().Length;
            }
            return codes;
        }

        public static void DrawFormattedString(this IGenericFont regularFont, SpriteBatch batch, Vector2 pos, string text, Dictionary<int, FormattingCode> codeLocations, Color color, float scale, IGenericFont boldFont = null, IGenericFont italicFont = null, float depth = 0, TimeSpan timeIntoAnimation = default) {
            var currColor = color;
            var currFont = regularFont;
            var currAnim = TextAnimation.Default;
            var animStart = 0;

            var innerOffset = new Vector2();
            for (var i = 0; i < text.Length; i++) {
                // check if the current character's index has a formatting code
                codeLocations.TryGetValue(i, out var code);
                if (code != null) {
                    // if so, apply it
                    switch (code.CodeType) {
                        case FormattingCode.Type.Color:
                            currColor = code.Color.CopyAlpha(color);
                            break;
                        case FormattingCode.Type.Style:
                            switch (code.Style) {
                                case TextStyle.Regular:
                                    currFont = regularFont;
                                    break;
                                case TextStyle.Bold:
                                    currFont = boldFont ?? regularFont;
                                    break;
                                case TextStyle.Italic:
                                    currFont = italicFont ?? regularFont;
                                    break;
                            }
                            break;
                        case FormattingCode.Type.Icon:
                            var iconSc = new Vector2(1F / code.Icon.Width, 1F / code.Icon.Height) * regularFont.LineHeight * scale;
                            batch.Draw(code.Icon, pos + innerOffset, color, 0, Vector2.Zero, iconSc, SpriteEffects.None, depth);
                            break;
                        case FormattingCode.Type.Animation:
                            currAnim = code.Animation;
                            animStart = i;
                            break;
                    }
                }

                var c = text[i];
                var cSt = c.ToString();
                if (c == '\n') {
                    innerOffset.X = 0;
                    innerOffset.Y += regularFont.LineHeight * scale;
                } else {
                    currAnim(currFont, batch, text, i, animStart, cSt, pos + innerOffset, currColor, scale, depth, timeIntoAnimation);
                    // we measure the string with the regular font here so that previously split
                    // strings don't get too long with a bolder font. This shouldn't effect visuals too much
                    innerOffset.X += regularFont.MeasureString(cSt).X * scale;
                }
            }
        }

        private static FormattingCode FromMatch(Capture match) {
            var rawCode = match.Value.Substring(1, match.Value.Length - 2).ToLowerInvariant();
            FormattingCodes.TryGetValue(rawCode, out var val);
            return val;
        }

    }
}