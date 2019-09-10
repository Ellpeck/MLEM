using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public abstract class Element {

        protected readonly List<Element> Children = new List<Element>();
        private readonly List<Element> sortedChildren = new List<Element>();
        protected List<Element> SortedChildren {
            get {
                this.UpdateSortedChildrenIfDirty();
                return this.sortedChildren;
            }
        }
        private Anchor anchor;
        private Vector2 size;
        private Vector2 offset;
        public Point Padding;
        public Point ScaledPadding => this.Padding.Multiply(this.Scale);
        private Point childPadding;
        public Anchor Anchor {
            get => this.anchor;
            set {
                if (this.anchor == value)
                    return;
                this.anchor = value;
                this.SetAreaDirty();
            }
        }
        public Vector2 Size {
            get => this.size;
            set {
                if (this.size == value)
                    return;
                this.size = value;
                this.SetAreaDirty();
            }
        }
        public Vector2 ScaledSize => this.size * this.Scale;
        public Vector2 PositionOffset {
            get => this.offset;
            set {
                if (this.offset == value)
                    return;
                this.offset = value;
                this.SetAreaDirty();
            }
        }
        public Point ScaledOffset => (this.offset * this.Scale).ToPoint();
        public Point ChildPadding {
            get => this.childPadding;
            set {
                if (this.childPadding == value)
                    return;
                this.childPadding = value;
                this.SetAreaDirty();
            }
        }
        public Rectangle ChildPaddedArea => this.UnscrolledArea.Shrink(this.ScaledChildPadding);
        public Point ScaledChildPadding => this.childPadding.Multiply(this.Scale);

        public GenericCallback OnPressed;
        public GenericCallback OnSecondaryPressed;
        public GenericCallback OnSelected;
        public GenericCallback OnDeselected;
        public GenericCallback OnMouseEnter;
        public GenericCallback OnMouseExit;
        public TextInputCallback OnTextInput;
        public GenericCallback OnAreaUpdated;
        public OtherElementCallback OnMousedElementChanged;
        public OtherElementCallback OnSelectedElementChanged;

        private UiSystem system;
        public UiSystem System {
            get => this.system;
            internal set {
                this.system = value;
                if (this.system != null && !this.HasCustomStyle)
                    this.InitStyle(this.system.Style);
            }
        }
        protected UiControls Controls => this.System.Controls;
        protected InputHandler Input => this.Controls.Input;
        public RootElement Root { get; internal set; }
        public float Scale => this.Root.ActualScale;
        public Element Parent { get; private set; }
        public bool IsMouseOver { get; private set; }
        public bool IsSelected { get; private set; }
        private bool isHidden;
        public bool IsHidden {
            get => this.isHidden;
            set {
                if (this.isHidden == value)
                    return;
                this.isHidden = value;
                this.SetAreaDirty();
            }
        }
        public bool CanBeSelected = true;
        public bool CanBeMoused = true;
        public float DrawAlpha = 1;
        public bool HasCustomStyle;
        public bool SetHeightBasedOnChildren;
        public bool CanAutoAnchorsAttach = true;

        private Rectangle area;
        public Rectangle UnscrolledArea {
            get {
                this.UpdateAreaIfDirty();
                return this.area;
            }
        }
        public Rectangle Area => this.UnscrolledArea.OffsetCopy(this.ScaledScrollOffset);
        public Point ScrollOffset;
        public Point ScaledScrollOffset => this.ScrollOffset.Multiply(this.Scale);
        public Rectangle DisplayArea => this.Area.Shrink(this.ScaledPadding);
        private int priority;
        public int Priority {
            get => this.priority;
            set {
                this.priority = value;
                if (this.Parent != null)
                    this.Parent.SetSortedChildrenDirty();
            }
        }
        private bool areaDirty;
        private bool sortedChildrenDirty;
        public NinePatch SelectionIndicator;

        public Element(Anchor anchor, Vector2 size) {
            this.anchor = anchor;
            this.size = size;

            this.OnMouseEnter += element => this.IsMouseOver = true;
            this.OnMouseExit += element => this.IsMouseOver = false;
            this.OnSelected += element => this.IsSelected = true;
            this.OnDeselected += element => this.IsSelected = false;

            this.SetAreaDirty();
        }

        public T AddChild<T>(T element, int index = -1) where T : Element {
            if (index < 0 || index > this.Children.Count)
                index = this.Children.Count;
            this.Children.Insert(index, element);
            element.Parent = this;
            element.AndChildren(e => {
                e.Root = this.Root;
                e.System = this.System;
            });
            this.SetSortedChildrenDirty();
            this.SetAreaDirty();
            return element;
        }

        public void RemoveChild(Element element) {
            this.Children.Remove(element);
            element.Parent = null;
            element.AndChildren(e => {
                e.Root = null;
                e.System = null;
            });
            this.SetSortedChildrenDirty();
            this.SetAreaDirty();
        }

        public void SetAreaDirty() {
            this.areaDirty = true;
            if (this.Anchor >= Anchor.AutoLeft && this.Parent != null)
                this.Parent.SetAreaDirty();
        }

        public void SetSortedChildrenDirty() {
            this.sortedChildrenDirty = true;
        }

        public void UpdateSortedChildrenIfDirty() {
            if (this.sortedChildrenDirty) {
                this.sortedChildrenDirty = false;

                this.sortedChildren.Clear();
                this.sortedChildren.AddRange(this.Children);
                this.sortedChildren.Sort((e1, e2) => e1.Priority.CompareTo(e2.Priority));
            }
        }

        public void UpdateAreaIfDirty() {
            if (this.areaDirty)
                this.ForceUpdateArea();
        }

        public virtual void ForceUpdateArea() {
            this.areaDirty = false;
            if (this.IsHidden)
                return;

            var parentArea = this.Parent != null ? this.Parent.ChildPaddedArea : this.system.Viewport;
            var parentCenterX = parentArea.X + parentArea.Width / 2;
            var parentCenterY = parentArea.Y + parentArea.Height / 2;

            var actualSize = this.CalcActualSize(parentArea);
            var pos = new Point();

            switch (this.anchor) {
                case Anchor.TopLeft:
                case Anchor.AutoLeft:
                case Anchor.AutoInline:
                case Anchor.AutoInlineIgnoreOverflow:
                    pos.X = parentArea.X + this.ScaledOffset.X;
                    pos.Y = parentArea.Y + this.ScaledOffset.Y;
                    break;
                case Anchor.TopCenter:
                case Anchor.AutoCenter:
                    pos.X = parentCenterX - actualSize.X / 2 + this.ScaledOffset.X;
                    pos.Y = parentArea.Y + this.ScaledOffset.Y;
                    break;
                case Anchor.TopRight:
                case Anchor.AutoRight:
                    pos.X = parentArea.Right - actualSize.X - this.ScaledOffset.X;
                    pos.Y = parentArea.Y + this.ScaledOffset.Y;
                    break;
                case Anchor.CenterLeft:
                    pos.X = parentArea.X + this.ScaledOffset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.ScaledOffset.Y;
                    break;
                case Anchor.Center:
                    pos.X = parentCenterX - actualSize.X / 2 + this.ScaledOffset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.ScaledOffset.Y;
                    break;
                case Anchor.CenterRight:
                    pos.X = parentArea.Right - actualSize.X - this.ScaledOffset.X;
                    pos.Y = parentCenterY - actualSize.Y / 2 + this.ScaledOffset.Y;
                    break;
                case Anchor.BottomLeft:
                    pos.X = parentArea.X + this.ScaledOffset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.ScaledOffset.Y;
                    break;
                case Anchor.BottomCenter:
                    pos.X = parentCenterX - actualSize.X / 2 + this.ScaledOffset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.ScaledOffset.Y;
                    break;
                case Anchor.BottomRight:
                    pos.X = parentArea.Right - actualSize.X - this.ScaledOffset.X;
                    pos.Y = parentArea.Bottom - actualSize.Y - this.ScaledOffset.Y;
                    break;
            }

            if (this.Anchor >= Anchor.AutoLeft) {
                Element previousChild;
                if (this.Anchor == Anchor.AutoInline || this.Anchor == Anchor.AutoInlineIgnoreOverflow) {
                    previousChild = this.GetOlderSibling(false, false);
                } else {
                    previousChild = this.GetLowestOlderSibling(false, false);
                }
                if (previousChild != null) {
                    var prevArea = previousChild.GetAreaForAutoAnchors();
                    switch (this.Anchor) {
                        case Anchor.AutoLeft:
                        case Anchor.AutoCenter:
                        case Anchor.AutoRight:
                            pos.Y = prevArea.Bottom + this.ScaledOffset.Y;
                            break;
                        case Anchor.AutoInline:
                            var newX = prevArea.Right + this.ScaledOffset.X;
                            if (newX + actualSize.X <= parentArea.Right) {
                                pos.X = newX;
                                pos.Y = prevArea.Y;
                            } else {
                                pos.Y = prevArea.Bottom + this.ScaledOffset.Y;
                            }
                            break;
                        case Anchor.AutoInlineIgnoreOverflow:
                            pos.X = prevArea.Right + this.ScaledOffset.X;
                            pos.Y = prevArea.Y;
                            break;
                    }
                }
            }

            this.area = new Rectangle(pos, actualSize);
            this.OnAreaUpdated?.Invoke(this);

            foreach (var child in this.Children)
                child.ForceUpdateArea();

            if (this.SetHeightBasedOnChildren && this.Children.Count > 0) {
                var lowest = this.GetLowestChild(false, true);
                var newHeight = (lowest.UnscrolledArea.Bottom - pos.Y + this.ScaledChildPadding.Y) / this.Scale;
                if (newHeight != this.size.Y) {
                    this.size.Y = newHeight;
                    if (this.Anchor > Anchor.TopRight)
                        this.ForceUpdateArea();
                }
            }
        }

        protected virtual Point CalcActualSize(Rectangle parentArea) {
            return new Point(
                (this.size.X > 1 ? this.ScaledSize.X : parentArea.Width * this.size.X).Ceil(),
                (this.size.Y > 1 ? this.ScaledSize.Y : parentArea.Height * this.size.Y).Ceil());
        }

        protected virtual Rectangle GetAreaForAutoAnchors() {
            return this.UnscrolledArea;
        }

        public Element GetLowestChild(bool hiddenAlso, bool unattachableAlso) {
            Element lowest = null;
            // the lowest child is expected to be towards the back, so search is usually faster if done backwards
            for (var i = this.Children.Count - 1; i >= 0; i--) {
                var child = this.Children[i];
                if (!hiddenAlso && child.IsHidden || !unattachableAlso && !child.CanAutoAnchorsAttach)
                    continue;
                if (child.Anchor > Anchor.TopRight && child.Anchor < Anchor.AutoLeft)
                    continue;
                if (lowest == null || child.UnscrolledArea.Bottom > lowest.UnscrolledArea.Bottom)
                    lowest = child;
            }
            return lowest;
        }

        public Element GetLowestOlderSibling(bool hiddenAlso, bool unattachableAlso) {
            if (this.Parent == null)
                return null;
            Element lowest = null;
            foreach (var child in this.Parent.Children) {
                if (child == this)
                    break;
                if (!hiddenAlso && child.IsHidden || !unattachableAlso && !child.CanAutoAnchorsAttach)
                    continue;
                if (lowest == null || child.UnscrolledArea.Bottom >= lowest.UnscrolledArea.Bottom)
                    lowest = child;
            }
            return lowest;
        }

        public Element GetOlderSibling(bool hiddenAlso, bool unattachableAlso) {
            if (this.Parent == null)
                return null;
            Element older = null;
            foreach (var child in this.Parent.Children) {
                if (child == this)
                    break;
                if (!hiddenAlso && child.IsHidden || !unattachableAlso && !child.CanAutoAnchorsAttach)
                    continue;
                older = child;
            }
            return older;
        }

        public IEnumerable<Element> GetSiblings(bool hiddenAlso) {
            if (this.Parent == null)
                yield break;
            foreach (var child in this.Parent.Children) {
                if (!hiddenAlso && child.IsHidden)
                    continue;
                if (child != this)
                    yield return child;
            }
        }

        public IEnumerable<Element> GetChildren(Func<Element, bool> condition = null, bool regardChildrensChildren = false) {
            foreach (var child in this.Children) {
                if (regardChildrensChildren) {
                    foreach (var cc in child.GetChildren(condition, true))
                        yield return cc;
                }
                if (condition == null || condition(child))
                    yield return child;
            }
        }

        public IEnumerable<T> GetChildren<T>(Func<T, bool> condition = null, bool regardChildrensChildren = false) where T : Element {
            foreach (var child in this.Children) {
                if (regardChildrensChildren) {
                    foreach (var cc in child.GetChildren(condition, true))
                        yield return cc;
                }
                if (child is T t && (condition == null || condition(t)))
                    yield return t;
            }
        }

        public IEnumerable<Element> GetParentTree() {
            if (this.Parent == null)
                yield break;
            yield return this.Parent;
            foreach (var parent in this.Parent.GetParentTree())
                yield return parent;
        }

        public virtual void Update(GameTime time) {
            foreach (var child in this.SortedChildren)
                child.Update(time);
        }

        public virtual void Draw(GameTime time, SpriteBatch batch, float alpha) {
            this.System.OnElementDrawn?.Invoke(this, time, batch, alpha);

            foreach (var child in this.SortedChildren) {
                if (!child.IsHidden)
                    child.Draw(time, batch, alpha * child.DrawAlpha);
            }

            if (this.IsSelected)
                this.System.OnSelectedElementDrawn?.Invoke(this, time, batch, alpha);
        }

        public virtual void DrawEarly(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            foreach (var child in this.SortedChildren) {
                if (!child.IsHidden)
                    child.DrawEarly(time, batch, alpha * child.DrawAlpha, blendState, samplerState, matrix);
            }
        }

        public virtual Element GetElementUnderPos(Point position) {
            if (this.IsHidden)
                return null;
            for (var i = this.SortedChildren.Count - 1; i >= 0; i--) {
                var element = this.SortedChildren[i].GetElementUnderPos(position);
                if (element != null)
                    return element;
            }
            return this.CanBeMoused && this.Area.Contains(position) ? this : null;
        }

        public virtual Element GetTabNextElement(bool backward, Element usualNext) {
            return usualNext;
        }

        public virtual Element GetGamepadNextElement(Direction2 dir, Element usualNext) {
            return usualNext;
        }

        public void AndChildren(Action<Element> action) {
            action(this);
            foreach (var child in this.Children)
                child.AndChildren(action);
        }

        protected virtual void InitStyle(UiStyle style) {
            this.SelectionIndicator = style.SelectionIndicator;
        }

        public delegate void TextInputCallback(Element element, Keys key, char character);

        public delegate void GenericCallback(Element element);

        public delegate void OtherElementCallback(Element thisElement, Element otherElement);

        public delegate void DrawCallback(Element element, GameTime time, SpriteBatch batch, float alpha);

    }
}