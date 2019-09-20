using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Image : Element {

        public Color Color = Color.White;
        private TextureRegion texture;
        public TextureRegion Texture {
            get => this.texture;
            set {
                if (this.texture != value) {
                    this.texture = value;
                    this.IsHidden = this.texture == null;
                    if (this.scaleToImage)
                        this.SetAreaDirty();
                }
            }
        }
        private bool scaleToImage;
        public bool ScaleToImage {
            get => this.scaleToImage;
            set {
                if (this.scaleToImage != value) {
                    this.scaleToImage = value;
                    this.SetAreaDirty();
                }
            }
        }
        public bool MaintainImageAspect = true;
        public SpriteEffects ImageEffects = SpriteEffects.None;
        public Vector2 ImageScale = Vector2.One;
        public float ImageRotation;

        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.texture = texture;
            this.scaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            return this.texture != null && this.scaleToImage ? this.texture.Size : base.CalcActualSize(parentArea);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var center = new Vector2(this.texture.Width / 2F, this.texture.Height / 2F);
            if (this.MaintainImageAspect) {
                var scale = Math.Min(this.DisplayArea.Width / (float) this.texture.Width, this.DisplayArea.Height / (float) this.texture.Height);
                var imageOffset = new Vector2(this.DisplayArea.Width / 2F - this.texture.Width * scale / 2, this.DisplayArea.Height / 2F - this.texture.Height * scale / 2);
                batch.Draw(this.texture, this.DisplayArea.Location.ToVector2() + center * scale + imageOffset, this.Color * alpha, this.ImageRotation, center, scale * this.ImageScale, this.ImageEffects, 0);
            } else {
                var scale = new Vector2(1F / this.texture.Width, 1F / this.texture.Height) * this.DisplayArea.Size.ToVector2();
                batch.Draw(this.texture, this.DisplayArea.Location.ToVector2() + center * scale, this.Color * alpha, this.ImageRotation, center, scale * this.ImageScale, this.ImageEffects, 0);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

    }
}