using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
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
                if (this.AutoHideWhenEmpty)
                    this.IsHidden = this.maxValue <= 0;
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
        private bool isDragging;
        private bool isTouchHeld;
        public bool AutoHideWhenEmpty;

        static ScrollBar() {
            InputHandler.EnableGestures(GestureType.HorizontalDrag, GestureType.VerticalDrag);
        }

        public ScrollBar(Anchor anchor, Vector2 size, int scrollerSize, float maxValue, bool horizontal = false) : base(anchor, size) {
            this.maxValue = maxValue;
            this.Horizontal = horizontal;
            this.ScrollerSize = new Point(horizontal ? scrollerSize : size.X.Floor(), !horizontal ? scrollerSize : size.Y.Floor());
            this.CanBeSelected = false;
        }

        public override void Update(GameTime time) {
            base.Update(time);

            // MOUSE INPUT
            var moused = this.Controls.MousedElement;
            if (moused == this && this.Controls.Input.IsMouseButtonPressed(MouseButton.Left)) {
                this.isMouseHeld = true;
            } else if (this.isMouseHeld && !this.Controls.Input.IsMouseButtonDown(MouseButton.Left)) {
                this.isMouseHeld = false;
            }
            if (this.isMouseHeld)
                this.ScrollToPos(this.Input.MousePosition.Transform(this.Root.InvTransform));
            if (!this.Horizontal && moused != null && (moused == this.Parent || moused.GetParentTree().Contains(this.Parent))) {
                var scroll = this.Input.LastScrollWheel - this.Input.ScrollWheel;
                if (scroll != 0)
                    this.CurrentValue += this.StepPerScroll * Math.Sign(scroll);
            }

            // TOUCH INPUT
            if (!this.Horizontal) {
                // are we dragging on top of the panel?
                if (this.Input.GetGesture(GestureType.VerticalDrag, out var drag)) {
                    // if the element under the drag's start position is on top of the panel, start dragging
                    var touched = this.Parent.GetElementUnderPos(drag.Position.ToPoint().Transform(this.Root.InvTransform));
                    if (touched != null && touched != this)
                        this.isDragging = true;

                    // if we're dragging at all, then move the scroller
                    if (this.isDragging)
                        this.CurrentValue -= drag.Delta.Y / this.Scale;
                } else {
                    this.isDragging = false;
                }
            }
            if (this.Input.TouchState.Count <= 0) {
                // if no touch has occured this tick, then reset the variable
                this.isTouchHeld = false;
            } else {
                foreach (var loc in this.Input.TouchState) {
                    var pos = Vector2.Transform(loc.Position, this.Root.InvTransform);
                    // if we just started touching and are on top of the scroller, then we should start scrolling
                    if (this.DisplayArea.Contains(pos) && !loc.TryGetPreviousLocation(out _)) {
                        this.isTouchHeld = true;
                        break;
                    }
                    // scroll no matter if we're on the scroller right now
                    if (this.isTouchHeld)
                        this.ScrollToPos(pos.ToPoint());
                }
            }
        }

        private void ScrollToPos(Point position) {
            if (this.Horizontal) {
                var internalX = position.X - this.Area.X - this.ScrollerSize.X * this.Scale / 2;
                this.CurrentValue = internalX / (this.Area.Width - this.ScrollerSize.X * this.Scale) * this.MaxValue;
            } else {
                var internalY = position.Y - this.Area.Y - this.ScrollerSize.Y * this.Scale / 2;
                this.CurrentValue = internalY / (this.Area.Height - this.ScrollerSize.Y * this.Scale) * this.MaxValue;
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