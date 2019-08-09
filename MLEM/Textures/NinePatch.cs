using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Textures {
    public class NinePatch {

        public readonly TextureRegion Region;
        public readonly int PaddingLeft;
        public readonly int PaddingRight;
        public readonly int PaddingTop;
        public readonly int PaddingBottom;

        public readonly Rectangle[] SourceRectangles;

        public NinePatch(TextureRegion texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) {
            this.Region = texture;
            this.PaddingLeft = paddingLeft;
            this.PaddingRight = paddingRight;
            this.PaddingTop = paddingTop;
            this.PaddingBottom = paddingBottom;
            this.SourceRectangles = this.CreateRectangles(this.Region.Area).ToArray();
        }

        public NinePatch(Texture2D texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) :
            this(new TextureRegion(texture), paddingLeft, paddingRight, paddingTop, paddingBottom) {
        }

        public NinePatch(TextureRegion texture, int padding) : this(texture, padding, padding, padding, padding) {
        }

        public IEnumerable<Rectangle> CreateRectangles(Rectangle area) {
            var centerW = area.Width - this.PaddingLeft - this.PaddingRight;
            var centerH = area.Height - this.PaddingTop - this.PaddingBottom;
            var leftX = area.X + this.PaddingLeft;
            var rightX = area.X + area.Width - this.PaddingRight;
            var topY = area.Y + this.PaddingTop;
            var bottomY = area.Y + area.Height - this.PaddingBottom;

            yield return new Rectangle(area.X, area.Y, this.PaddingLeft, this.PaddingTop);
            yield return new Rectangle(leftX, area.Y, centerW, this.PaddingTop);
            yield return new Rectangle(rightX, area.Y, this.PaddingRight, this.PaddingTop);
            yield return new Rectangle(area.X, topY, this.PaddingLeft, centerH);
            yield return new Rectangle(leftX, topY, centerW, centerH);
            yield return new Rectangle(rightX, topY, this.PaddingRight, centerH);
            yield return new Rectangle(area.X, bottomY, this.PaddingLeft, this.PaddingBottom);
            yield return new Rectangle(leftX, bottomY, centerW, this.PaddingBottom);
            yield return new Rectangle(rightX, bottomY, this.PaddingRight, this.PaddingBottom);
        }

    }

    public static class NinePatchExtensions {

        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            var dest = texture.CreateRectangles(destinationRectangle);
            var count = 0;
            foreach (var rect in dest) {
                batch.Draw(texture.Region.Texture, rect, texture.SourceRectangles[count], color, rotation, origin, effects, layerDepth);
                count++;
            }
        }

        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

    }
}