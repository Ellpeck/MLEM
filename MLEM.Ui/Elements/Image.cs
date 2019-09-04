using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Image : Element {

        public Color Color = Color.White;
        public TextureRegion Texture;
        public bool ScaleToImage;
        public bool MaintainImageAspect = true;

        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.Texture = texture;
            this.ScaleToImage = scaleToImage;
            this.CanBeSelected = false;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            return this.ScaleToImage ? this.Texture.Size : base.CalcActualSize(parentArea);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            if (this.MaintainImageAspect) {
                var scale = Math.Min(this.DisplayArea.Width / (float) this.Texture.Width, this.DisplayArea.Height / (float) this.Texture.Height);
                var imageOffset = new Vector2(this.DisplayArea.Width / 2F - this.Texture.Width * scale / 2, this.DisplayArea.Height / 2F - this.Texture.Height * scale / 2);
                batch.Draw(this.Texture, this.DisplayArea.Location.ToVector2() + imageOffset, this.Color * alpha, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            } else {
                batch.Draw(this.Texture, this.DisplayArea, this.Color * alpha);
            }
            base.Draw(time, batch, alpha);
        }

    }
}