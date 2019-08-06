using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Extensions {
    public static class SpriteBatchExtensions {

        public static void DrawCenteredString(this SpriteBatch batch, BitmapFont font, string text, Vector2 position, float scale, Color color, bool horizontal = true, bool vertical = false, float addedScale = 0) {
            var size = font.MeasureString(text);
            var center = new Vector2(
                horizontal ? size.Width * scale / 2F : 0,
                vertical ? size.Height * scale / 2F : 0);
            batch.DrawString(font, text,
                position + (Vector2) size * scale / 2 - center,
                color, 0, size / 2, scale + addedScale, SpriteEffects.None, 0);
        }


    }
}