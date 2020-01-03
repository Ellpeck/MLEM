using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Textures {
    public class UniformTextureAtlas {

        public readonly Texture2D Texture;
        public readonly int RegionAmountX;
        public readonly int RegionAmountY;
        public readonly int RegionWidth;
        public readonly int RegionHeight;
        public TextureRegion this[int index] => this[index % this.RegionAmountX, index / this.RegionAmountX];
        public TextureRegion this[Point point] => this[new Rectangle(point, new Point(1, 1))];
        public TextureRegion this[int x, int y] => this[new Point(x, y)];
        public TextureRegion this[Rectangle rect] => this.GetOrAddRegion(rect);
        public TextureRegion this[int x, int y, int width, int height] => this[new Rectangle(x, y, width, height)];

        private readonly Dictionary<Rectangle, TextureRegion> regions = new Dictionary<Rectangle, TextureRegion>();

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