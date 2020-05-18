using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extended.Extensions;
using MLEM.Font;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Font {
    public class GenericBitmapFont : GenericFont {

        public readonly BitmapFont Font;
        public override GenericFont Bold { get; }
        public override GenericFont Italic { get; }
        public override float LineHeight => this.Font.LineHeight;

        public GenericBitmapFont(BitmapFont font, BitmapFont bold = null, BitmapFont italic = null) {
            this.Font = font;
            this.Bold = bold != null ? new GenericBitmapFont(bold) : this;
            this.Italic = italic != null ? new GenericBitmapFont(italic) : this;
        }

        public override Vector2 MeasureString(string text) {
            if (text.Length == 1 && this.SingleCharacterWidthFix(text, out var size))
                return size;
            return this.Font.MeasureString(text);
        }

        public override Vector2 MeasureString(StringBuilder text) {
            if (text.Length == 1 && this.SingleCharacterWidthFix(text.ToString(), out var size))
                return size;
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

        // this fixes an issue with BitmapFonts where, if only given a single character,
        // only the width of the character itself (disregarding spacing) is returned
        private bool SingleCharacterWidthFix(string text, out Vector2 size) {
            var codePoint = char.ConvertToUtf32(text, 0);
            var region = this.Font.GetCharacterRegion(codePoint);
            if (region != null) {
                size = new Vector2(region.XAdvance, region.Height);
                return true;
            }
            size = default;
            return false;
        }

    }
}