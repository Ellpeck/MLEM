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
                AutoDispose(batch, blankTexture);
            }
            return blankTexture;
        }

        /// <summary>
        /// Generates a <see cref="NinePatch"/> that has a texture with a given color and outline color.
        /// This texture is automatically disposed of when the batch is disposed.
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
            AutoDispose(batch, tex);
            return new NinePatch(tex, 1);
        }

        /// <summary>
        /// Generates a 1x1 texture with the given color.
        /// This texture is automatically disposed of when the batch is disposed.
        /// </summary>
        /// <param name="batch">The sprite batch</param>
        /// <param name="color">The color of the texture</param>
        /// <returns>A new texture with the given data</returns>
        public static Texture2D GenerateSquareTexture(this SpriteBatch batch, Color color) {
            var tex = new Texture2D(batch.GraphicsDevice, 1, 1);
            tex.SetData(new[] {color});
            AutoDispose(batch, tex);
            return tex;
        }

        /// <summary>
        /// Generates a texture with the given size that contains a circle.
        /// The circle's center will be the center of the texture, and the circle will lead up to the edges of the texture.
        /// This texture is automatically disposed of when the batch is disposed.
        /// </summary>
        /// <param name="batch">The sprite batch</param>
        /// <param name="color">The color of the texture</param>
        /// <param name="size">The width and height of the texture, and the diameter of the circle</param>
        /// <returns>A new texture with the given data</returns>
        public static Texture2D GenerateCircleTexture(this SpriteBatch batch, Color color, int size) {
            var tex = new Texture2D(batch.GraphicsDevice, size, size);
            using (var data = tex.GetTextureData()) {
                for (var x = 0; x < tex.Width; x++) {
                    for (var y = 0; y < tex.Height; y++) {
                        var dist = Vector2.Distance(new Vector2(size / 2), new Vector2(x, y));
                        data[x, y] = dist <= size / 2 ? color : Color.Transparent;
                    }
                }
            }
            AutoDispose(batch, tex);
            return tex;
        }

        /// <summary>
        /// Generates a texture with the given size that contains a gradient between the four specified corner colors.
        /// If the same color is specified for two pairs of corners, a horizontal, vertical or diagonal gradient can be achieved.
        /// This texture is automatically disposed of when the batch is disposed.
        /// </summary>
        /// <param name="batch">The sprite batch</param>
        /// <param name="topLeft">The color of the texture's top left corner</param>
        /// <param name="topRight">The color of the texture's top right corner</param>
        /// <param name="bottomLeft">The color of the texture's bottom left corner</param>
        /// <param name="bottomRight">The color of the texture's bottom right corner</param>
        /// <param name="width">The width of the resulting texture, or 256 by default</param>
        /// <param name="height">The height of the resulting texture, or 256 by default</param>
        /// <returns>A new texture with the given data</returns>
        public static Texture2D GenerateGradientTexture(this SpriteBatch batch, Color topLeft, Color topRight, Color bottomLeft, Color bottomRight, int width = 256, int height = 256) {
            var tex = new Texture2D(batch.GraphicsDevice, width, height);
            using (var data = tex.GetTextureData()) {
                for (var x = 0; x < width; x++) {
                    var top = Color.Lerp(topLeft, topRight, x / (float) width);
                    var btm = Color.Lerp(bottomLeft, bottomRight, x / (float) width);
                    for (var y = 0; y < height; y++)
                        data[x, y] = Color.Lerp(top, btm, y / (float) height);
                }
            }
            AutoDispose(batch, tex);
            return tex;
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

        /// <inheritdoc cref="StaticSpriteBatch.Add(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>
        public static void Add(this StaticSpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            var source = sourceRectangle ?? new Rectangle(0, 0, texture.Width, texture.Height);
            var scale = new Vector2(1F / source.Width, 1F / source.Height) * destinationRectangle.Size;
            batch.Add(texture, destinationRectangle.Location, sourceRectangle, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc cref="StaticSpriteBatch.Add(Texture2D,Rectangle,Rectangle?,Color)"/>
        public static void Add(this StaticSpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color) {
            batch.Add(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <inheritdoc cref="StaticSpriteBatch.Add(Texture2D,Rectangle,Color)"/>
        public static void Draw(this StaticSpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Color color) {
            batch.Add(texture, destinationRectangle, null, color);
        }

        private static void AutoDispose(SpriteBatch batch, Texture2D texture) {
            batch.Disposing += (sender, ars) => {
                if (texture != null) {
                    texture.Dispose();
                    texture = null;
                }
            };
        }

    }
}