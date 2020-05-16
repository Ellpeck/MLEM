using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Font {
    public class GenericSpriteFont : GenericFont {

        public readonly SpriteFont Font;
        public override GenericFont Bold { get; }
        public override GenericFont Italic { get; }
        public override float LineHeight => this.Font.LineSpacing;

        public GenericSpriteFont(SpriteFont font, SpriteFont bold = null, SpriteFont italic = null) {
            this.Font = font;
            this.Bold = bold != null ? new GenericSpriteFont(bold) : this;
            this.Italic = italic != null ? new GenericSpriteFont(italic) : this;
        }

        public override Vector2 MeasureString(string text) {
            return this.Font.MeasureString(text);
        }

        public override Vector2 MeasureString(StringBuilder text) {
            return this.Font.MeasureString(text);
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
            batch.DrawString(this.Font, text, position, color);
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.DrawString(this.Font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }

    }
}