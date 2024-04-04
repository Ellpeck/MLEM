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
        private RectangleF area;

        internal TokenizedString(GenericFont font, TextAlignment alignment, string rawString, string strg, Token[] tokens, Code[] allCodes) {
            this.RawString = rawString;
            this.String = strg;
            this.Tokens = tokens;
            this.AllCodes = allCodes;
            this.Realign(font, alignment);
        }

        /// <summary>
        /// Splits this tokenized string, inserting newline characters if the width of the string is bigger than the maximum width.
        /// Note that a tokenized string can be re-split without losing any of its actual data, as this operation merely modifies the <see cref="DisplayString"/>.
        /// </summary>
        /// <param name="font">The font to use for width calculations</param>
        /// <param name="width">The maximum width, in display pixels based on the font and scale</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <param name="alignment">The text alignment that should be used for width calculations</param>
        public void Split(GenericFont font, float width, float scale, TextAlignment alignment = TextAlignment.Left) {
            var index = 0;
            var modified = new StringBuilder();
            foreach (var part in GenericFont.SplitStringSeparate(this.AsDecoratedSources(font), width, scale)) {
                var joined = string.Join("\n", part);
                this.Tokens[index].ModifiedSubstring = joined;
                modified.Append(joined);
                index++;
            }
            this.modifiedString = modified.ToString();
            this.Realign(font, alignment);
        }

        /// <summary>
        /// Truncates this tokenized string, removing any additional characters that exceed the length from the displayed string.
        /// Note that a tokenized string can be re-truncated without losing any of its actual data, as this operation merely modifies the <see cref="DisplayString"/>.
        /// <seealso cref="GenericFont.TruncateString(string,float,float,bool,string)"/>
        /// </summary>
        /// <param name="font">The font to use for width calculations</param>
        /// <param name="width">The maximum width, in display pixels based on the font and scale</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <param name="ellipsis">The characters to add to the end of the string if it is too long</param>
        /// <param name="alignment">The text alignment that should be used for width calculations</param>
        public void Truncate(GenericFont font, float width, float scale, string ellipsis = "", TextAlignment alignment = TextAlignment.Left) {
            var index = 0;
            var modified = new StringBuilder();
            foreach (var part in GenericFont.TruncateString(this.AsDecoratedSources(font), width, scale, false, ellipsis)) {
                this.Tokens[index].ModifiedSubstring = part.ToString();
                modified.Append(part);
                index++;
            }
            this.modifiedString = modified.ToString();
            this.Realign(font, alignment);
        }

        /// <summary>
        /// Realigns this tokenized string using the given <see cref="TextAlignment"/>.
        /// If the <paramref name="alignment"/> is <see cref="TextAlignment.Right"/>, trailing space characters (but not <see cref="GenericFont.Nbsp"/>) will be removed.
        /// </summary>
        /// <param name="font">The font to use for width calculations.</param>
        /// <param name="alignment">The text alignment that should be used for width calculations.</param>
        public void Realign(GenericFont font, TextAlignment alignment) {
            // split display strings
            foreach (var token in this.Tokens)
                token.SplitDisplayString = token.DisplayString.Split('\n');

            // token areas and inner offsets
            this.area = RectangleF.Empty;
            this.initialInnerOffset = this.GetInnerOffsetX(font, 0, 0, alignment);
            var innerOffset = new Vector2(this.initialInnerOffset, 0);
            for (var t = 0; t < this.Tokens.Length; t++) {
                var token = this.Tokens[t];
                var tokenFont = token.GetFont(font);
                token.InnerOffsets = new float[token.SplitDisplayString.Length - 1];

                var tokenArea = new List<RectangleF>();
                var selfRect = new RectangleF(innerOffset, new Vector2(token.GetSelfWidth(tokenFont), tokenFont.LineHeight));
                if (!selfRect.IsEmpty) {
                    tokenArea.Add(selfRect);
                    this.area = RectangleF.Union(this.area, selfRect);
                    innerOffset.X += selfRect.Width;
                }
                for (var l = 0; l < token.SplitDisplayString.Length; l++) {
                    var size = tokenFont.MeasureString(token.SplitDisplayString[l], !this.EndsLater(t, l));
                    var rect = new RectangleF(innerOffset, size);
                    if (!rect.IsEmpty) {
                        tokenArea.Add(rect);
                        this.area = RectangleF.Union(this.area, rect);
                    }

                    if (l < token.SplitDisplayString.Length - 1) {
                        innerOffset.X = token.InnerOffsets[l] = this.GetInnerOffsetX(font, t, l + 1, alignment);
                        innerOffset.Y += tokenFont.LineHeight;
                    } else {
                        innerOffset.X += size.X;
                    }
                }
                token.Area = tokenArea.ToArray();
            }
        }

        /// <inheritdoc cref="GenericFont.MeasureString(string,bool)"/>
        [Obsolete("Measure is deprecated. Use GetArea, which returns the string's total size measurement, instead.")]
        public Vector2 Measure(GenericFont font) {
            return this.GetArea(Vector2.Zero, 1).Size;
        }

        /// <summary>
        /// Measures the area that this entire tokenized string and all of its <see cref="Tokens"/> take up and returns it as a <see cref="RectangleF"/>.
        /// </summary>
        /// <param name="stringPos">The position that this string is being rendered at, which will offset the resulting <see cref="RectangleF"/>.</param>
        /// <param name="scale">The scale that this string is being rendered with, which will scale the resulting <see cref="RectangleF"/>.</param>
        /// <returns>The area that this tokenized string takes up.</returns>
        public RectangleF GetArea(Vector2 stringPos, float scale) {
            return new RectangleF(stringPos + this.area.Location * scale, this.area.Size * scale);
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
        public void Draw(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth, int? startIndex = null, int? endIndex = null) {
            var innerOffset = new Vector2(this.initialInnerOffset * scale, 0);
            for (var t = 0; t < this.Tokens.Length; t++) {
                var token = this.Tokens[t];
                if (endIndex != null && token.Index >= endIndex)
                    return;

                var drawFont = token.GetFont(font);
                var drawColor = token.GetColor(color);

                if (startIndex == null || token.Index >= startIndex)
                    token.DrawSelf(time, batch, pos + innerOffset, drawFont, drawColor, scale, depth);
                innerOffset.X += token.GetSelfWidth(drawFont) * scale;

                var indexInToken = 0;
                for (var l = 0; l < token.SplitDisplayString.Length; l++) {
                    var cpsIndex = 0;
                    var line = new CodePointSource(token.SplitDisplayString[l]);
                    while (cpsIndex < line.Length) {
                        if (endIndex != null && token.Index + indexInToken >= endIndex)
                            return;

                        var (codePoint, length) = line.GetCodePoint(cpsIndex);
                        var character = CodePointSource.ToString(codePoint);

                        if (startIndex == null || token.Index + indexInToken >= startIndex)
                            token.DrawCharacter(time, batch, codePoint, character, indexInToken, pos + innerOffset, drawFont, drawColor, scale, depth);

                        innerOffset.X += drawFont.MeasureString(character).X * scale;
                        indexInToken += length;
                        cpsIndex += length;
                    }

                    // only split at a new line, not between tokens!
                    if (l < token.SplitDisplayString.Length - 1) {
                        innerOffset.X = token.InnerOffsets[l] * scale;
                        innerOffset.Y += drawFont.LineHeight * scale;
                    }
                }
            }
        }

        private float GetInnerOffsetX(GenericFont defaultFont, int tokenIndex, int lineIndex, TextAlignment alignment) {
            if (alignment > TextAlignment.Left) {
                var token = this.Tokens[tokenIndex];
                var tokenFont = token.GetFont(defaultFont);
                var tokenWidth = lineIndex <= 0 ? token.GetSelfWidth(tokenFont) : 0;
                var endsLater = this.EndsLater(tokenIndex, lineIndex);
                // if the line ends in our token, we should ignore trailing white space
                var restOfLine = tokenFont.MeasureString(token.SplitDisplayString[lineIndex], !endsLater).X + tokenWidth;
                if (endsLater) {
                    for (var i = tokenIndex + 1; i < this.Tokens.Length; i++) {
                        var other = this.Tokens[i];
                        var otherFont = other.GetFont(defaultFont);
                        restOfLine += otherFont.MeasureString(other.SplitDisplayString[0], !this.EndsLater(i, 0)).X + other.GetSelfWidth(otherFont);
                        // if the token's split display string has multiple lines, then the line ends in it, which means we can stop
                        if (other.SplitDisplayString.Length > 1)
                            break;
                    }
                }
                if (alignment == TextAlignment.Center)
                    restOfLine /= 2;
                return -restOfLine;
            }
            return 0;
        }

        private bool EndsLater(int tokenIndex, int lineIndex) {
            // if we're the last line in our line array, then we don't contain a line split, so the line ends in a later token
            return lineIndex >= this.Tokens[tokenIndex].SplitDisplayString.Length - 1 && tokenIndex < this.Tokens.Length - 1;
        }

        private IEnumerable<GenericFont.DecoratedCodePointSource> AsDecoratedSources(GenericFont font) {
            return this.Tokens.Select(t => {
                var tokenFont = t.GetFont(font);
                return new GenericFont.DecoratedCodePointSource(new CodePointSource(t.Substring), tokenFont, t.GetSelfWidth(tokenFont));
            });
        }

    }
}
