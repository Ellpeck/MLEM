using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class SpriteFontExtensions {

        public static IEnumerable<string> SplitString(this SpriteFont font, string text, float width, float scale) {
            var builder = new StringBuilder();
            foreach (var line in text.Split('\n')) {
                foreach (var word in line.Split(' ')) {
                    builder.Append(word).Append(' ');
                    if (font.MeasureString(builder).X * scale >= width) {
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