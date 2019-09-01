using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Panel : Element {

        public NinePatch Texture;
        public readonly ScrollBar ScrollBar;
        private readonly bool scrollOverflow;
        private RenderTarget2D renderTarget;

        public Panel(Anchor anchor, Vector2 size, Vector2 positionOffset, bool setHeightBasedOnChildren = false, bool scrollOverflow = false, Point? scrollerSize = null) : base(anchor, size) {
            this.PositionOffset = positionOffset;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.scrollOverflow = scrollOverflow;
            this.ChildPadding = new Point(5);
            this.CanBeSelected = false;

            if (scrollOverflow) {
                var scrollSize = scrollerSize ?? Point.Zero;
                this.ScrollBar = new ScrollBar(Anchor.TopRight, new Vector2(scrollSize.X, 1), scrollSize.Y, 0) {
                    StepPerScroll = 10,
                    OnValueChanged = (element, value) => this.ForceChildrenScroll(),
                    CanAutoAnchorsAttach = false,
                    AutoHideWhenEmpty = true
                };
                this.AddChild(this.ScrollBar);

                // modify the padding so that the scroll bar isn't over top of something else
                this.ScrollBar.PositionOffset -= new Vector2(scrollSize.X + 1, 0);
                this.ChildPadding += new Point(scrollSize.X, 0);

                // handle automatic element selection, the scroller needs to scroll to the right location
                this.OnSelectedElementChanged += (element, otherElement) => {
                    if (this.Controls.SelectedLastElementWithMouse)
                        return;
                    if (otherElement == null || !otherElement.GetParentTree().Contains(this))
                        return;
                    this.ScrollBar.CurrentValue = (otherElement.Area.Bottom - this.Children[1].Area.Top - this.Area.Height / 2) / this.Scale;
                };
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
            this.ForceChildrenScroll();

            if (this.scrollOverflow) {
                // if there is only one child, then we have just the scroll bar
                if (this.Children.Count == 1)
                    return;
                // the "real" first child is the scroll bar, which we want to ignore
                var firstChild = this.Children[1];
                var lowestChild = this.GetLowestChild(false, true);
                // the max value of the scrollbar is the amount of non-scaled pixels taken up by overflowing components
                var childrenHeight = lowestChild.Area.Bottom - firstChild.Area.Top;
                this.ScrollBar.MaxValue = ((childrenHeight - this.Area.Height) / this.Scale + this.ChildPadding.Y * 2).Floor();

                // update the render target
                var targetArea = this.GetRenderTargetArea();
                if (this.renderTarget == null || targetArea.Width != this.renderTarget.Width || targetArea.Height != this.renderTarget.Height) {
                    if (this.renderTarget != null)
                        this.renderTarget.Dispose();
                    this.renderTarget = new RenderTarget2D(this.System.GraphicsDevice, targetArea.Width, targetArea.Height);
                }
            }
        }

        private void ForceChildrenScroll() {
            if (!this.scrollOverflow)
                return;
            var offset = -this.ScrollBar.CurrentValue.Floor();
            foreach (var child in this.GetChildren(c => c != this.ScrollBar, true))
                child.ScrollOffset = new Point(0, offset);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            batch.Draw(this.Texture, this.DisplayArea.OffsetCopy(offset), Color.White * alpha, this.Scale);
            // if we handle overflow, draw using the render target in DrawUnbound
            if (!this.scrollOverflow) {
                base.Draw(time, batch, alpha, offset);
            } else if (this.renderTarget != null) {
                // draw the actual render target (don't apply the alpha here because it's already drawn onto with alpha)
                batch.Draw(this.renderTarget, this.GetRenderTargetArea().OffsetCopy(offset), Color.White);
            }
        }

        public override void DrawEarly(GameTime time, SpriteBatch batch, float alpha, BlendState blendState = null, SamplerState samplerState = null) {
            if (this.scrollOverflow && this.renderTarget != null) {
                // draw children onto the render target
                batch.GraphicsDevice.SetRenderTarget(this.renderTarget);
                batch.GraphicsDevice.Clear(Color.Transparent);
                batch.Begin(SpriteSortMode.Deferred, blendState, samplerState);
                // offset children by the render target's location
                var area = this.GetRenderTargetArea();
                base.Draw(time, batch, alpha, new Point(-area.X, -area.Y));
                batch.End();
                batch.GraphicsDevice.SetRenderTarget(null);
            }
            base.DrawEarly(time, batch, alpha, blendState, samplerState);
        }

        public override Element GetElementUnderPos(Point position) {
            // if overflow is handled, don't propagate mouse checks to hidden children
            if (this.scrollOverflow && !this.GetRenderTargetArea().Contains(position))
                return null;
            return base.GetElementUnderPos(position);
        }

        private Rectangle GetRenderTargetArea() {
            var area = this.ChildPaddedArea;
            area.X = this.DisplayArea.X;
            area.Width = this.DisplayArea.Width;
            return area;
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = style.PanelTexture;
        }

    }
}