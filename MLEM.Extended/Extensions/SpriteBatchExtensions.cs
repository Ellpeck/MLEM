using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using MonoGame.Extended;
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

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture, destinationRectangle.ToMlem(), sourceRectangle, color, rotation, origin, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, null, color);
        }


    }
}