using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class ProgressBar : Element {

        public StyleProp<NinePatch> Texture;
        public StyleProp<Color> Color;
        public StyleProp<Point> ProgressPadding;
        public StyleProp<NinePatch> ProgressTexture;
        public StyleProp<Color> ProgressColor;

        public Direction2 Direction;
        public float MaxValue;
        private float currentValue;
        public float CurrentValue {
            get => this.currentValue;
            set => this.currentValue = MathHelper.Clamp(value, 0, this.MaxValue);
        }

        public ProgressBar(Anchor anchor, Vector2 size, Direction2 direction, float maxValue, float currentValue = 0) : base(anchor, size) {
            if (!direction.IsAdjacent())
                throw new NotSupportedException("Progress bars only support Up, Down, Left and Right directions");
            this.Direction = direction;
            this.MaxValue = maxValue;
            this.currentValue = currentValue;
            this.CanBeSelected = false;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            batch.Draw(this.Texture, this.DisplayArea, (Color) this.Color * alpha, this.Scale);

            var percentage = this.CurrentValue / this.MaxValue;
            var tex = this.ProgressTexture.Value;
            var padHor = tex != null ? (tex.PaddingLeft + tex.PaddingRight) * this.Scale : 0;
            var padVer = tex != null ? (tex.PaddingTop + tex.PaddingBottom) * this.Scale : 0;
            var width = (percentage * (this.DisplayArea.Width - padHor) + padHor).Floor();
            var height = (percentage * (this.DisplayArea.Height - padVer) + padVer).Floor();
            Rectangle progressArea;
            switch (this.Direction) {
                case Direction2.Up:
                    progressArea = new Rectangle(this.DisplayArea.X,
                        this.DisplayArea.Y + (this.DisplayArea.Height - height),
                        this.DisplayArea.Width, height);
                    break;
                case Direction2.Down:
                    progressArea = new Rectangle(this.DisplayArea.Location, new Point(this.DisplayArea.Width, height));
                    break;
                case Direction2.Left:
                    progressArea = new Rectangle(
                        this.DisplayArea.X + (this.DisplayArea.Width - width),
                        this.DisplayArea.Y, width, this.DisplayArea.Height);
                    break;
                default: // Right
                    progressArea = new Rectangle(this.DisplayArea.Location, new Point(width, this.DisplayArea.Height));
                    break;
            }
            var offsetArea = progressArea.Shrink(this.ProgressPadding.Value.Multiply(this.Scale));
            if (this.ProgressTexture.Value != null) {
                batch.Draw(this.ProgressTexture, offsetArea, (Color) this.ProgressColor * alpha, this.Scale);
            } else {
                batch.Draw(batch.GetBlankTexture(), offsetArea, (Color) this.ProgressColor * alpha);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

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