using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Textures {
    public class UniformTextureAtlas {

        public readonly Texture2D Texture;
        public readonly int RegionAmountX;
        public readonly int RegionAmountY;
        public TextureRegion this[Point point] => this.regions.TryGetValue(point, out var region) ? region : null;
        public TextureRegion this[int x, int y] => this[new Point(x, y)];

        private readonly Dictionary<Point, TextureRegion> regions = new Dictionary<Point, TextureRegion>();

        public UniformTextureAtlas(Texture2D texture, int regionAmountX, int regionAmountY) {
            this.Texture = texture;
            this.RegionAmountX = regionAmountX;
            this.RegionAmountY = regionAmountY;

            var regionWidth = texture.Width / regionAmountX;
            var regionHeight = texture.Height / regionAmountY;
            for (var x = 0; x < regionAmountX; x++) {
                for (var y = 0; y < regionAmountY; y++) {
                    this.regions.Add(new Point(x, y), new TextureRegion(texture, x * regionWidth, y * regionHeight, regionWidth, regionHeight));
                }
            }
        }

    }
}