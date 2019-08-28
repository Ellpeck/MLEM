using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Input;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class ScrollBar : Element {

        public NinePatch Background;
        public NinePatch ScrollerTexture;
        public Point ScrollerOffset;
        public Point ScrollerSize;
        private float maxValue;
        public float MaxValue {
            get => this.maxValue;
            set {
                this.maxValue = Math.Max(0, value);
                // force current value to be clamped
                this.CurrentValue = this.currValue;
            }
        }
        private float currValue;
        public float CurrentValue {
            get => this.currValue;
            set {
                var val = MathHelper.Clamp(value, 0, this.maxValue);
                if (this.currValue != val) {
                    this.currValue = val;
                    this.OnValueChanged?.Invoke(this, val);
                }
            }
        }
        public readonly bool Horizontal;
        public float StepPerScroll = 1;
        public ValueChanged OnValueChanged;
        private bool isMouseHeld;

        public ScrollBar(Anchor anchor, Vector2 size, int scrollerSize, float maxValue, bool horizontal = false) : base(anchor, size) {
            this.maxValue = maxValue;
            this.Horizontal = horizontal;
            this.ScrollerSize = new Point(horizontal ? scrollerSize : size.X.Floor(), !horizontal ? scrollerSize : size.Y.Floor());
        }

        public override void Update(GameTime time) {
            base.Update(time);
            var moused = this.System.MousedElement;
            if (moused == this && this.Controls.MainButton(this.Input)) {
                this.isMouseHeld = true;
            } else if (this.isMouseHeld && this.Controls.MainButton(this.Input)) {
                this.isMouseHeld = false;
            }

            if (this.isMouseHeld) {
                if (this.Horizontal) {
                    var internalX = this.MousePos.X - this.Area.X - this.ScrollerSize.X * this.Scale / 2;
                    this.CurrentValue = internalX / (this.Area.Width - this.ScrollerSize.X * this.Scale) * this.MaxValue;
                } else {
                    var internalY = this.MousePos.Y - this.Area.Y - this.ScrollerSize.Y * this.Scale / 2;
                    this.CurrentValue = internalY / (this.Area.Height - this.ScrollerSize.Y * this.Scale) * this.MaxValue;
                }
            }

            if (!this.Horizontal && moused != null && (moused == this.Parent || moused.GetParentTree().Contains(this.Parent))) {
                var scroll = this.Controls.Scroll(this.Input);
                if (scroll != 0)
                    this.CurrentValue += this.StepPerScroll * Math.Sign(scroll);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            batch.Draw(this.Background, this.DisplayArea.OffsetCopy(offset), Color.White * alpha, this.Scale);

            var scrollerPos = new Point(this.DisplayArea.X + offset.X + this.ScrollerOffset.X, this.DisplayArea.Y + offset.Y + this.ScrollerOffset.Y);
            var scrollerOffset = new Point(
                !this.Horizontal ? 0 : (this.currValue / this.maxValue * (this.DisplayArea.Width - this.ScrollerSize.X * this.Scale)).Floor(),
                this.Horizontal ? 0 : (this.currValue / this.maxValue * (this.DisplayArea.Height - this.ScrollerSize.Y * this.Scale)).Floor());
            var scrollerRect = new Rectangle(scrollerPos + scrollerOffset, this.ScrollerSize.Multiply(this.Scale));
            batch.Draw(this.ScrollerTexture, scrollerRect, Color.White * alpha, this.Scale);
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Background = style.ScrollBarBackground;
            this.ScrollerTexture = style.ScrollBarScrollerTexture;
        }

        public delegate void ValueChanged(Element element, float value);

    }
}