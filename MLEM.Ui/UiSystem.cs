using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace MLEM.Ui {
    public class UiSystem : GameComponent {

        public readonly GraphicsDevice GraphicsDevice;
        public readonly GameWindow Window;
        private readonly List<RootElement> rootElements = new List<RootElement>();

        public Rectangle Viewport { get; private set; }
        public bool AutoScaleWithScreen;
        public Point AutoScaleReferenceSize;
        private float globalScale = 1;
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
        public UiStyle Style {
            get => this.style;
            set {
                this.style = value;
                foreach (var root in this.rootElements) {
                    root.Element.AndChildren(e => e.System = this);
                    root.Element.SetAreaDirty();
                }
            }
        }
        public float DrawAlpha = 1;
        public BlendState BlendState;
        public SamplerState SamplerState = SamplerState.PointClamp;
        public UiControls Controls;

        public Element.DrawCallback OnElementDrawn = (e, time, batch, alpha) => e.OnDrawn?.Invoke(e, time, batch, alpha);
        public Element.DrawCallback OnSelectedElementDrawn;
        public Element.TimeCallback OnElementUpdated = (e, time) => e.OnUpdated?.Invoke(e, time);
        public Element.GenericCallback OnElementPressed = e => e.OnPressed?.Invoke(e);
        public Element.GenericCallback OnElementSecondaryPressed = e => e.OnSecondaryPressed?.Invoke(e);
        public Element.GenericCallback OnElementSelected = e => e.OnSelected?.Invoke(e);
        public Element.GenericCallback OnElementDeselected = e => e.OnDeselected?.Invoke(e);
        public Element.GenericCallback OnElementMouseEnter = e => e.OnMouseEnter?.Invoke(e);
        public Element.GenericCallback OnElementMouseExit = e => e.OnMouseExit?.Invoke(e);
        public Element.GenericCallback OnElementAreaUpdated = e => e.OnAreaUpdated?.Invoke(e);
        public Element.GenericCallback OnMousedElementChanged;
        public Element.GenericCallback OnSelectedElementChanged;
        public RootCallback OnRootAdded;
        public RootCallback OnRootRemoved;

        public UiSystem(GameWindow window, GraphicsDevice device, UiStyle style, InputHandler inputHandler = null) : base(null) {
            this.Controls = new UiControls(this, inputHandler);
            this.GraphicsDevice = device;
            this.Window = window;
            this.style = style;
            this.Viewport = device.Viewport.Bounds;
            this.AutoScaleReferenceSize = this.Viewport.Size;

            window.ClientSizeChanged += (sender, args) => {
                this.Viewport = device.Viewport.Bounds;
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            };

            window.AddTextInputListener((sender, key, character) => this.ApplyToAll(e => e.OnTextInput?.Invoke(e, key, character)));
            this.OnMousedElementChanged = e => this.ApplyToAll(t => t.OnMousedElementChanged?.Invoke(t, e));
            this.OnSelectedElementChanged = e => this.ApplyToAll(t => t.OnSelectedElementChanged?.Invoke(t, e));
            this.OnSelectedElementDrawn = (element, time, batch, alpha) => {
                if (this.Controls.IsAutoNavMode && element.SelectionIndicator.HasValue()) {
                    batch.Draw(element.SelectionIndicator, element.DisplayArea, Color.White * alpha, element.Scale / 2);
                }
            };
            this.OnElementPressed += e => {
                if (e.OnPressed != null)
                    e.ActionSound.Value?.Replay();
            };
            this.OnElementSecondaryPressed += e => {
                if (e.OnSecondaryPressed != null)
                    e.SecondActionSound.Value?.Replay();
            };
        }

        public override void Update(GameTime time) {
            this.Controls.Update();

            for (var i = this.rootElements.Count - 1; i >= 0; i--) {
                this.rootElements[i].Element.Update(time);
            }
        }

        public void DrawEarly(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.DrawEarly(time, batch, this.DrawAlpha * root.Element.DrawAlpha, this.BlendState, this.SamplerState, root.Transform);
            }
        }

        public void Draw(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (root.Element.IsHidden)
                    continue;
                batch.Begin(SpriteSortMode.Deferred, this.BlendState, this.SamplerState, null, null, null, root.Transform);
                var alpha = this.DrawAlpha * root.Element.DrawAlpha;
                root.Element.Draw(time, batch, alpha, this.BlendState, this.SamplerState, root.Transform);
                if (root.SelectedElement != null)
                    this.OnSelectedElementDrawn?.Invoke(root.SelectedElement, time, batch, alpha);
                batch.End();
            }
        }

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

        public void Remove(string name) {
            var root = this.Get(name);
            if (root == null)
                return;
            this.rootElements.Remove(root);
            root.SelectElement(null);
            root.Element.AndChildren(e => {
                e.Root = null;
                e.System = null;
                root.OnElementRemoved(e);
                e.SetAreaDirty();
            });
            this.OnRootRemoved?.Invoke(root);
        }

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

        public IEnumerable<RootElement> GetRootElements() {
            for (var i = this.rootElements.Count - 1; i >= 0; i--)
                yield return this.rootElements[i];
        }

        public void ApplyToAll(Action<Element> action) {
            foreach (var root in this.rootElements)
                root.Element.AndChildren(action);
        }

        public delegate void RootCallback(RootElement root);

    }

    public class RootElement {

        public readonly string Name;
        public readonly Element Element;
        public readonly UiSystem System;
        private float scale = 1;
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
        public int Priority {
            get => this.priority;
            set {
                this.priority = value;
                this.System.SortRoots();
            }
        }
        public float ActualScale => this.System.GlobalScale * this.Scale;

        public Matrix Transform = Matrix.Identity;
        public Matrix InvTransform => Matrix.Invert(this.Transform);

        public Element SelectedElement { get; private set; }
        public bool CanSelectContent { get; private set; }

        public Element.GenericCallback OnElementAdded;
        public Element.GenericCallback OnElementRemoved;

        public RootElement(string name, Element element, UiSystem system) {
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

        public void SelectElement(Element element) {
            if (this.SelectedElement == element)
                return;

            if (this.SelectedElement != null)
                this.System.OnElementDeselected?.Invoke(this.SelectedElement);
            if (element != null)
                this.System.OnElementSelected?.Invoke(element);
            this.SelectedElement = element;
            this.System.OnSelectedElementChanged?.Invoke(element);
        }

    }
}