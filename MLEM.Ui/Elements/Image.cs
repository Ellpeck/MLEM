using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Image : Element {

        public Color Color = Color.White;
        private readonly TextureRegion texture;
        private readonly bool scaleToImage;

        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.texture = texture;
            this.scaleToImage = scaleToImage;
            this.Padding = new Point(1);
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            return this.scaleToImage ? this.texture.Size : base.CalcActualSize(parentArea);
        }

        public override void Draw(GameTime time, SpriteBatch batch, Color color) {
            batch.Draw(this.texture, this.DisplayArea, this.Color.CopyAlpha(color));
            base.Draw(time, batch, color);
        }

    }
}