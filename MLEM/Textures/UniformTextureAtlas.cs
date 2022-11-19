using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;

namespace MLEM.Textures {
    /// <summary>
    /// This class represents an atlas of <see cref="TextureRegion"/> objects that is uniform.
    /// Uniform, in this case, means that the texture atlas' size is not determined by the width and height of the texture, but instead by the amount of sub-regions that the atlas has in the x and y direction.
    /// Using a uniform texture atlas over a regular texture as an atlas allows for texture artists to create higher resolution textures without coordinates becoming off.
    /// </summary>
    public class UniformTextureAtlas : GenericDataHolder {

        /// <summary>
        /// The <see cref="TextureRegion"/> that this uniform texture atlas uses as its basis.
        /// In most cases, <see cref="Region"/> has the full area of the underlying <see cref="Texture"/>.
        /// </summary>
        public readonly TextureRegion Region;
        /// <summary>
        /// The amount of sub-regions this atlas has in the x direction
        /// </summary>
        public readonly int RegionAmountX;
        /// <summary>
        /// The amount of sub-regions this atlas has in the y direction
        /// </summary>
        public readonly int RegionAmountY;
        /// <summary>
        /// The width of each region, based on the texture's width and the amount of regions
        /// </summary>
        public readonly int RegionWidth;
        /// <summary>
        /// The height of reach region, based on the texture's height and the amount of regions
        /// </summary>
        public readonly int RegionHeight;
        /// <summary>
        /// The padding that each texture region has around itself, in pixels, which will be taken away from each side of <see cref="TextureRegion"/> objects created and returned by this texture atlas.
        /// Creating a texture atlas with padding can be useful if texture bleeding issues occur due to texture coordinate rounding.
        /// </summary>
        public readonly int RegionPadding;

        /// <summary>
        /// The texture to use for this atlas.
        /// Note that <see cref="Region"/> stores the actual area that we depend on.
        /// </summary>
        public Texture2D Texture => this.Region.Texture;
        /// <summary>
        /// Returns the <see cref="TextureRegion"/> at this texture atlas's given index.
        /// The index is zero-based, where rows come first and columns come second.
        /// </summary>
        /// <param name="index">The zero-based texture index</param>
        public TextureRegion this[int index] => this[index % this.RegionAmountX, index / this.RegionAmountX];
        /// <summary>
        /// Returns the <see cref="TextureRegion"/> at this texture atlas' given region position
        /// </summary>
        /// <param name="point">The region's x and y location</param>
        public TextureRegion this[Point point] => this[new Rectangle(point.X, point.Y, 1, 1)];
        /// <inheritdoc cref="this[Point]"/>
        public TextureRegion this[int x, int y] => this[new Point(x, y)];
        /// <summary>
        /// Returns the <see cref="TextureRegion"/> at this texture atlas' given region position and size.
        /// Note that the region size is not in pixels, but in region units.
        /// </summary>
        /// <param name="rect">The region's area</param>
        public TextureRegion this[Rectangle rect] => this.GetOrAddRegion(rect);
        /// <inheritdoc cref="this[Rectangle]"/>
        public TextureRegion this[int x, int y, int width, int height] => this[new Rectangle(x, y, width, height)];

        private readonly Dictionary<Rectangle, TextureRegion> regions = new Dictionary<Rectangle, TextureRegion>();

        /// <summary>
        /// Creates a new uniform texture atlas with the given texture region and region amount.
        /// This atlas will only ever pull information from the given <see cref="TextureRegion"/> and never exit the region's bounds.
        /// </summary>
        /// <param name="region">The texture region to use for this atlas</param>
        /// <param name="regionAmountX">The amount of texture regions in the x direction</param>
        /// <param name="regionAmountY">The amount of texture regions in the y direction</param>
        /// <param name="regionPadding">The padding that each texture region has around itself, in pixels, which will be taken away from each side of <see cref="TextureRegion"/> objects created and returned by this texture atlas.</param>
        public UniformTextureAtlas(TextureRegion region, int regionAmountX, int regionAmountY, int regionPadding = 0) {
            this.Region = region;
            this.RegionAmountX = regionAmountX;
            this.RegionAmountY = regionAmountY;
            this.RegionPadding = regionPadding;
            this.RegionWidth = region.Width / regionAmountX;
            this.RegionHeight = region.Height / regionAmountY;
        }

        /// <summary>
        /// Creates a new uniform texture atlas with the given texture and region amount.
        /// </summary>
        /// <param name="texture">The texture to use for this atlas</param>
        /// <param name="regionAmountX">The amount of texture regions in the x direction</param>
        /// <param name="regionAmountY">The amount of texture regions in the y direction</param>
        /// <param name="regionPadding">The padding that each texture region has around itself, in pixels, which will be taken away from each side of <see cref="TextureRegion"/> objects created and returned by this texture atlas.</param>
        public UniformTextureAtlas(Texture2D texture, int regionAmountX, int regionAmountY, int regionPadding = 0) :
            this(new TextureRegion(texture), regionAmountX, regionAmountY, regionPadding) {}

        private TextureRegion GetOrAddRegion(Rectangle rect) {
            if (this.regions.TryGetValue(rect, out var region))
                return region;
            region = new TextureRegion(this.Region,
                rect.X * this.RegionWidth + this.RegionPadding, rect.Y * this.RegionHeight + this.RegionPadding,
                rect.Width * this.RegionWidth - 2 * this.RegionPadding, rect.Height * this.RegionHeight - 2 * this.RegionPadding);
            this.regions.Add(rect, region);
            return region;
        }

    }
}
