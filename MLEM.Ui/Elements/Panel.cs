using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Panel : Element {

        public StyleProp<NinePatch> Texture;
        public readonly ScrollBar ScrollBar;
        private readonly bool scrollOverflow;
        private RenderTarget2D renderTarget;
        private readonly List<Element> relevantChildren = new List<Element>();
        private bool relevantChildrenDirty;

        public Panel(Anchor anchor, Vector2 size, Vector2 positionOffset, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, Point? scrollerSize = null, bool autoHideScrollbar = true) : base(anchor, size) {
            this.PositionOffset = positionOffset;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.scrollOverflow = scrollOverflow;
            this.ChildPadding = new Vector2(5);
            this.CanBeSelected = false;

            if (scrollOverflow) {
                var scrollSize = scrollerSize ?? Point.Zero;
                this.ScrollBar = new ScrollBar(Anchor.TopRight, new Vector2(scrollSize.X, 1), scrollSize.Y, 0) {
                    StepPerScroll = 10,
                    OnValueChanged = (element, value) => this.ScrollChildren(),
                    CanAutoAnchorsAttach = false,
                    AutoHideWhenEmpty = autoHideScrollbar,
                    IsHidden = autoHideScrollbar
                };

                // modify the padding so that the scroll bar isn't over top of something else
                this.ScrollBar.PositionOffset -= new Vector2(scrollSize.X + 1, 0);
                if (autoHideScrollbar)
                    this.ScrollBar.OnAutoHide += e => this.ChildPadding += new Padding(0, scrollSize.X, 0, 0) * (e.IsHidden ? -1 : 1);

                // handle automatic element selection, the scroller needs to scroll to the right location
                this.OnSelectedElementChanged += (element, otherElement) => {
                    if (!this.Controls.IsAutoNavMode)
                        return;
                    if (otherElement == null || !otherElement.GetParentTree().Contains(this))
                        return;
                    this.ScrollBar.CurrentValue = (otherElement.Area.Bottom - this.Children[1].Area.Top - this.Area.Height / 2) / this.Scale;
                };
                this.AddChild(this.ScrollBar);
            }
        }

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
            this.ScrollChildren();

            if (this.scrollOverflow) {
                // if there is only one child, then we have just the scroll bar
                if (this.Children.Count == 1)
                    return;
                // the "real" first child is the scroll bar, which we want to ignore
                var firstChild = this.Children[1];
                var lowestChild = this.GetLowestChild(e => !e.IsHidden);
                // the max value of the scrollbar is the amount of non-scaled pixels taken up by overflowing components
                var childrenHeight = lowestChild.Area.Bottom - firstChild.Area.Top;
                this.ScrollBar.MaxValue = (childrenHeight - this.Area.Height) / this.Scale + this.ChildPadding.Height;

                // update the render target
                var targetArea = (Rectangle) this.GetRenderTargetArea();
                if (this.renderTarget == null || targetArea.Width != this.renderTarget.Width || targetArea.Height != this.renderTarget.Height) {
                    if (this.renderTarget != null)
                        this.renderTarget.Dispose();
                    this.renderTarget = targetArea.IsEmpty ? null : new RenderTarget2D(this.System.GraphicsDevice, targetArea.Width, targetArea.Height);
                }
            }
        }

        private void ScrollChildren() {
            if (!this.scrollOverflow)
                return;
            var offset = -this.ScrollBar.CurrentValue;
            foreach (var child in this.GetChildren(c => c != this.ScrollBar, true))
                child.ScrollOffset = new Vector2(0, offset);
            this.relevantChildrenDirty = true;
        }

        public override void ForceUpdateSortedChildren() {
            base.ForceUpdateSortedChildren();
            if (this.scrollOverflow)
                this.relevantChildrenDirty = true;
        }

        protected override List<Element> GetRelevantChildren() {
            var relevant = base.GetRelevantChildren();
            if (this.scrollOverflow) {
                if (this.relevantChildrenDirty) {
                    this.relevantChildrenDirty = false;

                    var visible = this.GetRenderTargetArea();
                    this.relevantChildren.Clear();
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
                relevant = this.relevantChildren;
            }
            return relevant;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            if (this.Texture.Value != null)
                batch.Draw(this.Texture, this.DisplayArea, Color.White * alpha, this.Scale);
            // if we handle overflow, draw using the render target in DrawUnbound
            if (!this.scrollOverflow) {
                base.Draw(time, batch, alpha, blendState, samplerState, matrix);
            } else if (this.renderTarget != null) {
                // draw the actual render target (don't apply the alpha here because it's already drawn onto with alpha)
                batch.Draw(this.renderTarget, this.GetRenderTargetArea(), Color.White);
            }
        }

        public override void DrawEarly(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            if (this.scrollOverflow && this.renderTarget != null) {
                this.UpdateAreaIfDirty();
                // draw children onto the render target
                batch.GraphicsDevice.SetRenderTarget(this.renderTarget);
                batch.GraphicsDevice.Clear(Color.Transparent);
                // offset children by the render target's location
                var area = this.GetRenderTargetArea();
                var trans = Matrix.CreateTranslation(-area.X, -area.Y, 0);
                // do the usual draw, but within the render target
                batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, trans);
                base.Draw(time, batch, alpha, blendState, samplerState, trans);
                batch.End();
                // also draw any children early within the render target with the translation applied
                base.DrawEarly(time, batch, alpha, blendState, samplerState, trans);
                batch.GraphicsDevice.SetRenderTarget(null);
            } else {
                base.DrawEarly(time, batch, alpha, blendState, samplerState, matrix);
            }
        }

        public override Element GetElementUnderPos(Vector2 position) {
            // if overflow is handled, don't propagate mouse checks to hidden children
            if (this.scrollOverflow && !this.GetRenderTargetArea().Contains(position))
                return null;
            return base.GetElementUnderPos(position);
        }

        private RectangleF GetRenderTargetArea() {
            var area = this.ChildPaddedArea;
            area.X = this.DisplayArea.X;
            area.Width = this.DisplayArea.Width;
            return area;
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.PanelTexture);
        }

    }
}