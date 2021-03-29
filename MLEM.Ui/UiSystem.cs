using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace MLEM.Ui {
    /// <summary>
    /// A ui system is the central location for the updating and rendering of all ui <see cref="Element"/>s.
    /// Each element added to the root of the ui system is assigned a <see cref="RootElement"/> that has additional data like a transformation matrix.
    /// For more information on how ui systems work, check out <see href="https://mlem.ellpeck.de/articles/ui.html"/>
    /// </summary>
    public class UiSystem : GameComponent {

        private readonly List<RootElement> rootElements = new List<RootElement>();

        /// <summary>
        /// The viewport that this ui system is rendering inside of.
        /// This is automatically updated during <see cref="GameWindow.ClientSizeChanged"/>
        /// </summary>
        public Rectangle Viewport;
        /// <summary>
        /// Set this field to true to cause the ui system and all of its elements to automatically scale up or down with greater and lower resolution, respectively.
        /// If this field is true, <see cref="AutoScaleReferenceSize"/> is used as the size that uses default <see cref="GlobalScale"/>
        /// </summary>
        public bool AutoScaleWithScreen;
        /// <summary>
        /// If <see cref="AutoScaleWithScreen"/> is true, this is used as the screen size that uses the default <see cref="GlobalScale"/>
        /// </summary>
        public Point AutoScaleReferenceSize;
        private float globalScale = 1;
        /// <summary>
        /// The global rendering scale of this ui system and all of its child elements.
        /// If <see cref="AutoScaleWithScreen"/> is true, this scale will be different based on the window size.
        /// </summary>
        public float GlobalScale {
            get {
                if (!this.AutoScaleWithScreen)
                    return this.globalScale;
                return Math.Min(this.Viewport.Width / (float) this.AutoScaleReferenceSize.X, this.Viewport.Height / (float) this.AutoScaleReferenceSize.Y) * this.globalScale;
            }
            set {
                this.globalScale = value;
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            }
        }

        private UiStyle style;
        /// <summary>
        /// The style options that this ui system and all of its elements use.
        /// To set the default, untextured style, use <see cref="UntexturedStyle"/>.
        /// </summary>
        public UiStyle Style {
            get => this.style;
            set {
                this.style = value;
                foreach (var root in this.rootElements) {
                    root.Element.AndChildren(e => e.System = this);
                    root.Element.ForceUpdateArea();
                }
            }
        }
        /// <summary>
        /// The transparency (alpha value) that this ui system and all of its elements draw at.
        /// </summary>
        public float DrawAlpha = 1;
        /// <summary>
        /// The blend state that this ui system and all of its elements draw with
        /// </summary>
        public BlendState BlendState;
        /// <summary>
        /// The sampler state that this ui system and all of its elements draw with.
        /// The default is <see cref="Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp"/>, as that is the one that works best with pixel graphics.
        /// </summary>
        public SamplerState SamplerState = SamplerState.PointClamp;
        /// <summary>
        /// The <see cref="TextFormatter"/> that this ui system's <see cref="Paragraph"/> elements format their text with.
        /// To add new formatting codes to the ui system, add them to this formatter.
        /// </summary>
        public TextFormatter TextFormatter;
        /// <summary>
        /// The action that should be executed when a <see cref="LinkCode"/> in a paragraph's <see cref="Paragraph.TokenizedText"/> is pressed.
        /// The actual link stored in the link code is stored in its <see cref="Code.Match"/>'s 1st group.
        /// By default, the browser is opened with the given link's address.
        /// </summary>
        public Action<LinkCode> LinkBehavior = l => Process.Start(new ProcessStartInfo(l.Match.Groups[1].Value) {UseShellExecute = true});
        /// <summary>
        /// The <see cref="UiControls"/> that this ui system is controlled by.
        /// The ui controls are also the place to change bindings for controller and keyboard input.
        /// </summary>
        public UiControls Controls;

        /// <summary>
        /// Event that is invoked after an <see cref="Element"/> is drawn, but before its children are drawn.
        /// </summary>
        public Element.DrawCallback OnElementDrawn = (e, time, batch, alpha) => e.OnDrawn?.Invoke(e, time, batch, alpha);
        /// <summary>
        /// Event that is invoked after the <see cref="RootElement.SelectedElement"/> for each root element is drawn, but before its children are drawn.
        /// </summary>
        public Element.DrawCallback OnSelectedElementDrawn;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is updated
        /// </summary>
        public Element.TimeCallback OnElementUpdated = (e, time) => e.OnUpdated?.Invoke(e, time);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is pressed with the primary action key
        /// </summary>
        public Element.GenericCallback OnElementPressed = e => e.OnPressed?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is pressed with the secondary action key
        /// </summary>
        public Element.GenericCallback OnElementSecondaryPressed = e => e.OnSecondaryPressed?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is newly selected using automatic navigation, or after it has been pressed with the mouse.
        /// </summary>
        public Element.GenericCallback OnElementSelected = e => e.OnSelected?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is deselected during the selection of a new element.
        /// </summary>
        public Element.GenericCallback OnElementDeselected = e => e.OnDeselected?.Invoke(e);
        /// <summary>
        /// Event that is invoked when the mouse enters an <see cref="Element"/>
        /// </summary>
        public Element.GenericCallback OnElementMouseEnter = e => e.OnMouseEnter?.Invoke(e);
        /// <summary>
        /// Event that is invoked when the mouse exits an <see cref="Element"/>
        /// </summary>
        public Element.GenericCallback OnElementMouseExit = e => e.OnMouseExit?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> starts being touched
        /// </summary>
        public Element.GenericCallback OnElementTouchEnter = e => e.OnTouchEnter?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> stops being touched
        /// </summary>
        public Element.GenericCallback OnElementTouchExit = e => e.OnTouchExit?.Invoke(e);
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/>'s display area changes
        /// </summary>
        public Element.GenericCallback OnElementAreaUpdated = e => e.OnAreaUpdated?.Invoke(e);
        /// <summary>
        /// Event that is invoked when the <see cref="Element"/> that the mouse is currently over changes
        /// </summary>
        public Element.GenericCallback OnMousedElementChanged;
        /// <summary>
        /// Event that is invoked when the <see cref="Element"/> that is being touched changes
        /// </summary>
        public Element.GenericCallback OnTouchedElementChanged;
        /// <summary>
        /// Event that is invoked when the selected <see cref="Element"/> changes, either through automatic navigation, or by pressing on an element with the mouse
        /// </summary>
        public Element.GenericCallback OnSelectedElementChanged;
        /// <summary>
        /// Event that is invoked when a new <see cref="RootElement"/> is added to this ui system
        /// </summary>
        public RootCallback OnRootAdded;
        /// <summary>
        /// Event that is invoked when a <see cref="RootElement"/> is removed from this ui system
        /// </summary>
        public RootCallback OnRootRemoved;

        /// <summary>
        /// Creates a new ui system with the given settings.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="style">The style settings that this ui should have. Use <see cref="UntexturedStyle"/> for the default, untextured style.</param>
        /// <param name="inputHandler">The input handler that this ui's <see cref="UiControls"/> should use. If none is supplied, a new input handler is created for this ui.</param>
        /// <param name="automaticViewport">If this value is set to true, the ui system's <see cref="Viewport"/> will be set automatically based on the <see cref="GameWindow"/>'s size. Defaults to true.</param>
        public UiSystem(Game game, UiStyle style, InputHandler inputHandler = null, bool automaticViewport = true) : base(game) {
            this.Controls = new UiControls(this, inputHandler);
            this.style = style;

            if (automaticViewport) {
                this.Viewport = new Rectangle(Point.Zero, game.Window.ClientBounds.Size);
                this.AutoScaleReferenceSize = this.Viewport.Size;
                game.Window.ClientSizeChanged += (sender, args) => {
                    this.Viewport = new Rectangle(Point.Zero, game.Window.ClientBounds.Size);
                    foreach (var root in this.rootElements)
                        root.Element.ForceUpdateArea();
                };
            }

            if (TextInputWrapper.Current != null)
                TextInputWrapper.Current.AddListener(game.Window, (sender, key, character) => this.ApplyToAll(e => e.OnTextInput?.Invoke(e, key, character)));
            this.OnMousedElementChanged = e => this.ApplyToAll(t => t.OnMousedElementChanged?.Invoke(t, e));
            this.OnTouchedElementChanged = e => this.ApplyToAll(t => t.OnTouchedElementChanged?.Invoke(t, e));
            this.OnSelectedElementChanged = e => this.ApplyToAll(t => t.OnSelectedElementChanged?.Invoke(t, e));
            this.OnSelectedElementDrawn = (element, time, batch, alpha) => {
                if (this.Controls.IsAutoNavMode && element.SelectionIndicator.HasValue()) {
                    batch.Draw(element.SelectionIndicator, element.DisplayArea, Color.White * alpha, element.Scale / 2);
                }
            };
            this.OnElementPressed += e => {
                if (e.OnPressed != null)
                    e.ActionSound.Value?.Play();
            };
            this.OnElementSecondaryPressed += e => {
                if (e.OnSecondaryPressed != null)
                    e.SecondActionSound.Value?.Play();
            };

            this.TextFormatter = new TextFormatter();
            this.TextFormatter.Codes.Add(new Regex("<l(?: ([^>]+))?>"), (f, m, r) => new LinkCode(m, r, 1 / 16F, 0.85F,
                t => this.Controls.MousedElement is Paragraph.Link l1 && l1.Token == t || this.Controls.TouchedElement is Paragraph.Link l2 && l2.Token == t));
        }

        /// <summary>
        /// Update this ui system, querying the necessary events and updating each element's data.
        /// </summary>
        /// <param name="time">The game's time</param>
        public override void Update(GameTime time) {
            this.Controls.Update();

            for (var i = this.rootElements.Count - 1; i >= 0; i--) {
                this.rootElements[i].Element.Update(time);
            }
        }

        /// <summary>
        /// Draws any <see cref="Panel"/> and other elements that draw onto <see cref="RenderTarget2D"/> rather than directly onto the screen.
        /// For drawing in this manner to work correctly, this method has to be called before your <see cref="GraphicsDevice"/> is cleared, and before everything else in your game is drawn.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        public void DrawEarly(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.DrawEarly(time, batch, this.DrawAlpha * root.Element.DrawAlpha, this.BlendState, this.SamplerState, root.Transform);
            }
        }

        /// <summary>
        /// Draws any <see cref="Element"/>s onto the screen.
        /// Note that, when using <see cref="Panel"/>s with a scroll bar, <see cref="DrawEarly"/> needs to be called as well.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        public void Draw(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (root.Element.IsHidden)
                    continue;
                batch.Begin(SpriteSortMode.Deferred, this.BlendState, this.SamplerState, null, null, null, root.Transform);
                var alpha = this.DrawAlpha * root.Element.DrawAlpha;
                root.Element.DrawTransformed(time, batch, alpha, this.BlendState, this.SamplerState, root.Transform);
                batch.End();
            }
        }

        /// <summary>
        /// Adds a new root element to this ui system and returns the newly created <see cref="RootElement"/>.
        /// Note that, when adding new elements that should be part of the same ui (like buttons on a panel), <see cref="Element.AddChild{T}"/> should be used.
        /// </summary>
        /// <param name="name">The name of the new root element</param>
        /// <param name="element">The root element to add</param>
        /// <returns>The newly created root element, or null if an element with the specified name already exists.</returns>
        public RootElement Add(string name, Element element) {
            if (this.IndexOf(name) >= 0)
                return null;
            var root = new RootElement(name, element, this);
            this.rootElements.Add(root);
            root.Element.AndChildren(e => {
                e.Root = root;
                e.System = this;
                root.OnElementAdded(e);
                e.SetAreaDirty();
            });
            this.OnRootAdded?.Invoke(root);
            this.SortRoots();
            return root;
        }

        /// <summary>
        /// Removes the <see cref="RootElement"/> with the specified name, or does nothing if there is no such element.
        /// </summary>
        /// <param name="name">The name of the root element to remove</param>
        public void Remove(string name) {
            var root = this.Get(name);
            if (root == null)
                return;
            this.rootElements.Remove(root);
            this.Controls.SelectElement(root, null);
            root.Element.AndChildren(e => {
                e.Root = null;
                e.System = null;
                root.OnElementRemoved(e);
                e.SetAreaDirty();
            });
            this.OnRootRemoved?.Invoke(root);
        }

        /// <summary>
        /// Finds the <see cref="RootElement"/> with the given name.
        /// </summary>
        /// <param name="name">The root element's name</param>
        /// <returns>The root element with the given name, or null if no such element exists</returns>
        public RootElement Get(string name) {
            var index = this.IndexOf(name);
            return index < 0 ? null : this.rootElements[index];
        }

        private int IndexOf(string name) {
            return this.rootElements.FindIndex(element => element.Name == name);
        }

        internal void SortRoots() {
            // Normal list sorting isn't stable, but ordering is
            var sorted = this.rootElements.OrderBy(root => root.Priority).ToArray();
            this.rootElements.Clear();
            this.rootElements.AddRange(sorted);
        }

        /// <summary>
        /// Returns an enumerable of all of the <see cref="RootElement"/> instances that this ui system holds.
        /// </summary>
        /// <returns>All of this ui system's root elements</returns>
        public IEnumerable<RootElement> GetRootElements() {
            for (var i = this.rootElements.Count - 1; i >= 0; i--)
                yield return this.rootElements[i];
        }

        /// <summary>
        /// Applies the given action to all <see cref="Element"/> instances in this ui system recursively.
        /// Note that, when this method is invoked, all root elements and all of their children receive the <see cref="Action"/>.
        /// </summary>
        /// <param name="action">The action to execute on each element</param>
        public void ApplyToAll(Action<Element> action) {
            foreach (var root in this.rootElements)
                root.Element.AndChildren(action);
        }

        /// <summary>
        /// A delegate used for callbacks that involve a <see cref="RootElement"/>
        /// </summary>
        /// <param name="root">The root element</param>
        public delegate void RootCallback(RootElement root);

    }

    /// <summary>
    /// A root element is a wrapper around an <see cref="Element"/> that contains additional data.
    /// Root elements are only used for the element in each element tree that doesn't have a <see cref="MLEM.Ui.Elements.Element.Parent"/>
    /// To create a new root element, use <see cref="UiSystem.Add"/>
    /// </summary>
    public class RootElement {

        /// <summary>
        /// The name of this root element
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The element that this root element represents.
        /// This is the only element in its family tree that does not have a <see cref="MLEM.Ui.Elements.Element.Parent"/>.
        /// </summary>
        public readonly Element Element;
        /// <summary>
        /// The <see cref="UiSystem"/> that this root element is a part of.
        /// </summary>
        public readonly UiSystem System;
        private float scale = 1;
        /// <summary>
        /// The scale of this root element.
        /// Note that, to change the scale of every root element, you can use <see cref="UiSystem.GlobalScale"/>
        /// </summary>
        public float Scale {
            get => this.scale;
            set {
                if (this.scale == value)
                    return;
                this.scale = value;
                this.Element.ForceUpdateArea();
            }
        }
        private int priority;
        /// <summary>
        /// The priority of this root element.
        /// A higher priority means the element will be updated first, as well as rendered on top.
        /// </summary>
        public int Priority {
            get => this.priority;
            set {
                this.priority = value;
                this.System.SortRoots();
            }
        }
        /// <summary>
        /// The actual scale of this root element.
        /// This is a combination of this root element's <see cref="Scale"/> as well as the ui system's <see cref="UiSystem.GlobalScale"/>.
        /// </summary>
        public float ActualScale => this.System.GlobalScale * this.Scale;

        /// <summary>
        /// The transformation that this root element (and all of its children) has.
        /// This transform is applied both to input, as well as to rendering.
        /// </summary>
        public Matrix Transform = Matrix.Identity;
        /// <summary>
        /// An inversion of <see cref="Transform"/>
        /// </summary>
        public Matrix InvTransform => Matrix.Invert(this.Transform);

        /// <summary>
        /// The child element of this root element that is currently selected.
        /// If there is no selected element in this root, this value will be <c>null</c>.
        /// </summary>
        public Element SelectedElement => this.System.Controls.GetSelectedElement(this);
        /// <summary>
        /// Determines whether this root element contains any children that <see cref="Elements.Element.CanBeSelected"/>.
        /// This value is automatically calculated.
        /// </summary>
        public bool CanSelectContent { get; private set; }

        /// <summary>
        /// Event that is invoked when a <see cref="Element"/> is added to this root element or any of its children.
        /// </summary>
        public Element.GenericCallback OnElementAdded;
        /// <summary>
        /// Even that is invoked when a <see cref="Element"/> is removed rom this root element of any of its children.
        /// </summary>
        public Element.GenericCallback OnElementRemoved;

        internal RootElement(string name, Element element, UiSystem system) {
            this.Name = name;
            this.Element = element;
            this.System = system;

            this.OnElementAdded += e => {
                if (e.CanBeSelected)
                    this.CanSelectContent = true;
            };
            this.OnElementRemoved += e => {
                if (e.CanBeSelected && !this.Element.GetChildren(regardGrandchildren: true).Any(c => c.CanBeSelected))
                    this.CanSelectContent = false;
            };
        }

        /// <summary>
        /// Selects the given element that is a child of this root element.
        /// Optionally, automatic navigation can be forced on, causing the <see cref="UiStyle.SelectionIndicator"/> to be drawn around the element.
        /// </summary>
        /// <param name="element">The element to select, or null to deselect the selected element.</param>
        /// <param name="autoNav">Whether automatic navigation should be forced on</param>
        public void SelectElement(Element element, bool? autoNav = null) {
            this.System.Controls.SelectElement(this, element, autoNav);
        }

        /// <summary>
        /// Scales this root element's <see cref="Transform"/> matrix based on the given scale and origin.
        /// </summary>
        /// <param name="scale">The scale to use</param>
        /// <param name="origin">The origin to use for scaling, or null to use this root's element's center point</param>
        public void ScaleOrigin(float scale, Vector2? origin = null) {
            this.Transform = Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(new Vector3((1 - scale) * (origin ?? this.Element.DisplayArea.Center), 0));
        }

    }
}