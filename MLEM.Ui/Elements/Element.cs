using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;
using SoundEffectInfo = MLEM.Sound.SoundEffectInfo;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// This class represents a generic base class for ui elements of a <see cref="UiSystem"/>.
    /// </summary>
    public abstract class Element : GenericDataHolder, IDisposable {

        /// <summary>
        /// This field holds an epsilon value used in element <see cref="Size"/>, position and resulting <see cref="Area"/> calculations to mitigate floating point rounding inaccuracies.
        /// If ui elements used are extremely small or extremely large, this value can be reduced or increased.
        /// </summary>
        public static float Epsilon = 0.01F;

        /// <summary>
        /// The ui system that this element is currently a part of
        /// </summary>
        public UiSystem System {
            get => this.system;
            internal set {
                this.system = value;
                this.Controls = value?.Controls;
                this.Style = value?.Style;
            }
        }
        public UiStyle Style {
            get => this.style;
            set {
                if (this.style != value) {
                    this.style = value;
                    if (value != null)
                        this.InitStyle(value);
                }
            }
        }
        /// <summary>
        /// The controls that this element's <see cref="System"/> uses
        /// </summary>
        public UiControls Controls;
        /// <summary>
        /// This element's parent element.
        /// If this element has no parent (it is the <see cref="RootElement"/> of a ui system), this value is <c>null</c>.
        /// </summary>
        public Element Parent { get; private set; }
        /// <summary>
        /// This element's <see cref="RootElement"/>.
        /// Note that this value is set even if this element has a <see cref="Parent"/>. To get the element that represents the root element, use <see cref="RootElement.Element"/>.
        /// </summary>
        public RootElement Root { get; internal set; }
        /// <summary>
        /// The scale that this ui element renders with
        /// </summary>
        public float Scale => this.Root.ActualScale;
        /// <summary>
        /// The <see cref="Anchor"/> that this element uses for positioning within its parent
        /// </summary>
        public Anchor Anchor {
            get => this.anchor;
            set {
                if (this.anchor == value)
                    return;
                this.anchor = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// The size of this element, where X represents the width and Y represents the height.
        /// If the x or y value of the size is between 0 and 1, the size will be seen as a percentage of its parent's size rather than as an absolute value.
        /// If the x (or y) value of the size is negative, the width (or height) is seen as a percentage of the element's resulting height (or width). 
        /// </summary>
        /// <example>
        /// The following example combines both types of percentage-based sizing.
        /// If this element is inside of a <see cref="Parent"/> whose width is 20, this element's width will be set to <c>0.5 * 20 = 10</c>, and its height will be set to <c>2.5 * 10 = 25</c>.
        /// <code>
        /// element.Size = new Vector2(0.5F, -2.5F);
        /// </code>
        /// </example>
        public Vector2 Size {
            get => this.size;
            set {
                if (this.size == value)
                    return;
                this.size = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// The <see cref="Size"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Vector2 ScaledSize => this.size * this.Scale;
        /// <summary>
        /// This element's offset from its default position, which is dictated by its <see cref="Anchor"/>.
        /// Note that, depending on the side that the element is anchored to, this offset moves it in a different direction.
        /// </summary>
        public Vector2 PositionOffset {
            get => this.offset;
            set {
                if (this.offset == value)
                    return;
                this.offset = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// The <see cref="PositionOffset"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Vector2 ScaledOffset => this.offset * this.Scale;
        /// <summary>
        /// The <see cref="Padding"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Padding ScaledPadding => this.Padding.Value * this.Scale;
        /// <summary>
        /// The <see cref="ChildPadding"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Padding ScaledChildPadding => this.ChildPadding.Value * this.Scale;
        /// <summary>
        /// This element's current <see cref="Area"/>, but with <see cref="ScaledChildPadding"/> applied.
        /// </summary>
        public RectangleF ChildPaddedArea => this.UnscrolledArea.Shrink(this.ScaledChildPadding);
        /// <summary>
        /// This element's area, without respecting its <see cref="ScrollOffset"/>.
        /// This area is updated automatically to fit this element's sizing and positioning properties.
        /// </summary>
        public RectangleF UnscrolledArea {
            get {
                this.UpdateAreaIfDirty();
                return this.area;
            }
        }
        /// <summary>
        /// The <see cref="UnscrolledArea"/> of this element, but with <see cref="ScaledScrollOffset"/> applied.
        /// </summary>
        public RectangleF Area => this.UnscrolledArea.OffsetCopy(this.ScaledScrollOffset);
        /// <summary>
        /// The area that this element is displayed in, which is <see cref="Area"/> shrunk by this element's <see cref="ScaledPadding"/>.
        /// This is the property that should be used for drawing this element, as well as mouse input handling and culling.
        /// </summary>
        public RectangleF DisplayArea => this.Area.Shrink(this.ScaledPadding);
        /// <summary>
        /// The offset that this element has as a result of <see cref="Panel"/> scrolling.
        /// </summary>
        public Vector2 ScrollOffset;
        /// <summary>
        /// The <see cref="ScrollOffset"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Vector2 ScaledScrollOffset => this.ScrollOffset * this.Scale;
        /// <summary>
        /// Set this property to <c>true</c> to cause this element to be hidden.
        /// Hidden elements don't receive input events, aren't rendered and don't factor into auto-anchoring.
        /// </summary>
        public bool IsHidden {
            get => this.isHidden;
            set {
                if (this.isHidden == value)
                    return;
                this.isHidden = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// The priority of this element as part of its <see cref="Parent"/> element.
        /// A higher priority means the element will be drawn first and, if auto-anchoring is used, anchored higher up within its parent.
        /// </summary>
        public int Priority {
            get => this.priority;
            set {
                if (this.priority == value)
                    return;
                this.priority = value;
                if (this.Parent != null)
                    this.Parent.SetSortedChildrenDirty();
            }
        }
        /// <summary>
        /// This element's transform matrix.
        /// Can easily be scaled using <see cref="ScaleTransform"/>.
        /// Note that, when this is non-null, a new <see cref="SpriteBatch.Begin"/> call is used for this element.
        /// </summary>
        public Matrix Transform = Matrix.Identity;
        /// <summary>
        /// The call that this element should make to <see cref="SpriteBatch"/> to begin drawing.
        /// Note that, when this is non-null, a new <see cref="SpriteBatch.Begin"/> call is used for this element.
        /// </summary>
        public BeginDelegate BeginImpl;
        /// <summary>
        /// Set this field to false to disallow the element from being selected.
        /// An unselectable element is skipped by automatic navigation and its <see cref="OnSelected"/> callback will never be called.
        /// </summary>
        public bool CanBeSelected = true;
        /// <summary>
        /// Set this field to false to disallow the element from reacting to being moused over.
        /// </summary>
        public bool CanBeMoused = true;
        /// <summary>
        /// Set this field to false to disallow this element's <see cref="OnPressed"/> and <see cref="OnSecondaryPressed"/> events to be called.
        /// </summary>
        public bool CanBePressed = true;
        /// <summary>
        /// Set this field to false to cause auto-anchored siblings to ignore this element as a possible anchor point.
        /// </summary>
        public bool CanAutoAnchorsAttach = true;
        /// <summary>
        /// Set this field to true to cause this element's width to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this element's <see cref="Size"/>'s X coordinate as a minimum or maximum width rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        public bool SetWidthBasedOnChildren;
        /// <summary>
        /// Set this field to true to cause this element's height to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this element's <see cref="Size"/>'s Y coordinate as a minimum or maximum height rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        public bool SetHeightBasedOnChildren;
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled, the resulting width or height will always be greather than or equal to this element's <see cref="Size"/>.
        /// For example, if an element's <see cref="Size"/>'s Y coordinate is set to 20, but there is only one child with a height of 10 in it, the element's height would be shrunk to 10 if this value was false, but would remain at 20 if it was true.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        public bool TreatSizeAsMinimum;
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/>are enabled, the resulting width or height weill always be less than or equal to this element's <see cref="Size"/>.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        public bool TreatSizeAsMaximum;
        /// <summary>
        /// Set this field to true to cause this element's final display area to never exceed that of its <see cref="Parent"/>.
        /// If the resulting area is too large, the size of this element is shrunk to fit the target area.
        /// This can be useful if an element should fill the remaining area of a parent exactly.
        /// </summary>
        public bool PreventParentSpill;
        /// <summary>
        /// The transparency (alpha value) that this element is rendered with.
        /// Note that, when <see cref="Draw"/> is called, this alpha value is multiplied with the <see cref="Parent"/>'s alpha value and passed down to this element's <see cref="Children"/>.
        /// </summary>
        public float DrawAlpha = 1;
        /// <summary>
        /// Stores whether this element is currently being moused over or touched.
        /// </summary>
        public bool IsMouseOver { get; protected set; }
        /// <summary>
        /// Stores whether this element is its <see cref="Root"/>'s <see cref="RootElement.SelectedElement"/>.
        /// </summary>
        public bool IsSelected { get; protected set; }

        /// <summary>
        /// A style property that contains the selection indicator that is displayed on this element if it is the <see cref="RootElement.SelectedElement"/>
        /// </summary>
        public StyleProp<NinePatch> SelectionIndicator;
        /// <summary>
        /// A style property that contains the sound effect that is played when this element's <see cref="OnPressed"/> is called
        /// </summary>
        public StyleProp<SoundEffectInfo> ActionSound;
        /// <summary>
        /// A style property that contains the sound effect that is played when this element's <see cref="OnSecondaryPressed"/> is called
        /// </summary>
        public StyleProp<SoundEffectInfo> SecondActionSound;
        /// <summary>
        /// The padding that this element has.
        /// The padding is subtracted from the element's <see cref="Size"/>, and it is an area that the element does not extend into. This means that this element's resulting <see cref="DisplayArea"/> does not include this padding.
        /// </summary>
        public StyleProp<Padding> Padding;
        /// <summary>
        /// The child padding that this element has.
        /// The child padding moves any <see cref="Children"/> added to this element inwards by the given amount in each direction.
        /// When setting this style after this element has already been added to a ui, <see cref="SetAreaDirty"/> should be called.
        /// </summary>
        public StyleProp<Padding> ChildPadding;

        /// <summary>
        /// Event that is called after this element is drawn, but before its children are drawn
        /// </summary>
        public DrawCallback OnDrawn;
        /// <summary>
        /// Event that is called when this element is updated
        /// </summary>
        public TimeCallback OnUpdated;
        /// <summary>
        /// Event that is called when this element is pressed
        /// </summary>
        public GenericCallback OnPressed;
        /// <summary>
        /// Event that is called when this element is pressed using the secondary action
        /// </summary>
        public GenericCallback OnSecondaryPressed;
        /// <summary>
        /// Event that is called when this element's <see cref="IsSelected"/> is turned true
        /// </summary>
        public GenericCallback OnSelected;
        /// <summary>
        /// Event that is called when this element's <see cref="IsSelected"/> is turned false
        /// </summary>
        public GenericCallback OnDeselected;
        /// <summary>
        /// Event that is called when this element starts being moused over
        /// </summary>
        public GenericCallback OnMouseEnter;
        /// <summary>
        /// Event that is called when this element stops being moused over
        /// </summary>
        public GenericCallback OnMouseExit;
        /// <summary>
        /// Event that is called when this element starts being touched
        /// </summary>
        public GenericCallback OnTouchEnter;
        /// <summary>
        /// Event that is called when this element stops being touched
        /// </summary>
        public GenericCallback OnTouchExit;
        /// <summary>
        /// Event that is called when text input is made.
        /// When an element uses this event, it should call <see cref="MlemPlatform.EnsureExists"/> on construction to ensure that the MLEM platform is initialized.
        /// Note that this event is called for every element, even if it is not selected.
        /// Also note that if the active <see cref="MlemPlatform"/> uses an on-screen keyboard, this event is never called.
        /// </summary>
        public TextInputCallback OnTextInput;
        /// <summary>
        /// Event that is called when this element's <see cref="DisplayArea"/> is changed.
        /// </summary>
        public GenericCallback OnAreaUpdated;
        /// <summary>
        /// Event that is called when the element that is currently being moused changes within the ui system.
        /// Note that the event fired doesn't necessarily correlate to this specific element.
        /// </summary>
        public OtherElementCallback OnMousedElementChanged;
        /// <summary>
        /// Event that is called when the element that is currently being touched changes within the ui system.
        /// Note that the event fired doesn't necessarily correlate to this specific element.
        /// </summary>
        public OtherElementCallback OnTouchedElementChanged;
        /// <summary>
        /// Event that is called when the element that is currently selected changes within the ui system.
        /// Note that the event fired doesn't necessarily correlate to this specific element.
        /// </summary>
        public OtherElementCallback OnSelectedElementChanged;
        /// <summary>
        /// Event that is called when the next element to select when pressing tab is calculated.
        /// To cause a different element than the default one to be selected, return it during this event.
        /// </summary>
        public TabNextElementCallback GetTabNextElement;
        /// <summary>
        /// Event that is called when the next element to select when using gamepad input is calculated.
        /// To cause a different element than the default one to be selected, return it during this event.
        /// </summary>
        public GamepadNextElementCallback GetGamepadNextElement;
        /// <summary>
        /// Event that is called when a child is added to this element using <see cref="AddChild{T}"/>
        /// </summary>
        public OtherElementCallback OnChildAdded;
        /// <summary>
        /// Event that is called when a child is removed from this element using <see cref="RemoveChild"/>
        /// </summary>
        public OtherElementCallback OnChildRemoved;
        /// <summary>
        /// Event that is called when this element's <see cref="Dispose"/> method is called, which also happens in <see cref="Finalize"/>.
        /// This event is useful for unregistering global event handlers when this object should be destroyed.
        /// </summary>
        public GenericCallback OnDisposed;

        /// <summary>
        /// A list of all of this element's direct children.
        /// Use <see cref="AddChild{T}"/> or <see cref="RemoveChild"/> to manipulate this list while calling all of the necessary callbacks.
        /// </summary>
        protected readonly IList<Element> Children;
        /// <summary>
        /// A sorted version of <see cref="Children"/>. The children are sorted by their <see cref="Priority"/>.
        /// </summary>
        protected IList<Element> SortedChildren {
            get {
                this.UpdateSortedChildrenIfDirty();
                return this.sortedChildren;
            }
        }
        /// <summary>
        /// The input handler that this element's <see cref="Controls"/> use
        /// </summary>
        protected InputHandler Input => this.Controls.Input;

        private readonly List<Element> children = new List<Element>();
        private bool sortedChildrenDirty;
        private IList<Element> sortedChildren;
        private UiSystem system;
        private Anchor anchor;
        private Vector2 size;
        private Vector2 offset;
        private RectangleF area;
        private bool areaDirty;
        private bool isHidden;
        private int priority;
        private UiStyle style;

        /// <summary>
        /// Creates a new element with the given anchor and size and sets up some default event reactions.
        /// </summary>
        /// <param name="anchor">This element's <see cref="Anchor"/></param>
        /// <param name="size">This element's default <see cref="Size"/></param>
        protected Element(Anchor anchor, Vector2 size) {
            this.anchor = anchor;
            this.size = size;

            this.Children = new ReadOnlyCollection<Element>(this.children);

            this.OnMouseEnter += element => this.IsMouseOver = true;
            this.OnMouseExit += element => this.IsMouseOver = false;
            this.OnTouchEnter += element => this.IsMouseOver = true;
            this.OnTouchExit += element => this.IsMouseOver = false;
            this.OnSelected += element => this.IsSelected = true;
            this.OnDeselected += element => this.IsSelected = false;
            this.GetTabNextElement += (backward, next) => next;
            this.GetGamepadNextElement += (dir, next) => next;

            this.SetAreaDirty();
            this.SetSortedChildrenDirty();
        }

        /// <inheritdoc />
        ~Element() {
            this.Dispose();
        }

        /// <summary>
        /// Adds a child to this element.
        /// </summary>
        /// <param name="element">The child element to add</param>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Children"/> list</param>
        /// <typeparam name="T">The type of child to add</typeparam>
        /// <returns>This element, for chaining</returns>
        public virtual T AddChild<T>(T element, int index = -1) where T : Element {
            if (index < 0 || index > this.children.Count)
                index = this.children.Count;
            this.children.Insert(index, element);
            element.Parent = this;
            element.AndChildren(e => {
                e.Root = this.Root;
                e.System = this.System;
                this.Root?.InvokeOnElementAdded(e);
                this.OnChildAdded?.Invoke(this, e);
            });
            this.SetSortedChildrenDirty();
            element.SetAreaDirty();
            return element;
        }

        /// <summary>
        /// Removes the given child from this element.
        /// </summary>
        /// <param name="element">The child element to remove</param>
        public virtual void RemoveChild(Element element) {
            this.children.Remove(element);
            // set area dirty here so that a dirty call is made
            // upwards to us if the element is auto-positioned
            element.SetAreaDirty();
            element.Parent = null;
            element.AndChildren(e => {
                e.Root = null;
                e.System = null;
                this.Root?.InvokeOnElementRemoved(e);
                this.OnChildRemoved?.Invoke(this, e);
            });
            this.SetSortedChildrenDirty();
        }

        /// <summary>
        /// Removes all children from this element that match the given condition.
        /// </summary>
        /// <param name="condition">The condition that determines if a child should be removed</param>
        public virtual void RemoveChildren(Func<Element, bool> condition = null) {
            for (var i = this.Children.Count - 1; i >= 0; i--) {
                var child = this.Children[i];
                if (condition == null || condition(child)) {
                    this.RemoveChild(child);
                }
            }
        }

        /// <summary>
        /// Causes <see cref="SortedChildren"/> to be recalculated as soon as possible.
        /// </summary>
        public void SetSortedChildrenDirty() {
            this.sortedChildrenDirty = true;
        }

        /// <summary>
        /// Updates the <see cref="SortedChildren"/> list if <see cref="SetSortedChildrenDirty"/> is true.
        /// </summary>
        public void UpdateSortedChildrenIfDirty() {
            if (this.sortedChildrenDirty)
                this.ForceUpdateSortedChildren();
        }

        /// <summary>
        /// Forces an update of the <see cref="SortedChildren"/> list.
        /// </summary>
        public virtual void ForceUpdateSortedChildren() {
            this.sortedChildrenDirty = false;
            this.sortedChildren = new ReadOnlyCollection<Element>(this.Children.OrderBy(e => e.Priority).ToArray());
        }

        /// <summary>
        /// Causes this element's <see cref="Area"/> to be recalculated as soon as possible.
        /// If this element is auto-anchored or its parent automatically changes its size based on its children, this element's parent's area is also marked dirty.
        /// </summary>
        public void SetAreaDirty() {
            this.areaDirty = true;
            if (this.Parent != null && (this.Anchor >= Anchor.AutoLeft || this.Parent.SetWidthBasedOnChildren || this.Parent.SetHeightBasedOnChildren))
                this.Parent.SetAreaDirty();
        }

        /// <summary>
        /// Updates this element's <see cref="Area"/> list if <see cref="areaDirty"/> is true.
        /// </summary>
        public void UpdateAreaIfDirty() {
            if (this.areaDirty)
                this.ForceUpdateArea();
        }

        /// <summary>
        /// Forces this element's <see cref="Area"/> to be updated if it is not <see cref="IsHidden"/>.
        /// This method also updates all of this element's <see cref="Children"/>'s areas.
        /// </summary>
        public virtual void ForceUpdateArea() {
            this.areaDirty = false;
            if (this.IsHidden)
                return;

            var parentArea = this.Parent != null ? this.Parent.ChildPaddedArea : (RectangleF) this.system.Viewport;
            var parentCenterX = parentArea.X + parentArea.Width / 2;
            var parentCenterY = parentArea.Y + parentArea.Height / 2;
            var actualSize = this.CalcActualSize(parentArea);

            var recursion = 0;
            UpdateDisplayArea(actualSize);

            void UpdateDisplayArea(Vector2 newSize) {
                var pos = new Vector2();
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
                        pos.X = parentCenterX - newSize.X / 2 + this.ScaledOffset.X;
                        pos.Y = parentArea.Y + this.ScaledOffset.Y;
                        break;
                    case Anchor.TopRight:
                    case Anchor.AutoRight:
                        pos.X = parentArea.Right - newSize.X - this.ScaledOffset.X;
                        pos.Y = parentArea.Y + this.ScaledOffset.Y;
                        break;
                    case Anchor.CenterLeft:
                        pos.X = parentArea.X + this.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + this.ScaledOffset.Y;
                        break;
                    case Anchor.Center:
                        pos.X = parentCenterX - newSize.X / 2 + this.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + this.ScaledOffset.Y;
                        break;
                    case Anchor.CenterRight:
                        pos.X = parentArea.Right - newSize.X - this.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + this.ScaledOffset.Y;
                        break;
                    case Anchor.BottomLeft:
                        pos.X = parentArea.X + this.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - this.ScaledOffset.Y;
                        break;
                    case Anchor.BottomCenter:
                        pos.X = parentCenterX - newSize.X / 2 + this.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - this.ScaledOffset.Y;
                        break;
                    case Anchor.BottomRight:
                        pos.X = parentArea.Right - newSize.X - this.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - this.ScaledOffset.Y;
                        break;
                }

                if (this.Anchor >= Anchor.AutoLeft) {
                    Element previousChild;
                    if (this.Anchor == Anchor.AutoInline || this.Anchor == Anchor.AutoInlineIgnoreOverflow) {
                        previousChild = this.GetOlderSibling(e => !e.IsHidden && e.CanAutoAnchorsAttach);
                    } else {
                        previousChild = this.GetLowestOlderSibling(e => !e.IsHidden && e.CanAutoAnchorsAttach);
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
                                // with awkward ui scale values, floating point rounding can cause an element that would usually
                                // be positioned correctly to be pushed into the next line due to a very small deviation
                                if (newX + newSize.X <= parentArea.Right + Epsilon) {
                                    pos.X = newX;
                                    pos.Y = prevArea.Y + this.ScaledOffset.Y;
                                } else {
                                    pos.Y = prevArea.Bottom + this.ScaledOffset.Y;
                                }
                                break;
                            case Anchor.AutoInlineIgnoreOverflow:
                                pos.X = prevArea.Right + this.ScaledOffset.X;
                                pos.Y = prevArea.Y + this.ScaledOffset.Y;
                                break;
                        }
                    }
                }

                if (this.PreventParentSpill) {
                    if (pos.X < parentArea.X)
                        pos.X = parentArea.X;
                    if (pos.Y < parentArea.Y)
                        pos.Y = parentArea.Y;
                    if (pos.X + newSize.X > parentArea.Right)
                        newSize.X = parentArea.Right - pos.X;
                    if (pos.Y + newSize.Y > parentArea.Bottom)
                        newSize.Y = parentArea.Bottom - pos.Y;
                }

                this.area = new RectangleF(pos, newSize);
                this.System.InvokeOnElementAreaUpdated(this);

                foreach (var child in this.Children)
                    child.ForceUpdateArea();

                if (this.SetWidthBasedOnChildren || this.SetHeightBasedOnChildren) {
                    Element foundChild = null;
                    var autoSize = this.UnscrolledArea.Size;

                    if (this.SetHeightBasedOnChildren) {
                        var lowest = this.GetLowestChild(e => !e.IsHidden);
                        if (lowest != null) {
                            autoSize.Y = lowest.UnscrolledArea.Bottom - pos.Y + this.ScaledChildPadding.Bottom;
                            foundChild = lowest;
                        } else {
                            if (this.Children.Any(e => !e.IsHidden))
                                throw new InvalidOperationException($"{this} with root {this.Root.Name} sets its height based on children but it only has visible children anchored too low ({string.Join(", ", this.Children.Where(c => !c.IsHidden).Select(c => c.Anchor))})");
                            autoSize.Y = 0;
                        }
                    }

                    if (this.SetWidthBasedOnChildren) {
                        var rightmost = this.GetRightmostChild(e => !e.IsHidden);
                        if (rightmost != null) {
                            autoSize.X = rightmost.UnscrolledArea.Right - pos.X + this.ScaledChildPadding.Right;
                            foundChild = rightmost;
                        } else {
                            if (this.Children.Any(e => !e.IsHidden))
                                throw new InvalidOperationException($"{this} with root {this.Root.Name} sets its width based on children but it only has visible children anchored too far right ({string.Join(", ", this.Children.Where(c => !c.IsHidden).Select(c => c.Anchor))})");
                            autoSize.X = 0;
                        }
                    }

                    if (this.TreatSizeAsMinimum) {
                        autoSize = Vector2.Max(autoSize, actualSize);
                    } else if (this.TreatSizeAsMaximum) {
                        autoSize = Vector2.Min(autoSize, actualSize);
                    }

                    // we want to leave some leeway to prevent float rounding causing an infinite loop
                    if (!autoSize.Equals(this.UnscrolledArea.Size, Epsilon)) {
                        recursion++;
                        if (recursion >= 16) {
                            throw new ArithmeticException($"The area of {this} with root {this.Root.Name} has recursively updated too often. Does its child {foundChild} contain any conflicting auto-sizing settings?");
                        } else {
                            UpdateDisplayArea(autoSize);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the actual size that this element should take up, based on the area that its parent encompasses.
        /// By default, this is based on the information specified in <see cref="Size"/>'s documentation.
        /// </summary>
        /// <param name="parentArea">This parent's area, or the ui system's viewport if it has no parent</param>
        /// <returns>The actual size of this element, taking <see cref="Scale"/> into account</returns>
        protected virtual Vector2 CalcActualSize(RectangleF parentArea) {
            var ret = new Vector2(
                this.size.X > 1 ? this.ScaledSize.X : parentArea.Width * this.size.X,
                this.size.Y > 1 ? this.ScaledSize.Y : parentArea.Height * this.size.Y);
            if (this.size.X < 0)
                ret.X = -this.size.X * ret.Y;
            if (this.size.Y < 0)
                ret.Y = -this.size.Y * ret.X;
            return ret;
        }

        /// <summary>
        /// Returns the area that should be used for determining where auto-anchoring children should attach.
        /// </summary>
        /// <returns>The area for auto anchors</returns>
        protected virtual RectangleF GetAreaForAutoAnchors() {
            return this.UnscrolledArea;
        }

        /// <summary>
        /// Returns this element's lowest child element (in terms of y position) that matches the given condition.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>The lowest element, or null if no such element exists</returns>
        public Element GetLowestChild(Func<Element, bool> condition = null) {
            Element lowest = null;
            foreach (var child in this.Children) {
                if (condition != null && !condition(child))
                    continue;
                if (child.Anchor > Anchor.TopRight && child.Anchor < Anchor.AutoLeft)
                    continue;
                if (lowest == null || child.UnscrolledArea.Bottom >= lowest.UnscrolledArea.Bottom)
                    lowest = child;
            }
            return lowest;
        }

        /// <summary>
        /// Returns this element's rightmost child (in terms of x position) that matches the given condition.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>The rightmost element, or null if no such element exists</returns>
        public Element GetRightmostChild(Func<Element, bool> condition = null) {
            Element rightmost = null;
            foreach (var child in this.Children) {
                if (condition != null && !condition(child))
                    continue;
                if (child.Anchor < Anchor.AutoLeft && child.Anchor != Anchor.TopLeft && child.Anchor != Anchor.CenterLeft && child.Anchor != Anchor.BottomLeft)
                    continue;
                if (rightmost == null || child.UnscrolledArea.Right >= rightmost.UnscrolledArea.Right)
                    rightmost = child;
            }
            return rightmost;
        }

        /// <summary>
        /// Returns this element's lowest sibling that is also older (lower in its parent's <see cref="Children"/> list) than this element that also matches the given condition.
        /// The returned element's <see cref="Parent"/> will always be equal to this element's <see cref="Parent"/>.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>The lowest older sibling of this element, or null if no such element exists</returns>
        public Element GetLowestOlderSibling(Func<Element, bool> condition = null) {
            if (this.Parent == null)
                return null;
            Element lowest = null;
            foreach (var child in this.Parent.Children) {
                if (child == this)
                    break;
                if (condition != null && !condition(child))
                    continue;
                if (lowest == null || child.UnscrolledArea.Bottom >= lowest.UnscrolledArea.Bottom)
                    lowest = child;
            }
            return lowest;
        }

        /// <summary>
        /// Returns this element's first older sibling that matches the given condition.
        /// The returned element's <see cref="Parent"/> will always be equal to this element's <see cref="Parent"/>.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>The older sibling, or null if no such element exists</returns>
        public Element GetOlderSibling(Func<Element, bool> condition = null) {
            if (this.Parent == null)
                return null;
            Element older = null;
            foreach (var child in this.Parent.Children) {
                if (child == this)
                    break;
                if (condition != null && !condition(child))
                    continue;
                older = child;
            }
            return older;
        }

        /// <summary>
        /// Returns all of this element's siblings that match the given condition.
        /// Siblings are elements that have the same <see cref="Parent"/> as this element.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>This element's siblings</returns>
        public IEnumerable<Element> GetSiblings(Func<Element, bool> condition = null) {
            if (this.Parent == null)
                yield break;
            foreach (var child in this.Parent.Children) {
                if (condition != null && !condition(child))
                    continue;
                if (child != this)
                    yield return child;
            }
        }

        /// <summary>
        /// Returns all of this element's children of the given type that match the given condition.
        /// Optionally, the entire tree of children (grandchildren) can be searched.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <param name="regardGrandchildren">If this value is true, children of children of this element are also searched</param>
        /// <param name="ignoreFalseGrandchildren">If this value is true, children for which the condition does not match will not have their children searched</param>
        /// <typeparam name="T">The type of children to search for</typeparam>
        /// <returns>All children that match the condition</returns>
        public IEnumerable<T> GetChildren<T>(Func<T, bool> condition = null, bool regardGrandchildren = false, bool ignoreFalseGrandchildren = false) where T : Element {
            foreach (var child in this.Children) {
                var applies = child is T t && (condition == null || condition(t));
                if (applies)
                    yield return (T) child;
                if (regardGrandchildren && (!ignoreFalseGrandchildren || applies)) {
                    foreach (var cc in child.GetChildren(condition, true, ignoreFalseGrandchildren))
                        yield return cc;
                }
            }
        }

        /// <inheritdoc cref="GetChildren{T}"/>
        public IEnumerable<Element> GetChildren(Func<Element, bool> condition = null, bool regardGrandchildren = false, bool ignoreFalseGrandchildren = false) {
            return this.GetChildren<Element>(condition, regardGrandchildren, ignoreFalseGrandchildren);
        }

        /// <summary>
        /// Returns the parent tree of this element.
        /// The parent tree is this element's <see cref="Parent"/>, followed by its parent, and so on, up until the <see cref="RootElement"/>'s <see cref="RootElement.Element"/>.
        /// </summary>
        /// <returns>This element's parent tree</returns>
        public IEnumerable<Element> GetParentTree() {
            if (this.Parent == null)
                yield break;
            yield return this.Parent;
            foreach (var parent in this.Parent.GetParentTree())
                yield return parent;
        }

        /// <summary>
        /// Returns a subset of <see cref="Children"/> that are currently relevant in terms of drawing and input querying.
        /// A <see cref="Panel"/> only returns elements that are currently in view here.
        /// </summary>
        /// <returns>This element's relevant children</returns>
        protected virtual IList<Element> GetRelevantChildren() {
            return this.SortedChildren;
        }

        /// <summary>
        /// Updates this element and all of its <see cref="GetRelevantChildren"/>
        /// </summary>
        /// <param name="time">The game's time</param>
        public virtual void Update(GameTime time) {
            this.System.InvokeOnElementUpdated(this, time);

            foreach (var child in this.GetRelevantChildren())
                if (child.System != null)
                    child.Update(time);
        }

        /// <summary>
        /// Draws this element by calling <see cref="Draw"/> internally.
        /// If <see cref="Transform"/> or <see cref="BeginImpl"/> is set, a new <see cref="SpriteBatch.Begin"/> call is also started.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="alpha">The alpha to draw this element and its children with</param>
        /// <param name="blendState">The blend state that is used for drawing</param>
        /// <param name="samplerState">The sampler state that is used for drawing</param>
        /// <param name="matrix">The transformation matrix that is used for drawing</param>
        public void DrawTransformed(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var customDraw = this.BeginImpl != null || this.Transform != Matrix.Identity;
            var mat = this.Transform * matrix;
            if (customDraw) {
                // end the usual draw so that we can begin our own
                batch.End();
                // begin our own draw call
                if (this.BeginImpl != null) {
                    this.BeginImpl(this, time, batch, alpha, blendState, samplerState, mat);
                } else {
                    batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, mat);
                }
            }
            // draw content in custom begin call
            this.Draw(time, batch, alpha, blendState, samplerState, mat);
            if (customDraw) {
                // end our draw
                batch.End();
                // begin the usual draw again for other elements
                batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix);
            }
        }

        /// <summary>
        /// Draws this element and all of its children. Override this method to draw the content of custom elements.
        /// Note that, when this is called, <see cref="SpriteBatch.Begin"/> has already been called with custom <see cref="Transform"/> etc. applied.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="alpha">The alpha to draw this element and its children with</param>
        /// <param name="blendState">The blend state that is used for drawing</param>
        /// <param name="samplerState">The sampler state that is used for drawing</param>
        /// <param name="matrix">The transformation matrix that is used for drawing</param>
        public virtual void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            this.System.InvokeOnElementDrawn(this, time, batch, alpha);
            if (this.IsSelected)
                this.System.InvokeOnSelectedElementDrawn(this, time, batch, alpha);

            foreach (var child in this.GetRelevantChildren()) {
                if (!child.IsHidden)
                    child.DrawTransformed(time, batch, alpha * child.DrawAlpha, blendState, samplerState, matrix);
            }
        }

        /// <summary>
        /// Draws this element and all of its <see cref="GetRelevantChildren"/> early.
        /// Drawing early involves drawing onto <see cref="RenderTarget2D"/> instances rather than onto the screen.
        /// Note that, when this is called, <see cref="SpriteBatch.Begin"/> has not yet been called.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="alpha">The alpha to draw this element and its children with</param>
        /// <param name="blendState">The blend state that is used for drawing</param>
        /// <param name="samplerState">The sampler state that is used for drawing</param>
        /// <param name="matrix">The transformation matrix that is used for drawing</param>
        public virtual void DrawEarly(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            foreach (var child in this.GetRelevantChildren()) {
                if (!child.IsHidden)
                    child.DrawEarly(time, batch, alpha * child.DrawAlpha, blendState, samplerState, matrix);
            }
        }

        /// <summary>
        /// Returns the element under the given position, searching the current element and all of its <see cref="GetRelevantChildren"/>.
        /// </summary>
        /// <param name="position">The position to query</param>
        /// <returns>The element under the position, or null if no such element exists</returns>
        public virtual Element GetElementUnderPos(Vector2 position) {
            if (this.IsHidden)
                return null;
            if (this.Transform != Matrix.Identity)
                position = Vector2.Transform(position, Matrix.Invert(this.Transform));
            var relevant = this.GetRelevantChildren();
            for (var i = relevant.Count - 1; i >= 0; i--) {
                var element = relevant[i].GetElementUnderPos(position);
                if (element != null)
                    return element;
            }
            return this.CanBeMoused && this.DisplayArea.Contains(position) ? this : null;
        }

        /// <inheritdoc />
        public virtual void Dispose() {
            this.OnDisposed?.Invoke(this);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs the specified action on this element and all of its <see cref="Children"/>
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void AndChildren(Action<Element> action) {
            action(this);
            foreach (var child in this.Children)
                child.AndChildren(action);
        }

        /// <summary>
        /// Sorts this element's <see cref="Children"/> using the given comparison.
        /// </summary>
        /// <param name="comparison">The comparison to sort by</param>
        public void ReorderChildren(Comparison<Element> comparison) {
            this.children.Sort(comparison);
        }

        /// <summary>
        /// Reverses this element's <see cref="Children"/> list in the given range.
        /// </summary>
        /// <param name="index">The index to start reversing at</param>
        /// <param name="count">The amount of elements to reverse</param>
        public void ReverseChildren(int index = 0, int? count = null) {
            this.children.Reverse(index, count ?? this.Children.Count);
        }

        /// <summary>
        /// Scales this element's <see cref="Transform"/> matrix based on the given scale and origin.
        /// </summary>
        /// <param name="scale">The scale to use</param>
        /// <param name="origin">The origin to use for scaling, or null to use this element's center point</param>
        public void ScaleTransform(float scale, Vector2? origin = null) {
            this.Transform = Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(new Vector3((1 - scale) * (origin ?? this.DisplayArea.Center), 0));
        }

        /// <summary>
        /// Initializes this element's <see cref="StyleProp{T}"/> instances using the ui system's <see cref="UiStyle"/>.
        /// </summary>
        /// <param name="style">The new style</param>
        protected virtual void InitStyle(UiStyle style) {
            this.SelectionIndicator.SetFromStyle(style.SelectionIndicator);
            this.ActionSound.SetFromStyle(style.ActionSound);
            this.SecondActionSound.SetFromStyle(style.ActionSound);
        }

        /// <summary>
        /// A delegate used for the <see cref="Element.OnTextInput"/> event.
        /// </summary>
        /// <param name="element">The current element</param>
        /// <param name="key">The key that was pressed</param>
        /// <param name="character">The character that was input</param>
        public delegate void TextInputCallback(Element element, Keys key, char character);

        /// <summary>
        /// A generic element-specific delegate.
        /// </summary>
        /// <param name="element">The current element</param>
        public delegate void GenericCallback(Element element);

        /// <summary>
        /// A generic element-specific delegate that includes a second element.
        /// </summary>
        /// <param name="thisElement">The current element</param>
        /// <param name="otherElement">The other element</param>
        public delegate void OtherElementCallback(Element thisElement, Element otherElement);

        /// <summary>
        /// A delegate used inside of <see cref="Element.Draw"/>
        /// </summary>
        /// <param name="element">The element that is being drawn</param>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch used for drawing</param>
        /// <param name="alpha">The alpha this element is drawn with</param>
        public delegate void DrawCallback(Element element, GameTime time, SpriteBatch batch, float alpha);

        /// <summary>
        /// A generic delegate used inside of <see cref="Element.Update"/>
        /// </summary>
        /// <param name="element">The current element</param>
        /// <param name="time">The game's time</param>
        public delegate void TimeCallback(Element element, GameTime time);

        /// <summary>
        /// A delegate used by <see cref="Element.GetTabNextElement"/>.
        /// </summary>
        /// <param name="backward">If this value is true, <see cref="ModifierKey.Shift"/> is being held</param>
        /// <param name="usualNext">The element that is considered to be the next element by default</param>
        public delegate Element TabNextElementCallback(bool backward, Element usualNext);

        /// <summary>
        /// A delegate used by <see cref="Element.GetGamepadNextElement"/>.
        /// </summary>
        /// <param name="dir">The direction of the gamepad button that was pressed</param>
        /// <param name="usualNext">The element that is considered to be the next element by default</param>
        public delegate Element GamepadNextElementCallback(Direction2 dir, Element usualNext);

        /// <summary>
        /// A delegate method used for <see cref="BeginImpl"/>
        /// </summary>
        /// <param name="element">The custom draw group</param>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch used for drawing</param>
        /// <param name="alpha">This element's draw alpha</param>
        /// <param name="blendState">The blend state used for drawing</param>
        /// <param name="samplerState">The sampler state used for drawing</param>
        /// <param name="matrix">The transform matrix used for drawing</param>
        public delegate void BeginDelegate(Element element, GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix);

    }
}