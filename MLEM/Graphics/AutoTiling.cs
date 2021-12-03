using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Graphics {
    /// <summary>
    /// This class contains a <see cref="DrawAutoTile"/> method that allows users to easily draw a tile with automatic connections, as well as a more complex <see cref="DrawExtendedAutoTile"/> method.
    /// Note that <see cref="StaticSpriteBatch"/> can also be used for drawing by using the <see cref="AddAutoTile"/> and <see cref="AddExtendedAutoTile"/> methods instead.
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
        /// For more information and an example, see https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs and its source texture https://github.com/Ellpeck/MLEM/blob/main/Demos/Content/Textures/AutoTiling.png.
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
        public static void AddAutoTile(StaticSpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color color, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = CalculateAutoTile(pos, textureRegion, connectsTo, sc);
            batch.Add(texture, p1, r1, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p2, r2, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p3, r3, color, 0, orig, sc, SpriteEffects.None, layerDepth);
            batch.Add(texture, p4, r4, color, 0, orig, sc, SpriteEffects.None, layerDepth);
        }

        /// <summary>
        /// This method allows for a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        /// 
        /// This method is a more complex version of <see cref="DrawAutoTile"/> that overlays separate border textures on a base texture, which also allows for non-rectangular texture areas to be used easily. 
        /// For auto-tiling in this way to work, the tiles have to be laid out as follows: 17 tiles aligned horizontally within the texture file, with the following information:
        /// <list type="number">
        /// <item><description>The texture used for filling big areas</description></item>
        /// <item><description>The texture used for straight, horizontal borders, with the borders facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for outer corners, with the corners facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for straight, vertical borders, with the borders facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// <item><description>The texture used for inner corners, with the corners facing away from the center, split up into four parts: top left, then top right, then bottom left, then bottom right</description></item>
        /// </list>
        /// For more information and an example, see https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs and its source texture https://github.com/Ellpeck/MLEM/blob/main/Demos/Content/Textures/AutoTiling.png.
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="pos">The position to draw at.</param>
        /// <param name="texture">The texture to use for drawing.</param>
        /// <param name="textureRegion">The location of the first texture region, as described in the summary.</param>
        /// <param name="connectsTo">A function that determines whether two positions should connect.</param>
        /// <param name="backgroundColor">The color to draw the texture used for filling big areas with.</param>
        /// <param name="overlayColor">The color to draw border and corner textures with.</param>
        /// <param name="origin">The origin to draw from.</param>
        /// <param name="scale">The scale to draw with.</param>
        /// <param name="layerDepth">The layer depth to draw with.</param>
        /// <param name="overlayDepthOffset">An optional depth offset from <paramref name="layerDepth"/> that the overlay should be drawn with</param>
        public static void DrawExtendedAutoTile(SpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var od = layerDepth + overlayDepthOffset;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = CalculateExtendedAutoTile(pos, textureRegion, connectsTo, sc);
            batch.Draw(texture, pos, textureRegion, backgroundColor, 0, orig, sc, SpriteEffects.None, layerDepth);
            if (r1 != Rectangle.Empty)
                batch.Draw(texture, p1, r1, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r2 != Rectangle.Empty)
                batch.Draw(texture, p2, r2, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r3 != Rectangle.Empty)
                batch.Draw(texture, p3, r3, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r4 != Rectangle.Empty)
                batch.Draw(texture, p4, r4, overlayColor, 0, orig, sc, SpriteEffects.None, od);
        }

        /// <inheritdoc cref="DrawExtendedAutoTile"/>
        public static void AddExtendedAutoTile(StaticSpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color backgroundColor, Color overlayColor, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0, float overlayDepthOffset = 0) {
            var orig = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;
            var od = layerDepth + overlayDepthOffset;
            var (p1, r1, p2, r2, p3, r3, p4, r4) = CalculateExtendedAutoTile(pos, textureRegion, connectsTo, sc);
            batch.Add(texture, pos, textureRegion, backgroundColor, 0, orig, sc, SpriteEffects.None, layerDepth);
            if (r1 != Rectangle.Empty)
                batch.Add(texture, p1, r1, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r2 != Rectangle.Empty)
                batch.Add(texture, p2, r2, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r3 != Rectangle.Empty)
                batch.Add(texture, p3, r3, overlayColor, 0, orig, sc, SpriteEffects.None, od);
            if (r4 != Rectangle.Empty)
                batch.Add(texture, p4, r4, overlayColor, 0, orig, sc, SpriteEffects.None, od);
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

        private static (Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle, Vector2, Rectangle) CalculateExtendedAutoTile(Vector2 pos, Rectangle textureRegion, ConnectsTo connectsTo, Vector2 scale) {
            var up = connectsTo(0, -1);
            var down = connectsTo(0, 1);
            var left = connectsTo(-1, 0);
            var right = connectsTo(1, 0);
            var xUl = up && left ? connectsTo(-1, -1) ? -1 : 13 : left ? 1 : up ? 9 : 5;
            var xUr = up && right ? connectsTo(1, -1) ? -1 : 14 : right ? 2 : up ? 10 : 6;
            var xDl = down && left ? connectsTo(-1, 1) ? -1 : 15 : left ? 3 : down ? 11 : 7;
            var xDr = down && right ? connectsTo(1, 1) ? -1 : 16 : right ? 4 : down ? 12 : 8;

            var (w, h) = textureRegion.Size;
            var (w2, h2) = new Point(w / 2, h / 2);

            return (
                new Vector2(pos.X, pos.Y), xUl < 0 ? Rectangle.Empty : new Rectangle(textureRegion.X + xUl * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y), xUr < 0 ? Rectangle.Empty : new Rectangle(textureRegion.X + w2 + xUr * w, textureRegion.Y, w2, h2),
                new Vector2(pos.X, pos.Y + h2 * scale.Y), xDl < 0 ? Rectangle.Empty : new Rectangle(textureRegion.X + xDl * w, textureRegion.Y + h2, w2, h2),
                new Vector2(pos.X + w2 * scale.X, pos.Y + h2 * scale.Y), xDr < 0 ? Rectangle.Empty : new Rectangle(textureRegion.X + w2 + xDr * w, textureRegion.Y + h2, w2, h2));
        }

        /// <summary>
        /// A delegate function that determines if a given offset position connects to an auto-tile location.
        /// </summary>
        /// <param name="xOff">The x offset</param>
        /// <param name="yOff">The y offset</param>
        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}