using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting.Codes;
using MLEM.Misc;

namespace MLEM.Formatting {
    /// <summary>
    /// A tokenized string that was created using a <see cref="TextFormatter"/>
    /// </summary>
    public class TokenizedString : GenericDataHolder {

        /// <summary>
        /// The raw string that was used to create this tokenized string.
        /// </summary>
        public readonly string RawString;
        /// <summary>
        /// The <see cref="RawString"/>, but with formatting codes stripped out.
        /// </summary>
        public readonly string String;
        /// <summary>
        /// The string that is actually displayed by this tokenized string.
        /// If this string has been <see cref="Split"/>, this string will contain the newline characters.
        /// </summary>
        public string DisplayString => this.splitString ?? this.String;
        /// <summary>
        /// The tokens that this tokenized string contains.
        /// </summary>
        public readonly Token[] Tokens;
        /// <summary>
        /// All of the formatting codes that are applied over this tokenized string.
        /// Note that, to get a formatting code for a certain token, use <see cref="Token.AppliedCodes"/>
        /// </summary>
        public readonly Code[] AllCodes;
        private string splitString;

        internal TokenizedString(GenericFont font, string rawString, string strg, Token[] tokens) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
            // since a code can be present in multiple tokens, we use Distinct here
            this.AllCodes = tokens.SelectMany(t => t.AppliedCodes).Distinct().ToArray();
            this.CalculateTokenAreas(font);
        }

        /// <summary>
        /// Splits this tokenized string, inserting newline characters if the width of the string is bigger than the maximum width.
        /// <seealso cref="GenericFont.SplitString"/>
        /// </summary>
        /// <param name="font">The font to use for width calculations</param>
        /// <param name="width">The maximum width</param>
        /// <param name="scale">The scale to use fr width calculations</param>
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

        /// <inheritdoc cref="GenericFont.MeasureString(string)"/>
        public Vector2 Measure(GenericFont font) {
            return font.MeasureString(this.DisplayString);
        }

        /// <summary>
        /// Updates the formatting codes in this formatted string, causing animations to animate etc.
        /// </summary>
        /// <param name="time">The game's time</param>
        public void Update(GameTime time) {
            foreach (var code in this.AllCodes)
                code.Update(time);
        }

        /// <summary>
        /// Returns the token under the given position.
        /// This can be used for hovering effects when the mouse is over a token, etc.
        /// </summary>
        /// <param name="stringPos">The position that the string is drawn at</param>
        /// <param name="target">The position to use for checking the token</param>
        /// <param name="scale">The scale that the string is drawn at</param>
        /// <returns>The token under the target position</returns>
        public Token GetTokenUnderPos(Vector2 stringPos, Vector2 target, float scale) {
            return this.Tokens.FirstOrDefault(t => t.GetArea(stringPos, scale).Any(r => r.Contains(target)));
        }

        /// <inheritdoc cref="GenericFont.DrawString(SpriteBatch,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
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

                    token.DrawCharacter(time, batch, c, c.ToString(), i, pos + innerOffset, drawFont, drawColor, scale, depth);
                    innerOffset.X += font.MeasureChar(c).X * scale;
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
                    var rect = new RectangleF(innerOffset, size);
                    if (!rect.IsEmpty)
                        area.Add(rect);

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