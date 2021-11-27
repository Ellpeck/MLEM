using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
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
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString"/>.
        /// </summary>
        public const char OneEmSpace = '\u2003';
        /// <summary>
        /// This field holds the unicode representation of a non-breaking space.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString"/>.
        /// </summary>
        public const char Nbsp = '\u00A0';
        /// <summary>
        /// This field holds the unicode representation of a zero-width space.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations and string splitting, generic fonts implicitly support it in <see cref="MeasureString"/> and <see cref="SplitString"/>.
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
        /// Measures the width of the given character with the default scale for use in <see cref="MeasureString"/>.
        /// Note that this method does not support <see cref="Nbsp"/>, <see cref="Zwsp"/> and <see cref="OneEmSpace"/> for most generic fonts, which is why <see cref="MeasureString"/> should be used even for single characters.
        /// </summary>
        /// <param name="c">The character whose width to calculate</param>
        /// <returns>The width of the given character with the default scale</returns>
        protected abstract float MeasureChar(char c);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

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
        /// This method uses <see cref="MeasureChar"/> internally to calculate the size of known characters and calculates additional characters like <see cref="Nbsp"/>, <see cref="Zwsp"/> and <see cref="OneEmSpace"/>.
        /// If the text contains newline characters (\n), the size returned will represent a rectangle that encompasses the width of the longest line and the string's full height.
        /// </summary>
        /// <param name="text">The text whose size to calculate</param>
        /// <param name="ignoreTrailingSpaces">Whether trailing whitespace should be ignored in the returned size, causing the end of each line to be effectively trimmed</param>
        /// <returns>The size of the string when drawn with this font</returns>
        public Vector2 MeasureString(string text, bool ignoreTrailingSpaces = false) {
            return MeasureString(i => this, text, ignoreTrailingSpaces);
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
            return TruncateString(i => this, text, width, scale, fromBack, ellipsis);
        }

        /// <summary>
        /// Splits a string to a given maximum width, adding newline characters between each line.
        /// Also splits long words and supports zero-width spaces and takes into account existing newline characters in the passed <paramref name="text"/>.
        /// See <see cref="SplitStringSeparate"/> for a method that differentiates between existing newline characters and splits due to maximum width.
        /// </summary>
        /// <param name="text">The text to split into multiple lines</param>
        /// <param name="width">The maximum width that each line should have</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <returns>The split string, containing newline characters at each new line</returns>
        public string SplitString(string text, float width, float scale) {
            return string.Join("\n", this.SplitStringSeparate(text, width, scale));
        }

        /// <summary>
        /// Splits a string to a given maximum width and returns each split section as a separate string.
        /// Note that existing new lines are taken into account for line length, but not split in the resulting strings.
        /// This method differs from <see cref="SplitString"/> in that it differentiates between pre-existing newline characters and splits due to maximum width.
        /// </summary>
        /// <param name="text">The text to split into multiple lines</param>
        /// <param name="width">The maximum width that each line should have</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <returns>The split string as an enumerable of split sections</returns>
        public IEnumerable<string> SplitStringSeparate(string text, float width, float scale) {
            return SplitStringSeparate(i => this, text, width, scale);
        }

        internal static Vector2 MeasureString(Func<int, GenericFont> fontFunction, string text, bool ignoreTrailingSpaces) {
            var size = Vector2.Zero;
            if (text.Length <= 0)
                return size;
            var xOffset = 0F;
            for (var i = 0; i < text.Length; i++) {
                var font = fontFunction(i);
                switch (text[i]) {
                    case '\n':
                        xOffset = 0;
                        size.Y += font.LineHeight;
                        break;
                    case OneEmSpace:
                        xOffset += font.LineHeight;
                        break;
                    case Nbsp:
                        xOffset += font.MeasureChar(' ');
                        break;
                    case Zwsp:
                        // don't add width for a zero-width space
                        break;
                    case ' ':
                        if (ignoreTrailingSpaces && IsTrailingSpace(text, i)) {
                            // if this is a trailing space, we can skip remaining spaces too
                            i = text.Length - 1;
                            break;
                        }
                        xOffset += font.MeasureChar(' ');
                        break;
                    default:
                        xOffset += font.MeasureChar(text[i]);
                        break;
                }
                // increase x size if this line is the longest
                if (xOffset > size.X)
                    size.X = xOffset;
            }
            // include the last line's height too!
            size.Y += fontFunction(text.Length - 1).LineHeight;
            return size;
        }

        internal static string TruncateString(Func<int, GenericFont> fontFunction, string text, float width, float scale, bool fromBack, string ellipsis) {
            var total = new StringBuilder();
            for (var i = 0; i < text.Length; i++) {
                if (fromBack) {
                    total.Insert(0, text[text.Length - 1 - i]);
                } else {
                    total.Append(text[i]);
                }

                if (fontFunction(i).MeasureString(total + ellipsis).X * scale >= width) {
                    if (fromBack) {
                        return total.Remove(0, 1).Insert(0, ellipsis).ToString();
                    } else {
                        return total.Remove(total.Length - 1, 1).Append(ellipsis).ToString();
                    }
                }
            }
            return total.ToString();
        }

        internal static IEnumerable<string> SplitStringSeparate(Func<int, GenericFont> fontFunction, string text, float width, float scale) {
            var currWidth = 0F;
            var lastSpaceIndex = -1;
            var widthSinceLastSpace = 0F;
            var curr = new StringBuilder();
            for (var i = 0; i < text.Length; i++) {
                var c = text[i];
                if (c == '\n') {
                    // fake split at pre-defined new lines
                    curr.Append(c);
                    lastSpaceIndex = -1;
                    widthSinceLastSpace = 0;
                    currWidth = 0;
                } else {
                    var cWidth = fontFunction(i).MeasureString(c.ToCachedString()).X * scale;
                    if (c == ' ' || c == OneEmSpace || c == Zwsp) {
                        // remember the location of this (breaking!) space
                        lastSpaceIndex = curr.Length;
                        widthSinceLastSpace = 0;
                    } else if (currWidth + cWidth >= width) {
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
                    currWidth += cWidth;
                    widthSinceLastSpace += cWidth;
                    curr.Append(c);
                }
            }
            if (curr.Length > 0)
                yield return curr.ToString();
        }

        private static bool IsTrailingSpace(string s, int index) {
            for (var i = index + 1; i < s.Length; i++) {
                if (s[i] != ' ')
                    return false;
            }
            return true;
        }

    }
}