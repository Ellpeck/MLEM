using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A panel element to be used inside of a <see cref="UiSystem"/>.
    /// The panel is a complex element that displays a box as a background to all of its child elements.
    /// Additionally, a panel can be set to scroll overflowing elements on construction, which causes all elements that don't fit into the panel to be hidden until scrolled to using a <see cref="ScrollBar"/>.
    /// </summary>
    public class Panel : Element {

        /// <summary>
        /// The scroll bar that this panel contains.
        /// This is only nonnull if scrolling overflow was enabled in the constructor.
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
        public StyleProp<float> ScrollBarOffset {
            get => this.scrollBarOffset;
            set {
                this.scrollBarOffset = value;
                this.SetAreaDirty();
            }
        }

        private readonly List<Element> relevantChildren = new List<Element>();
        private readonly HashSet<Element> scrolledChildren = new HashSet<Element>();
        private readonly float[] scrollBarMaxHistory;
        private readonly bool scrollOverflow;

        private RenderTarget2D renderTarget;
        private bool relevantChildrenDirty;
        private float scrollBarChildOffset;
        private StyleProp<float> scrollBarOffset;
        private float lastScrollOffset;
        private bool childrenDirtyForScroll;

        /// <summary>
        /// Creates a new panel with the given settings.
        /// </summary>
        /// <param name="anchor">The panel's anchor</param>
        /// <param name="size">The panel's default size</param>
        /// <param name="positionOffset">The panel's offset from its anchor point</param>
        /// <param name="setHeightBasedOnChildren">Whether the panel should automatically calculate its height based on its children's size</param>
        /// <param name="scrollOverflow">Whether this panel should automatically add a scroll bar to scroll towards elements that are beyond the area this panel covers</param>
        /// <param name="autoHideScrollbar">Whether the scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling. This only has an effect if <paramref name="scrollOverflow"/> is <see langword="true"/>.</param>
        public Panel(Anchor anchor, Vector2 size, Vector2 positionOffset, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, bool autoHideScrollbar = true) : base(anchor, size) {
            this.PositionOffset = positionOffset;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.TreatSizeAsMaximum = setHeightBasedOnChildren && scrollOverflow;
            this.scrollOverflow = scrollOverflow;
            this.CanBeSelected = false;

            if (scrollOverflow) {
                this.scrollBarMaxHistory = new float[3];
                this.ResetScrollBarMaxHistory();

                this.ScrollBar = new ScrollBar(Anchor.TopRight, Vector2.Zero, 0, 0) {
                    OnValueChanged = (element, value) => this.ScrollChildren(),
                    CanAutoAnchorsAttach = false,
                    AutoHideWhenEmpty = autoHideScrollbar,
                    IsHidden = autoHideScrollbar
                };

                // handle automatic element selection, the scroller needs to scroll to the right location
                this.OnSelectedElementChanged += (_, e) => {
                    if (!this.Controls.IsAutoNavMode)
                        return;
                    if (e == null || !e.GetParentTree().Contains(this))
                        return;
                    this.ScrollToElement(e);
                };
                this.AddChild(this.ScrollBar);
            }
        }

        /// <summary>
        /// Creates a new panel with the given settings.
        /// </summary>
        /// <param name="anchor">The panel's anchor</param>
        /// <param name="size">The panel's default size</param>
        /// <param name="setHeightBasedOnChildren">Whether the panel should automatically calculate its height based on its children's size</param>
        /// <param name="scrollOverflow">Whether this panel should automatically add a scroll bar to scroll towards elements that are beyond the area this panel covers</param>
        /// <param name="autoHideScrollbar">Whether the scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling. This only has an effect if <paramref name="scrollOverflow"/> is <see langword="true"/>.</param>
        public Panel(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, bool autoHideScrollbar = true) : this(anchor, size, Vector2.Zero, setHeightBasedOnChildren, scrollOverflow, autoHideScrollbar) {}

        /// <inheritdoc />
        public override void ForceUpdateArea() {
            if (this.scrollOverflow) {
                // sanity check
                if (this.SetHeightBasedOnChildren && !this.TreatSizeAsMaximum)
                    throw new NotSupportedException("A panel can't both scroll overflow and set height based on children without a maximum");
                foreach (var child in this.Children) {
                    if (child != this.ScrollBar && !child.Anchor.IsAuto())
                        throw new NotSupportedException($"A panel that handles overflow can't contain non-automatic anchors ({child})");
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

            this.ResetScrollBarMaxHistory();

            // when removing children, our scroll bar might have to be hidden
            // if we don't do this before adding children again, they might incorrectly assume that the scroll bar will still be visible and adjust their size accordingly
            this.childrenDirtyForScroll = true;
        }

        /// <inheritdoc />
        public override T AddChild<T>(T element, int index = -1) {
            // if children were recently removed, make sure to update the scroll bar before adding new ones so that they can't incorrectly assume the scroll bar will be visible
            if (this.childrenDirtyForScroll && this.System != null)
                this.ScrollSetup();

            this.ResetScrollBarMaxHistory();

            return base.AddChild(element, index);
        }

        /// <inheritdoc />
        public override void RemoveChildren(Func<Element, bool> condition = null) {
            base.RemoveChildren(e => e != this.ScrollBar && (condition == null || condition(e)));
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            // draw children onto the render target if we have one
            if (this.scrollOverflow && this.renderTarget != null) {
                this.UpdateAreaIfDirty();
                batch.End();
                // force render target usage to preserve so that previous content isn't cleared
                var lastUsage = batch.GraphicsDevice.PresentationParameters.RenderTargetUsage;
                batch.GraphicsDevice.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
                using (batch.GraphicsDevice.WithRenderTarget(this.renderTarget)) {
                    batch.GraphicsDevice.Clear(Color.Transparent);
                    // offset children by the render target's location
                    var area = this.GetRenderTargetArea();
                    // do the usual draw, but within the render target
                    var trans = context;
                    trans.TransformMatrix = Matrix.CreateTranslation(-area.X, -area.Y, 0);
                    batch.Begin(trans);
                    base.Draw(time, batch, alpha, trans);
                    batch.End();
                }
                batch.GraphicsDevice.PresentationParameters.RenderTargetUsage = lastUsage;
                batch.Begin(context);
            }

            if (this.Texture.HasValue())
                batch.Draw(this.Texture, this.DisplayArea, this.DrawColor.OrDefault(Color.White) * alpha, this.Scale);
            // if we handle overflow, draw using the render target in DrawUnbound
            if (!this.scrollOverflow || this.renderTarget == null) {
                base.Draw(time, batch, alpha, context);
            } else {
                // draw the actual render target (don't apply the alpha here because it's already drawn onto with alpha)
                batch.Draw(this.renderTarget, this.GetRenderTargetArea(), Color.White);
            }
        }

        /// <inheritdoc />
        public override Element GetElementUnderPos(Vector2 position) {
            // if overflow is handled, don't propagate mouse checks to hidden children
            var transformed = this.TransformInverse(position);
            if (this.scrollOverflow && !this.GetRenderTargetArea().Contains(transformed))
                return !this.IsHidden && this.CanBeMoused && this.DisplayArea.Contains(transformed) ? this : null;
            return base.GetElementUnderPos(position);
        }

        /// <summary>
        /// Scrolls this panel's <see cref="ScrollBar"/> to the given <see cref="Element"/> in such a way that its center is positioned in the center of this panel.
        /// </summary>
        /// <param name="element">The element to scroll to.</param>
        public void ScrollToElement(Element element) {
            this.ScrollToElement(element.Area.Center.Y);
        }

        /// <summary>
        /// Scrolls this panel's <see cref="ScrollBar"/> to the given <paramref name="elementY"/> coordinate in such a way that the coordinate is positioned in the center of this panel.
        /// </summary>
        /// <param name="elementY">The y coordinate to scroll to, which should have this element's <see cref="Element.Scale"/> applied.</param>
        public void ScrollToElement(float elementY) {
            var highestValidChild = this.Children.FirstOrDefault(c => c != this.ScrollBar && !c.IsHidden);
            if (highestValidChild == null)
                return;
            this.UpdateAreaIfDirty();
            this.ScrollBar.CurrentValue = (elementY - this.Area.Height / 2 - highestValidChild.Area.Top) / this.Scale + this.ChildPadding.Value.Height / 2;
        }

        /// <summary>
        /// Scrolls this panel's <see cref="ScrollBar"/> to the top, causing the top of this panel to be shown.
        /// </summary>
        public void ScrollToTop() {
            this.UpdateAreaIfDirty();
            this.ScrollBar.CurrentValue = 0;
        }

        /// <summary>
        /// Scrolls this panel's <see cref="ScrollBar"/> to the bottom, causing the bottom of this panel to be shown.
        /// </summary>
        public void ScrollToBottom() {
            this.UpdateAreaIfDirty();
            this.ScrollBar.CurrentValue = this.ScrollBar.MaxValue;
        }

        /// <summary>
        /// Returns whether the given <paramref name="element"/> is currently visible within this panel if it scrolls overflow.
        /// This method will return <see langword="true"/> on any elements whose <see cref="Element.Area"/> intersects this panel's render target area, regardless of whether it is a child or grandchild of this panel.
        /// </summary>
        /// <param name="element">The element to query for visibility.</param>
        /// <returns>Whether the element is in this panel's visible area.</returns>
        public bool IsVisible(Element element) {
            return element.Area.Intersects(this.GetRenderTargetArea());
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = this.Texture.OrStyle(style.PanelTexture);
            this.DrawColor = this.DrawColor.OrStyle(style.PanelColor);
            this.StepPerScroll = this.StepPerScroll.OrStyle(style.PanelStepPerScroll);
            this.ScrollerSize = this.ScrollerSize.OrStyle(style.PanelScrollerSize);
            this.ScrollBarOffset = this.ScrollBarOffset.OrStyle(style.PanelScrollBarOffset);
            this.ChildPadding = this.ChildPadding.OrStyle(style.PanelChildPadding);
            this.SetScrollBarStyle();
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
        protected override void OnChildAreaDirty(Element child, bool grandchild) {
            base.OnChildAreaDirty(child, grandchild);
            if (grandchild && !this.AreaDirty) {
                // we only need to scroll when a grandchild changes, since all of our children are forced
                // to be auto-anchored and so will automatically propagate their changes up to us
                this.ScrollChildren();
                // we also need to re-setup here in case the child is involved in a special GetTotalCoveredArea
                this.ScrollSetup();
            }
        }

        /// <inheritdoc />
        protected internal override void RemovedFromUi() {
            base.RemovedFromUi();
            // we dispose our render target when removing so that it doesn't cause a memory leak
            // if we're added back afterwards, it'll be recreated in ScrollSetup anyway
            this.renderTarget?.Dispose();
            this.renderTarget = null;
        }

        /// <summary>
        /// Prepares the panel for auto-scrolling, creating the render target and setting up the scroll bar's maximum value.
        /// </summary>
        protected virtual void ScrollSetup() {
            this.childrenDirtyForScroll = false;

            if (!this.scrollOverflow || this.IsHidden)
                return;

            float childrenHeight;
            if (this.Children.Count > 1) {
                var highestValidChild = this.Children.FirstOrDefault(c => c != this.ScrollBar && !c.IsHidden);
                var lowestChild = this.GetLowestChild(c => c != this.ScrollBar && !c.IsHidden, true);
                childrenHeight = lowestChild.GetTotalCoveredArea(true).Bottom - highestValidChild.UnscrolledArea.Top;
            } else {
                // if we only have one child (the scroll bar), then the children take up no visual height
                childrenHeight = 0;
            }

            // the max value of the scroll bar is the amount of non-scaled pixels taken up by overflowing components
            var scrollBarMax = Math.Max(0, (childrenHeight - this.ChildPaddedArea.Height) / this.Scale);
            // avoid an infinite show/hide oscillation that occurs while updating our area by simply using the maximum recent height in that case
            if (this.scrollBarMaxHistory[0].Equals(this.scrollBarMaxHistory[2], Element.Epsilon) && this.scrollBarMaxHistory[1].Equals(scrollBarMax, Element.Epsilon))
                scrollBarMax = Math.Max(scrollBarMax, this.scrollBarMaxHistory.Max());
            if (!this.ScrollBar.MaxValue.Equals(scrollBarMax, Element.Epsilon)) {
                this.scrollBarMaxHistory[0] = this.scrollBarMaxHistory[1];
                this.scrollBarMaxHistory[1] = this.scrollBarMaxHistory[2];
                this.scrollBarMaxHistory[2] = scrollBarMax;

                this.ScrollBar.MaxValue = scrollBarMax;
                this.relevantChildrenDirty = true;
            }

            // update child padding based on whether the scroll bar is visible
            var childOffset = this.ScrollBar.IsHidden ? 0 : this.ScrollerSize.Value.X + this.ScrollBarOffset;
            var childOffsetDelta = childOffset - this.scrollBarChildOffset;
            if (!childOffsetDelta.Equals(0, Element.Epsilon)) {
                this.scrollBarChildOffset = childOffset;
                this.ChildPadding += new Padding(0, childOffsetDelta, 0, 0);
            }

            // the scroller height has the same relation to the scroll bar height as the visible area has to the total height of the panel's content
            var scrollerHeight = Math.Min(this.ChildPaddedArea.Height / childrenHeight / this.Scale, 1) * this.ScrollBar.Area.Height;
            this.ScrollBar.ScrollerSize = new Vector2(this.ScrollerSize.Value.X, Math.Max(this.ScrollerSize.Value.Y, scrollerHeight));

            // update the render target
            var area = (Rectangle) this.GetRenderTargetArea();
            if (area.Width <= 0 || area.Height <= 0) {
                this.renderTarget?.Dispose();
                this.renderTarget = null;
                return;
            }
            if (this.renderTarget == null || area.Width != this.renderTarget.Width || area.Height != this.renderTarget.Height) {
                this.renderTarget?.Dispose();
                this.renderTarget = new RenderTarget2D(this.System.Game.GraphicsDevice, area.Width, area.Height, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
                this.relevantChildrenDirty = true;
            }
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
            foreach (var child in this.SortedChildren) {
                if (this.IsVisible(child)) {
                    this.relevantChildren.Add(child);
                } else {
                    foreach (var c in child.GetChildren(regardGrandchildren: true)) {
                        if (this.IsVisible(c)) {
                            this.relevantChildren.Add(child);
                            break;
                        }
                    }
                }
            }
        }

        private RectangleF GetRenderTargetArea() {
            var area = this.ChildPaddedArea.OffsetCopy(this.ScaledScrollOffset);
            area.X = this.DisplayArea.X;
            area.Width = this.DisplayArea.Width;
            return area;
        }

        private void ScrollChildren() {
            if (!this.scrollOverflow)
                return;

            var currentChildren = new HashSet<Element>();
            // scroll all our children (and cache newly added ones)
            // we ignore false grandchildren so that the children of the scroll bar stay in place
            foreach (var child in this.GetChildren(c => c != this.ScrollBar, true, true)) {
                // if a child was newly added later, the last scroll offset was never applied
                if (this.scrolledChildren.Add(child))
                    child.ScrollOffset.Y -= this.lastScrollOffset;
                child.ScrollOffset.Y += (this.lastScrollOffset - this.ScrollBar.CurrentValue);
                currentChildren.Add(child);
            }
            // remove cached scrolled children that aren't our children anymore
            this.scrolledChildren.IntersectWith(currentChildren);

            this.lastScrollOffset = this.ScrollBar.CurrentValue;
            this.relevantChildrenDirty = true;
        }

        private void ResetScrollBarMaxHistory() {
            if (this.scrollOverflow) {
                for (var i = 0; i < this.scrollBarMaxHistory.Length; i++)
                    this.scrollBarMaxHistory[i] = -1;
            }
        }

    }
}
