using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="SpriteBatch"/> and <see cref="RectangleF"/> in combination.
    /// </summary>
    public static class SpriteBatchExtensions {

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D,Rectangle,Rectangle?,Color,float,Vector2,SpriteEffects,float)"/>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture, destinationRectangle.ToMlem(), sourceRectangle, color, rotation, origin, effects, layerDepth);
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