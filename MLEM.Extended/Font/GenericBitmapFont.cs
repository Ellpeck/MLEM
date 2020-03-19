using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Extensions;
using MLEM.Font;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Font {
    public class GenericBitmapFont : IGenericFont {

        public readonly BitmapFont Font;
        public float LineHeight => this.Font.LineHeight;

        public GenericBitmapFont(BitmapFont font) {
            this.Font = font;
        }

        public Vector2 MeasureString(string text) {
            // For some reason, Extended's bitmap fonts don't take kerning into
            // account when calculating the size of a single character
            if (text.Length == 1)
                text += ' ';
            return this.Font.MeasureString(text);
        }

        public Vector2 MeasureString(StringBuilder text) {
            return this.Font.MeasureString(text);
        }

        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public void DrawCenteredString(SpriteBatch batch, string text, Vector2 position, float scale, Color color, bool horizontal = true, bool vertical = false, float addedScale = 0) {
            batch.DrawCenteredString(this.Font, text, position, scale, color, horizontal, vertical, addedScale);
        }

        public string SplitString(string text, float width, float scale) {
            return this.Font.SplitString(text, width, scale);
        }

        public string TruncateString(string text, float width, float scale, bool fromBack = false) {
            return this.Font.TruncateString(text, width, scale, fromBack);
        }

    }
}