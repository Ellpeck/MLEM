using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
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
        /// If this string has been <see cref="Split"/> or <see cref="Truncate"/> has been used, this string will contain the newline characters.
        /// </summary>
        public string DisplayString => this.modifiedString ?? this.String;
        /// <summary>
        /// The tokens that this tokenized string contains.
        /// </summary>
        public readonly Token[] Tokens;
        /// <summary>
        /// All of the formatting codes that are applied over this tokenized string.
        /// Note that, to get a formatting code for a certain token, use <see cref="Token.AppliedCodes"/>
        /// </summary>
        public readonly Code[] AllCodes;
        private string modifiedString;
        private float initialInnerOffset;

        internal TokenizedString(GenericFont font, TextAlignment alignment, string rawString, string strg, Token[] tokens) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
            // since a code can be present in multiple tokens, we use Distinct here
            this.AllCodes = tokens.SelectMany(t => t.AppliedCodes).Distinct().ToArray();
            this.RecalculateTokenData(font, alignment);
        }

        /// <summary>
        /// Splits this tokenized string, inserting newline characters if the width of the string is bigger than the maximum width.
        /// Note that a tokenized string can be re-split without losing any of its actual data, as this operation merely modifies the <see cref="DisplayString"/>.
        /// <seealso cref="GenericFont.SplitString"/>
        /// </summary>
        /// <param name="font">The font to use for width calculations</param>
        /// <param name="width">The maximum width, in display pixels based on the font and scale</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <param name="alignment">The text alignment that should be used for width calculations</param>
        public void Split(GenericFont font, float width, float scale, TextAlignment alignment = TextAlignment.Left) {
            // a split string has the same character count as the input string but with newline characters added
            this.modifiedString = font.SplitString(this.String, width, scale);
            this.StoreModifiedSubstrings(font, alignment);
        }

        /// <summary>
        /// Truncates this tokenized string, removing any additional characters that exceed the length from the displayed string.
        /// Note that a tokenized string can be re-truncated without losing any of its actual data, as this operation merely modifies the <see cref="DisplayString"/>.
        /// <seealso cref="GenericFont.TruncateString"/>
        /// </summary>
        /// <param name="font">The font to use for width calculations</param>
        /// <param name="width">The maximum width, in display pixels based on the font and scale</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <param name="ellipsis">The characters to add to the end of the string if it is too long</param>
        /// <param name="alignment">The text alignment that should be used for width calculations</param>
        public void Truncate(GenericFont font, float width, float scale, string ellipsis = "", TextAlignment alignment = TextAlignment.Left) {
            this.modifiedString = font.TruncateString(this.String, width, scale, false, ellipsis);
            this.StoreModifiedSubstrings(font, alignment);
        }

        /// <inheritdoc cref="GenericFont.MeasureString(string,bool)"/>
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
            foreach (var token in this.Tokens) {
                foreach (var rect in token.GetArea(stringPos, scale)) {
                    if (rect.Contains(target))
                        return token;
                }
            }
            return null;
        }

        /// <inheritdoc cref="GenericFont.DrawString(SpriteBatch,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void Draw(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth, TextAlignment alignment = TextAlignment.Left) {
            var innerOffset = new Vector2(this.initialInnerOffset * scale, 0);
            for (var t = 0; t < this.Tokens.Length; t++) {
                var token = this.Tokens[t];
                var drawFont = token.GetFont(font) ?? font;
                var drawColor = token.GetColor(color) ?? color;
                for (var l = 0; l < token.SplitDisplayString.Length; l++) {
                    var line = token.SplitDisplayString[l];
                    for (var i = 0; i < line.Length; i++) {
                        var c = line[i];
                        if (l == 0 && i == 0)
                            token.DrawSelf(time, batch, pos + innerOffset, font, color, scale, depth);

                        var cString = c.ToCachedString();
                        token.DrawCharacter(time, batch, c, cString, i, pos + innerOffset, drawFont, drawColor, scale, depth);
                        innerOffset.X += font.MeasureString(cString).X * scale;
                    }
                    // only split at a new line, not between tokens!
                    if (l < token.SplitDisplayString.Length - 1) {
                        innerOffset.X = token.InnerOffsets[l] * scale;
                        innerOffset.Y += font.LineHeight * scale;
                    }
                }
            }
        }

        private void StoreModifiedSubstrings(GenericFont font, TextAlignment alignment) {
            if (this.Tokens.Length == 1) {
                // skip substring logic for unformatted text
                this.Tokens[0].ModifiedSubstring = this.modifiedString;
            } else {
                // this is basically a substring function that ignores added newlines for indexing
                var index = 0;
                var currToken = 0;
                var splitIndex = 0;
                var ret = new StringBuilder();
                while (splitIndex < this.modifiedString.Length && currToken < this.Tokens.Length) {
                    var token = this.Tokens[currToken];
                    if (token.Substring.Length > 0) {
                        ret.Append(this.modifiedString[splitIndex]);
                        // if the current char is not an added newline, we simulate length increase
                        if (this.modifiedString[splitIndex] != '\n' || this.String[index] == '\n')
                            index++;
                        splitIndex++;
                    }
                    // move on to the next token if we reached its end
                    if (index >= token.Index + token.Substring.Length) {
                        token.ModifiedSubstring = ret.ToString();
                        ret.Clear();
                        currToken++;
                    }
                }
                // set additional token contents beyond our string in case we truncated
                if (ret.Length > 0)
                    this.Tokens[currToken++].ModifiedSubstring = ret.ToString();
                while (currToken < this.Tokens.Length)
                    this.Tokens[currToken++].ModifiedSubstring = string.Empty;
            }

            this.RecalculateTokenData(font, alignment);
        }

        private void RecalculateTokenData(GenericFont font, TextAlignment alignment) {
            // split display strings
            foreach (var token in this.Tokens)
                token.SplitDisplayString = token.DisplayString.Split('\n');

            // token areas
            this.initialInnerOffset = this.GetInnerOffsetX(font, 0, 0, alignment);
            var innerOffset = new Vector2(this.initialInnerOffset, 0);
            for (var t = 0; t < this.Tokens.Length; t++) {
                var token = this.Tokens[t];
                token.InnerOffsets = new float[token.SplitDisplayString.Length - 1];
                var area = new List<RectangleF>();
                for (var l = 0; l < token.SplitDisplayString.Length; l++) {
                    var size = font.MeasureString(token.SplitDisplayString[l]);
                    var rect = new RectangleF(innerOffset, size);
                    if (!rect.IsEmpty)
                        area.Add(rect);

                    if (l < token.SplitDisplayString.Length - 1) {
                        innerOffset.X = token.InnerOffsets[l] = this.GetInnerOffsetX(font, t, l + 1, alignment);
                        innerOffset.Y += font.LineHeight;
                    } else {
                        innerOffset.X += size.X;
                    }
                }
                token.Area = area.ToArray();
            }
        }

        private float GetInnerOffsetX(GenericFont font, int tokenIndex, int lineIndex, TextAlignment alignment) {
            if (alignment > TextAlignment.Left) {
                var token = this.Tokens[tokenIndex];
                var restOfLine = font.MeasureString(token.SplitDisplayString[lineIndex], true).X;
                if (lineIndex >= token.SplitDisplayString.Length - 1) {
                    // the line ends somewhere in or after the next token
                    for (var i = tokenIndex + 1; i < this.Tokens.Length; i++) {
                        var other = this.Tokens[i];
                        if (other.SplitDisplayString.Length > 1) {
                            // the line ends in this token
                            restOfLine += font.MeasureString(other.SplitDisplayString[0]).X;
                            break;
                        } else {
                            // the line doesn't end in this token, so add it fully
                            restOfLine += font.MeasureString(other.DisplayString).X;
                        }
                    }
                }
                if (alignment == TextAlignment.Center)
                    restOfLine /= 2;
                return -restOfLine;
            }
            return 0;
        }

    }
}