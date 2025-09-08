using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
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
                this.CheckTextureChange();
                return this.displayedTexture;
            }
            set {
                this.explicitlySetTexture = value;
                this.CheckTextureChange();
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
        /// <summary>
        /// Whether to take into account the <see cref="Texture"/>'s <see cref="TextureRegion.PivotPixels"/> value when calculating the location to draw this image at.
        /// </summary>
        public bool UseImagePivot = true;
        /// <summary>
        /// Whether this image's width should automatically be calculated based on this image's calculated height in relation to its <see cref="Texture"/>'s aspect ratio.
        /// Note that, if this is <see langword="true"/>, the <see cref="Element.AutoSizeAddedAbsolute"/> value will still be applied to this image's width.
        /// </summary>
        public bool SetWidthBasedOnAspect {
            get => this.setWidthBasedOnAspect;
            set {
                if (this.setWidthBasedOnAspect != value) {
                    this.setWidthBasedOnAspect = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Whether this image's height should automatically be calculated based on this image's calculated width in relation to its <see cref="Texture"/>'s aspect ratio.
        /// This behavior is useful if an image should take up a certain width, but the aspect ratio of its texture can vary and the image should not take up more height than is necessary.
        /// Note that, if this is <see langword="true"/>, the <see cref="Element.AutoSizeAddedAbsolute"/> value will still be applied to this image's height.
        /// </summary>
        public bool SetHeightBasedOnAspect {
            get => this.setHeightBasedOnAspect;
            set {
                if (this.setHeightBasedOnAspect != value) {
                    this.setHeightBasedOnAspect = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// The sampler state that this image's <see cref="Texture"/> should be drawn with.
        /// If this is <see langword="null"/>, the current <see cref="SpriteBatchContext"/>'s <see cref="SpriteBatchContext.SamplerState"/> will be used, which will likely be the same as <see cref="UiSystem.SpriteBatchContext"/>.
        /// </summary>
        public SamplerState SamplerState;

        /// <inheritdoc />
        public override bool IsHidden => base.IsHidden || this.Texture == null;

        private bool scaleToImage;
        private bool setWidthBasedOnAspect;
        private bool setHeightBasedOnAspect;
        private TextureRegion explicitlySetTexture;
        private TextureRegion displayedTexture;

        /// <summary>
        /// Creates a new image with the given settings
        /// </summary>
        /// <param name="anchor">The image's anchor</param>
        /// <param name="size">The image's size</param>
        /// <param name="texture">The texture the image should render</param>
        /// <param name="scaleToImage">Whether this image's size should be based on the texture's size</param>
        public Image(Anchor anchor, Vector2 size, TextureRegion texture, bool scaleToImage = false) : base(anchor, size) {
            this.Texture = texture;
            this.ScaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc cref="Image(Anchor,Vector2,TextureRegion,bool)"/>
        public Image(Anchor anchor, Vector2 size, TextureCallback getTextureCallback, bool scaleToImage = false) : base(anchor, size) {
            this.GetTextureCallback = getTextureCallback;
            this.ScaleToImage = scaleToImage;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc />
        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var ret = base.CalcActualSize(parentArea);
            if (this.Texture != null) {
                if (this.ScaleToImage)
                    ret = this.Texture.Size.ToVector2() * this.Scale;
                if (this.SetWidthBasedOnAspect)
                    ret.X = ret.Y * this.Texture.Width / this.Texture.Height + this.ScaledAutoSizeAddedAbsolute.X;
                if (this.SetHeightBasedOnAspect)
                    ret.Y = ret.X * this.Texture.Height / this.Texture.Width + this.ScaledAutoSizeAddedAbsolute.Y;
            } else {
                // if we don't have a texture and we auto-set width or height, calculate as if we had a texture with a size of 0
                if (this.SetWidthBasedOnAspect)
                    ret.X = this.ScaledAutoSizeAddedAbsolute.X;
                if (this.SetHeightBasedOnAspect)
                    ret.Y = this.ScaledAutoSizeAddedAbsolute.Y;
            }
            return ret;
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.CheckTextureChange();
            base.Update(time);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            if (this.Texture == null)
                return;

            if (this.SamplerState != null) {
                batch.End();
                var localContext = context;
                localContext.SamplerState = this.SamplerState;
                batch.Begin(localContext);
            }

            var center = new Vector2(this.Texture.Width / 2F, this.Texture.Height / 2F);
            var pivot = center - (this.UseImagePivot ? Vector2.Zero : this.Texture.PivotPixels);
            var color = this.Color.OrDefault(Microsoft.Xna.Framework.Color.White) * alpha;
            if (this.MaintainImageAspect) {
                var scale = Math.Min(this.DisplayArea.Width / this.Texture.Width, this.DisplayArea.Height / this.Texture.Height);
                var imageOffset = new Vector2(this.DisplayArea.Width / 2F - this.Texture.Width * scale / 2, this.DisplayArea.Height / 2F - this.Texture.Height * scale / 2);
                batch.Draw(this.Texture, this.DisplayArea.Location + center * scale + imageOffset, color, this.ImageRotation, pivot, scale * this.ImageScale, this.ImageEffects, 0);
            } else {
                var scale = new Vector2(1F / this.Texture.Width, 1F / this.Texture.Height) * this.DisplayArea.Size;
                batch.Draw(this.Texture, this.DisplayArea.Location + center * scale, color, this.ImageRotation, pivot, scale * this.ImageScale, this.ImageEffects, 0);
            }

            if (this.SamplerState != null) {
                batch.End();
                batch.Begin(context);
            }

            base.Draw(time, batch, alpha, context);
        }

        private void CheckTextureChange() {
            var newTexture = this.GetTextureCallback?.Invoke(this) ?? this.explicitlySetTexture;
            if (this.displayedTexture == newTexture)
                return;
            var nullChanged = this.displayedTexture == null != (newTexture == null);
            this.displayedTexture = newTexture;
            if (nullChanged || this.ScaleToImage || this.SetWidthBasedOnAspect || this.SetHeightBasedOnAspect)
                this.SetAreaDirty();
        }

        /// <summary>
        /// A delegate method used for <see cref="Image.GetTextureCallback"/>
        /// </summary>
        /// <param name="image">The current image element</param>
        public delegate TextureRegion TextureCallback(Image image);

    }
}
