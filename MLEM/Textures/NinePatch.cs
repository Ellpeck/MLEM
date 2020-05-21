using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Textures {
    /// <summary>
    /// This class represents a texture with nine areas.
    /// A nine patch texture is useful if a big area should be covered by a small texture that has a specific outline, like a gui panel texture. The center of the texture will be stretched, while the outline of the texture will remain at its original size, keeping aspect ratios alive.
    /// </summary>
    public class NinePatch : GenericDataHolder {

        /// <summary>
        /// The texture region of this nine patch
        /// </summary>
        public readonly TextureRegion Region;
        /// <summary>
        /// The padding in each direction that marks where the outline area stops
        /// </summary>
        public readonly Padding Padding;
        /// <summary>
        /// The nine patches that result from the <see cref="Padding"/>
        /// </summary>
        public readonly Rectangle[] SourceRectangles;

        /// <summary>
        /// Creates a new nine patch from a texture and a padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="padding">The padding that marks where the outline area stops</param>
        public NinePatch(TextureRegion texture, Padding padding) {
            this.Region = texture;
            this.Padding = padding;
            this.SourceRectangles = this.CreateRectangles(this.Region.Area).ToArray();
        }

        /// <summary>
        /// Creates a new nine patch from a texture and a padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="paddingLeft">The padding on the left edge</param>
        /// <param name="paddingRight">The padding on the right edge</param>
        /// <param name="paddingTop">The padding on the top edge</param>
        /// <param name="paddingBottom">The padding on the bottom edge</param>
        public NinePatch(TextureRegion texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) :
            this(texture, new Padding(paddingLeft, paddingRight, paddingTop, paddingBottom)) {
        }

        /// <inheritdoc cref="NinePatch(TextureRegion, int, int, int, int)"/>
        public NinePatch(Texture2D texture, int paddingLeft, int paddingRight, int paddingTop, int paddingBottom) :
            this(new TextureRegion(texture), paddingLeft, paddingRight, paddingTop, paddingBottom) {
        }

        /// <summary>
        /// Creates a new nine patch from a texture and a uniform padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="padding">The padding that each edge should have</param>
        public NinePatch(Texture2D texture, int padding) : this(new TextureRegion(texture), padding) {
        }

        /// <inheritdoc cref="NinePatch(TextureRegion, int)"/>
        public NinePatch(TextureRegion texture, int padding) : this(texture, padding, padding, padding, padding) {
        }

        private IEnumerable<Rectangle> CreateRectangles(Rectangle area, float patchScale = 1) {
            return this.CreateRectangles((RectangleF) area, patchScale).Select(r => (Rectangle) r);
        }

        internal IEnumerable<RectangleF> CreateRectangles(RectangleF area, float patchScale = 1) {
            var pl = this.Padding.Left * patchScale;
            var pr = this.Padding.Right * patchScale;
            var pt = this.Padding.Top * patchScale;
            var pb = this.Padding.Bottom * patchScale;

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

    /// <summary>
    /// A set of extensions that allow for <see cref="NinePatch"/> rendering
    /// </summary>
    public static class NinePatchExtensions {

        /// <summary>
        /// Draws a nine patch area using the given sprite batch
        /// </summary>
        /// <param name="batch">The batch to draw with</param>
        /// <param name="texture">The nine patch to draw</param>
        /// <param name="destinationRectangle">The area that should be covered by this nine patch</param>
        /// <param name="color">The color to use</param>
        /// <param name="rotation">The rotation</param>
        /// <param name="origin">The origin position</param>
        /// <param name="effects">The effects that the sprite should have</param>
        /// <param name="layerDepth">The depth</param>
        /// <param name="patchScale">The scale of each area of the nine patch</param>
        public static void Draw(this SpriteBatch batch, NinePatch texture, RectangleF destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, float patchScale = 1) {
            var dest = texture.CreateRectangles(destinationRectangle, patchScale);
            var count = 0;
            foreach (var rect in dest) {
                if (!rect.IsEmpty)
                    batch.Draw(texture.Region.Texture, rect, texture.SourceRectangles[count], color, rotation, origin, effects, layerDepth);
                count++;
            }
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,MLEM.Misc.RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, float patchScale = 1) {
            batch.Draw(texture, (RectangleF) destinationRectangle, color, rotation, origin, effects, layerDepth, patchScale);
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,MLEM.Misc.RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, RectangleF destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,MLEM.Misc.RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

    }
}