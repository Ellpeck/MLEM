using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace MLEM.Ui {
    public class UiSystem {

        public readonly GraphicsDevice GraphicsDevice;
        public Rectangle Viewport { get; private set; }
        private readonly List<RootElement> rootElements = new List<RootElement>();
        public readonly InputHandler InputHandler;
        private readonly bool isInputOurs;

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
        public Element MousedElement { get; private set; }
        public Element SelectedElement { get; private set; }
        private UiStyle style;
        public UiStyle Style {
            get => this.style;
            set {
                this.style = value;
                foreach (var root in this.rootElements) {
                    root.Element.PropagateUiSystem(this);
                    root.Element.SetAreaDirty();
                }
            }
        }
        public float DrawAlpha = 1;
        public BlendState BlendState;
        public SamplerState SamplerState = SamplerState.PointClamp;
        public UiControls Controls = new UiControls();

        public UiSystem(GameWindow window, GraphicsDevice device, UiStyle style, InputHandler inputHandler = null) {
            this.GraphicsDevice = device;
            this.InputHandler = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;
            this.style = style;
            this.Viewport = device.Viewport.Bounds;
            this.AutoScaleReferenceSize = this.Viewport.Size;

            window.ClientSizeChanged += (sender, args) => {
                this.Viewport = device.Viewport.Bounds;
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            };
            window.TextInput += (sender, args) => {
                foreach (var root in this.rootElements)
                    root.Element.PropagateInput(args.Key, args.Character);
            };
        }

        public void Update(GameTime time) {
            if (this.isInputOurs)
                this.InputHandler.Update();

            var mousedNow = this.GetMousedElement();
            // mouse new element
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow);
                this.MousedElement = mousedNow;
            }

            if (this.Controls.MainButton(this.InputHandler)) {
                // select element
                if (this.SelectedElement != mousedNow) {
                    if (this.SelectedElement != null)
                        this.SelectedElement.OnDeselected?.Invoke(this.SelectedElement);
                    if (mousedNow != null)
                        mousedNow.OnSelected?.Invoke(mousedNow);
                    this.SelectedElement = mousedNow;
                }

                // first action on element
                if (mousedNow != null)
                    mousedNow.OnPressed?.Invoke(mousedNow);
            } else if (this.Controls.SecondaryButton(this.InputHandler)) {
                // secondary action on element
                if (mousedNow != null)
                    mousedNow.OnSecondaryPressed?.Invoke(mousedNow);
            }

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
            root.Element.PropagateRoot(root);
            root.Element.PropagateUiSystem(this);
            return true;
        }

        public void Remove(string name) {
            var root = this.Get(name);
            if (root == null)
                return;
            this.rootElements.Remove(root);
            root.Element.PropagateRoot(null);
            root.Element.PropagateUiSystem(null);
        }

        public RootElement Get(string name) {
            var index = this.IndexOf(name);
            return index < 0 ? null : this.rootElements[index];
        }

        private int IndexOf(string name) {
            return this.rootElements.FindIndex(element => element.Name == name);
        }

        private Element GetMousedElement() {
            for (var i = this.rootElements.Count - 1; i >= 0; i--) {
                var moused = this.rootElements[i].Element.GetMousedElement();
                if (moused != null)
                    return moused;
            }
            return null;
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