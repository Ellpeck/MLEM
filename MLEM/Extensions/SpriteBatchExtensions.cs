using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Extensions {
    public static class SpriteBatchExtensions {

        private static Texture2D blankTexture;

        public static Texture2D GetBlankTexture(this SpriteBatch batch) {
            if (blankTexture == null) {
                blankTexture = new Texture2D(batch.GraphicsDevice, 1, 1);
                blankTexture.SetData(new[] {Color.White});
                batch.Disposing += (sender, args) => {
                    if (blankTexture != null) {
                        blankTexture.Dispose();
                        blankTexture = null;
                    }
                };
            }
            return blankTexture;
        }

        public static NinePatch GenerateTexture(this SpriteBatch batch, Color color, Color? outlineColor = null) {
            var outli = outlineColor ?? Color.Black;
            var tex = new Texture2D(batch.GraphicsDevice, 3, 3);
            tex.SetData(new[] {
                outli, outli, outli,
                outli, color, outli,
                outli, outli, outli
            });
            batch.Disposing += (sender, args) => {
                if (tex != null) {
                    tex.Dispose();
                    tex = null;
                }
            };
            return new NinePatch(tex, 1);
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

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            var source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
            var scale = new Vector2(1F / source.Width, 1F / source.Height) * destinationRectangle.Size;
            batch.Draw(texture, destinationRectangle.Location, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, null, color);
        }

    }
}