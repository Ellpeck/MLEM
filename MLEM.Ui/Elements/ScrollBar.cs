using System;
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
        public Color HoveredColor;
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
        public float StepPerScroll = 1;
        public ValueChanged OnValueChanged;
        private bool isMouseHeld;

        public ScrollBar(Anchor anchor, Vector2 size, int scrollerHeight, float maxValue) : base(anchor, size) {
            this.maxValue = maxValue;
            this.ScrollerSize = new Point(size.X.Floor(), scrollerHeight);
        }

        public override void Update(GameTime time) {
            base.Update(time);
            var moused = this.System.MousedElement;
            if (moused == this && this.Input.IsMouseButtonDown(MouseButton.Left)) {
                this.isMouseHeld = true;
            } else if (this.isMouseHeld && this.Input.IsMouseButtonUp(MouseButton.Left)) {
                this.isMouseHeld = false;
            }
            
            if (this.isMouseHeld) {
                var internalY = this.MousePos.Y - this.Area.Y;
                this.CurrentValue = internalY / (float) this.Area.Height * this.MaxValue;
            }
            
            if (moused == this.Parent || moused?.Parent == this.Parent) {
                var scroll = this.Input.LastScrollWheel - this.Input.ScrollWheel;
                if (scroll != 0)
                    this.CurrentValue += this.StepPerScroll * Math.Sign(scroll);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            batch.Draw(this.Background, this.DisplayArea.OffsetCopy(offset), Color.White * alpha, this.Scale);

            var scrollerPos = new Point(this.DisplayArea.X + offset.X + this.ScrollerOffset.X, this.DisplayArea.Y + offset.Y + this.ScrollerOffset.Y);
            var scrollerYOffset = (this.currValue / this.maxValue * (this.DisplayArea.Height - this.ScrollerSize.Y * this.Scale)).Floor();
            var scrollerRect = new Rectangle(scrollerPos + new Point(0, scrollerYOffset), this.ScrollerSize.Multiply(this.Scale));
            batch.Draw(this.ScrollerTexture, scrollerRect, Color.White * alpha, this.Scale);
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Background = style.ScrollBarBackground;
            this.ScrollerTexture = style.ScrollBarScrollerTexture;
            this.HoveredColor = style.ScrollBarHoveredColor;
        }

        public delegate void ValueChanged(Element element, float value);

    }
}