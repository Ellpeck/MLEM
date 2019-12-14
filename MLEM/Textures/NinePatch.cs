using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Textures {
    public class NinePatch {

        public readonly TextureRegion Region;
        public readonly Padding Padding;
        public readonly Rectangle[] SourceRectangles;

        public NinePatch(TextureRegion texture, Padding padding) {
            this.Region = texture;
            this.Padding = padding;
            this.SourceRectangles = this.CreateRectangles(this.Region.Area).ToArray();
        }

        public NinePatch(TextureRegion texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) :
            this(texture, new Padding(paddingLeft, paddingRight, paddingTop, paddingBottom)) {
        }

        public NinePatch(Texture2D texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) :
            this(new TextureRegion(texture), paddingLeft, paddingRight, paddingTop, paddingBottom) {
        }

        public NinePatch(Texture2D texture, int padding) : this(new TextureRegion(texture), padding) {
        }

        public NinePatch(TextureRegion texture, int padding) : this(texture, padding, padding, padding, padding) {
        }

        public IEnumerable<Rectangle> CreateRectangles(Rectangle area, float patchScale = 1) {
            return this.CreateRectangles((RectangleF) area, patchScale).Select(r => (Rectangle) r);
        }

        public IEnumerable<RectangleF> CreateRectangles(RectangleF area, float patchScale = 1) {
            var pl = (int) (this.Padding.Left * patchScale);
            var pr = (int) (this.Padding.Right * patchScale);
            var pt = (int) (this.Padding.Top * patchScale);
            var pb = (int) (this.Padding.Bottom * patchScale);

            var centerW = area.Width - pl - pr;
            var centerH = area.Height - pt - pb;
            var leftX = area.X + pl;
            var rightX = area.X + area.Width - pr;
            var topY = area.Y + pt;
            var bottomY = area.Y + area.Height - pb;

            yield return new RectangleF(area.X, area.Y, pl, pt);
            yield return new RectangleF(leftX, area.Y, centerW, pt);
            yield return new RectangleF(rightX, area.Y, pr, pt);
            yield return new RectangleF(area.X, topY, pl, centerH);
            yield return new RectangleF(leftX, topY, centerW, centerH);
            yield return new RectangleF(rightX, topY, pr, centerH);
            yield return new RectangleF(area.X, bottomY, pl, pb);
            yield return new RectangleF(leftX, bottomY, centerW, pb);
            yield return new RectangleF(rightX, bottomY, pr, pb);
        }

    }

    public static class NinePatchExtensions {

        public static void Draw(this SpriteBatch batch, NinePatch texture, RectangleF destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, float patchScale = 1) {
            var dest = texture.CreateRectangles(destinationRectangle, patchScale);
            var count = 0;
            foreach (var rect in dest) {
                if (!rect.IsEmpty)
                    batch.Draw(texture.Region.Texture, rect, texture.SourceRectangles[count], color, rotation, origin, effects, layerDepth);
                count++;
            }
        }

        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, float patchScale = 1) {
            batch.Draw(texture, (RectangleF) destinationRectangle, color, rotation, origin, effects, layerDepth, patchScale);
        }

        public static void Draw(this SpriteBatch batch, NinePatch texture, RectangleF destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

    }
}