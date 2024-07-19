using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Misc;

namespace MLEM.Textures {
    /// <summary>
    /// This class represents a texture with nine areas.
    /// A nine patch texture is useful if a big area should be covered by a small texture that has a specific outline, like a gui panel texture. The center of the texture will be stretched or tiled, while the outline of the texture will remain at its original size, keeping aspect ratios alive.
    /// The nine patch can then be drawn using <see cref="NinePatchExtensions"/>.
    /// </summary>
    public class NinePatch : GenericDataHolder {

        /// <summary>
        /// The texture region of this nine patch
        /// </summary>
        public readonly TextureRegion Region;
        /// <summary>
        /// The padding in each direction that marks where the outline area stops, in pixels
        /// </summary>
        public readonly Padding Padding;
        /// <summary>
        /// The <see cref="NinePatchMode"/> that this nine patch should use for drawing
        /// </summary>
        public readonly NinePatchMode Mode;
        /// <summary>
        /// The nine patches that result from the <see cref="Padding"/>
        /// </summary>
        public readonly Rectangle[] SourceRectangles;

        /// <summary>
        /// Creates a new nine patch from a texture and a padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="padding">The padding that marks where the outline area stops in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="mode">The mode to use for drawing this nine patch, defaults to <see cref="NinePatchMode.Stretch"/></param>
        /// <param name="paddingPercent">Whether the padding should represent a percentage of the underlying <paramref name="texture"/>'s size, rather than an absolute pixel amount</param>
        public NinePatch(TextureRegion texture, Padding padding, NinePatchMode mode = NinePatchMode.Stretch, bool paddingPercent = false) {
            this.Region = texture;
            this.Padding = paddingPercent ? padding * texture.Size.ToVector2() : padding;
            this.Mode = mode;
            this.SourceRectangles = new Rectangle[9];
            for (var i = 0; i < this.SourceRectangles.Length; i++)
                this.SourceRectangles[i] = (Rectangle) this.GetRectangleForIndex((RectangleF) this.Region.Area, i);
        }

        /// <summary>
        /// Creates a new nine patch from a texture and a padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="paddingLeft">The padding on the left edge in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="paddingRight">The padding on the right edge in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="paddingTop">The padding on the top edge in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="paddingBottom">The padding on the bottom edge in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="mode">The mode to use for drawing this nine patch, defaults to <see cref="NinePatchMode.Stretch"/></param>
        /// <param name="paddingPercent">Whether the padding should represent a percentage of the underlying <paramref name="texture"/>'s size, rather than an absolute pixel amount</param>
        public NinePatch(TextureRegion texture, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom, NinePatchMode mode = NinePatchMode.Stretch, bool paddingPercent = false) : this(texture, new Padding(paddingLeft, paddingRight, paddingTop, paddingBottom), mode, paddingPercent) {}

        /// <inheritdoc cref="NinePatch(TextureRegion, float, float, float, float, NinePatchMode, bool)"/>
        public NinePatch(Texture2D texture, float paddingLeft, float paddingRight, float paddingTop, float paddingBottom, NinePatchMode mode = NinePatchMode.Stretch, bool paddingPercent = false) : this(new TextureRegion(texture), paddingLeft, paddingRight, paddingTop, paddingBottom, mode, paddingPercent) {}

        /// <summary>
        /// Creates a new nine patch from a texture and a uniform padding
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="padding">The padding that each edge should have in pixels, or as a percentage if <paramref name="paddingPercent"/> is <see langword="true"/></param>
        /// <param name="mode">The mode to use for drawing this nine patch, defaults to <see cref="NinePatchMode.Stretch"/></param>
        /// <param name="paddingPercent">Whether the padding should represent a percentage of the underlying <paramref name="texture"/>'s size, rather than an absolute pixel amount</param>
        public NinePatch(Texture2D texture, float padding, NinePatchMode mode = NinePatchMode.Stretch, bool paddingPercent = false) : this(new TextureRegion(texture), padding, mode, paddingPercent) {}

        /// <inheritdoc cref="NinePatch(TextureRegion, float, NinePatchMode, bool)"/>
        public NinePatch(TextureRegion texture, float padding, NinePatchMode mode = NinePatchMode.Stretch, bool paddingPercent = false) : this(texture, padding, padding, padding, padding, mode, paddingPercent) {}

        internal RectangleF GetRectangleForIndex(RectangleF area, int index, float patchScale = 1) {
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

            switch (index) {
                case 0:
                    return new RectangleF(area.X, area.Y, pl, pt);
                case 1:
                    return new RectangleF(leftX, area.Y, centerW, pt);
                case 2:
                    return new RectangleF(rightX, area.Y, pr, pt);
                case 3:
                    return new RectangleF(area.X, topY, pl, centerH);
                case 4:
                    return new RectangleF(leftX, topY, centerW, centerH);
                case 5:
                    return new RectangleF(rightX, topY, pr, centerH);
                case 6:
                    return new RectangleF(area.X, bottomY, pl, pb);
                case 7:
                    return new RectangleF(leftX, bottomY, centerW, pb);
                case 8:
                    return new RectangleF(rightX, bottomY, pr, pb);
                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

    }

    /// <summary>
    /// An enumeration that represents the modes that a <see cref="NinePatch"/> uses to be drawn
    /// </summary>
    public enum NinePatchMode {

        /// <summary>
        /// The nine resulting patches will each be stretched.
        /// This mode is fitting for textures that don't have an intricate design on their edges.
        /// </summary>
        Stretch,
        /// <summary>
        /// The nine resulting paches will be tiled, repeating the texture multiple times.
        /// This mode is fitting for textures that have a more complex design on their edges.
        /// </summary>
        Tile

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
            for (var i = 0; i < texture.SourceRectangles.Length; i++) {
                var rect = texture.GetRectangleForIndex(destinationRectangle, i, patchScale);
                if (!rect.IsEmpty) {
                    var src = texture.SourceRectangles[i];
                    switch (texture.Mode) {
                        case NinePatchMode.Stretch:
                            batch.Draw(texture.Region.Texture, rect, src, color, rotation, origin, effects, layerDepth);
                            break;
                        case NinePatchMode.Tile:
                            var width = src.Width * patchScale;
                            var height = src.Height * patchScale;
                            if (width > 0 && height > 0) {
                                for (var x = 0F; x < rect.Width; x += width) {
                                    for (var y = 0F; y < rect.Height; y += height) {
                                        var size = new Vector2(Math.Min(rect.Width - x, width), Math.Min(rect.Height - y, height));
                                        var srcSize = (size / patchScale).CeilCopy().ToPoint();
                                        batch.Draw(texture.Region.Texture, new RectangleF(rect.Location + new Vector2(x, y), size), new Rectangle(src.X, src.Y, srcSize.X, srcSize.Y), color, rotation, origin, effects, layerDepth);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth, float patchScale = 1) {
            batch.Draw(texture, (RectangleF) destinationRectangle, color, rotation, origin, effects, layerDepth, patchScale);
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, RectangleF destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

        /// <inheritdoc cref="Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch,MLEM.Textures.NinePatch,RectangleF,Microsoft.Xna.Framework.Color,float,Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Graphics.SpriteEffects,float,float)"/>
        public static void Draw(this SpriteBatch batch, NinePatch texture, Rectangle destinationRectangle, Color color, float patchScale = 1) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0, patchScale);
        }

    }
}
