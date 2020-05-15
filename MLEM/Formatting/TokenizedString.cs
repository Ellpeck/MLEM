using System;
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
        public readonly Token[] Tokens;
        public readonly Code[] AllCodes;

        public TokenizedString(string rawString, string strg, Token[] tokens) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
            // since a code can be present in multiple tokens, we use Distinct here
            this.AllCodes = tokens.SelectMany(t => t.AppliedCodes).Distinct().ToArray();
        }

        public void Split(GenericFont font, float width, float scale) {
            // a split string has the same character count as the input string
            // but with newline characters added
            var split = font.SplitString(this.String, width, scale);
            foreach (var token in this.Tokens) {
                var index = 0;
                var length = 0;
                var ret = new StringBuilder();
                // this is basically a substring function that ignores newlines for indexing
                for (var i = 0; i < split.Length; i++) {
                    // if we're within the bounds of the token's substring, append to the new substring
                    if (index >= token.Index && length < token.Substring.Length)
                        ret.Append(split[i]);
                    // if the current char is not a newline, we simulate length increase
                    if (split[i] != '\n') {
                        if (index >= token.Index)
                            length++;
                        index++;
                    }
                }
                token.Substring = ret.ToString();
            }
        }

        public void Update(GameTime time) {
            foreach (var code in this.AllCodes)
                code.Update(time);
        }

        public void Draw(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            var innerOffset = new Vector2();
            foreach (var token in this.Tokens) {
                var drawFont = token.GetFont() ?? font;
                var drawColor = token.GetColor() ?? color;
                for (var i = 0; i < token.Substring.Length; i++) {
                    var c = token.Substring[i];
                    if (c == '\n') {
                        innerOffset.X = 0;
                        innerOffset.Y += font.LineHeight * scale;
                        continue;
                    }

                    var cString = c.ToString();
                    token.DrawCharacter(time, batch, c, cString, i, pos + innerOffset, drawFont, drawColor, scale, depth);
                    innerOffset.X += font.MeasureString(cString).X * scale;
                }
            }
        }

    }
}