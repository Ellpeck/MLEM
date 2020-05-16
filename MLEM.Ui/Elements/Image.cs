using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Image : Element {

        public StyleProp<Color> Color;
        private TextureRegion texture;
        public TextureCallback GetTextureCallback;
        public TextureRegion Texture {
            get {
                if (this.GetTextureCallback != null)
                    this.Texture = this.GetTextureCallback(this);
                return this.texture;
            }
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
            this.Texture = texture;
            this.scaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        public Image(Anchor anchor, Vector2 size, TextureCallback getTextureCallback, bool scaleToImage = false) : base(anchor, size) {
            this.GetTextureCallback = getTextureCallback;
            this.Texture = getTextureCallback(this);
            this.scaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            return this.Texture != null && this.scaleToImage ? this.Texture.Size.ToVector2() : base.CalcActualSize(parentArea);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            if (this.Texture == null)
                return;
            var center = new Vector2(this.Texture.Width / 2F, this.Texture.Height / 2F);
            var color = this.Color.OrDefault(Microsoft.Xna.Framework.Color.White) * alpha;
            if (this.MaintainImageAspect) {
                var scale = Math.Min(this.DisplayArea.Width / this.Texture.Width, this.DisplayArea.Height / this.Texture.Height);
                var imageOffset = new Vector2(this.DisplayArea.Width / 2F - this.Texture.Width * scale / 2, this.DisplayArea.Height / 2F - this.Texture.Height * scale / 2);
                batch.Draw(this.Texture, this.DisplayArea.Location + center * scale + imageOffset, color, this.ImageRotation, center, scale * this.ImageScale, this.ImageEffects, 0);
            } else {
                var scale = new Vector2(1F / this.Texture.Width, 1F / this.Texture.Height) * this.DisplayArea.Size;
                batch.Draw(this.Texture, this.DisplayArea.Location + center * scale, color, this.ImageRotation, center, scale * this.ImageScale, this.ImageEffects, 0);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        public delegate TextureRegion TextureCallback(Image image);

    }
}