using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    public class UiSystem {

        private readonly GraphicsDevice graphicsDevice;
        private readonly List<RootElement> rootElements = new List<RootElement>();
        private readonly InputHandler inputHandler;
        private readonly bool isInputOurs;

        public readonly float GlobalScale;
        public readonly IGenericFont DefaultFont;
        public Rectangle ScaledViewport {
            get {
                var bounds = this.graphicsDevice.Viewport.Bounds;
                return new Rectangle(bounds.X, bounds.Y, (bounds.Width / this.GlobalScale).Floor(), (bounds.Height / this.GlobalScale).Floor());
            }
        }
        public Vector2 MousePos => this.inputHandler.MousePosition.ToVector2() / this.GlobalScale;
        public Element MousedElement { get; private set; }

        public UiSystem(GameWindow window, GraphicsDevice device, float scale, IGenericFont defaultFont, InputHandler inputHandler = null) {
            this.graphicsDevice = device;
            this.GlobalScale = scale;
            this.DefaultFont = defaultFont;
            this.inputHandler = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;

            window.ClientSizeChanged += (sender, args) => {
                foreach (var root in this.rootElements)
                    root.Element.ForceUpdateArea();
            };
        }

        public void Update(GameTime time) {
            if (this.isInputOurs)
                this.inputHandler.Update();

            var mousedNow = this.GetMousedElement();
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement, this.MousePos);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow, this.MousePos);
                this.MousedElement = mousedNow;
            }

            if (mousedNow != null && (mousedNow.OnMouseDown != null || mousedNow.OnClicked != null)) {
                foreach (var button in InputHandler.MouseButtons) {
                    if (mousedNow.OnMouseDown != null && this.inputHandler.IsMouseButtonDown(button))
                        mousedNow.OnMouseDown(mousedNow, this.MousePos, button);
                    if (mousedNow.OnClicked != null && this.inputHandler.IsMouseButtonPressed(button))
                        mousedNow.OnClicked(mousedNow, this.MousePos, button);
                }
            }

            foreach (var root in this.rootElements)
                root.Element.Update(time);
        }

        public void Draw(GameTime time, SpriteBatch batch, Color? color = null, BlendState blendState = null, SamplerState samplerState = null) {
            var col = color ?? Color.White;

            batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, transformMatrix: Matrix.CreateScale(this.GlobalScale));
            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.Draw(time, batch, col);
            }
            batch.End();

            foreach (var root in this.rootElements) {
                if (!root.Element.IsHidden)
                    root.Element.DrawUnbound(time, batch, col, blendState, samplerState);
            }
        }

        public void Add(string name, Element root) {
            if (this.IndexOf(name) >= 0)
                throw new ArgumentException($"There is already a root element with name {name}");

            this.rootElements.Add(new RootElement(name, root));
            root.SetUiSystem(this);
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