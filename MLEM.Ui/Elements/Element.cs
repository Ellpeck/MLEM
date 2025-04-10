using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Graphics;
using MLEM.Input;
using MLEM.Maths;
using MLEM.Misc;
using MLEM.Sound;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// This class represents a generic base class for ui elements of a <see cref="UiSystem"/>.
    /// </summary>
    public abstract class Element : GenericDataHolder, ILayoutItem {

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
            private set {
                this.system = value;
                this.Controls = value?.Controls;
                this.AndChildren(e => e.Style = e.Style.OrStyle(value?.Style));
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
        public RootElement Root { get; private set; }
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
        /// Additionally, if auto-sizing is used, <see cref="AutoSizeAddedAbsolute"/> can be set to add or subtract an absolute value from the automatically calculated size.
        /// </summary>
        /// <example>
        /// The following example, ignoring <see cref="Scale"/>, combines both types of percentage-based sizing.
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
        /// If auto-sizing is used by setting <see cref="Size"/> less than or equal to 1, this property allows adding or subtracting an additional, absolute value from the automatically calculated size.
        /// If this element is not using auto-sizing, this property is ignored.
        /// </summary>
        /// <example>
        /// Ignoring <see cref="Scale"/>, if this element's <see cref="Size"/> is set to <c>0.5, 0.75</c> and its <see cref="Parent"/> has a size of <c>200, 100</c>, then an added absolute size of <c>-50, 25</c> will result in a final <see cref="Area"/> size of <c>0.5 * 200 - 50, 0.75 * 100 + 25</c>, or <c>50, 100</c>.
        /// </example>
        public Vector2 AutoSizeAddedAbsolute {
            get => this.autoSizeAddedAbsolute;
            set {
                if (this.autoSizeAddedAbsolute == value)
                    return;
                this.autoSizeAddedAbsolute = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// The <see cref="AutoSizeAddedAbsolute"/>, but with <see cref="Scale"/> applied.
        /// </summary>
        public Vector2 ScaledAutoSizeAddedAbsolute => this.autoSizeAddedAbsolute * this.Scale;
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
        public virtual bool IsHidden {
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
        /// A higher priority means the element will be drawn first, but not anchored higher up if auto-anchoring is used.
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
        /// This element's transform matrix, which is used for drawing and mouse/touch interaction. All children and grandchildren of this element will have this transform applied to them, along with their own transforms. This can be useful for briefly changing the visuals of an element when using a <see cref="UiAnimation"/>.
        /// This matrix has no bearing on this element's permanent <see cref="DisplayArea"/> or <see cref="Scale"/>, as it is only applied "on the fly" in <see cref="Draw"/> and <see cref="TransformInverse"/>.
        /// When this is anything other than <see cref="Matrix.Identity"/>, a new <c>SpriteBatch.Begin</c> call is used for this element when drawing.
        /// This matrix can easily be scaled relative to its center or an arbitrary point using <see cref="ScaleTransform"/>.
        /// </summary>
        public Matrix Transform = Matrix.Identity;
        /// <summary>
        /// Set this field to false to disallow the element from being selected.
        /// An unselectable element is skipped by automatic navigation and its <see cref="OnSelected"/> callback will never be called.
        /// </summary>
        public virtual bool CanBeSelected {
            get => this.canBeSelected;
            set {
                this.canBeSelected = value;
                if (!this.canBeSelected && this.Root?.SelectedElement == this)
                    this.Root.SelectElement(null);
            }
        }
        /// <summary>
        /// Set this field to false to disallow the element from reacting to being moused over.
        /// </summary>
        public virtual bool CanBeMoused { get; set; } = true;
        /// <summary>
        /// Set this field to false to disallow this element's <see cref="OnPressed"/> and <see cref="OnSecondaryPressed"/> events to be called.
        /// </summary>
        public virtual bool CanBePressed { get; set; } = true;
        /// <summary>
        /// Set this field to false to cause auto-anchored siblings to ignore this element as a possible anchor point.
        /// </summary>
        public virtual bool CanAutoAnchorsAttach {
            get => this.canAutoAnchorsAttach;
            set {
                if (this.canAutoAnchorsAttach != value) {
                    this.canAutoAnchorsAttach = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Set this field to true to cause this element's width to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this element's <see cref="Size"/>'s X coordinate as a minimum or maximum width rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        public virtual bool SetWidthBasedOnChildren {
            get => this.setWidthBasedOnChildren;
            set {
                if (this.setWidthBasedOnChildren != value) {
                    this.setWidthBasedOnChildren = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Set this field to true to cause this element's height to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this element's <see cref="Size"/>'s Y coordinate as a minimum or maximum height rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        public virtual bool SetHeightBasedOnChildren {
            get => this.setHeightBasedOnChildren;
            set {
                if (this.setHeightBasedOnChildren != value) {
                    this.setHeightBasedOnChildren = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled, the resulting width or height will always be greather than or equal to this element's <see cref="Size"/>.
        /// For example, if an element's <see cref="Size"/>'s Y coordinate is set to 20, but there is only one child with a height of 10 in it, the element's height would be shrunk to 10 if this value was false, but would remain at 20 if it was true.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        public virtual bool TreatSizeAsMinimum {
            get => this.treatSizeAsMinimum;
            set {
                if (this.treatSizeAsMinimum != value) {
                    this.treatSizeAsMinimum = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/>are enabled, the resulting width or height weill always be less than or equal to this element's <see cref="Size"/>.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        public virtual bool TreatSizeAsMaximum {
            get => this.treatSizeAsMaximum;
            set {
                if (this.treatSizeAsMaximum != value) {
                    this.treatSizeAsMaximum = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Set this field to true to cause this element's final display area to never exceed that of its <see cref="Parent"/>.
        /// If the resulting area is too large, the size of this element is shrunk to fit the target area.
        /// This can be useful if an element should fill the remaining area of a parent exactly.
        /// </summary>
        public virtual bool PreventParentSpill {
            get => this.preventParentSpill;
            set {
                if (this.preventParentSpill != value) {
                    this.preventParentSpill = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// The transparency (alpha value) that this element is rendered with.
        /// Note that, when <see cref="Draw(Microsoft.Xna.Framework.GameTime,Microsoft.Xna.Framework.Graphics.SpriteBatch,float,MLEM.Graphics.SpriteBatchContext)"/> is called, this alpha value is multiplied with the <see cref="Parent"/>'s alpha value and passed down to this element's <see cref="Children"/>.
        /// </summary>
        public virtual float DrawAlpha { get; set; } = 1;
        /// <summary>
        /// Stores whether this element is currently being moused over or touched.
        /// </summary>
        public bool IsMouseOver => this.Controls.MousedElement == this || this.Controls.TouchedElement == this;
        /// <summary>
        /// Returns whether this element is its <see cref="Root"/>'s <see cref="RootElement.SelectedElement"/>.
        /// Note that, unlike <see cref="IsSelectedActive"/>, this property will be <see langword="true"/> even if this element's <see cref="Root"/> is not the <see cref="UiControls.ActiveRoot"/>.
        /// </summary>
        public bool IsSelected => this.Root.SelectedElement == this;
        /// <summary>
        /// Returns whether this element is its <see cref="Controls"/>'s <see cref="UiControls.SelectedElement"/>.
        /// Note that <see cref="IsSelected"/> can be used to query whether this element is its <see cref="Root"/>'s <see cref="RootElement.SelectedElement"/> instead.
        /// </summary>
        public bool IsSelectedActive => this.Controls.SelectedElement == this;
        /// <summary>
        /// Returns whether this element's <see cref="SetAreaDirty"/> method has been recently called and its area has not been updated since then using <see cref="UpdateAreaIfDirty"/> or <see cref="ForceUpdateArea"/>.
        /// </summary>
        public bool AreaDirty { get; private set; }
        /// <summary>
        /// An optional string that represents a group of elements for automatic (keyboard and gamepad) navigation.
        /// All elements that share the same auto-nav group will be able to be navigated between, and all other elements will not be reachable from elements of other groups.
        /// Note that, if no element is previously selected and auto-navigation is invoked, this element cannot be chosen as the first element to navigate to if its auto-nav group is non-null.
        /// </summary>
        public virtual string AutoNavGroup { get; set; }

        /// <summary>
        /// This Element's current <see cref="UiStyle"/>.
        /// When this property is set, <see cref="InitStyle"/> is called.
        /// </summary>
        public StyleProp<UiStyle> Style {
            get => this.style;
            set {
                this.style = value;
                if (this.style.HasValue())
                    this.InitStyle(this.style);
            }
        }
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
        /// </summary>
        public StyleProp<Padding> ChildPadding {
            get => this.childPadding;
            set {
                this.childPadding = value;
                this.SetAreaDirty();
            }
        }
        /// <summary>
        /// A <see cref="UiAnimation"/> that is played when the mouse enters this element, in <see cref="OnMouseEnter"/>.
        /// </summary>
        public StyleProp<UiAnimation> MouseEnterAnimation;
        /// <summary>
        /// A <see cref="UiAnimation"/> that is played when the mouse exits this element, in <see cref="OnMouseExit"/>.
        /// </summary>
        public StyleProp<UiAnimation> MouseExitAnimation;

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
        /// Also note that if the active <see cref="MlemPlatform"/> doesn't support <see cref="MlemPlatform.AddTextInputListener"/>, this event is never called.
        /// </summary>
        public TextInputCallback OnTextInput;
        /// <summary>
        /// Event that is called when this element's <see cref="DisplayArea"/> is changed.
        /// </summary>
        public GenericCallback OnAreaUpdated;
        /// <summary>
        /// Event that is called when this element's <see cref="InitStyle"/> method is called while setting the <see cref="Style"/>.
        /// </summary>
        public GenericCallback OnStyleInit;
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
        /// Note that, while this event is only called for immediate children of this element, <see cref="RootElement.OnElementAdded"/> is called for all children and grandchildren.
        /// </summary>
        public OtherElementCallback OnChildAdded;
        /// <summary>
        /// Event that is called when a child is removed from this element using <see cref="RemoveChild"/>.
        /// Note that, while this event is only called for immediate children of this element, <see cref="RootElement.OnElementRemoved"/> is called for all children and grandchildren.
        /// </summary>
        public OtherElementCallback OnChildRemoved;
        /// <summary>
        /// Event that is called when this element is added to a <see cref="UiSystem"/>, that is, when this element's <see cref="System"/> is set to a non-<see langword="null"/> value.
        /// </summary>
        public GenericCallback OnAddedToUi;
        /// <summary>
        /// Event that is called when this element is removed from a <see cref="UiSystem"/>, that is, when this element's <see cref="System"/> is set to <see langword="null"/>.
        /// </summary>
        public GenericCallback OnRemovedFromUi;

        /// <summary>
        /// A list of all of this element's direct children.
        /// Use <see cref="AddChild{T}"/> or <see cref="RemoveChild"/> to manipulate this list while calling all of the necessary callbacks.
        /// </summary>
        public readonly IList<Element> Children;
        /// <summary>
        /// A list of all of the <see cref="UiAnimation"/> instances that are currently playing.
        /// You can modify this collection through <see cref="PlayAnimation"/> and <see cref="StopAnimation"/>.
        /// </summary>
        protected readonly List<UiAnimation> PlayingAnimations = new List<UiAnimation>();

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
        /// <summary>
        /// The <see cref="ChildPaddedArea"/> of this element's <see cref="Parent"/>, or the <see cref="UiSystem.Viewport"/> if this element has no parent.
        /// This value is the one that is passed to <see cref="CalcActualSize"/> during <see cref="ForceUpdateArea"/>.
        /// </summary>
        protected RectangleF ParentArea => this.Parent?.ChildPaddedArea ?? (RectangleF) this.System.Viewport;

        ILayoutItem ILayoutItem.Parent => this.Parent;
        RectangleF ILayoutItem.ParentArea => this.ParentArea;
        IEnumerable<ILayoutItem> ILayoutItem.Children => this.Children;
        RectangleF ILayoutItem.AutoAnchorArea => this.GetAreaForAutoAnchors();

        private readonly List<Element> children = new List<Element>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private bool sortedChildrenDirty;
        private IList<Element> sortedChildren;
        private UiSystem system;
        private Anchor anchor;
        private Vector2 size;
        private Vector2 autoSizeAddedAbsolute;
        private Vector2 offset;
        private RectangleF area;
        private bool isHidden;
        private int priority;
        private StyleProp<UiStyle> style;
        private StyleProp<Padding> childPadding;
        private bool canBeSelected = true;
        private bool canAutoAnchorsAttach = true;
        private bool setWidthBasedOnChildren;
        private bool setHeightBasedOnChildren;
        private bool treatSizeAsMinimum;
        private bool treatSizeAsMaximum;
        private bool preventParentSpill;

        /// <summary>
        /// Creates a new element with the given anchor and size and sets up some default event reactions.
        /// </summary>
        /// <param name="anchor">This element's <see cref="Anchor"/></param>
        /// <param name="size">This element's default <see cref="Size"/></param>
        protected Element(Anchor anchor, Vector2 size) {
            this.anchor = anchor;
            this.size = size;

            this.Children = new ReadOnlyCollection<Element>(this.children);
            this.GetTabNextElement += (backward, next) => next;
            this.GetGamepadNextElement += (dir, next) => next;
            this.OnMouseEnter += e => {
                if (e.MouseEnterAnimation.HasValue())
                    e.PlayAnimation(e.MouseEnterAnimation);
            };
            this.OnMouseExit += e => {
                if (e.MouseExitAnimation.HasValue())
                    e.PlayAnimation(e.MouseExitAnimation);
            };

            this.SetAreaDirty();
            this.SetSortedChildrenDirty();
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
            if (this.System != null)
                element.AndChildren(e => e.AddedToUi(this.System, this.Root));
            this.OnChildAdded?.Invoke(this, element);
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
            if (this.Root?.SelectedElement == element)
                this.Root.SelectElement(null);
            // set area dirty here so that a dirty call is made
            // upwards to us if the element is auto-positioned
            element.SetAreaDirty();
            element.Parent = null;
            if (this.System != null)
                element.AndChildren(e => e.RemovedFromUi());
            this.OnChildRemoved?.Invoke(this, element);
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
            this.AreaDirty = true;
            this.Parent?.OnChildAreaDirty(this, false);
        }

        /// <summary>
        /// Updates this element's <see cref="Area"/> and all of its <see cref="Children"/> by calling <see cref="ForceUpdateArea"/> if <see cref="AreaDirty"/> is true.
        /// </summary>
        /// <returns>Whether <see cref="AreaDirty"/> was true and <see cref="ForceUpdateArea"/> was called</returns>
        public bool UpdateAreaIfDirty() {
            if (this.AreaDirty) {
                this.ForceUpdateArea();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Forces this element's <see cref="Area"/> to be updated if it is not <see cref="IsHidden"/>.
        /// This method also updates all of this element's <see cref="Children"/>'s areas.
        /// </summary>
        public virtual void ForceUpdateArea() {
            this.AreaDirty = false;
            if (this.IsHidden || this.System == null)
                return;
            // if the parent's area is dirty, it would get updated anyway when querying its ChildPaddedArea,
            // which would cause our ForceUpdateArea code to be run twice, so we only update our parent instead
            if (this.Parent != null && this.Parent.UpdateAreaIfDirty())
                return;
            this.stopwatch.Restart();

            UiLayouter.Layout(this, Element.Epsilon);

            this.stopwatch.Stop();
            this.System.Metrics.ForceAreaUpdateTime += this.stopwatch.Elapsed;
            this.System.Metrics.ForceAreaUpdates++;
        }

        /// <summary>
        /// Sets this element's <see cref="Area"/> to the given <see cref="RectangleF"/> and invokes the <see cref="UiSystem.OnElementAreaUpdated"/> event.
        /// This method also updates all of this element's <see cref="Children"/>'s areas.
        /// Note that this method does not take into account any auto-sizing, anchoring or positioning, and so it should be used sparingly, if at all.
        /// </summary>
        /// <seealso cref="ForceUpdateArea"/>
        public virtual void SetAreaAndUpdateChildren(RectangleF area) {
            this.area = area;
            this.System.InvokeOnElementAreaUpdated(this);
            foreach (var child in this.Children)
                child.ForceUpdateArea();
            this.System.Metrics.ActualAreaUpdates++;
        }

        /// <summary>
        /// Calculates the actual size that this element should take up, based on the area that its parent encompasses.
        /// By default, this is based on the information specified in <see cref="Size"/>'s documentation.
        /// </summary>
        /// <param name="parentArea">This parent's area, or the ui system's viewport if it has no parent</param>
        /// <returns>The actual size of this element, taking <see cref="Scale"/> into account</returns>
        protected virtual Vector2 CalcActualSize(RectangleF parentArea) {
            var ret = new Vector2(
                this.size.X > 1 ? this.ScaledSize.X : parentArea.Width * this.size.X + this.ScaledAutoSizeAddedAbsolute.X,
                this.size.Y > 1 ? this.ScaledSize.Y : parentArea.Height * this.size.Y + this.ScaledAutoSizeAddedAbsolute.Y);
            if (this.size.X < 0)
                ret.X = -this.size.X * ret.Y + this.ScaledAutoSizeAddedAbsolute.X;
            if (this.size.Y < 0)
                ret.Y = -this.size.Y * ret.X + this.ScaledAutoSizeAddedAbsolute.Y;
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
        /// <param name="total">Whether to evaluate based on the child's <see cref="GetTotalCoveredArea"/>, rather than its <see cref="UnscrolledArea"/>.</param>
        /// <returns>The lowest element, or null if no such element exists</returns>
        public Element GetLowestChild(Func<Element, bool> condition = null, bool total = false) {
            return UiLayouter.GetLowestChild(this, condition, total);
        }

        /// <summary>
        /// Returns this element's rightmost child (in terms of x position) that matches the given condition.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <param name="total">Whether to evaluate based on the child's <see cref="GetTotalCoveredArea"/>, rather than its <see cref="UnscrolledArea"/>.</param>
        /// <returns>The rightmost element, or null if no such element exists</returns>
        public Element GetRightmostChild(Func<Element, bool> condition = null, bool total = false) {
            return UiLayouter.GetRightmostChild(this, condition, total);
        }

        /// <summary>
        /// Returns this element's lowest sibling that is also older (lower in its parent's <see cref="Children"/> list) than this element that also matches the given condition.
        /// The returned element's <see cref="Parent"/> will always be equal to this element's <see cref="Parent"/>.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <param name="total">Whether to evaluate based on the child's <see cref="GetTotalCoveredArea"/>, rather than its <see cref="UnscrolledArea"/>.</param>
        /// <returns>The lowest older sibling of this element, or null if no such element exists</returns>
        public Element GetLowestOlderSibling(Func<Element, bool> condition = null, bool total = false) {
            return UiLayouter.GetLowestOlderSibling(this, condition, total);
        }

        /// <summary>
        /// Returns this element's first older sibling that matches the given condition.
        /// The returned element's <see cref="Parent"/> will always be equal to this element's <see cref="Parent"/>.
        /// </summary>
        /// <param name="condition">The condition to match</param>
        /// <returns>The older sibling, or null if no such element exists</returns>
        public Element GetOlderSibling(Func<Element, bool> condition = null) {
            return UiLayouter.GetOlderSibling(this, condition);
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
            return UiLayouter.GetChildren(this, condition, regardGrandchildren, ignoreFalseGrandchildren);
        }

        /// <inheritdoc cref="GetChildren{T}"/>
        public IEnumerable<Element> GetChildren(Func<Element, bool> condition = null, bool regardGrandchildren = false, bool ignoreFalseGrandchildren = false) {
            return UiLayouter.GetChildren(this, condition, regardGrandchildren, ignoreFalseGrandchildren);
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
        /// Returns the total covered area of this element, which is its <see cref="Area"/> (or <see cref="UnscrolledArea"/>), unioned with all of the total covered areas of its <see cref="Children"/>.
        /// The returned area is only different from this element's <see cref="Area"/> (or <see cref="UnscrolledArea"/>) if it has any <see cref="Children"/> that are outside of this element's area, or are bigger than this element.
        /// </summary>
        /// <param name="unscrolled">Whether to use elements' <see cref="UnscrolledArea"/> (instead of their <see cref="Area"/>).</param>
        /// <returns>This element's total covered area.</returns>
        public RectangleF GetTotalCoveredArea(bool unscrolled) {
            var ret = unscrolled ? this.UnscrolledArea : this.Area;
            foreach (var child in this.Children) {
                if (!child.IsHidden)
                    ret = RectangleF.Union(ret, child.GetTotalCoveredArea(unscrolled));
            }
            return ret;
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
        /// Updates this element and all of its <see cref="SortedChildren"/>
        /// </summary>
        /// <param name="time">The game's time</param>
        public virtual void Update(GameTime time) {
            this.System.InvokeOnElementUpdated(this, time);

            for (var i = this.PlayingAnimations.Count - 1; i >= 0; i--) {
                var anim = this.PlayingAnimations[i];
                if (anim.Update(this, time)) {
                    anim.OnFinished(this);
                    this.PlayingAnimations.RemoveAt(i);
                }
            }

            // update all sorted children, not just relevant ones, because they might become relevant or irrelevant through updates
            foreach (var child in this.SortedChildren) {
                if (child.System != null)
                    child.Update(time);
            }

            if (this.System != null)
                this.System.Metrics.Updates++;
        }

        /// <summary>
        /// Draws this element by calling <see cref="Draw(Microsoft.Xna.Framework.GameTime,Microsoft.Xna.Framework.Graphics.SpriteBatch,float,MLEM.Graphics.SpriteBatchContext)"/> internally.
        /// If <see cref="Transform"/> is set, a new <c>SpriteBatch.Begin</c> call is also started.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="alpha">The alpha to draw this element and its children with</param>
        /// <param name="context">The sprite batch context to use for drawing</param>
        public void DrawTransformed(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            var customDraw = this.Transform != Matrix.Identity;
            var transformed = context;
            transformed.TransformMatrix = this.Transform * transformed.TransformMatrix;
            // TODO ending and beginning again when the matrix changes isn't ideal (https://github.com/MonoGame/MonoGame/issues/3156)
            if (customDraw) {
                // end the usual draw so that we can begin our own
                batch.End();
                // begin our own draw call
                batch.Begin(transformed);
            }

            // draw content in custom begin call
            this.Draw(time, batch, alpha, transformed);
            if (this.System != null)
                this.System.Metrics.Draws++;

            if (customDraw) {
                // end our draw
                batch.End();
                // begin the usual draw again for other elements
                batch.Begin(context);
            }
        }

        /// <summary>
        /// Draws this element and all of its children. Override this method to draw the content of custom elements.
        /// Note that, when this is called, <c>SpriteBatch.Begin</c> has already been called with custom <see cref="Transform"/> etc. applied.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        /// <param name="alpha">The alpha to draw this element and its children with</param>
        /// <param name="context">The sprite batch context to use for drawing</param>
        public virtual void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            this.System.InvokeOnElementDrawn(this, time, batch, alpha, context);
            if (this.IsSelected)
                this.System.InvokeOnSelectedElementDrawn(this, time, batch, alpha, context);

            foreach (var child in this.GetRelevantChildren()) {
                if (!child.IsHidden)
                    child.DrawTransformed(time, batch, alpha * child.DrawAlpha, context);
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
            position = this.TransformInverse(position);
            var relevant = this.GetRelevantChildren();
            for (var i = relevant.Count - 1; i >= 0; i--) {
                var element = relevant[i].GetElementUnderPos(position);
                if (element != null)
                    return element;
            }
            return this.CanBeMoused && this.DisplayArea.Contains(position) ? this : null;
        }

        /// <summary>
        /// Plays the given <see cref="UiAnimation"/> on this element, causing it to be added to the <see cref="PlayingAnimations"/> and updated in <see cref="Update"/>.
        /// If the given <paramref name="animation"/> is already playing on this element, it will be restarted.
        /// </summary>
        /// <param name="animation">The animation to play.</param>
        public virtual void PlayAnimation(UiAnimation animation) {
            if (this.PlayingAnimations.Contains(animation)) {
                // if we're already playing this animation, just restart it
                animation.OnFinished(this);
            } else {
                this.PlayingAnimations.Add(animation);
            }
        }

        /// <summary>
        /// Stops the given <see cref="UiAnimation"/> on this element, causing it to be removed from the <see cref="PlayingAnimations"/> and <see cref="UiAnimation.OnFinished"/> to be invoked.
        /// </summary>
        /// <param name="animation">The animation to stop.</param>
        /// <returns>Whether the animation was present in this element's <see cref="PlayingAnimations"/>.</returns>
        public virtual bool StopAnimation(UiAnimation animation) {
            if (this.PlayingAnimations.Remove(animation)) {
                animation.OnFinished(this);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override string ToString() {
            var ret = this.GetType().Name;
            // elements will contain their path up to the root and their index in each parent
            // eg Paragraph 2 @ Panel 3 @ ... @ Group RootName
            if (this.Parent != null) {
                ret += $" {this.Parent.Children.IndexOf(this)} @ {this.Parent}";
            } else if (this.Root?.Element == this) {
                ret += $" {this.Root.Name}";
            }
            return ret;
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
        /// Please note the documentation for <see cref="Transform"/> for in-depth information about how this matrix is used.
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
            this.SelectionIndicator = this.SelectionIndicator.OrStyle(style.SelectionIndicator);
            this.ActionSound = this.ActionSound.OrStyle(style.ActionSound);
            this.SecondActionSound = this.SecondActionSound.OrStyle(style.ActionSound);
            this.MouseEnterAnimation = this.MouseEnterAnimation.OrStyle(style.MouseEnterAnimation);
            this.MouseExitAnimation = this.MouseExitAnimation.OrStyle(style.MouseExitAnimation);

            this.System?.InvokeOnElementStyleInit(this);
            style.ApplyCustomStyle(this);
        }

        /// <summary>
        /// A method that gets called by this element's <see cref="Children"/> or any of its grandchildren when their <see cref="SetAreaDirty"/> methods get called.
        /// Note that the element's area might already be dirty, which will not stop this method from being called.
        /// </summary>
        /// <param name="child">The child whose area is being set dirty.</param>
        /// <param name="grandchild">Whether the <paramref name="child"/> is a grandchild of this element, rather than a direct child.</param>
        protected virtual void OnChildAreaDirty(Element child, bool grandchild) {
            if (!grandchild) {
                if (child.Anchor.IsAuto() || child.PreventParentSpill || this.SetWidthBasedOnChildren || this.SetHeightBasedOnChildren)
                    this.SetAreaDirty();
            }
            this.Parent?.OnChildAreaDirty(child, true);
        }

        /// <summary>
        /// Transforms the given <paramref name="position"/> by the inverse of this element's <see cref="Transform"/> matrix.
        /// </summary>
        /// <param name="position">The position to transform</param>
        /// <returns>The transformed position</returns>
        protected Vector2 TransformInverse(Vector2 position) {
            return this.Transform != Matrix.Identity ? Vector2.Transform(position, Matrix.Invert(this.Transform)) : position;
        }

        /// <summary>
        /// Transforms the given <paramref name="position"/> by this element's <see cref="Root"/>'s <see cref="RootElement.InvTransform"/>, the inverses of all of the <see cref="Transform"/> matrices of this element's parent tree (<see cref="GetParentTree"/>), and the inverse of this element's <see cref="Transform"/> matrix.
        /// Note that, when using <see cref="UiControls.GetElementUnderPos"/>, this operation is done recursively, which is more efficient.
        /// </summary>
        /// <param name="position">The position to transform</param>
        /// <returns>The transformed position</returns>
        protected Vector2 TransformInverseAll(Vector2 position) {
            position = Vector2.Transform(position, this.Root.InvTransform);
            foreach (var parent in this.GetParentTree().Reverse())
                position = parent.TransformInverse(position);
            return this.TransformInverse(position);
        }

        /// <summary>
        /// Called when this element is added to a <see cref="UiSystem"/> and, optionally, a given <see cref="RootElement"/>.
        /// This method is called in <see cref="AddChild{T}"/> for a parent whose <see cref="System"/> is set, as well as <see cref="UiSystem.Add"/>.
        /// </summary>
        /// <param name="system">The ui system to add to.</param>
        /// <param name="root">The root element to add to.</param>
        protected internal virtual void AddedToUi(UiSystem system, RootElement root) {
            this.Root = root;
            this.System = system;
            this.OnAddedToUi?.Invoke(this);
            root?.InvokeOnElementAdded(this);
        }

        /// <summary>
        /// Called when this element is removed from a <see cref="UiSystem"/> and <see cref="RootElement"/>.
        /// This method is called in <see cref="RemoveChild"/> for a parent whose <see cref="System"/> is set, as well as <see cref="UiSystem.Remove"/>.
        /// </summary>
        protected internal virtual void RemovedFromUi() {
            var root = this.Root;
            this.Root = null;
            this.System = null;
            this.OnRemovedFromUi?.Invoke(this);
            root?.InvokeOnElementRemoved(this);
        }

        void ILayoutItem.OnLayoutRecursion(int recursion, ILayoutItem relevantChild) {
            this.System.Metrics.SummedRecursionDepth++;
            if (recursion > this.System.Metrics.MaxRecursionDepth)
                this.System.Metrics.MaxRecursionDepth = recursion;

            if (recursion >= 64)
                throw new ArithmeticException($"The area of {this} has recursively updated too often. Does its child {relevantChild} contain any conflicting auto-sizing settings?");
        }

        Vector2 ILayoutItem.CalcActualSize(RectangleF parentArea) {
            return this.CalcActualSize(parentArea);
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
        /// A delegate used inside of <see cref="Element.Draw(Microsoft.Xna.Framework.GameTime,Microsoft.Xna.Framework.Graphics.SpriteBatch,float,MLEM.Graphics.SpriteBatchContext)"/>
        /// </summary>
        /// <param name="element">The element that is being drawn</param>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch used for drawing</param>
        /// <param name="alpha">The alpha this element is drawn with</param>
        /// <param name="context">The sprite batch context to use for drawing</param>
        public delegate void DrawCallback(Element element, GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context);

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

    }
}
