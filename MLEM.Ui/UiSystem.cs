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
    public class UiSystem {

        public readonly GraphicsDevice GraphicsDevice;
        public readonly GameWindow Window;
        public Rectangle Viewport { get; private set; }
        private readonly List<RootElement> rootElements = new List<RootElement>();

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
                    root.Element.Propagate(e => e.System = this);
                    root.Element.SetAreaDirty();
                }
            }
        }
        public float DrawAlpha = 1;
        public BlendState BlendState;
        public SamplerState SamplerState = SamplerState.PointClamp;
        public UiControls Controls;

        public Element.DrawCallback OnElementDrawn;
        public Element.DrawCallback OnSelectedElementDrawn;

        public UiSystem(GameWindow window, GraphicsDevice device, UiStyle style, InputHandler inputHandler = null) {
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

            window.AddTextInputListener((sender, key, character) => {
                foreach (var root in this.rootElements)
                    root.Element.Propagate(e => e.OnTextInput?.Invoke(e, key, character));
            });

            this.OnSelectedElementDrawn = (element, time, batch, alpha, offset) => {
                if (!this.Controls.SelectedLastElementWithMouse && element.SelectionIndicator != null) {
                    batch.Draw(element.SelectionIndicator, element.DisplayArea.OffsetCopy(offset), Color.White * alpha);
                }
            };
        }

        public void Update(GameTime time) {
            this.Controls.Update();

            foreach (var root in this.rootElements)
                root.Element.Update(time);
        }

        public void DrawEarly(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.DrawEarly(time, batch, this.DrawAlpha * root.Element.DrawAlpha, this.BlendState, this.SamplerState);
            }
        }

        public void Draw(GameTime time, SpriteBatch batch) {
            foreach (var root in this.rootElements) {
                if (root.Element.IsHidden)
                    continue;
                batch.Begin(SpriteSortMode.Deferred, this.BlendState, this.SamplerState);
                root.Element.Draw(time, batch, this.DrawAlpha * root.Element.DrawAlpha, Point.Zero);
                batch.End();
            }
        }

        public RootElement Add(string name, Element element) {
            var root = new RootElement(name, element, this);
            return !this.Add(root) ? null : root;
        }

        internal bool Add(RootElement root, int index = -1) {
            if (this.IndexOf(root.Name) >= 0)
                return false;
            if (index < 0 || index > this.rootElements.Count)
                index = this.rootElements.Count;
            this.rootElements.Insert(index, root);
            root.Element.Propagate(e => {
                e.Root = root;
                e.System = this;
            });
            return true;
        }

        public void Remove(string name) {
            var root = this.Get(name);
            if (root == null)
                return;
            this.rootElements.Remove(root);
            root.Element.Propagate(e => {
                e.Root = null;
                e.System = null;
            });
        }

        public RootElement Get(string name) {
            var index = this.IndexOf(name);
            return index < 0 ? null : this.rootElements[index];
        }

        private int IndexOf(string name) {
            return this.rootElements.FindIndex(element => element.Name == name);
        }

        public IEnumerable<RootElement> GetRootElements() {
            for (var i = this.rootElements.Count - 1; i >= 0; i--)
                yield return this.rootElements[i];
        }

        internal void Propagate(Action<Element> action) {
            foreach (var root in this.rootElements)
                root.Element.Propagate(action);
        }

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
        public float ActualScale => this.System.GlobalScale * this.Scale;
        public bool CanSelectContent = true;

        public RootElement(string name, Element element, UiSystem system) {
            this.Name = name;
            this.Element = element;
            this.System = system;
        }

        public void MoveToFront() {
            this.System.Remove(this.Name);
            this.System.Add(this);
        }

        public void MoveToBack() {
            this.System.Remove(this.Name);
            this.System.Add(this, 0);
        }

    }
}