using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Input;

namespace MLEM.Ui.Elements {
    public abstract class Element {

        private readonly List<Element> children = new List<Element>();
        private Anchor anchor;
        private Vector2 size;
        private Point offset;
        private Point padding;
        private Point childPadding;
        public Anchor Anchor {
            get => this.anchor;
            set {
                this.anchor = value;
                this.SetDirty();
            }
        }
        public Vector2 Size {
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
        public Point ChildPadding {
            get => this.childPadding;
            set {
                this.childPadding = value;
                this.SetDirty();
            }
        }

        public MouseClickCallback OnClicked;
        public MouseClickCallback OnMouseDown;
        public MouseCallback OnMouseEnter;
        public MouseCallback OnMouseExit;

        public UiSystem System { get; private set; }
        public Element Parent { get; private set; }
        public bool IsMouseOver { get; private set; }
        private bool isHidden;
        public bool IsHidden {
            get => this.isHidden;
            set {
                this.isHidden = value;
                this.SetDirty();
            }
        }
        public bool IgnoresMouse;
        public float DrawAlpha = 1;

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
        private bool areaDirty;

        public Element(Anchor anchor, Vector2 size) {
            this.anchor = anchor;
            this.size = size;

            this.OnMouseEnter += (element, mousePos) => this.IsMouseOver = true;
            this.OnMouseExit += (element, mousePos) => this.IsMouseOver = false;

            this.SetDirty();
        }

        public Element AddChild(Element element, int index = -1) {
            if (index < 0 || index > this.children.Count)
                index = this.children.Count;
            this.children.Insert(index, element);
            element.Parent = this;
            element.System = this.System;
            this.SetDirty();
            return element;
        }

        public void RemoveChild(Element element) {
            this.children.Remove(element);
            element.Parent = null;
            element.System = null;
            this.SetDirty();
        }

        public void MoveToFront() {
            if (this.Parent != null) {
                this.Parent.RemoveChild(this);
                this.Parent.AddChild(this);
            }
        }

        public void MoveToBack() {
            if (this.Parent != null) {
                this.Parent.RemoveChild(this);
                this.Parent.AddChild(this, 0);
            }
        }

        public void SetDirty() {
            this.areaDirty = true;
            if (this.Parent != null)
                this.Parent.SetDirty();
        }

        public void UpdateAreaIfDirty() {
            if (this.areaDirty)
                this.ForceUpdateArea();
        }

        public virtual void ForceUpdateArea() {
            this.areaDirty = false;

            Rectangle parentArea;
            if (this.Parent != null) {
                parentArea = this.Parent.area;
                parentArea.Location += this.Parent.ChildPadding;
                parentArea.Width -= this.Parent.ChildPadding.X * 2;
                parentArea.Height -= this.Parent.ChildPadding.Y * 2;
            } else {
                parentArea = this.System.ScaledViewport;
            }
            var parentCenterX = parentArea.X + parentArea.Width / 2;
            var parentCenterY = parentArea.Y + parentArea.Height / 2;

            var actualSize = this.CalcActualSize(parentArea);
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
                var previousChild = this.GetPreviousChild(false);
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

        protected virtual Point CalcActualSize(Rectangle parentArea) {
            return new Point(
                (this.size.X > 1 ? this.size.X : parentArea.Width * this.size.X).Floor(),
                (this.size.Y > 1 ? this.size.Y : parentArea.Height * this.size.Y).Floor());
        }

        protected Element GetPreviousChild(bool hiddenAlso) {
            if (this.Parent == null)
                return null;

            Element lastChild = null;
            foreach (var child in this.Parent.children) {
                if (!hiddenAlso && child.IsHidden)
                    continue;
                if (child == this)
                    break;
                lastChild = child;
            }
            return lastChild;
        }

        public virtual void Update(GameTime time) {
            foreach (var child in this.children)
                child.Update(time);
        }

        public virtual void Draw(GameTime time, SpriteBatch batch, float alpha) {
            foreach (var child in this.children) {
                if (!child.IsHidden)
                    child.Draw(time, batch, alpha * child.DrawAlpha);
            }
        }

        public virtual void DrawUnbound(GameTime time, SpriteBatch batch, float alpha, BlendState blendState = null, SamplerState samplerState = null) {
            foreach (var child in this.children) {
                if (!child.IsHidden)
                    child.DrawUnbound(time, batch, alpha * child.DrawAlpha, blendState, samplerState);
            }
        }

        public void SetUiSystem(UiSystem system) {
            this.System = system;
            foreach (var child in this.children)
                child.SetUiSystem(system);
        }

        public Element GetMousedElement(Vector2 mousePos) {
            if (this.IsHidden || this.IgnoresMouse)
                return null;
            if (!this.Area.Contains(mousePos))
                return null;
            for (var i = this.children.Count - 1; i >= 0; i--) {
                var element = this.children[i].GetMousedElement(mousePos);
                if (element != null)
                    return element;
            }
            return this;
        }

        public delegate void MouseClickCallback(Element element, Vector2 mousePos, MouseButton button);

        public delegate void MouseCallback(Element element, Vector2 mousePos);

    }
}