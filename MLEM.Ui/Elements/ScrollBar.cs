using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Graphics;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A scroll bar element to be used inside of a <see cref="UiSystem"/>.
    /// A scroll bar is an element that features a smaller scroller indicator inside of it that can move up and down.
    /// A scroll bar can be scrolled using the mouse or by using the scroll wheel while hovering over its <see cref="Element.Parent"/> or any of its siblings.
    /// </summary>
    public class ScrollBar : Element {

        /// <summary>
        /// Whether this scroll bar is horizontal
        /// </summary>
        public readonly bool Horizontal;

        /// <summary>
        /// The background texture for this scroll bar
        /// </summary>
        public StyleProp<NinePatch> Background;
        /// <summary>
        /// The texture of this scroll bar's scroller indicator
        /// </summary>
        public StyleProp<NinePatch> ScrollerTexture;
        /// <summary>
        /// The scroller's width and height
        /// </summary>
        public Vector2 ScrollerSize;
        /// <summary>
        /// The max value that this scroll bar should be able to scroll to.
        /// Note that the maximum value does not change the height of the scroll bar.
        /// </summary>
        public float MaxValue {
            get => this.maxValue;
            set {
                this.maxValue = Math.Max(0, value);
                // force current value to be clamped
                this.CurrentValue = this.CurrentValue;
                // auto-hide if necessary
                var shouldHide = this.maxValue <= Epsilon;
                if (this.AutoHideWhenEmpty && this.IsHidden != shouldHide) {
                    this.IsHidden = shouldHide;
                    this.OnAutoHide?.Invoke(this);
                }
            }
        }
        /// <summary>
        /// The current value of the scroll bar.
        /// This is between 0 and <see cref="MaxValue"/> at all times.
        /// </summary>
        public float CurrentValue {
            get => this.currValue - this.scrollAdded;
            set {
                var val = MathHelper.Clamp(value, 0, this.maxValue);
                if (this.currValue != val) {
                    if (this.SmoothScrolling)
                        this.scrollAdded = val - this.currValue;
                    this.currValue = val;
                    this.OnValueChanged?.Invoke(this, val);
                }
            }
        }
        /// <summary>
        /// The amount added or removed from <see cref="CurrentValue"/> per single movement of the scroll wheel
        /// </summary>
        public float StepPerScroll = 1;
        /// <summary>
        /// An event that is called when <see cref="CurrentValue"/> changes
        /// </summary>
        public ValueChanged OnValueChanged;
        /// <summary>
        /// An event that is called when this scroll bar is automatically hidden from a <see cref="Panel"/>
        /// </summary>
        public GenericCallback OnAutoHide;
        /// <summary>
        /// This property is true while the user scrolls on the scroll bar using the mouse or touch input
        /// </summary>
        public bool IsBeingScrolled => this.isMouseHeld || this.isDragging || this.isTouchHeld;
        /// <summary>
        /// This field determines if this scroll bar should automatically be hidden from a <see cref="Panel"/> if there aren't enough children to allow for scrolling.
        /// </summary>
        public bool AutoHideWhenEmpty;
        /// <summary>
        /// The position that this scroll bar's scroller is currently at.
        /// This value takes the <see cref="Element.DisplayArea"/>, as well as the <see cref="Element.Scale"/> into account.
        /// </summary>
        public Vector2 ScrollerPosition => this.DisplayArea.Location + new Vector2(
            !this.Horizontal ? 0 : this.CurrentValue / this.maxValue * (this.DisplayArea.Width - this.ScrollerSize.X * this.Scale),
            this.Horizontal ? 0 : this.CurrentValue / this.maxValue * (this.DisplayArea.Height - this.ScrollerSize.Y * this.Scale));
        /// <summary>
        /// Whether smooth scrolling should be enabled for this scroll bar.
        /// Smooth scrolling causes the <see cref="CurrentValue"/> to change gradually rather than instantly when scrolling.
        /// </summary>
        public StyleProp<bool> SmoothScrolling;
        /// <summary>
        /// The factor with which <see cref="SmoothScrolling"/> happens.
        /// </summary>
        public StyleProp<float> SmoothScrollFactor;

        private bool isMouseHeld;
        private bool isDragging;
        private bool isTouchHeld;
        private float maxValue;
        private float scrollAdded;
        private float currValue;
        private Vector2 scrollStartOffset;

        static ScrollBar() {
            InputHandler.EnableGestures(GestureType.HorizontalDrag, GestureType.VerticalDrag);
        }

        /// <summary>
        /// Creates a new scroll bar with the given settings
        /// </summary>
        /// <param name="anchor">The scroll bar's anchor</param>
        /// <param name="size">The scroll bar's size</param>
        /// <param name="scrollerSize"></param>
        /// <param name="maxValue"></param>
        /// <param name="horizontal"></param>
        public ScrollBar(Anchor anchor, Vector2 size, int scrollerSize, float maxValue, bool horizontal = false) : base(anchor, size) {
            this.maxValue = maxValue;
            this.Horizontal = horizontal;
            this.ScrollerSize = new Vector2(horizontal ? scrollerSize : size.X, !horizontal ? scrollerSize : size.Y);
            this.CanBeSelected = false;
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);

            // MOUSE INPUT
            var moused = this.Controls.MousedElement;
            if (moused == this && this.Input.IsMouseButtonPressed(MouseButton.Left)) {
                this.isMouseHeld = true;
                this.scrollStartOffset = this.TransformInverseAll(this.Input.ViewportMousePosition.ToVector2()) - this.ScrollerPosition;
            } else if (this.isMouseHeld && !this.Input.IsMouseButtonDown(MouseButton.Left)) {
                this.isMouseHeld = false;
            }
            if (this.isMouseHeld)
                this.ScrollToPos(this.TransformInverseAll(this.Input.ViewportMousePosition.ToVector2()));
            if (!this.Horizontal && moused != null && (moused == this.Parent || moused.GetParentTree().Contains(this.Parent))) {
                var scroll = this.Input.LastScrollWheel - this.Input.ScrollWheel;
                if (scroll != 0)
                    this.CurrentValue += this.StepPerScroll * Math.Sign(scroll);
            }

            // TOUCH INPUT
            if (!this.Horizontal) {
                // are we dragging on top of the panel?
                if (this.Input.GetViewportGesture(GestureType.VerticalDrag, out var drag)) {
                    // if the element under the drag's start position is on top of the panel, start dragging
                    var touched = this.Parent.GetElementUnderPos(this.TransformInverseAll(drag.Position));
                    if (touched != null && touched != this)
                        this.isDragging = true;

                    // if we're dragging at all, then move the scroller
                    if (this.isDragging)
                        this.CurrentValue -= drag.Delta.Y / this.Scale;
                } else {
                    this.isDragging = false;
                }
            }
            if (this.Input.ViewportTouchState.Count <= 0) {
                // if no touch has occured this tick, then reset the variable
                this.isTouchHeld = false;
            } else {
                foreach (var loc in this.Input.ViewportTouchState) {
                    var pos = this.TransformInverseAll(loc.Position);
                    // if we just started touching and are on top of the scroller, then we should start scrolling
                    if (this.DisplayArea.Contains(pos) && !loc.TryGetPreviousLocation(out _)) {
                        this.isTouchHeld = true;
                        this.scrollStartOffset = pos - this.ScrollerPosition;
                        break;
                    }
                    // scroll no matter if we're on the scroller right now
                    if (this.isTouchHeld)
                        this.ScrollToPos(pos);
                }
            }

            if (this.SmoothScrolling && this.scrollAdded != 0) {
                this.scrollAdded *= this.SmoothScrollFactor;
                if (Math.Abs(this.scrollAdded) <= Epsilon)
                    this.scrollAdded = 0;
                this.OnValueChanged?.Invoke(this, this.CurrentValue);
            }
        }

        private void ScrollToPos(Vector2 position) {
            var (width, height) = this.ScrollerSize * this.Scale;
            if (this.Horizontal) {
                var offset = this.scrollStartOffset.X >= 0 && this.scrollStartOffset.X <= width ? this.scrollStartOffset.X : width / 2;
                this.CurrentValue = (position.X - this.Area.X - offset) / (this.Area.Width - width) * this.MaxValue;
            } else {
                var offset = this.scrollStartOffset.Y >= 0 && this.scrollStartOffset.Y <= height ? this.scrollStartOffset.Y : height / 2;
                this.CurrentValue = (position.Y - this.Area.Y - offset) / (this.Area.Height - height) * this.MaxValue;
            }
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            batch.Draw(this.Background, this.DisplayArea, Color.White * alpha, this.Scale);
            if (this.MaxValue > 0) {
                var scrollerRect = new RectangleF(this.ScrollerPosition, this.ScrollerSize * this.Scale);
                batch.Draw(this.ScrollerTexture, scrollerRect, Color.White * alpha, this.Scale);
            }
            base.Draw(time, batch, alpha, context);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Background = this.Background.OrStyle(style.ScrollBarBackground);
            this.ScrollerTexture = this.ScrollerTexture.OrStyle(style.ScrollBarScrollerTexture);
            this.SmoothScrolling = this.SmoothScrolling.OrStyle(style.ScrollBarSmoothScrolling);
            this.SmoothScrollFactor = this.SmoothScrollFactor.OrStyle(style.ScrollBarSmoothScrollFactor);
        }

        /// <summary>
        /// A delegate method used for <see cref="ScrollBar.OnValueChanged"/>
        /// </summary>
        /// <param name="element">The element whose current value changed</param>
        /// <param name="value">The element's new <see cref="ScrollBar.CurrentValue"/></param>
        public delegate void ValueChanged(Element element, float value);

    }
}