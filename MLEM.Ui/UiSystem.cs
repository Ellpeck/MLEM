using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Graphics;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace MLEM.Ui {
    /// <summary>
    /// A ui system is the central location for the updating and rendering of all ui <see cref="Element"/>s.
    /// Each element added to the root of the ui system is assigned a <see cref="RootElement"/> that has additional data like a transformation matrix.
    /// For more information on how ui systems work, check out https://mlem.ellpeck.de/articles/ui.html.
    /// </summary>
    public class UiSystem : GameComponent {

        /// <summary>
        /// The viewport that this ui system is rendering inside of.
        /// This is automatically updated during <see cref="GameWindow.ClientSizeChanged"/> by default.
        /// </summary>
        public Rectangle Viewport {
            get => this.viewport;
            set {
                this.viewport = value;
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            }
        }
        /// <summary>
        /// Set this field to true to cause the ui system and all of its elements to automatically scale up or down with greater and lower resolution, respectively.
        /// If this field is true, <see cref="AutoScaleReferenceSize"/> is used as the size that uses default <see cref="GlobalScale"/>
        /// </summary>
        public bool AutoScaleWithScreen;
        /// <summary>
        /// If <see cref="AutoScaleWithScreen"/> is true, this is used as the screen size that uses the default <see cref="GlobalScale"/>
        /// </summary>
        public Point AutoScaleReferenceSize;
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
        /// <summary>
        /// The style options that this ui system and all of its elements use.
        /// To set the default, untextured style, use <see cref="UntexturedStyle"/>.
        /// </summary>
        public UiStyle Style {
            get => this.style;
            set {
                this.style = value;
                foreach (var root in this.rootElements)
                    root.Element.AndChildren(e => e.Style = e.Style.OrStyle(value));
            }
        }
        /// <summary>
        /// The transparency (alpha value) that this ui system and all of its elements draw at.
        /// </summary>
        public float DrawAlpha = 1;
        /// <summary>
        /// The spriteb atch context that this ui system and all of its elements should draw with.
        /// The default <see cref="MLEM.Graphics.SpriteBatchContext.SamplerState"/> is <see cref="Microsoft.Xna.Framework.Graphics.SamplerState.PointClamp"/>, as that is the one that works best with pixel graphics.
        /// </summary>
        public SpriteBatchContext SpriteBatchContext = new SpriteBatchContext(samplerState: SamplerState.PointClamp);
        /// <summary>
        /// The <see cref="TextFormatter"/> that this ui system's <see cref="Paragraph"/> elements format their text with.
        /// To add new formatting codes to the ui system, add them to this formatter.
        /// </summary>
        public TextFormatter TextFormatter;
        /// <summary>
        /// The <see cref="UiControls"/> that this ui system is controlled by.
        /// The ui controls are also the place to change bindings for controller and keyboard input.
        /// </summary>
        public UiControls Controls;
        /// <summary>
        /// The update and rendering statistics to be used for runtime debugging and profiling.
        /// The metrics are reset accordingly every frame: <see cref="UiMetrics.ResetUpdates"/> is called at the start of <see cref="Update"/>, and <see cref="UiMetrics.ResetDraws"/> is called at the start of <see cref="Draw"/>.
        /// </summary>
        public UiMetrics Metrics;

        /// <summary>
        /// Event that is invoked after an <see cref="Element"/> is drawn, but before its children are drawn.
        /// </summary>
        public event Element.DrawCallback OnElementDrawn;
        /// <summary>
        /// Event that is invoked after the <see cref="RootElement.SelectedElement"/> for each root element is drawn, but before its children are drawn.
        /// </summary>
        public event Element.DrawCallback OnSelectedElementDrawn;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is updated
        /// </summary>
        public event Element.TimeCallback OnElementUpdated;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is pressed with the primary action key
        /// </summary>
        public event Element.GenericCallback OnElementPressed;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is pressed with the secondary action key
        /// </summary>
        public event Element.GenericCallback OnElementSecondaryPressed;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is newly selected using automatic navigation, or after it has been pressed with the mouse.
        /// </summary>
        public event Element.GenericCallback OnElementSelected;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> is deselected during the selection of a new element.
        /// </summary>
        public event Element.GenericCallback OnElementDeselected;
        /// <summary>
        /// Event that is invoked when the mouse enters an <see cref="Element"/>
        /// </summary>
        public event Element.GenericCallback OnElementMouseEnter;
        /// <summary>
        /// Event that is invoked when the mouse exits an <see cref="Element"/>
        /// </summary>
        public event Element.GenericCallback OnElementMouseExit;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> starts being touched
        /// </summary>
        public event Element.GenericCallback OnElementTouchEnter;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/> stops being touched
        /// </summary>
        public event Element.GenericCallback OnElementTouchExit;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/>'s display area is marked dirty
        /// </summary>
        public event Element.GenericCallback OnElementAreaDirty;
        /// <summary>
        /// Event that is invoked when an <see cref="Element"/>'s display area changes
        /// </summary>
        public event Element.GenericCallback OnElementAreaUpdated;
        /// <summary>
        /// Event that is called when an <see cref="Element"/>'s <see cref="Element.InitStyle"/> method is called while setting its <see cref="Element.Style"/>.
        /// </summary>
        public event Element.GenericCallback OnElementStyleInit;
        /// <summary>
        /// Event that is invoked when the <see cref="Element"/> that the mouse is currently over changes
        /// </summary>
        public event Element.GenericCallback OnMousedElementChanged;
        /// <summary>
        /// Event that is invoked when the <see cref="Element"/> that is being touched changes
        /// </summary>
        public event Element.GenericCallback OnTouchedElementChanged;
        /// <summary>
        /// Event that is invoked when the selected <see cref="Element"/> changes, either through automatic navigation, or by pressing on an element with the mouse
        /// </summary>
        public event Element.GenericCallback OnSelectedElementChanged;
        /// <summary>
        /// Event that is invoked when a new <see cref="RootElement"/> is added to this ui system
        /// </summary>
        public event RootCallback OnRootAdded;
        /// <summary>
        /// Event that is invoked when a <see cref="RootElement"/> is removed from this ui system
        /// </summary>
        public event RootCallback OnRootRemoved;

        private readonly Queue<(Action<UiSystem, object> Action, object ActionObject)> nextUpdateActions = new Queue<(Action<UiSystem, object> Action, object ActionObject)>();
        private readonly object nextUpdateActionsLock = new object();
        private readonly List<RootElement> rootElements = new List<RootElement>();
        private readonly Stopwatch stopwatch = new Stopwatch();
        private float globalScale = 1;
        private bool drewEarly;
        private UiStyle style;
        private Rectangle viewport;

        /// <summary>
        /// Creates a new ui system with the given settings.
        /// </summary>
        /// <param name="game">The game</param>
        /// <param name="style">The style settings that this ui should have. Use <see cref="UntexturedStyle"/> for the default, untextured style.</param>
        /// <param name="inputHandler">The input handler that this ui's <see cref="UiControls"/> should use. If none is supplied, a new input handler is created for this ui.</param>
        /// <param name="automaticViewport">If this value is set to true, the ui system's <see cref="Viewport"/> will be set automatically based on the <see cref="GameWindow"/>'s size. Defaults to true.</param>
        /// <param name="hasFontModifierFormatting">Whether default font modifier codes should be added to this ui system's <see cref="TextFormatter"/>, including bold, italic, strikethrough, shadow, subscript, and more.</param>
        /// <param name="hasColorFormatting">Whether default color codes should be added to this ui system's <see cref="TextFormatter"/>, including all <see cref="Color"/> values and the ability to use custom colors.</param>
        /// <param name="hasAnimationFormatting">Whether default animation codes should be added to this ui system's <see cref="TextFormatter"/>, namely the wobbly animation.</param>
        /// <param name="hasMacroFormatting">Whether default macros should be added to this ui system's <see cref="TextFormatter"/>, including TeX's ~ non-breaking space and more.</param>
        /// <param name="hasUiFormatting">Whether <see cref="UiSystem"/>-based formatting codes should be added to this ui system's <see cref="TextFormatter"/>, including <see cref="Paragraph.Link"/> codes and font switching.</param>
        public UiSystem(Game game, UiStyle style, InputHandler inputHandler = null, bool automaticViewport = true, bool hasFontModifierFormatting = true, bool hasColorFormatting = true, bool hasAnimationFormatting = true, bool hasMacroFormatting = true, bool hasUiFormatting = true) : base(game) {
            this.Controls = new UiControls(this, inputHandler);
            this.style = style;

            this.OnElementDrawn += (e, time, batch, alpha, context) => e.OnDrawn?.Invoke(e, time, batch, alpha, context);
            this.OnElementUpdated += (e, time) => e.OnUpdated?.Invoke(e, time);
            this.OnElementPressed += e => e.OnPressed?.Invoke(e);
            this.OnElementSecondaryPressed += e => e.OnSecondaryPressed?.Invoke(e);
            this.OnElementSelected += e => e.OnSelected?.Invoke(e);
            this.OnElementDeselected += e => e.OnDeselected?.Invoke(e);
            this.OnElementMouseEnter += e => e.OnMouseEnter?.Invoke(e);
            this.OnElementMouseExit += e => e.OnMouseExit?.Invoke(e);
            this.OnElementTouchEnter += e => e.OnTouchEnter?.Invoke(e);
            this.OnElementTouchExit += e => e.OnTouchExit?.Invoke(e);
            this.OnElementAreaDirty += e => e.OnAreaDirty?.Invoke(e);
            this.OnElementAreaUpdated += e => e.OnAreaUpdated?.Invoke(e);
            this.OnElementStyleInit += e => e.OnStyleInit?.Invoke(e);
            this.OnMousedElementChanged += e => this.ApplyToAll(t => t.OnMousedElementChanged?.Invoke(t, e));
            this.OnTouchedElementChanged += e => this.ApplyToAll(t => t.OnTouchedElementChanged?.Invoke(t, e));
            this.OnSelectedElementChanged += e => this.ApplyToAll(t => t.OnSelectedElementChanged?.Invoke(t, e));
            this.OnSelectedElementDrawn += (element, time, batch, alpha, context) => {
                if (this.Controls.IsAutoNavMode && element.SelectionIndicator.HasValue())
                    batch.Draw(element.SelectionIndicator, element.DisplayArea, element.SelectionColor.OrDefault(Color.White) * alpha, element.Scale / 2);
            };
            this.OnElementPressed += e => {
                if (e.OnPressed != null)
                    e.ActionSound.Value?.Play();
            };
            this.OnElementSecondaryPressed += e => {
                if (e.OnSecondaryPressed != null)
                    e.SecondActionSound.Value?.Play();
            };

            MlemPlatform.Current?.AddTextInputListener(game.Window, (sender, key, character) => this.ApplyToAll(e => e.OnTextInput?.Invoke(e, key, character)));

            if (automaticViewport) {
                this.Viewport = new Rectangle(0, 0, game.Window.ClientBounds.Width, game.Window.ClientBounds.Height);
                this.AutoScaleReferenceSize = new Point(this.Viewport.Width, this.Viewport.Height);
                game.Window.ClientSizeChanged += (sender, args) => {
                    this.Viewport = new Rectangle(0, 0, game.Window.ClientBounds.Width, game.Window.ClientBounds.Height);
                };
            }

            this.TextFormatter = new TextFormatter(hasFontModifierFormatting, hasColorFormatting, hasAnimationFormatting, hasMacroFormatting);
            if (hasUiFormatting) {
                this.TextFormatter.Codes.Add(new Regex("<l(?: (.+?))?>"), (f, m, r) => new LinkCode(m, r, 1 / 16F, 0.85F,
                    t => this.Controls.MousedElement is Paragraph.Link l1 && l1.Token == t || this.Controls.TouchedElement is Paragraph.Link l2 && l2.Token == t));
                this.TextFormatter.Codes.Add(new Regex("<f (.+?)>"), (_, m, r) => new FontCode(m, r,
                    f => this.Style.AdditionalFonts != null && this.Style.AdditionalFonts.TryGetValue(m.Groups[1].Value, out var c) ? c : f));
            }
        }

        /// <summary>
        /// Update this ui system, querying the necessary events and updating each element's data.
        /// </summary>
        /// <param name="time">The game's time</param>
        public override void Update(GameTime time) {
            this.Metrics.ResetUpdates();
            this.stopwatch.Restart();

            this.Controls.Update();
            lock (this.nextUpdateActionsLock) {
                while (this.nextUpdateActions.Count > 0) {
                    var next = this.nextUpdateActions.Dequeue();
                    next.Action.Invoke(this, next.ActionObject);
                }
            }
            for (var i = this.rootElements.Count - 1; i >= 0; i--)
                this.rootElements[i].Element.Update(time);

            this.stopwatch.Stop();
            this.Metrics.UpdateTime += this.stopwatch.Elapsed;
        }

        /// <summary>
        /// Draws any <see cref="Element"/>s onto the screen.
        /// </summary>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch to use for drawing</param>
        public void Draw(GameTime time, SpriteBatch batch) {
            if (!this.drewEarly)
                this.Metrics.ResetDraws();
            this.stopwatch.Restart();

            foreach (var root in this.rootElements) {
                if (root.Element.IsHidden)
                    continue;
                var context = this.SpriteBatchContext;
                context.TransformMatrix = root.Transform * context.TransformMatrix;

                batch.Begin(context);
                root.Element.DrawTransformed(time, batch, this.DrawAlpha * root.Element.DrawAlpha, context);
                batch.End();
            }

            this.stopwatch.Stop();
            this.Metrics.DrawTime += this.stopwatch.Elapsed;
            this.drewEarly = false;
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
                e.AddedToUi(this, root);
                e.SetAreaDirty();
            });
            this.OnRootAdded?.Invoke(root);
            root.InvokeOnAddedToUi(this);
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
                e.RemovedFromUi();
                e.SetAreaDirty();
            });
            this.OnRootRemoved?.Invoke(root);
            root.InvokeOnRemovedFromUi(this);
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

        /// <summary>
        /// Enqueues the given <paramref name="action"/> (optionally with the given <paramref name="actionObject"/> attached) to be executed the next time <see cref="Update"/> is called, before all of this ui system's elements are updated.
        /// This method is thread-safe, meaning actions can be enqueued from other threads (for example, after downloading content to display) and they will be executed on the main thread.
        /// </summary>
        /// <param name="action">The action to enqueue.</param>
        /// <param name="actionObject">An optional object to attach to the action, which will be passed as the second parameter to <paramref name="action"/>.</param>
        public void EnqueueAction(Action<UiSystem, object> action, object actionObject = null) {
            lock (this.nextUpdateActionsLock)
                this.nextUpdateActions.Enqueue((action, actionObject));
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

        /// <inheritdoc />
        protected override void Dispose(bool disposing) {
            if (disposing) {
                while (this.rootElements.Count > 0)
                    this.Remove(this.rootElements[0].Name);
            }
        }

        internal void InvokeOnElementDrawn(Element element, GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            this.OnElementDrawn?.Invoke(element, time, batch, alpha, context);
        }

        internal void InvokeOnSelectedElementDrawn(Element element, GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            this.OnSelectedElementDrawn?.Invoke(element, time, batch, alpha, context);
        }

        internal void InvokeOnElementUpdated(Element element, GameTime time) {
            this.OnElementUpdated?.Invoke(element, time);
        }

        internal void InvokeOnElementAreaDirty(Element element) {
            this.OnElementAreaDirty?.Invoke(element);
        }

        internal void InvokeOnElementAreaUpdated(Element element) {
            this.OnElementAreaUpdated?.Invoke(element);
        }

        internal void InvokeOnElementStyleInit(Element element) {
            this.OnElementStyleInit?.Invoke(element);
        }

        internal void InvokeOnElementPressed(Element element) {
            this.OnElementPressed?.Invoke(element);
        }

        internal void InvokeOnElementSecondaryPressed(Element element) {
            this.OnElementSecondaryPressed?.Invoke(element);
        }

        internal void InvokeOnElementSelected(Element element) {
            this.OnElementSelected?.Invoke(element);
        }

        internal void InvokeOnElementDeselected(Element element) {
            this.OnElementDeselected?.Invoke(element);
        }

        internal void InvokeOnSelectedElementChanged(Element element) {
            this.OnSelectedElementChanged?.Invoke(element);
        }

        internal void InvokeOnElementMouseExit(Element element) {
            this.OnElementMouseExit?.Invoke(element);
        }

        internal void InvokeOnElementMouseEnter(Element element) {
            this.OnElementMouseEnter?.Invoke(element);
        }

        internal void InvokeOnMousedElementChanged(Element element) {
            this.OnMousedElementChanged?.Invoke(element);
        }

        internal void InvokeOnElementTouchExit(Element element) {
            this.OnElementTouchExit?.Invoke(element);
        }

        internal void InvokeOnElementTouchEnter(Element element) {
            this.OnElementTouchEnter?.Invoke(element);
        }

        internal void InvokeOnTouchedElementChanged(Element element) {
            this.OnTouchedElementChanged?.Invoke(element);
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
    public class RootElement : GenericDataHolder {

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
        /// This value is automatically calculated, and used in <see cref="CanBeActive"/>.
        /// </summary>
        public bool CanSelectContent => this.Element.CanBeSelected || this.Element.GetChildren(c => c.CanBeSelected, true).Any();
        /// <summary>
        /// Determines whether this root element can become the <see cref="UiControls.ActiveRoot"/>.
        /// This property returns <see langword="true"/> if <see cref="CanSelectContent"/> is <see langword="true"/> and the <see cref="Element"/> is not hidden, or if it has been set to <see langword="true"/> manually.
        /// </summary>
        public bool CanBeActive {
            get => this.canBeActive || !this.Element.IsHidden && this.CanSelectContent;
            set => this.canBeActive = value;
        }

        /// <summary>
        /// Event that is invoked when a <see cref="Element"/> is added to this root element or any of its children.
        /// </summary>
        public event Element.GenericCallback OnElementAdded;
        /// <summary>
        /// Event that is invoked when a <see cref="Element"/> is removed rom this root element or any of its children.
        /// </summary>
        public event Element.GenericCallback OnElementRemoved;
        /// <summary>
        /// Event that is invoked when this <see cref="RootElement"/> gets added to a <see cref="UiSystem"/> in <see cref="UiSystem.Add"/>
        /// </summary>
        [Obsolete("This event is only invoked once, after internal RootElement construction, and has very little practical use. Use UiSystem.OnRootAdded instead.")]
        public event Action<UiSystem> OnAddedToUi;
        /// <summary>
        /// Event that is invoked when this <see cref="RootElement"/> gets removed from a <see cref="UiSystem"/> in <see cref="UiSystem.Remove"/>
        /// </summary>
        public event Action<UiSystem> OnRemovedFromUi;

        private float scale = 1;
        private bool canBeActive;
        private int priority;

        internal RootElement(string name, Element element, UiSystem system) {
            this.Name = name;
            this.Element = element;
            this.System = system;
        }

        /// <summary>
        /// Selects the given element that is a child of this root element.
        /// Optionally, automatic navigation can be forced on, causing the <see cref="UiStyle.SelectionIndicator"/> to be drawn around the element.
        /// </summary>
        /// <param name="element">The element to select, or null to deselect the selected element.</param>
        /// <param name="autoNav">Whether automatic navigation should be forced on. If this is <see langword="null"/>, the automatic navigation state will stay the same.</param>
        /// <param name="navType">An optional <see cref="UiControls.NavigationType"/> to set. If this is <see langword="null"/>, the navigation type will stay the same.</param>
        public void SelectElement(Element element, bool? autoNav = null, UiControls.NavigationType? navType = null) {
            this.System.Controls.SelectElement(this, element, autoNav, navType);
        }

        /// <summary>
        /// Scales this root element's <see cref="Transform"/> matrix based on the given scale and origin.
        /// </summary>
        /// <param name="scale">The scale to use</param>
        /// <param name="origin">The origin to use for scaling, or null to use this root's element's center point</param>
        public void ScaleOrigin(float scale, Vector2? origin = null) {
            this.Transform = Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(new Vector3((1 - scale) * (origin ?? this.Element.DisplayArea.Center), 0));
        }

        internal void InvokeOnElementAdded(Element element) {
            this.OnElementAdded?.Invoke(element);
        }

        internal void InvokeOnElementRemoved(Element element) {
            this.OnElementRemoved?.Invoke(element);
        }

        internal void InvokeOnAddedToUi(UiSystem system) {
            this.OnAddedToUi?.Invoke(system);
        }

        internal void InvokeOnRemovedFromUi(UiSystem system) {
            this.OnRemovedFromUi?.Invoke(system);
        }

    }
}
