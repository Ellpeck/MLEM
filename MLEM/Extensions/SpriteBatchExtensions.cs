using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class SpriteBatchExtensions {

        private static Texture2D blankTexture;

        public static Texture2D GetBlankTexture(SpriteBatch batch) {
            if (blankTexture == null) {
                blankTexture = new Texture2D(batch.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
                blankTexture.SetData(new[] {Color.White});
            }
            return blankTexture;
        }

        public static void DrawCenteredString(this SpriteBatch batch, SpriteFont font, string text, Vector2 position, float scale, Color color, bool horizontal = true, bool vertical = false, float addedScale = 0) {
            var size = font.MeasureString(text);
            var center = new Vector2(
                horizontal ? size.X * scale / 2F : 0,
                vertical ? size.Y * scale / 2F : 0);
            batch.DrawString(font, text,
                position + size * scale / 2 - center,
                color, 0, size / 2, scale + addedScale, SpriteEffects.None, 0);
        }

    }
}