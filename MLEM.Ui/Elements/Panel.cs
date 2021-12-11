using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A panel element to be used inside of a <see cref="UiSystem"/>.
    /// The panel is a complex element that displays a box as a background to all of its child elements.
    /// Additionally, a panel can be set to <see cref="scrollOverflow"/> on construction, which causes all elements that don't fit into the panel to be hidden until scrolled to using a <see cref="ScrollBar"/>.
    /// As this behavior is accomplished using a <see cref="RenderTarget2D"/>, scrolling panels need to have their <see cref="DrawEarly"/> methods called using <see cref="UiSystem.DrawEarly"/>.
    /// </summary>
    public class Panel : Element {

        /// <summary>
        /// The scroll bar that this panel contains.
        /// This is only nonnull if <see cref="scrollOverflow"/> is true.
        /// Note that some scroll bar styling is controlled by this panel, namely <see cref="StepPerScroll"/> and <see cref="ScrollerSize"/>.
        /// </summary>
        public readonly ScrollBar ScrollBar;

        /// <summary>
        /// The texture that this panel should have, or null if it should be invisible.
        /// </summary>
        public StyleProp<NinePatch> Texture;
        /// <summary>
        /// The color that this panel's <see cref="Texture"/> should be drawn with.
        /// If this style property has no value, <see cref="Color.White"/> is used.
        /// </summary>
        public StyleProp<Color> DrawColor;
        /// <summary>
        /// The amount that the scrollable area is moved per single movement of the scroll wheel
        /// This value is passed to the <see cref="ScrollBar"/>'s <see cref="Elements.ScrollBar.StepPerScroll"/>
        /// </summary>
        public StyleProp<float> StepPerScroll;
        /// <summary>
        /// The size that the <see cref="ScrollBar"/>'s scroller should have, in pixels.
        /// The scroller size's height specified here is the minimum height, otherwise, it is automatically calculated based on panel content.
        /// </summary>
        public StyleProp<Vector2> ScrollerSize;
        /// <summary>
        /// The amount of pixels of room there should be between the <see cref="ScrollBar"/> and the rest of the content
        /// </summary>
        public StyleProp<float> ScrollBarOffset;

        private readonly List<Element> relevantChildren = new List<Element>();
        private readonly bool scrollOverflow;

        private RenderTarget2D renderTarget;
        private bool relevantChildrenDirty;
        private float scrollBarChildOffset;

        /// <summary>
        /// Creates a new panel with the given settings.
        /// </summary>
        /// <param name="anchor">The panel's anchor</param>
        /// <param name="size">The panel's default size</param>
        /// <param name="positionOffset">The panel's offset from its anchor point</param>
        /// <param name="setHeightBasedOnChildren">Whether the panel should automatically calculate its height based on its children's size</param>
        /// <param name="scrollOverflow">Whether this panel should automatically add a scroll bar to scroll towards elements that are beyond the area this panel covers</param>
        /// <param name="autoHideScrollbar">Whether the scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling</param>
        public Panel(Anchor anchor, Vector2 size, Vector2 positionOffset, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, bool autoHideScrollbar = true) : base(anchor, size) {
            this.PositionOffset = positionOffset;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.scrollOverflow = scrollOverflow;
            this.CanBeSelected = false;

            if (scrollOverflow) {
                this.ScrollBar = new ScrollBar(Anchor.TopRight, Vector2.Zero, 0, 0) {
                    OnValueChanged = (element, value) => this.ScrollChildren(),
                    CanAutoAnchorsAttach = false,
                    AutoHideWhenEmpty = autoHideScrollbar,
                    IsHidden = autoHideScrollbar
                };

                // handle automatic element selection, the scroller needs to scroll to the right location
                this.OnSelectedElementChanged += (element, otherElement) => {
                    if (!this.Controls.IsAutoNavMode)
                        return;
                    if (otherElement == null || !otherElement.GetParentTree().Contains(this))
                        return;
                    var firstChild = this.Children.First(c => c != this.ScrollBar);
                    this.ScrollBar.CurrentValue = (otherElement.Area.Bottom - firstChild.Area.Top - this.Area.Height / 2) / this.Scale;
                };
                this.AddChild(this.ScrollBar);
            }
        }

        /// <inheritdoc />
        public override void ForceUpdateArea() {
            if (this.scrollOverflow) {
                // sanity check
                if (this.SetHeightBasedOnChildren)
                    throw new NotSupportedException("A panel can't both set height based on children and scroll overflow");
                foreach (var child in this.Children) {
                    if (child != this.ScrollBar && child.Anchor < Anchor.AutoLeft)
                        throw new NotSupportedException($"A panel that handles overflow can't contain non-automatic anchors ({child})");
                    if (child is Panel panel && panel.scrollOverflow)
                        throw new NotSupportedException($"A panel that scrolls overflow cannot contain another panel that scrolls overflow ({child})");
                }
            }
            base.ForceUpdateArea();
            this.SetScrollBarStyle();
        }

        /// <inheritdoc />
        public override void SetAreaAndUpdateChildren(RectangleF area) {
            base.SetAreaAndUpdateChildren(area);
            this.ScrollChildren();
            this.ScrollSetup();
        }

        private void ScrollChildren() {
            if (!this.scrollOverflow)
                return;
            var offset = new Vector2(0, -this.ScrollBar.CurrentValue);
            foreach (var child in this.GetChildren(c => c != this.ScrollBar, true)) {
                if (child.ScrollOffset != offset) {
                    child.ScrollOffset = offset;
                    this.relevantChildrenDirty = true;
                }
            }
        }

        /// <inheritdoc />
        public override void ForceUpdateSortedChildren() {
            base.ForceUpdateSortedChildren();
            if (this.scrollOverflow)
                this.ForceUpdateRelevantChildren();
        }

        /// <inheritdoc />
        public override void RemoveChild(Element element) {
            if (element == this.ScrollBar)
                throw new NotSupportedException("A panel that scrolls overflow cannot have its scroll bar removed from its list of children");
            base.RemoveChild(element);
        }

        /// <inheritdoc />
        public override void RemoveChildren(Func<Element, bool> condition = null) {
            base.RemoveChildren(e => e != this.ScrollBar && (condition == null || condition(e)));
        }

        /// <inheritdoc />
        protected override IList<Element> GetRelevantChildren() {
            var relevant = base.GetRelevantChildren();
            if (this.scrollOverflow) {
                if (this.relevantChildrenDirty)
                    this.ForceUpdateRelevantChildren();
                relevant = this.relevantChildren;
            }
            return relevant;
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, Effect effect, Matrix matrix) {
            if (this.Texture.HasValue())
                batch.Draw(this.Texture, this.DisplayArea, this.DrawColor.OrDefault(Color.White) * alpha, this.Scale);
            // if we handle overflow, draw using the render target in DrawUnbound
            if (!this.scrollOverflow || this.renderTarget == null) {
                base.Draw(time, batch, alpha, blendState, samplerState, depthStencilState, effect, matrix);
            } else {
                // draw the actual render target (don't apply the alpha here because it's already drawn onto with alpha)
                batch.Draw(this.renderTarget, this.GetRenderTargetArea(), Color.White);
            }
        }

        /// <inheritdoc />
        public override void DrawEarly(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, Effect effect, Matrix matrix) {
            this.UpdateAreaIfDirty();
            if (this.scrollOverflow && this.renderTarget != null) {
                // draw children onto the render target
                using (batch.GraphicsDevice.WithRenderTarget(this.renderTarget)) {
                    batch.GraphicsDevice.Clear(Color.Transparent);
                    // offset children by the render target's location
                    var area = this.GetRenderTargetArea();
                    var trans = Matrix.CreateTranslation(-area.X, -area.Y, 0);
                    // do the usual draw, but within the render target
                    batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, depthStencilState, null, effect, trans);
                    base.Draw(time, batch, alpha, blendState, samplerState, depthStencilState, effect, trans);
                    batch.End();
                }
            }
            base.DrawEarly(time, batch, alpha, blendState, samplerState, depthStencilState, effect, matrix);
        }

        /// <inheritdoc />
        public override Element GetElementUnderPos(Vector2 position) {
            // if overflow is handled, don't propagate mouse checks to hidden children
            var transformed = this.TransformInverse(position);
            if (this.scrollOverflow && !this.GetRenderTargetArea().Contains(transformed))
                return !this.IsHidden && this.CanBeMoused && this.DisplayArea.Contains(transformed) ? this : null;
            return base.GetElementUnderPos(position);
        }

        private RectangleF GetRenderTargetArea() {
            var area = this.ChildPaddedArea;
            area.X = this.DisplayArea.X;
            area.Width = this.DisplayArea.Width;
            return area;
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.PanelTexture);
            this.StepPerScroll.SetFromStyle(style.PanelStepPerScroll);
            this.ScrollerSize.SetFromStyle(style.PanelScrollerSize);
            this.ChildPadding.SetFromStyle(style.PanelChildPadding);
            this.ScrollBarOffset.SetFromStyle(style.PanelScrollBarOffset);
            this.SetScrollBarStyle();
        }

        /// <summary>
        /// Prepares the panel for auto-scrolling, creating the render target and setting up the scroll bar's maximum value.
        /// </summary>
        protected virtual void ScrollSetup() {
            if (!this.scrollOverflow || this.IsHidden)
                return;
            // if there is only one child, then we have just the scroll bar
            if (this.Children.Count == 1)
                return;

            // the "real" first child is the scroll bar, which we want to ignore
            var firstChild = this.Children.First(c => c != this.ScrollBar);
            var lowestChild = this.GetLowestChild(c => c != this.ScrollBar && !c.IsHidden);
            var childrenHeight = lowestChild.Area.Bottom - firstChild.Area.Top;

            // the max value of the scrollbar is the amount of non-scaled pixels taken up by overflowing components
            var scrollBarMax = (childrenHeight - this.ChildPaddedArea.Height) / this.Scale;
            if (!this.ScrollBar.MaxValue.Equals(scrollBarMax, Epsilon)) {
                this.ScrollBar.MaxValue = scrollBarMax;
                this.relevantChildrenDirty = true;

                // update child padding based on whether the scroll bar is visible
                var childOffset = this.ScrollBar.IsHidden ? 0 : this.ScrollerSize.Value.X + this.ScrollBarOffset;
                if (!this.scrollBarChildOffset.Equals(childOffset, Epsilon)) {
                    this.ChildPadding += new Padding(0, -this.scrollBarChildOffset + childOffset, 0, 0);
                    this.scrollBarChildOffset = childOffset;
                    this.SetAreaDirty();
                }
            }

            // the scroller height has the same relation to the scroll bar height as the visible area has to the total height of the panel's content
            var scrollerHeight = Math.Min(this.ChildPaddedArea.Height / childrenHeight / this.Scale, 1) * this.ScrollBar.Area.Height;
            this.ScrollBar.ScrollerSize = new Vector2(this.ScrollerSize.Value.X, Math.Max(this.ScrollerSize.Value.Y, scrollerHeight));

            // update the render target
            var targetArea = (Rectangle) this.GetRenderTargetArea();
            if (targetArea.Width <= 0 || targetArea.Height <= 0)
                return;
            if (this.renderTarget == null || targetArea.Width != this.renderTarget.Width || targetArea.Height != this.renderTarget.Height) {
                if (this.renderTarget != null)
                    this.renderTarget.Dispose();
                this.renderTarget = targetArea.IsEmpty ? null : new RenderTarget2D(this.System.Game.GraphicsDevice, targetArea.Width, targetArea.Height);
                this.relevantChildrenDirty = true;
            }
        }

        /// <inheritdoc />
        public override void Dispose() {
            if (this.renderTarget != null) {
                this.renderTarget.Dispose();
                this.renderTarget = null;
            }
            base.Dispose();
        }

        private void SetScrollBarStyle() {
            if (this.ScrollBar == null)
                return;
            this.ScrollBar.StepPerScroll = this.StepPerScroll;
            this.ScrollBar.Size = new Vector2(this.ScrollerSize.Value.X, 1);
            this.ScrollBar.PositionOffset = new Vector2(-this.ScrollerSize.Value.X - this.ScrollBarOffset, 0);
        }

        private void ForceUpdateRelevantChildren() {
            this.relevantChildrenDirty = false;
            this.relevantChildren.Clear();
            var visible = this.GetRenderTargetArea();
            foreach (var child in this.SortedChildren) {
                if (child.Area.Intersects(visible)) {
                    this.relevantChildren.Add(child);
                } else {
                    foreach (var c in child.GetChildren(regardGrandchildren: true)) {
                        if (c.Area.Intersects(visible)) {
                            this.relevantChildren.Add(child);
                            break;
                        }
                    }
                }
            }
        }

    }
}