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
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString"/>.
        /// </summary>
        public const char OneEmSpace = '\u2003';
        /// <summary>
        /// This field holds the unicode representation of a non-breaking space.
        /// Whereas a regular <see cref="SpriteFont"/> would have to explicitly support this character for width calculations, generic fonts implicitly support it in <see cref="MeasureString"/>.
        /// </summary>
        public const char Nbsp = '\u00A0';

        /// <summary>
        /// The bold version of this font.
        /// </summary>
        public abstract GenericFont Bold { get; }

        /// <summary>
        /// The italic version of this font.
        /// </summary>
        public abstract GenericFont Italic { get; }

        ///<inheritdoc cref="SpriteFont.LineSpacing"/>
        public abstract float LineHeight { get; }

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteBatch.DrawString(SpriteFont,string,Vector2,Color,float,Vector2,float,SpriteEffects,float)"/>
        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        ///<inheritdoc cref="SpriteFont.MeasureString(string)"/>
        protected abstract Vector2 MeasureChar(char c);

        /// <summary>
        /// Draws a string with the given text alignment.
        /// </summary>
        /// <param name="batch">The sprite batch to use</param>
        /// <param name="text">The string to draw</param>
        /// <param name="position">The position of the top left corner of the string</param>
        /// <param name="align">The alignment to use</param>
        /// <param name="color">The color to use</param>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, TextAlign align, Color color) {
            this.DrawString(batch, text, position, align, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        ///<inheritdoc cref="DrawString(SpriteBatch,string,Vector2,TextAlign,Color)"/>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, TextAlign align, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            this.DrawString(batch, text, position, align, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        ///<inheritdoc cref="DrawString(SpriteBatch,string,Vector2,TextAlign,Color)"/>
        public void DrawString(SpriteBatch batch, string text, Vector2 position, TextAlign align, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            switch (align) {
                case TextAlign.Center:
                case TextAlign.CenterBothAxes:
                    var (w, h) = this.MeasureString(text) * scale;
                    position.X -= w / 2;
                    if (align == TextAlign.CenterBothAxes)
                        position.Y -= h / 2;
                    break;
                case TextAlign.Right:
                    position.X -= this.MeasureString(text).X * scale.X;
                    break;
            }
            this.DrawString(batch, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        ///<inheritdoc cref="SpriteFont.MeasureString(string)"/>
        public Vector2 MeasureString(string text) {
            var size = Vector2.Zero;
            if (text.Length <= 0)
                return size;
            var xOffset = 0F;
            foreach (var c in text) {
                switch (c) {
                    case '\n':
                        xOffset = 0;
                        size.Y += this.LineHeight;
                        break;
                    case OneEmSpace:
                        xOffset += this.LineHeight;
                        break;
                    case Nbsp:
                        xOffset += this.MeasureChar(' ').X;
                        break;
                    default:
                        xOffset += this.MeasureChar(c).X;
                        break;
                }
                // increase x size if this line is the longest
                if (xOffset > size.X)
                    size.X = xOffset;
            }
            // include the last line's height too!
            size.Y += this.LineHeight;
            return size;
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
            var total = new StringBuilder();
            var ellipsisWidth = this.MeasureString(ellipsis).X * scale;
            for (var i = 0; i < text.Length; i++) {
                if (fromBack) {
                    total.Insert(0, text[text.Length - 1 - i]);
                } else {
                    total.Append(text[i]);
                }

                if (this.MeasureString(total.ToString()).X * scale + ellipsisWidth >= width) {
                    if (fromBack) {
                        return total.Remove(0, 1).Insert(0, ellipsis).ToString();
                    } else {
                        return total.Remove(total.Length - 1, 1).Append(ellipsis).ToString();
                    }
                }
            }
            return total.ToString();
        }

        /// <summary>
        /// Splits a string to a given maximum width, adding newline characters between each line.
        /// Also splits long words and supports zero-width spaces.
        /// </summary>
        /// <param name="text">The text to split into multiple lines</param>
        /// <param name="width">The maximum width that each line should have</param>
        /// <param name="scale">The scale to use for width measurements</param>
        /// <returns>The split string, containing newline characters at each new line</returns>
        public string SplitString(string text, float width, float scale) {
            var total = new StringBuilder();
            foreach (var line in text.Split('\n')) {
                var curr = new StringBuilder();
                foreach (var word in line.Split(' ')) {
                    if (this.MeasureString(word).X * scale >= width) {
                        if (curr.Length > 0) {
                            total.Append(curr).Append('\n');
                            curr.Clear();
                        }
                        var wordBuilder = new StringBuilder();
                        for (var i = 0; i < word.Length; i++) {
                            wordBuilder.Append(word[i]);
                            if (this.MeasureString(wordBuilder.ToString()).X * scale >= width) {
                                total.Append(wordBuilder.ToString(0, wordBuilder.Length - 1)).Append('\n');
                                wordBuilder.Remove(0, wordBuilder.Length - 1);
                            }
                        }
                        curr.Append(wordBuilder).Append(' ');
                    } else {
                        curr.Append(word).Append(' ');
                        if (this.MeasureString(curr.ToString()).X * scale >= width) {
                            var len = curr.Length - word.Length - 1;
                            if (len > 0) {
                                total.Append(curr.ToString(0, len)).Append('\n');
                                curr.Remove(0, len);
                            }
                        }
                    }
                }
                total.Append(curr).Append('\n');
            }
            return total.ToString(0, total.Length - 2);
        }

    }

    /// <summary>
    /// An enum that represents the text alignment options for <see cref="GenericFont.DrawString(SpriteBatch,string,Vector2,TextAlign,Color)"/>
    /// </summary>
    public enum TextAlign {

        /// <summary>
        /// The text is aligned as normal
        /// </summary>
        Left,
        /// <summary>
        /// The position passed represents the center of the resulting string in the x axis
        /// </summary>
        Center,
        /// <summary>
        /// The position passed represents the right edge of the resulting string
        /// </summary>
        Right,
        /// <summary>
        /// The position passed represents the center of the resulting string, both in the x and y axes
        /// </summary>
        CenterBothAxes

    }
}