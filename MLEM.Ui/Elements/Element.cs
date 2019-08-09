using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Ui.Elements {
    public class Element {

        private Anchor anchor;
        private Point size;
        private Point offset;
        private Point padding;
        public Anchor Anchor {
            get => this.anchor;
            set {
                this.anchor = value;
                this.SetDirty();
            }
        }
        public Point Size {
            get => this.size;
            set {
                this.size = value;
                this.SetDirty();
            }
        }
        public Point PositionOffset {
            get => this.offset;
            set {
                this.offset = value;
                this.SetDirty();
            }
        }
        public Point Padding {
            get => this.padding;
            set {
                this.padding = value;
                this.SetDirty();
            }
        }

        public UiSystem System;
        public Element Parent { get; private set; }
        private readonly List<Element> children = new List<Element>();

        private Rectangle area;
        public Rectangle Area {
            get {
                this.UpdateAreaIfDirty();
                return this.area;
            }
        }
        public Rectangle DisplayArea {
            get {
                var padded = this.Area;
                padded.Location += this.Padding;
                padded.Width -= this.Padding.X * 2;
                padded.Height -= this.Padding.Y * 2;
                return padded;
            }

        }
        protected bool AreaDirty;

        public Element(Anchor anchor, Point size, Point positionOffset) {
            this.anchor = anchor;
            this.size = size;
            this.offset = positionOffset;
        }

        public void AddChild(Element element) {
            this.children.Add(element);
            element.Parent = this;
            this.SetDirty();
        }

        public void RemoveChild(Element element) {
            this.children.Remove(element);
            element.Parent = null;
            this.SetDirty();
        }

        public void SetDirty() {
            this.AreaDirty = true;
        }

        public void UpdateAreaIfDirty() {
            if (this.AreaDirty)
                this.ForceUpdateArea();
        }

        public void ForceUpdateArea() {
            this.AreaDirty = false;

            var parentArea = this.Parent != null ? this.Parent.area : this.System.ScaledViewport;
            var parentCenterX = parentArea.X + parentArea.Width / 2;
            var parentCenterY = parentArea.Y + parentArea.Height / 2;

            var actualSize = this.CalcActualSize();
            var pos = new Point();

            switch (this.anchor) {
                case Anchor.TopLeft:
                case Anchor.AutoLeft:
                case Anchor.AutoInline:
                case Anchor.AutoInlineIgnoreOverflow:
                    pos.X = parentArea.X + this.offset.X;
                    pos.Y = parentArea.Y + this.offset.Y;
                    break;
                case Anchor.TopCenter:
                case Anchor.AutoCenter:
                    pos.X = parentCenterX - actualSize.X / 2 + this.offset.X;
                    pos.Y = parentArea.Y + this.offset.Y;
                    break;
                case Anchor.TopRight:
                case Anchor.AutoRight:
                    pos.X = parentArea.Right - actualSize.X - this.offset.X;
                    pos.Y = parentArea.Y + this.offset.Y;
                    break;
                case Anchor.CenterLeft:
                    pos.X = parentArea.X + this.offset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.offset.Y;
                    break;
                case Anchor.Center:
                    pos.X = parentCenterX - actualSize.X / 2 + this.offset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.offset.Y;
                    break;
                case Anchor.CenterRight:
                    pos.X = parentArea.Right - actualSize.X - this.offset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.offset.Y;
                    break;
                case Anchor.BottomLeft:
                    pos.X = parentArea.X + this.offset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.offset.Y;
                    break;
                case Anchor.BottomCenter:
                    pos.X = parentCenterX - actualSize.X / 2 + this.offset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.offset.Y;
                    break;
                case Anchor.BottomRight:
                    pos.X = parentArea.Right - actualSize.X - this.offset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.offset.Y;
                    break;
            }

            if (this.Anchor >= Anchor.AutoLeft) {
                var previousChild = this.GetPreviousChild();
                if (previousChild != null) {
                    var prevArea = previousChild.Area;
                    switch (this.Anchor) {
                        case Anchor.AutoLeft:
                        case Anchor.AutoCenter:
                        case Anchor.AutoRight:
                            pos.Y = prevArea.Bottom + this.PositionOffset.Y;
                            break;
                        case Anchor.AutoInline:
                            var newX = prevArea.Right + this.PositionOffset.X;
                            if (newX + actualSize.X <= parentArea.Right) {
                                pos.X = newX;
                                pos.Y = prevArea.Y;
                            } else {
                                pos.Y = prevArea.Bottom + this.PositionOffset.Y;
                            }
                            break;
                        case Anchor.AutoInlineIgnoreOverflow:
                            pos.X = prevArea.Right + this.PositionOffset.X;
                            pos.Y = prevArea.Y;
                            break;
                    }
                }
            }

            this.area = new Rectangle(pos, actualSize);

            foreach (var child in this.children)
                child.ForceUpdateArea();
        }

        private Point CalcActualSize() {
            return this.size;
        }

        public Element GetPreviousChild() {
            if (this.Parent == null)
                return null;

            Element lastChild = null;
            foreach (var child in this.Parent.children) {
                if (child == this)
                    break;
                lastChild = child;
            }
            return lastChild;
        }

        public void Update(GameTime time) {
            foreach (var child in this.children)
                child.Update(time);
        }

        public void Draw(GameTime time, SpriteBatch batch) {
            batch.Draw(batch.GetBlankTexture(), this.DisplayArea, this.Parent == null ? Color.Blue : Color.Red);

            foreach (var child in this.children)
                child.Draw(time, batch);
        }

    }
}