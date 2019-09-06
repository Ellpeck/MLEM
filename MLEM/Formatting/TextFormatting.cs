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
            return formatRegex.Replace(s, match => FromMatch(match).GetReplacementString());
        }

        public static Dictionary<int, FormattingCode> GetFormattingCodes(this string s) {
            var codes = new Dictionary<int, FormattingCode>();
            var codeLengths = 0;
            foreach (Match match in formatRegex.Matches(s)) {
                var code = FromMatch(match);
                codes[match.Index - codeLengths] = code;
                codeLengths += match.Length - code.GetReplacementString().Length;
            }
            return codes;
        }

        public static void DrawFormattedString(this IGenericFont regularFont, SpriteBatch batch, Vector2 pos, string text, Dictionary<int, FormattingCode> codeLocations, Color color, float scale, IGenericFont boldFont = null, IGenericFont italicFont = null, float depth = 0) {
            var characterCounter = 0;
            var currColor = color;
            var currFont = regularFont;

            var innerOffset = new Vector2();
            foreach (var c in text) {
                // check if the current character's index has a formatting code
                codeLocations.TryGetValue(characterCounter, out var code);
                if (code != null) {
                    // if so, apply it
                    if (code.CodeType == FormattingCode.Type.Color) {
                        currColor = code.Color.CopyAlpha(color);
                    } else if (code.CodeType == FormattingCode.Type.Style) {
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
                    } else if (code.CodeType == FormattingCode.Type.Icon) {
                        var iconSc = new Vector2(1F / code.Icon.Width, 1F / code.Icon.Height) * regularFont.LineHeight * scale;
                        batch.Draw(code.Icon, pos + innerOffset, color, 0, Vector2.Zero, iconSc, SpriteEffects.None, depth);
                    }
                }
                characterCounter++;

                var cSt = c.ToString();
                if (c == '\n') {
                    innerOffset.X = 0;
                    innerOffset.Y += regularFont.LineHeight * scale;
                } else {
                    currFont.DrawString(batch, cSt, pos + innerOffset, currColor, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
                    // we measure the string with the regular font here so that previously split
                    // strings don't get too long with a bolder font. This shouldn't effect visuals too much
                    innerOffset.X += regularFont.MeasureString(cSt).X * scale;
                }
            }
        }

        private static FormattingCode FromMatch(Capture match) {
            var rawCode = match.Value.Substring(1, match.Value.Length - 2).ToLowerInvariant();
            return FormattingCodes[rawCode];
        }

    }
}