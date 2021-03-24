using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Misc {
    /// <summary>
    /// This class contains a <see cref="DrawAutoTile"/> method that allows users to easily draw a tile with automatic connections.
    /// For auto-tiling in this manner to work, auto-tiled textures have to be laid out in a format described in <see cref="DrawAutoTile"/>.
    /// </summary>
    public static class AutoTiling {

        /// <summary>
        /// This method allows for a tiled texture to be drawn in an auto-tiling mode.
        /// This allows, for example, a grass patch on a tilemap to have nice looking edges that transfer over into a path without any hard edges between tiles.
        ///
        /// For auto-tiling in this way to work, the tiles have to be laid out in a specific order. This order is shown in the auto-tiling demo's textures.
        /// For more information and an example, see <see href="https://github.com/Ellpeck/MLEM/blob/main/Demos/AutoTilingDemo.cs#L20-L28"/>
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="pos"></param>
        /// <param name="texture"></param>
        /// <param name="textureRegion"></param>
        /// <param name="connectsTo"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        /// <param name="scale"></param>
        /// <param name="layerDepth"></param>
        public static void DrawAutoTile(SpriteBatch batch, Vector2 pos, Texture2D texture, Rectangle textureRegion, ConnectsTo connectsTo, Color color, float rotation = 0, Vector2? origin = null, Vector2? scale = null, float layerDepth = 0) {
            var org = origin ?? Vector2.Zero;
            var sc = scale ?? Vector2.One;

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
            batch.Draw(texture, new Vector2(pos.X, pos.Y), new Rectangle(textureRegion.X + 0 + xUl * w, textureRegion.Y + 0, w2, h2), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X + 0.5F * w * sc.X, pos.Y), new Rectangle(textureRegion.X + w2 + xUr * w, textureRegion.Y + 0, w2, h2), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X, pos.Y + 0.5F * h * sc.Y), new Rectangle(textureRegion.X + xDl * w, textureRegion.Y + h2, w2, h2), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X + 0.5F * w * sc.X, pos.Y + 0.5F * h * sc.Y), new Rectangle(textureRegion.X + w2 + xDr * w, textureRegion.Y + h2, w2, h2), color, rotation, org, sc, SpriteEffects.None, layerDepth);
        }

        /// <summary>
        /// A delegate function that determines if a given offset position connects to an auto-tile location.
        /// </summary>
        /// <param name="xOff">The x offset</param>
        /// <param name="yOff">The y offset</param>
        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}