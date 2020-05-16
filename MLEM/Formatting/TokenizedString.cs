using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting.Codes;
using MLEM.Misc;

namespace MLEM.Formatting {
    public class TokenizedString : GenericDataHolder {

        public readonly string RawString;
        public readonly string String;
        public string DisplayString => this.splitString ?? this.String;
        public readonly Token[] Tokens;
        public readonly Code[] AllCodes;
        private string splitString;

        public TokenizedString(GenericFont font, string rawString, string strg, Token[] tokens) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
            // since a code can be present in multiple tokens, we use Distinct here
            this.AllCodes = tokens.SelectMany(t => t.AppliedCodes).Distinct().ToArray();
            this.CalculateTokenAreas(font);
        }

        public void Split(GenericFont font, float width, float scale) {
            // a split string has the same character count as the input string
            // but with newline characters added
            this.splitString = font.SplitString(this.String, width, scale);
            // skip splitting logic for unformatted text
            if (this.Tokens.Length == 1) {
                this.Tokens[0].SplitSubstring = this.splitString;
                return;
            }
            foreach (var token in this.Tokens) {
                var index = 0;
                var length = 0;
                var ret = new StringBuilder();
                // this is basically a substring function that ignores newlines for indexing
                for (var i = 0; i < this.splitString.Length; i++) {
                    // if we're within the bounds of the token's substring, append to the new substring
                    if (index >= token.Index && length < token.Substring.Length)
                        ret.Append(this.splitString[i]);
                    // if the current char is not a newline, we simulate length increase
                    if (this.splitString[i] != '\n') {
                        if (index >= token.Index)
                            length++;
                        index++;
                    }
                }
                token.SplitSubstring = ret.ToString();
            }
            this.CalculateTokenAreas(font);
        }

        public Vector2 Measure(GenericFont font) {
            return font.MeasureString(this.DisplayString);
        }

        public void Update(GameTime time) {
            foreach (var code in this.AllCodes)
                code.Update(time);
        }

        public Token GetTokenUnderPos(Vector2 stringPos, Vector2 target, float scale) {
            return this.Tokens.FirstOrDefault(t => t.GetArea(stringPos, scale).Any(r => r.Contains(target)));
        }

        public void Draw(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            var innerOffset = new Vector2();
            foreach (var token in this.Tokens) {
                var drawFont = token.GetFont(font) ?? font;
                var drawColor = token.GetColor(color) ?? color;
                for (var i = 0; i < token.DisplayString.Length; i++) {
                    var c = token.DisplayString[i];
                    if (c == '\n') {
                        innerOffset.X = 0;
                        innerOffset.Y += font.LineHeight * scale;
                    }
                    if (i == 0)
                        token.DrawSelf(time, batch, pos + innerOffset, font, color, scale, depth);

                    var cString = c.ToString();
                    token.DrawCharacter(time, batch, c, cString, i, pos + innerOffset, drawFont, drawColor, scale, depth);
                    innerOffset.X += font.MeasureString(cString).X * scale;
                }
            }
        }

        private void CalculateTokenAreas(GenericFont font) {
            var innerOffset = new Vector2();
            foreach (var token in this.Tokens) {
                var area = new List<RectangleF>();
                var split = token.DisplayString.Split('\n');
                for (var i = 0; i < split.Length; i++) {
                    var size = font.MeasureString(split[i]);
                    area.Add(new RectangleF(innerOffset, size));

                    if (i < split.Length - 1) {
                        innerOffset.X = 0;
                        innerOffset.Y += font.LineHeight;
                    } else {
                        innerOffset.X += size.X;
                    }
                }
                token.Area = area.ToArray();
            }
        }

    }
}