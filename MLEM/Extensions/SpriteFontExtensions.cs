using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class SpriteFontExtensions {

        public static string SplitString(this SpriteFont font, string text, float width, float scale) {
            return SplitString(s => font.MeasureString(s).X, text, width, scale);
        }

        public static string TruncateString(this SpriteFont font, string text, float width, float scale, bool fromBack = false) {
            return TruncateString(s => font.MeasureString(s).X, text, width, scale, fromBack);
        }

        public static string TruncateString(Func<StringBuilder, float> widthFunc, string text, float width, float scale, bool fromBack = false) {
            var total = new StringBuilder();
            for (var i = 0; i < text.Length; i++) {
                if (fromBack) {
                    total.Insert(0, text[text.Length - 1 - i]);
                } else {
                    total.Append(text[i]);
                }

                if (widthFunc(total) * scale >= width)
                    return total.ToString(fromBack ? 1 : 0, total.Length - 1);
            }
            return total.ToString();
        }

        public static string SplitString(Func<string, float> widthFunc, string text, float width, float scale) {
            var total = new StringBuilder();
            foreach (var line in text.Split('\n')) {
                var curr = new StringBuilder();
                foreach (var word in line.Split(' ')) {
                    if (widthFunc(word) * scale >= width) {
                        if (curr.Length > 0) {
                            total.Append(curr).Append('\n');
                            curr.Clear();
                        }
                        var wordBuilder = new StringBuilder();
                        for (var i = 0; i < word.Length; i++) {
                            wordBuilder.Append(word[i]);
                            if (widthFunc(wordBuilder.ToString()) * scale >= width) {
                                total.Append(wordBuilder.ToString(0, wordBuilder.Length - 1)).Append('\n');
                                wordBuilder.Remove(0, wordBuilder.Length - 1);
                            }
                        }
                        curr.Append(wordBuilder).Append(' ');
                    } else {
                        curr.Append(word).Append(' ');
                        if (widthFunc(curr.ToString()) * scale >= width) {
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
}