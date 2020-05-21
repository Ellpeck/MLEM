using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="SpriteBatch"/>
    /// </summary>
    public static class SpriteBatchExtensions {

        private static Texture2D blankTexture;

        /// <summary>
        /// Returns a 1x1 pixel white texture that can be used for drawing solid color shapes.
        /// This texture is automatically disposed of when the batch is disposed.
        /// </summary>
        /// <param name="batch">The sprite batch</param>
        /// <returns>A 1x1 pixel white texture</returns>
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

        /// <summary>
        /// Generates a <see cref="NinePatch"/> that has a texture with a given color and outline color
        /// </summary>
        /// <param name="batch">The sprite batch</param>
        /// <param name="color">The fill color of the texture</param>
        /// <param name="outlineColor">The outline color of the texture</param>
        /// <returns>A <see cref="NinePatch"/> containing a 3x3 texture with an outline</returns>
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

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            var source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
            var scale = new Vector2(1F / source.Width, 1F / source.Height) * destinationRectangle.Size;
            batch.Draw(texture, destinationRectangle.Location, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color)"/>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D,Rectangle,Color)"/>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, null, color);
        }

    }
}