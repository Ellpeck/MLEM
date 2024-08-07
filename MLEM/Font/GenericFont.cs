using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Misc;

namespace MLEM.Font {
    /// <summary>
    /// Represents a font with additional abilities.
    /// <seealso cref="GenericSpriteFont"/>
    /// </summary>
    public abstract class GenericFont : GenericDataHolder {

        /// <summary>
        /// This field holds the unicode representation of a one em space.
        /// This is a character that isn't drawn, but has the same width as <see cref="LineHeight"/>.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString(string,bool)"/>.
        /// </summary>
        public const char Emsp = '\u2003';
        /// <summary>
        /// This field holds the unicode representation of a non-breaking space.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString(string,bool)"/>.
        /// </summary>
        public const char Nbsp = '\u00A0';
        /// <summary>
        /// This field holds the unicode representation of a zero-width space.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations and string splitting, generic fonts implicitly support it in <see cref="MeasureString(string,bool)"/> and <see cref="SplitString(string,float,float)"/>.
        /// </summary>
        public const char Zwsp = '\u200B';

        /// <summary>
        /// The bold version of this font.
        /// </summary>
        public abstract GenericFont Bold { get; }

        /// <summary>
        /// The italic version of this font.
        /// </summary>
        public abstract GenericFont Italic { get; }

        /// <summary>
        /// The height of each line of text of this font.
        /// This is the value that the text's draw position is offset by every time a newline character is reached.
        /// </summary>
        public abstract float LineHeight { get; }

        /// <summary>
        /// Measures the width of the given code point with the default scale for use in <see cref="MeasureString(string,bool)"/>.
        /// Note that this method does not support <see cref="Nbsp"/>, <see cref="Zwsp"/> and <see cref="Emsp"/> for most generic fonts, which is why <see cref="MeasureString(string,bool)"/> should be used even for single characters.
        /// </summary>
        /// <param name="codePoint">The code point whose width to calculate</param>
        /// <returns>The width of the given character with the default scale</returns>
        protected abstract float MeasureCharacter(int codePoint);

        /// <summary>
        /// Draws the given code point with the given data for use in <see cref="DrawString(Microsoft.Xna.Framework.Graphics.SpriteBatch,System.Text.StringBuilder,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float)"/>.
        /// Note that this method should only be called internally for rendering of more complex strings, like in <see cref="TextFormatter"/> <see cref="Code"/> implementations.
        /// </summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="codePoint">The code point which will be drawn.</param>
        /// <param name="character">A string representation of the character which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this character.</param>
        /// <param name="scale">A scaling of this character.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this character.</param>
        public abstract void DrawCharacter(SpriteBatch batch, int codePoint, string character, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            this.DrawString(batch, new CodePointSource(text), position, color, rotation, origin, scale, effects, layerDepth);
        }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            this.DrawString(batch, new CodePointSource(text), position, color, rotation, origin, scale, effects, layerDepth);
        }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            this.DrawString(batch, text, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            this.DrawString(batch, text, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            this.DrawString(batch, text, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            this.DrawString(batch, text, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Measures the width of the given string when drawn with this font's underlying font.
        /// This method uses <see cref="MeasureCharacter"/> internally to calculate the size of known characters and calculates additional characters like <see cref="Nbsp"/>, <see cref="Zwsp"/> and <see cref="Emsp"/>.
        /// If the text contains newline characters (\n), the size returned will represent a rectangle that encompasses the width of the longest line and the string's full height.
        /// </summary>
        /// <param name="text">The text whose size to calculate</param>
        /// <param name="ignoreTrailingSpaces">Whether trailing whitespace should be ignored in the returned size, causing the end of each line to be effectively trimmed</param>
        /// <returns>The size of the string when drawn with this font</returns>
        public Vector2 MeasureString(string text, bool ignoreTrailingSpaces = false) {
            return this.MeasureString(new CodePointSource(text), ignoreTrailingSpaces);
        }

        /// <inheritdoc cref="MeasureString(string,bool)"/>
        public Vector2 MeasureString(StringBuilder text, bool ignoreTrailingSpaces = false) {
            return this.MeasureString(new CodePointSource(text), ignoreTrailingSpaces);
        }

        /// <summary>
        /// Truncates a string to a given width. If the string's displayed area is larger than the maximum width, the string is cut off.
        /// Optionally, the string can be cut off a bit sooner, adding the <paramref name="ellipsis"/> at the end instead.
        /// </summary>
        /// <param name="text">The text to truncate</param>
        /// <param name="width">The maximum width, in display pixels based on the font and scale</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <param name="fromBack">If the string should be truncated from the back rather than the front</param>
        /// <param name="ellipsis">The characters to add to the end of the string if it is too long</param>
        /// <returns>The truncated string, or the same string if it is shorter than the maximum width</returns>
        public string TruncateString(string text, float width, float scale, bool fromBack = false, string ellipsis = "") {
            return GenericFont.TruncateString(Enumerable.Repeat(new DecoratedCodePointSource(new CodePointSource(text), this, 0), 1), width, scale, fromBack, ellipsis).First().ToString();
        }

        /// <inheritdoc cref="TruncateString(string,float,float,bool,string)"/>
        public StringBuilder TruncateString(StringBuilder text, float width, float scale, bool fromBack = false, string ellipsis = "") {
            return GenericFont.TruncateString(Enumerable.Repeat(new DecoratedCodePointSource(new CodePointSource(text), this, 0), 1), width, scale, fromBack, ellipsis).First();
        }

        /// <summary>
        /// Splits a string to a given maximum width, adding newline characters between each line.
        /// Also splits long words and supports zero-width spaces and takes into account existing newline characters in the passed <paramref name="text"/>.
        /// See <see cref="SplitStringSeparate(string,float,float)"/> for a method that differentiates between existing newline characters and splits due to maximum width.
        /// </summary>
        /// <param name="text">The text to split into multiple lines</param>
        /// <param name="width">The maximum width that each line should have</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <returns>The split string, containing newline characters at each new line</returns>
        public string SplitString(string text, float width, float scale) {
            return string.Join("\n", this.SplitStringSeparate(text, width, scale));
        }

        /// <inheritdoc cref="SplitString(string,float,float)"/>
        public string SplitString(StringBuilder text, float width, float scale) {
            return string.Join("\n", this.SplitStringSeparate(text, width, scale));
        }

        /// <summary>
        /// Splits a string to a given maximum width and returns each split section as a separate string.
        /// Note that existing new lines are taken into account for line length, but not split in the resulting strings.
        /// This method differs from <see cref="SplitString(string,float,float)"/> in that it differentiates between pre-existing newline characters and splits due to maximum width.
        /// </summary>
        /// <param name="text">The text to split into multiple lines</param>
        /// <param name="width">The maximum width that each line should have</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <returns>The split string as an enumerable of split sections</returns>
        public IEnumerable<string> SplitStringSeparate(string text, float width, float scale) {
            return GenericFont.SplitStringSeparate(Enumerable.Repeat(new DecoratedCodePointSource(new CodePointSource(text), this, 0), 1), width, scale).First();
        }

        /// <inheritdoc cref="SplitStringSeparate(string,float,float)"/>
        public IEnumerable<string> SplitStringSeparate(StringBuilder text, float width, float scale) {
            return GenericFont.SplitStringSeparate(Enumerable.Repeat(new DecoratedCodePointSource(new CodePointSource(text), this, 0), 1), width, scale).First();
        }

        /// <summary>
        /// Calculates a transformation matrix for drawing a string with the given data.
        /// </summary>
        /// <param name="position">The position to draw at.</param>
        /// <param name="rotation">The rotation to draw with.</param>
        /// <param name="origin">The origin to subtract from the position.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="effects">The flipping to draw with.</param>
        /// <param name="flipSize">The size of the string, which is only used when <paramref name="effects"/> is not <see cref="SpriteEffects.None"/>.</param>
        /// <returns>A transformation matrix.</returns>
        public Matrix CalculateStringTransform(Vector2 position, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Vector2 flipSize) {
            var (flipX, flipY) = (0F, 0F);
            var flippedV = (effects & SpriteEffects.FlipVertically) != 0;
            var flippedH = (effects & SpriteEffects.FlipHorizontally) != 0;
            if (flippedV || flippedH) {
                if (flippedH) {
                    origin.X *= -1;
                    flipX = -flipSize.X;
                }
                if (flippedV) {
                    origin.Y *= -1;
                    flipY = this.LineHeight - flipSize.Y;
                }
            }

            var trans = Matrix.Identity;
            if (rotation == 0) {
                trans.M11 = flippedH ? -scale.X : scale.X;
                trans.M22 = flippedV ? -scale.Y : scale.Y;
                trans.M41 = (flipX - origin.X) * trans.M11 + position.X;
                trans.M42 = (flipY - origin.Y) * trans.M22 + position.Y;
            } else {
                var sin = (float) Math.Sin(rotation);
                var cos = (float) Math.Cos(rotation);
                trans.M11 = (flippedH ? -scale.X : scale.X) * cos;
                trans.M12 = (flippedH ? -scale.X : scale.X) * sin;
                trans.M21 = (flippedV ? -scale.Y : scale.Y) * -sin;
                trans.M22 = (flippedV ? -scale.Y : scale.Y) * cos;
                trans.M41 = (flipX - origin.X) * trans.M11 + (flipY - origin.Y) * trans.M21 + position.X;
                trans.M42 = (flipX - origin.X) * trans.M12 + (flipY - origin.Y) * trans.M22 + position.Y;
            }
            return trans;
        }

        /// <summary>
        /// Moves the passed <paramref name="charPos"/> based on the given flipping data.
        /// </summary>
        /// <param name="charPos">The position to move.</param>
        /// <param name="effects">The flipping to move based on.</param>
        /// <param name="charSize">The size of the object to move, which is only used when <paramref name="effects"/> is not <see cref="SpriteEffects.None"/>.</param>
        /// <returns>The moved position.</returns>
        public Vector2 MoveFlipped(Vector2 charPos, SpriteEffects effects, Vector2 charSize) {
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
                charPos.X += charSize.X;
            if ((effects & SpriteEffects.FlipVertically) != 0)
                charPos.Y += charSize.Y - this.LineHeight;
            return charPos;
        }

        /// <summary>
        /// Transforms the position of a single character to draw.
        /// In general, it is efficient to calculate the transformation matrix once at the start (using <see cref="CalculateStringTransform"/>) and to then apply flipping data for each character individually (using <see cref="MoveFlipped"/>).
        /// </summary>
        /// <param name="stringPos">The position that the string is drawn at.</param>
        /// <param name="charPosOffset">The offset from the <paramref name="stringPos"/> that the current character is drawn at.</param>
        /// <param name="rotation">The rotation to draw with.</param>
        /// <param name="origin">The origin to subtract from the position.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="effects">The flipping to draw with.</param>
        /// <param name="stringSize">The size of the string, which is only used when <paramref name="effects"/> is not <see cref="SpriteEffects.None"/>.</param>
        /// <param name="charSize">The size of the current character, which is only used when <paramref name="effects"/> is not <see cref="SpriteEffects.None"/>.</param>
        /// <returns>The transformed final draw position.</returns>
        public Vector2 TransformSingleCharacter(Vector2 stringPos, Vector2 charPosOffset, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            return Vector2.Transform(this.MoveFlipped(charPosOffset, effects, charSize), this.CalculateStringTransform(stringPos, rotation, origin, scale, effects, stringSize));
        }

        private Vector2 MeasureString(CodePointSource text, bool ignoreTrailingSpaces) {
            var size = Vector2.Zero;
            if (text.Length <= 0)
                return size;
            var xOffset = 0F;
            var index = 0;
            while (index < text.Length) {
                var (codePoint, length) = text.GetCodePoint(index);
                switch (codePoint) {
                    case '\n':
                        xOffset = 0;
                        size.Y += this.LineHeight;
                        break;
                    case GenericFont.Emsp:
                        xOffset += this.LineHeight;
                        break;
                    case GenericFont.Nbsp:
                        xOffset += this.MeasureCharacter(' ');
                        break;
                    case GenericFont.Zwsp:
                        // don't add width for a zero-width space
                        break;
                    case ' ':
                        if (ignoreTrailingSpaces && GenericFont.IsTrailingSpace(text, index)) {
                            // if this is a trailing space, we can skip remaining spaces too
                            index = text.Length - 1;
                            break;
                        }
                        xOffset += this.MeasureCharacter(' ');
                        break;
                    default:
                        xOffset += this.MeasureCharacter(codePoint);
                        break;
                }
                // increase x size if this line is the longest
                if (xOffset > size.X)
                    size.X = xOffset;
                index += length;
            }
            // include the last line's height too!
            size.Y += this.LineHeight;
            return size;
        }

        private void DrawString(SpriteBatch batch, CodePointSource text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            var flipSize = effects != SpriteEffects.None ? this.MeasureString(text, false) : Vector2.Zero;
            var trans = this.CalculateStringTransform(position, rotation, origin, scale, effects, flipSize);

            var offset = Vector2.Zero;
            var index = 0;
            while (index < text.Length) {
                var (codePoint, length) = text.GetCodePoint(index);
                if (codePoint == '\n') {
                    offset.X = 0;
                    offset.Y += this.LineHeight;
                } else {
                    var character = CodePointSource.ToString(codePoint);
                    var charSize = this.MeasureString(character);
                    var charPos = Vector2.Transform(this.MoveFlipped(offset, effects, charSize), trans);
                    this.DrawCharacter(batch, codePoint, character, charPos, color, rotation, scale, effects, layerDepth);
                    offset.X += charSize.X;
                }
                index += length;
            }
        }

        internal static IEnumerable<IEnumerable<string>> SplitStringSeparate(IEnumerable<DecoratedCodePointSource> text, float maxWidth, float scale) {
            var currWidth = 0F;
            var lastSpacePart = -1;
            var lastSpaceIndex = -1;
            var widthSinceLastSpace = 0F;
            var curr = new StringBuilder();
            var fullSplit = new List<List<string>>();
            foreach (var part in text) {
                var partSplit = new List<string>();
                AddWidth(partSplit, part.ExtraWidth * scale, true);

                var index = 0;
                while (index < part.Source.Length) {
                    var (codePoint, length) = part.Source.GetCodePoint(index);
                    if (codePoint == '\n') {
                        // fake split at pre-defined new lines
                        curr.Append('\n');
                        lastSpacePart = -1;
                        lastSpaceIndex = -1;
                        widthSinceLastSpace = 0;
                        currWidth = 0;
                    } else {
                        var character = CodePointSource.ToString(codePoint);
                        var charWidth = part.Font.MeasureString(character).X * scale;
                        if (codePoint == ' ' || codePoint == GenericFont.Emsp || codePoint == GenericFont.Zwsp) {
                            // remember the location of this (breaking!) space
                            lastSpacePart = fullSplit.Count;
                            lastSpaceIndex = curr.Length;
                            widthSinceLastSpace = 0;
                            // we never want to insert a line break before a space!
                            AddWidth(partSplit, charWidth, false);
                        } else {
                            AddWidth(partSplit, charWidth, true);
                        }
                        curr.Append(character);
                    }
                    index += length;
                }

                if (curr.Length > 0) {
                    partSplit.Add(curr.ToString());
                    curr.Clear();
                }
                fullSplit.Add(partSplit);
            }
            return fullSplit;

            void AddWidth(ICollection<string> partSplit, float width, bool canBreakHere) {
                if (canBreakHere && currWidth + width >= maxWidth) {
                    // check if this line contains a space
                    if (lastSpaceIndex < 0) {
                        // if there is no last space, the word is longer than a line so we split here
                        partSplit.Add(curr.ToString());
                        curr.Clear();
                        currWidth = 0;
                    } else {
                        if (lastSpacePart < fullSplit.Count) {
                            // the last space exists, but isn't a part of curr, so we have to backtrack and split the previous token
                            var prevPart = fullSplit[lastSpacePart];
                            var prevCurr = prevPart[prevPart.Count - 1];
                            prevPart[prevPart.Count - 1] = prevCurr.Substring(0, lastSpaceIndex + 1);
                            prevPart.Add(prevCurr.Substring(lastSpaceIndex + 1));
                        } else {
                            // split after the last space
                            partSplit.Add(curr.ToString().Substring(0, lastSpaceIndex + 1));
                            curr.Remove(0, lastSpaceIndex + 1);
                        }
                        // we need to restore the width accumulated since the last space for the new line
                        currWidth = widthSinceLastSpace;
                    }
                    widthSinceLastSpace = 0;
                    lastSpacePart = -1;
                    lastSpaceIndex = -1;
                }

                currWidth += width;
                widthSinceLastSpace += width;
            }
        }

        internal static IEnumerable<StringBuilder> TruncateString(IEnumerable<DecoratedCodePointSource> text, float maxWidth, float scale, bool fromBack, string ellipsis) {
            var total = new StringBuilder();
            var extraWidth = 0F;
            var endReached = false;
            foreach (var part in fromBack ? text.Reverse() : text) {
                var curr = new StringBuilder();
                // if we reached the end previously, all the other parts should just be empty
                if (!endReached) {
                    extraWidth += part.ExtraWidth * scale;
                    var index = 0;
                    while (index < part.Source.Length) {
                        var innerIndex = fromBack ? part.Source.Length - 1 - index : index;
                        var (codePoint, length) = part.Source.GetCodePoint(innerIndex, fromBack);
                        var character = CodePointSource.ToString(codePoint);
                        if (fromBack) {
                            curr.Insert(0, character);
                            total.Insert(0, character);
                        } else {
                            curr.Append(character);
                            total.Append(character);
                        }

                        if (part.Font.MeasureString(new CodePointSource(total + ellipsis), false).X * scale + extraWidth >= maxWidth) {
                            if (fromBack) {
                                curr.Remove(0, length).Insert(0, ellipsis);
                                total.Remove(0, length).Insert(0, ellipsis);
                            } else {
                                curr.Remove(curr.Length - length, length).Append(ellipsis);
                                total.Remove(total.Length - length, length).Append(ellipsis);
                            }
                            endReached = true;
                            break;
                        }
                        index += length;
                    }
                }
                yield return curr;
            }
        }

        private static bool IsTrailingSpace(CodePointSource s, int index) {
            while (index < s.Length) {
                var (codePoint, length) = s.GetCodePoint(index);
                if (codePoint != ' ')
                    return false;
                index += length;
            }
            return true;
        }

        internal readonly struct DecoratedCodePointSource {

            public readonly CodePointSource Source;
            public readonly GenericFont Font;
            public readonly float ExtraWidth;

            public DecoratedCodePointSource(CodePointSource source, GenericFont font, float extraWidth) {
                this.Source = source;
                this.Font = font;
                this.ExtraWidth = extraWidth;
            }

        }

    }
}
