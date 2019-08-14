using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Image : Element {

        public Color Color = Color.White;
        public TextureRegion Texture;
        private readonly bool scaleToImage;

        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.Texture = texture;
            this.scaleToImage = scaleToImage;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            return this.scaleToImage ? this.Texture.Size : base.CalcActualSize(parentArea);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            batch.Draw(this.Texture, this.DisplayArea.OffsetCopy(offset), this.Color * alpha);
            base.Draw(time, batch, alpha, offset);
        }

    }
}