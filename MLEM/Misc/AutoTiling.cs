using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Misc {
    public class AutoTiling {

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

            var size = textureRegion.Size;
            var halfSize = new Point(size.X / 2, size.Y / 2);
            batch.Draw(texture, new Vector2(pos.X, pos.Y), new Rectangle(textureRegion.X + 0 + xUl * size.X, textureRegion.Y + 0, halfSize.X, halfSize.Y), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X + 0.5F * size.X * sc.X, pos.Y), new Rectangle(textureRegion.X + halfSize.X + xUr * size.X, textureRegion.Y + 0, halfSize.X, halfSize.Y), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X, pos.Y + 0.5F * size.Y * sc.Y), new Rectangle(textureRegion.X + xDl * size.X, textureRegion.Y + halfSize.Y, halfSize.X, halfSize.Y), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(pos.X + 0.5F * size.X * sc.X, pos.Y + 0.5F * size.Y * sc.Y), new Rectangle(textureRegion.X + halfSize.X + xDr * size.X, textureRegion.Y + halfSize.Y, halfSize.X, halfSize.Y), color, rotation, org, sc, SpriteEffects.None, layerDepth);
        }

        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}