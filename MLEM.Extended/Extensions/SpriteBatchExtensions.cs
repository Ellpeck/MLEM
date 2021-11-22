using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MonoGame.Extended;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="SpriteBatch"/> and <see cref="RectangleF"/> in combination.
    /// </summary>
    public static class SpriteBatchExtensions {

        /// <summary>Submit a sprite for drawing in the current batch.</summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture, destinationRectangle.ToMlem(), sourceRectangle, color, rotation, origin, effects, layerDepth);
        }

        /// <summary>Submit a sprite for drawing in the current batch.</summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Rectangle? sourceRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <summary>Submit a sprite for drawing in the current batch.</summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public static void Draw(this SpriteBatch batch, Texture2D texture, RectangleF destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, null, color);
        }

        /// <summary>
        /// Draws a grid of tile outlines reminiscent of graph paper.
        /// </summary>
        /// <param name="batch">The sprite batch to draw with</param>
        /// <param name="start">The top left coordinate of the grid</param>
        /// <param name="tileSize">The size of each tile</param>
        /// <param name="tileCount">The amount of tiles in the x and y axes</param>
        /// <param name="gridColor">The color to draw the grid outlines in</param>
        /// <param name="gridThickness">The thickness of each grid line. Defaults to 1.</param>
        public static void DrawGrid(this SpriteBatch batch, Vector2 start, Vector2 tileSize, Point tileCount, Color gridColor, float gridThickness = 1) {
            for (var y = 0; y < tileCount.Y; y++) {
                for (var x = 0; x < tileCount.X; x++)
                    batch.DrawRectangle(start + new Vector2(x, y) * tileSize, tileSize, gridColor, gridThickness / 2);
            }

            var size = tileSize * tileCount.ToVector2() + new Vector2(gridThickness);
            batch.DrawRectangle(start - new Vector2(gridThickness / 2), size, gridColor, gridThickness / 2);
        }

    }
}