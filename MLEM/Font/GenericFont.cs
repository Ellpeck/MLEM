using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Font {
    public abstract class GenericFont {

        public abstract float LineHeight { get; }

        public abstract Vector2 MeasureString(string text);

        public abstract Vector2 MeasureString(StringBuilder text);

        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color);

        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        public abstract void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color);

        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth);

        public abstract void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth);

        public string TruncateString(string text, float width, float scale, bool fromBack = false) {
            var total = new StringBuilder();
            for (var i = 0; i < text.Length; i++) {
                if (fromBack) {
                    total.Insert(0, text[text.Length - 1 - i]);
                } else {
                    total.Append(text[i]);
                }

                if (this.MeasureString(total).X * scale >= width)
                    return total.ToString(fromBack ? 1 : 0, total.Length - 1);
            }
            return total.ToString();
        }

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

        public string GetWidthString(float width, char content = ' ') {
            var strg = content.ToString();
            while (this.MeasureString(strg).X < width)
                strg += content;
            return strg;
        }

    }
}