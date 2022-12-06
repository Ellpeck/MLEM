using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        /// <inheritdoc cref="Emsp"/>
        [Obsolete("Use the Emsp field instead.")]
        public const char OneEmSpace = GenericFont.Emsp;
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
        /// Note that this method is only called internally.
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
        protected abstract void DrawCharacter(SpriteBatch batch, int codePoint, string character, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth);

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
            return this.MeasureString(new CodePointSource(text), ignoreTrailingSpaces, null, null);
        }

        /// <inheritdoc cref="MeasureString(string,bool)"/>
        public Vector2 MeasureString(StringBuilder text, bool ignoreTrailingSpaces = false) {
            return this.MeasureString(new CodePointSource(text), ignoreTrailingSpaces, null, null);
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
            return this.TruncateString(new CodePointSource(text), width, scale, fromBack, ellipsis, null, null).ToString();
        }

        /// <inheritdoc cref="TruncateString(string,float,float,bool,string)"/>
        public StringBuilder TruncateString(StringBuilder text, float width, float scale, bool fromBack = false, string ellipsis = "") {
            return this.TruncateString(new CodePointSource(text), width, scale, fromBack, ellipsis, null, null);
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
            return this.SplitStringSeparate(new CodePointSource(text), width, scale, null, null);
        }

        /// <inheritdoc cref="SplitStringSeparate(string,float,float)"/>
        public IEnumerable<string> SplitStringSeparate(StringBuilder text, float width, float scale) {
            return this.SplitStringSeparate(new CodePointSource(text), width, scale, null, null);
        }

        internal Vector2 MeasureString(CodePointSource text, bool ignoreTrailingSpaces, Func<int, GenericFont> fontFunction, Func<int, float> extraWidthFunction) {
            var size = Vector2.Zero;
            if (text.Length <= 0)
                return size;
            var xOffset = 0F;
            var index = 0;
            while (index < text.Length) {
                xOffset += extraWidthFunction?.Invoke(index) ?? 0;
                var font = fontFunction?.Invoke(index) ?? this;
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
                        xOffset += font.MeasureCharacter(' ');
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
                        xOffset += font.MeasureCharacter(' ');
                        break;
                    default:
                        xOffset += font.MeasureCharacter(codePoint);
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

        internal StringBuilder TruncateString(CodePointSource text, float width, float scale, bool fromBack, string ellipsis, Func<int, GenericFont> fontFunction, Func<int, float> extraWidthFunction) {
            var total = new StringBuilder();
            var index = 0;
            while (index < text.Length) {
                var innerIndex = fromBack ? text.Length - 1 - index : index;
                var (codePoint, length) = text.GetCodePoint(innerIndex, fromBack);
                if (fromBack) {
                    total.Insert(0, CodePointSource.ToString(codePoint));
                } else {
                    total.Append(CodePointSource.ToString(codePoint));
                }

                if (this.MeasureString(new CodePointSource(total + ellipsis), false, fontFunction, extraWidthFunction).X * scale >= width) {
                    if (fromBack) {
                        return total.Remove(0, length).Insert(0, ellipsis);
                    } else {
                        return total.Remove(total.Length - length, length).Append(ellipsis);
                    }
                }
                index += length;
            }
            return total;
        }

        internal IEnumerable<string> SplitStringSeparate(CodePointSource text, float width, float scale, Func<int, GenericFont> fontFunction, Func<int, float> extraWidthFunction) {
            var currWidth = 0F;
            var lastSpaceIndex = -1;
            var widthSinceLastSpace = 0F;
            var curr = new StringBuilder();
            var index = 0;
            while (index < text.Length) {
                var (codePoint, length) = text.GetCodePoint(index);
                if (codePoint == '\n') {
                    // fake split at pre-defined new lines
                    curr.Append('\n');
                    lastSpaceIndex = -1;
                    widthSinceLastSpace = 0;
                    currWidth = 0;
                } else {
                    var font = fontFunction?.Invoke(index) ?? this;
                    var character = CodePointSource.ToString(codePoint);
                    var charWidth = (font.MeasureString(character).X + (extraWidthFunction?.Invoke(index) ?? 0)) * scale;
                    if (codePoint == ' ' || codePoint == GenericFont.Emsp || codePoint == GenericFont.Zwsp) {
                        // remember the location of this (breaking!) space
                        lastSpaceIndex = curr.Length;
                        widthSinceLastSpace = 0;
                    } else if (currWidth + charWidth >= width) {
                        // check if this line contains a space
                        if (lastSpaceIndex < 0) {
                            // if there is no last space, the word is longer than a line so we split here
                            yield return curr.ToString();
                            currWidth = 0;
                            curr.Clear();
                        } else {
                            // split after the last space
                            yield return curr.ToString().Substring(0, lastSpaceIndex + 1);
                            curr.Remove(0, lastSpaceIndex + 1);
                            // we need to restore the width accumulated since the last space for the new line
                            currWidth = widthSinceLastSpace;
                        }
                        widthSinceLastSpace = 0;
                        lastSpaceIndex = -1;
                    }

                    // add current character
                    currWidth += charWidth;
                    widthSinceLastSpace += charWidth;
                    curr.Append(character);
                }
                index += length;
            }
            if (curr.Length > 0)
                yield return curr.ToString();
        }

        private void DrawString(SpriteBatch batch, CodePointSource text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            var (flipX, flipY) = (0F, 0F);
            var flippedV = (effects & SpriteEffects.FlipVertically) != 0;
            var flippedH = (effects & SpriteEffects.FlipHorizontally) != 0;
            if (flippedV || flippedH) {
                var size = this.MeasureString(text, false, null, null);
                if (flippedH) {
                    origin.X *= -1;
                    flipX = -size.X;
                }
                if (flippedV) {
                    origin.Y *= -1;
                    flipY = this.LineHeight - size.Y;
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

                    var charPos = offset;
                    if (flippedH)
                        charPos.X += charSize.X;
                    if (flippedV)
                        charPos.Y += charSize.Y - this.LineHeight;
                    Vector2.Transform(ref charPos, ref trans, out charPos);

                    this.DrawCharacter(batch, codePoint, character, charPos, color, rotation, scale, effects, layerDepth);
                    offset.X += charSize.X;
                }
                index += length;
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

    }
}
