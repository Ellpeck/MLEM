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
        private readonly List<RootElement> rootElements = new List<RootElement>();
        public readonly InputHandler InputHandler;
        private readonly bool isInputOurs;

        private float globalScale;
        public float GlobalScale {
            get => this.globalScale;
            set {
                this.globalScale = value;
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            }
        }
        public Rectangle ScaledViewport {
            get {
                var bounds = this.GraphicsDevice.Viewport.Bounds;
                return new Rectangle(bounds.X, bounds.Y, (bounds.Width / this.globalScale).Floor(), (bounds.Height / this.globalScale).Floor());
            }
        }
        public Vector2 MousePos => this.InputHandler.MousePosition.ToVector2() / this.globalScale;
        public Element MousedElement { get; private set; }
        public Element SelectedElement { get; private set; }
        private UiStyle style;
        public UiStyle Style {
            get => this.style;
            set {
                this.style = value;
                foreach (var root in this.rootElements) {
                    root.Element.PropagateUiSystem(this);
                    root.Element.SetDirty();
                }
            }
        }
        public float DrawAlpha = 1;
        public BlendState BlendState;
        public SamplerState SamplerState = SamplerState.PointClamp;

        public UiSystem(GameWindow window, GraphicsDevice device, UiStyle style, InputHandler inputHandler = null) {
            this.GraphicsDevice = device;
            this.InputHandler = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;
            this.style = style;

            window.ClientSizeChanged += (sender, args) => {
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
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement, this.MousePos);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow, this.MousePos);
                this.MousedElement = mousedNow;
            }

            if (this.SelectedElement != mousedNow && this.InputHandler.IsMouseButtonPressed(MouseButton.Left)) {
                if (this.SelectedElement != null)
                    this.SelectedElement.OnDeselected?.Invoke(this.SelectedElement);
                if (mousedNow != null)
                    mousedNow.OnSelected?.Invoke(mousedNow);
                this.SelectedElement = mousedNow;
            }

            if (mousedNow?.OnClicked != null) {
                foreach (var button in InputHandler.MouseButtons) {
                    if (this.InputHandler.IsMouseButtonPressed(button))
                        mousedNow.OnClicked(mousedNow, this.MousePos, button);
                }
            }

            foreach (var root in this.rootElements)
                root.Element.Update(time);
        }

        public void Draw(GameTime time, SpriteBatch batch) {
            batch.Begin(SpriteSortMode.Deferred, this.BlendState, this.SamplerState, transformMatrix: Matrix.CreateScale(this.globalScale));
            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.Draw(time, batch, this.DrawAlpha * root.Element.DrawAlpha);
            }
            batch.End();

            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.DrawUnbound(time, batch, this.DrawAlpha * root.Element.DrawAlpha, this.BlendState, this.SamplerState);
            }
        }

        public void Add(string name, Element root) {
            if (this.IndexOf(name) >= 0)
                throw new ArgumentException($"There is already a root element with name {name}");

            this.rootElements.Add(new RootElement(name, root));
            root.PropagateUiSystem(this);
        }

        public void Remove(string name) {
            var index = this.IndexOf(name);
            if (index < 0)
                return;
            this.rootElements.RemoveAt(index);
        }

        public Element Get(string name) {
            var index = this.IndexOf(name);
            return index < 0 ? null : this.rootElements[index].Element;
        }

        private int IndexOf(string name) {
            return this.rootElements.FindIndex(element => element.Name == name);
        }

        private Element GetMousedElement() {
            foreach (var root in this.rootElements) {
                var moused = root.Element.GetMousedElement(this.MousePos);
                if (moused != null)
                    return moused;
            }
            return null;
        }

    }

    public struct RootElement {

        public readonly string Name;
        public readonly Element Element;

        public RootElement(string name, Element element) {
            this.Name = name;
            this.Element = element;
        }

    }
}