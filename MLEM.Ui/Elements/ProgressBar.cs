using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A progress bar element to use inside of a <see cref="UiSystem"/>.
    /// A progress bar is an element that fills up a bar based on a given <see cref="currentValue"/> percentage.
    /// </summary>
    public class ProgressBar : Element {

        /// <summary>
        /// The background texture that this progress bar should render
        /// </summary>
        public StyleProp<NinePatch> Texture;
        /// <summary>
        /// The color that this progress bar's <see cref="Texture"/> should render with
        /// </summary>
        public StyleProp<Color> Color;
        /// <summary>
        /// The padding that this progress bar's <see cref="ProgressTexture"/> should have.
        /// The padding is the amount of pixels that the progress texture is away from the borders of the progress bar.
        /// </summary>
        public StyleProp<Vector2> ProgressPadding;
        /// <summary>
        /// The texture that this progress bar's progress should render
        /// </summary>
        public StyleProp<NinePatch> ProgressTexture;
        /// <summary>
        /// The color that this progress bar's <see cref="ProgressTexture"/> is rendered with.
        /// </summary>
        public StyleProp<Color> ProgressColor;
        /// <summary>
        /// The direction that this progress bar goes in.
        /// Note that only <see cref="Direction2Helper.Adjacent"/> directions are supported.
        /// </summary>
        public Direction2 Direction;
        /// <summary>
        /// The maximum value that this progress bar should be able to have.
        /// </summary>
        public float MaxValue;
        /// <summary>
        /// The current value that this progress bar has.
        /// This value is always between 0 and <see cref="MaxValue"/>.
        /// </summary>
        public float CurrentValue {
            get => this.currentValue;
            set => this.currentValue = MathHelper.Clamp(value, 0, this.MaxValue);
        }

        private float currentValue;

        /// <summary>
        /// Creates a new progress bar with the given settings
        /// </summary>
        /// <param name="anchor">The progress bar's anchor</param>
        /// <param name="size">The size of the progress bar</param>
        /// <param name="direction">The direction that the progress bar goes into</param>
        /// <param name="maxValue">The progress bar's maximum value</param>
        /// <param name="currentValue">The progress bar's current value</param>
        /// <exception cref="NotSupportedException">If the provided direction is not <see cref="Direction2Helper.IsAdjacent"/></exception>
        public ProgressBar(Anchor anchor, Vector2 size, Direction2 direction, float maxValue, float currentValue = 0) : base(anchor, size) {
            if (!direction.IsAdjacent())
                throw new NotSupportedException("Progress bars only support Up, Down, Left and Right directions");
            this.Direction = direction;
            this.MaxValue = maxValue;
            this.currentValue = currentValue;
            this.CanBeSelected = false;
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            batch.Draw(this.Texture, this.DisplayArea, (Color) this.Color * alpha, this.Scale);

            var percentage = this.CurrentValue / this.MaxValue;
            var padHor = this.ProgressTexture.HasValue() ? this.ProgressTexture.Value.Padding.Width * this.Scale : 0;
            var padVer = this.ProgressTexture.HasValue() ? this.ProgressTexture.Value.Padding.Height * this.Scale : 0;
            var width = percentage * (this.DisplayArea.Width - padHor) + padHor;
            var height = percentage * (this.DisplayArea.Height - padVer) + padVer;
            RectangleF progressArea;
            switch (this.Direction) {
                case Direction2.Up:
                    progressArea = new RectangleF(this.DisplayArea.X,
                        this.DisplayArea.Y + (this.DisplayArea.Height - height),
                        this.DisplayArea.Width, height);
                    break;
                case Direction2.Down:
                    progressArea = new RectangleF(this.DisplayArea.Location, new Vector2(this.DisplayArea.Width, height));
                    break;
                case Direction2.Left:
                    progressArea = new RectangleF(
                        this.DisplayArea.X + (this.DisplayArea.Width - width),
                        this.DisplayArea.Y, width, this.DisplayArea.Height);
                    break;
                default: // Right
                    progressArea = new RectangleF(this.DisplayArea.Location, new Vector2(width, this.DisplayArea.Height));
                    break;
            }
            var offsetArea = progressArea.Shrink(this.ProgressPadding.Value * this.Scale);
            if (this.ProgressTexture.HasValue()) {
                batch.Draw(this.ProgressTexture, offsetArea, (Color) this.ProgressColor * alpha, this.Scale);
            } else {
                batch.Draw(batch.GetBlankTexture(), offsetArea, (Color) this.ProgressColor * alpha);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.ProgressBarTexture);
            this.Color.SetFromStyle(style.ProgressBarColor);
            this.ProgressPadding.SetFromStyle(style.ProgressBarProgressPadding);
            this.ProgressTexture.SetFromStyle(style.ProgressBarProgressTexture);
            this.ProgressColor.SetFromStyle(style.ProgressBarProgressColor);
        }

    }
}