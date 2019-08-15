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

        private float globalScale = 1;
        public float GlobalScale {
            get => this.globalScale;
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

        public UiSystem(GameWindow window, GraphicsDevice device, UiStyle style, InputHandler inputHandler = null) {
            this.GraphicsDevice = device;
            this.InputHandler = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;
            this.style = style;
            this.Viewport = device.Viewport.Bounds;

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
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow);
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
                        mousedNow.OnClicked(mousedNow, button);
                }
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

        public RootElement Add(string name, Element root) {
            if (this.IndexOf(name) >= 0)
                throw new ArgumentException($"There is already a root element with name {name}");

            var rootInst = new RootElement(name, root, this);
            this.rootElements.Add(rootInst);
            root.PropagateRoot(rootInst);
            root.PropagateUiSystem(this);
            return rootInst;
        }

        public void Remove(string name) {
            var index = this.IndexOf(name);
            if (index < 0)
                return;
            this.rootElements.RemoveAt(index);
        }

        public RootElement Get(string name) {
            var index = this.IndexOf(name);
            return index < 0 ? null : this.rootElements[index];
        }

        private int IndexOf(string name) {
            return this.rootElements.FindIndex(element => element.Name == name);
        }

        private Element GetMousedElement() {
            foreach (var root in this.rootElements) {
                var moused = root.Element.GetMousedElement();
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

    }
}