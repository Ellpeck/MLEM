using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Misc {
    /// <summary>
    /// This class contains a <see cref="DrawAutoTile"/> method that allows users to easily draw a tile with automatic connections.
    /// For auto-tiling in this manner to work, auto-tiled textures have to be laid out in a format described in <see cref="DrawAutoTile"/>.
    /// Note that <see cref="StaticSpriteBatch"/> can also be used for drawing by using the <see cref="AddAutoTile"/> method instead.
    /// </summary>
    public static class AutoTiling {

        /// <summary>
        /// This method allows for a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        ///
        /// For auto-tiling in this way to work, the tiles have to be laid out as follows: five tiles aligned horizontally within the texture file, with the following information:
        /// <list type="number">
        /// <item><description>The texture used for filling big areas</description></item>
        /// <item><description>The texture used for straight, horizontal borders, with the borders facing away from the center</description></item>
        /// <item><description>The texture used for outer corners, with the corners facing away from the center</description></item>
        /// <item><description>The texture used for straight, vertical borders, with the borders facing away from the center</description></item>
        /// <item><description>The texture used for inner corners, with the corners facing away from the center</description></item>
        /// </list>
        /// For more information and an example, see https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs#L20-L28.
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="pos">The position to draw at.</param>
        /// <param name="texture">The texture to use for drawing.</param>
        /// <param name="textureRegion">The location of the first texture region, as described in the summary.</param>
        /// <param name="connectsTo">A function that determines whether two positions should connect.</param>
        /// <param name="color">The color to draw with.</param>
        /// <param name="origin">The origin to draw from.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="layerDepth">The layer depth to draw with.</param>
        public static void DrawAutoTile(SpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color color, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = CalculateAutoTile(pos, textureRegion, connectsTo, sc);
            batch.Draw(texture, p1, r1, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, p2, r2, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, p3, r3, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, p4, r4, color, 0, orig, sc, SpriteEffects.None, layerDepth);
        }

        /// <inheritdoc cref="DrawAutoTile"/>
        public static void AddAutoTile(StaticSpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color color, float rotation = 0, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = CalculateAutoTile(pos, textureRegion, connectsTo, sc);
            batch.Add(texture, p1, r1, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p2, r2, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p3, r3, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p4, r4, color, 0, orig, sc, SpriteEffects.None, layerDepth);
        }

        private static (Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle) CalculateAutoTile(Vector2 pos, Rectangle textureRegion, ConnectsTo connectsTo, Vector2 scale) {
            var up = connectsTo(0, -1);
            var down = connectsTo(0, 1);
            var left = connectsTo(-1, 0);
            var right = connectsTo(1, 0);
            var xUl = up && left ? connectsTo(-1, -1) ? 0 : 4 : left ? 1 : up ? 3 : 2;
            var xUr = up && right ? connectsTo(1, -1) ? 0 : 4 : right ? 1 : up ? 3 : 2;
            var xDl = down && left ? connectsTo(-1, 1) ? 0 : 4 : left ? 1 : down ? 3 : 2;
            var xDr = down && right ? connectsTo(1, 1) ? 0 : 4 : right ? 1 : down ? 3 : 2;

            var (w, h) = textureRegion.Size;
            var (w2, h2) = new Point(w / 2, h / 2);

            return (
                new Vector2(pos.X, pos.Y), new Rectangle(textureRegion.X + xUl * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y), new Rectangle(textureRegion.X + w2 + xUr * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X, pos.Y + h2 * scale.Y), new Rectangle(textureRegion.X + xDl * w, textureRegion.Y + h2, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y + h2 * scale.Y), new Rectangle(textureRegion.X + w2 + xDr * w, textureRegion.Y + h2, w2, h2));
        }

        /// <summary>
        /// A delegate function that determines if a given offset position connects to an auto-tile location.
        /// </summary>
        /// <param name="xOff">The x offset</param>
        /// <param name="yOff">The y offset</param>
        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}