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

            var (x, y) = pos;
            var (sizeX, sizeY) = textureRegion.Size;
            var (halfSizeX, halfSizeY) = new Point(sizeX / 2, sizeY / 2);
            batch.Draw(texture, new Vector2(x, y), new Rectangle(textureRegion.X + 0 + xUl * sizeX, textureRegion.Y + 0, halfSizeX, halfSizeY), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(x + 0.5F * sizeX, y), new Rectangle(textureRegion.X + halfSizeX + xUr * sizeX, textureRegion.Y + 0, halfSizeX, halfSizeY), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(x, y + 0.5F * sizeY), new Rectangle(textureRegion.X + xDl * sizeX, textureRegion.Y + halfSizeY, halfSizeX, halfSizeY), color, rotation, org, sc, SpriteEffects.None, layerDepth);
            batch.Draw(texture, new Vector2(x + 0.5F * sizeX, y + 0.5F * sizeY), new Rectangle(textureRegion.X + halfSizeX + xDr * sizeX, textureRegion.Y + halfSizeY, halfSizeX, halfSizeY), color, rotation, org, sc, SpriteEffects.None, layerDepth);
        }

        public delegate bool ConnectsTo(int xOff, int yOff);

    }
}