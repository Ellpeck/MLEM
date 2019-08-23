using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Extensions {
    public static class BitmapFontExtensions {

        public static IEnumerable<string> SplitString(this BitmapFont font, string text, float width, float scale) {
            var builder = new StringBuilder();
            foreach (var line in text.Split('\n')) {
                foreach (var word in line.Split(' ')) {
                    builder.Append(word).Append(' ');
                    if (font.MeasureString(builder).Width * scale >= width) {
                        var len = builder.Length - word.Length - 1;
                        yield return builder.ToString(0, len - 1);
                        builder.Remove(0, len);
                    }
                }
                yield return builder.ToString(0, builder.Length - 1);
                builder.Clear();
            }
        }

    }
}