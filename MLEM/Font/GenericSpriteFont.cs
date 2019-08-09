using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Font {
    public class GenericSpriteFont : IGenericFont {

        public readonly SpriteFont Font;

        public GenericSpriteFont(SpriteFont font) {
            this.Font = font;
        }

        public static implicit operator GenericSpriteFont(SpriteFont font) {
            return new GenericSpriteFont(font);
        }

        public Vector2 MeasureString(string text) {
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

        public IEnumerable<string> SplitString(string text, float width, float scale) {
            return this.Font.SplitString(text, width, scale);
        }

    }
}