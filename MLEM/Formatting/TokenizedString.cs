using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public struct TokenizedString {

        public readonly string RawString;
        public readonly string String;
        public readonly Token[] Tokens;

        public TokenizedString(string rawString, string strg, Token[] tokens) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
        }

        public void Split(GenericFont font, float width, float scale) {
            var split = font.SplitString(this.String, width, scale);
            // remove spaces at the end of new lines since we want the same character count
            split = split.Replace(" \n", "\n");
            foreach (var token in this.Tokens)
                token.Substring = split.Substring(token.Index, token.Substring.Length);
        }

        public void Draw(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            var innerOffset = new Vector2();
            foreach (var token in this.Tokens) {
                var drawFont = token.GetFont() ?? font;
                var drawColor = token.GetColor() ?? color;
                foreach (var c in token.Substring) {
                    if (c == '\n') {
                        innerOffset.X = 0;
                        innerOffset.Y += font.LineHeight * scale;
                        continue;
                    }

                    var cString = c.ToString();
                    token.DrawCharacter(time, batch, c, cString, pos + innerOffset, drawFont, drawColor, scale, depth);
                    innerOffset.X += font.MeasureString(cString).X * scale;
                }
            }
        }

    }
}