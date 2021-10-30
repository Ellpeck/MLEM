using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Extensions;
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
        /// The <see cref="ScrollerTexture"/>'s offset from the calculated position of the scroller. Use this to pad the scroller.
        /// </summary>
        public Vector2 ScrollerOffset;
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
                    var touched = this.Parent.GetElementUnderPos(Vector2.Transform(drag.Position, this.Root.InvTransform));
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

            if (this.SmoothScrolling && this.scrollAdded != 0) {
                this.scrollAdded *= this.SmoothScrollFactor;
                if (Math.Abs(this.scrollAdded) <= Epsilon)
                    this.scrollAdded = 0;
                this.OnValueChanged?.Invoke(this, this.CurrentValue);
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

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            batch.Draw(this.Background, this.DisplayArea, Color.White * alpha, this.Scale);

            if (this.MaxValue > 0) {
                var scrollerPos = new Vector2(this.DisplayArea.X + this.ScrollerOffset.X, this.DisplayArea.Y + this.ScrollerOffset.Y);
                var scrollerOffset = new Vector2(
                    !this.Horizontal ? 0 : this.CurrentValue / this.maxValue * (this.DisplayArea.Width - this.ScrollerSize.X * this.Scale),
                    this.Horizontal ? 0 : this.CurrentValue / this.maxValue * (this.DisplayArea.Height - this.ScrollerSize.Y * this.Scale));
                var scrollerRect = new RectangleF(scrollerPos + scrollerOffset, this.ScrollerSize * this.Scale);
                batch.Draw(this.ScrollerTexture, scrollerRect, Color.White * alpha, this.Scale);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Background.SetFromStyle(style.ScrollBarBackground);
            this.ScrollerTexture.SetFromStyle(style.ScrollBarScrollerTexture);
            this.SmoothScrolling.SetFromStyle(style.ScrollBarSmoothScrolling);
            this.SmoothScrollFactor.SetFromStyle(style.ScrollBarSmoothScrollFactor);
        }

        /// <summary>
        /// A delegate method used for <see cref="ScrollBar.OnValueChanged"/>
        /// </summary>
        /// <param name="element">The element whose current value changed</param>
        /// <param name="value">The element's new <see cref="ScrollBar.CurrentValue"/></param>
        public delegate void ValueChanged(Element element, float value);

    }
}