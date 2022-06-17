using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// An image element to be used inside of a <see cref="UiSystem"/>.
    /// An image is simply an element that displays a supplied <see cref="TextureRegion"/> and optionally allows for the texture region to remain at its original aspect ratio, regardless of the element's size.
    /// </summary>
    public class Image : Element {

        /// <summary>
        /// The color to render the image at
        /// </summary>
        public StyleProp<Color> Color;
        /// <summary>
        /// A callback to retrieve the <see cref="TextureRegion"/> that this image should render.
        /// This can be used if the image changes frequently.
        /// </summary>
        public TextureCallback GetTextureCallback;
        /// <summary>
        /// The texture that this <see cref="TextureRegion"/> should render
        /// </summary>
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
        /// <summary>
        /// Whether this image element's <see cref="Element.Size"/> should be based on the size of the <see cref="TextureRegion"/> given.
        /// Note that, when scaling to the image's size, the <see cref="Element.Scale"/> is also taken into account.
        /// </summary>
        public bool ScaleToImage {
            get => this.scaleToImage;
            set {
                if (this.scaleToImage != value) {
                    this.scaleToImage = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Whether to cause the <see cref="TextureRegion"/> to be rendered at its proper aspect ratio.
        /// If this is false, the image will be stretched according to this component's size.
        /// </summary>
        public bool MaintainImageAspect = true;
        /// <summary>
        /// The <see cref="SpriteEffects"/> that the texture should be rendered with
        /// </summary>
        public SpriteEffects ImageEffects = SpriteEffects.None;
        /// <summary>
        /// The scale that the image should be rendered with
        /// </summary>
        public Vector2 ImageScale = Vector2.One;
        /// <summary>
        /// The rotation that the image should be rendered with.
        /// Note that increased rotation does not increase this component's size, even if the rotated texture would go out of bounds of this component.
        /// </summary>
        public float ImageRotation;

        private bool scaleToImage;
        private TextureRegion texture;

        /// <summary>
        /// Creates a new image with the given settings
        /// </summary>
        /// <param name="anchor">The image's anchor</param>
        /// <param name="size">The image's size</param>
        /// <param name="texture">The texture the image should render</param>
        /// <param name="scaleToImage">Whether this image's size should be based on the texture's size</param>
        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.Texture = texture;
            this.scaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc cref="Image(Anchor,Vector2,TextureRegion,bool)"/>
        public Image(Anchor anchor, Vector2 size, TextureCallback getTextureCallback, bool scaleToImage = false) : base(anchor, size) {
            this.GetTextureCallback = getTextureCallback;
            this.Texture = getTextureCallback(this);
            this.scaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc />
        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            return this.Texture != null && this.scaleToImage ? this.Texture.Size.ToVector2() * this.Scale : base.CalcActualSize(parentArea);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
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
            base.Draw(time, batch, alpha, context);
        }

        /// <summary>
        /// A delegate method used for <see cref="Image.GetTextureCallback"/>
        /// </summary>
        /// <param name="image">The current image element</param>
        public delegate TextureRegion TextureCallback(Image image);

    }
}
