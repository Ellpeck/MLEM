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
        /// The texture to use for this atlas
        /// </summary>
        public readonly Texture2D Texture;
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
        /// Returns the <see cref="TextureRegion"/> at this texture atlas's given index.
        /// The index is zero-based, where rows come first and columns come second.
        /// </summary>
        /// <param name="index">The zero-based texture index</param>
        public TextureRegion this[int index] => this[index % this.RegionAmountX, index / this.RegionAmountX];
        /// <summary>
        /// Returns the <see cref="TextureRegion"/> at this texture atlas' given region position
        /// </summary>
        /// <param name="point">The region's x and y location</param>
        public TextureRegion this[Point point] => this[new Rectangle(point, new Point(1, 1))];
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
        /// Creates a new uniform texture atlas with the given texture and region amount.
        /// </summary>
        /// <param name="texture">The texture to use for this atlas</param>
        /// <param name="regionAmountX">The amount of texture regions in the x direction</param>
        /// <param name="regionAmountY">The amount of texture regions in the y direction</param>
        public UniformTextureAtlas(Texture2D texture, int regionAmountX, int regionAmountY) {
            this.Texture = texture;
            this.RegionAmountX = regionAmountX;
            this.RegionAmountY = regionAmountY;
            this.RegionWidth = texture.Width / regionAmountX;
            this.RegionHeight = texture.Height / regionAmountY;
        }

        private TextureRegion GetOrAddRegion(Rectangle rect) {
            if (this.regions.TryGetValue(rect, out var region))
                return region;
            region = new TextureRegion(this.Texture,
                rect.X * this.RegionWidth, rect.Y * this.RegionHeight,
                rect.Width * this.RegionWidth, rect.Height * this.RegionHeight);
            this.regions.Add(rect, region);
            return region;
        }

    }
}