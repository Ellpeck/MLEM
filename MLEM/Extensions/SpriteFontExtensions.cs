using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class SpriteFontExtensions {

        public static string SplitString(this SpriteFont font, string text, float width, float scale) {
            return SplitString(s => font.MeasureString(s).X, text, width, scale);
        }

        public static string SplitString(Func<StringBuilder, float> widthFunc, string text, float width, float scale) {
            var total = new StringBuilder();
            foreach (var line in text.Split('\n')) {
                var curr = new StringBuilder();
                foreach (var word in line.Split(' ')) {
                    curr.Append(word).Append(' ');
                    if (widthFunc(curr) * scale >= width) {
                        var len = curr.Length - word.Length - 1;
                        total.Append(curr.ToString(0, len - 1)).Append('\n');
                        curr.Remove(0, len);
                    }
                }
                total.Append(curr.ToString(0, curr.Length - 1)).Append('\n');
            }
            return total.ToString(0, total.Length - 1);
        }

    }
}