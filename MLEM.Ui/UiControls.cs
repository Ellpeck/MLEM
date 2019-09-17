using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Extensions;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    public class UiControls {

        public readonly InputHandler Input;
        protected readonly bool IsInputOurs;
        protected readonly UiSystem System;

        public RootElement ActiveRoot { get; private set; }
        public Element MousedElement { get; private set; }
        public Element SelectedElement => this.ActiveRoot?.SelectedElement;

        public Buttons[] GamepadButtons = {Buttons.A};
        public Buttons[] SecondaryGamepadButtons = {Buttons.X};
        public Keys[] KeyboardButtons = {Keys.Space, Keys.Enter};
        public object[] UpButtons = {Buttons.DPadUp, Buttons.LeftThumbstickUp};
        public object[] DownButtons = {Buttons.DPadDown, Buttons.LeftThumbstickDown};
        public object[] LeftButtons = {Buttons.DPadLeft, Buttons.LeftThumbstickLeft};
        public object[] RightButtons = {Buttons.DPadRight, Buttons.LeftThumbstickRight};
        public int GamepadIndex = -1;

        public bool IsAutoNavMode { get; private set; }

        public UiControls(UiSystem system, InputHandler inputHandler = null) {
            this.System = system;
            this.Input = inputHandler ?? new InputHandler();
            this.IsInputOurs = inputHandler == null;

            // enable all required gestures
            InputHandler.EnableGestures(GestureType.Tap, GestureType.Hold);
        }

        public virtual void Update() {
            if (this.IsInputOurs)
                this.Input.Update();
            this.ActiveRoot = this.System.GetRootElements().FirstOrDefault(root => root.CanSelectContent);

            // MOUSE INPUT
            var mousedNow = this.GetElementUnderPos(this.Input.MousePosition);
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.System.OnElementMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    this.System.OnElementMouseEnter?.Invoke(mousedNow);
                this.MousedElement = mousedNow;
                this.System.OnMousedElementChanged?.Invoke(mousedNow);
            }

            if (this.Input.IsMouseButtonPressed(MouseButton.Left)) {
                this.IsAutoNavMode = false;
                var selectedNow = mousedNow != null && mousedNow.CanBeSelected ? mousedNow : null;
                this.ActiveRoot?.SelectElement(selectedNow);
                if (mousedNow != null)
                    this.System.OnElementPressed?.Invoke(mousedNow);
            } else if (this.Input.IsMouseButtonPressed(MouseButton.Right)) {
                this.IsAutoNavMode = false;
                if (mousedNow != null)
                    this.System.OnElementSecondaryPressed?.Invoke(mousedNow);
            }

            // KEYBOARD INPUT
            else if (this.KeyboardButtons.Any(this.Input.IsKeyPressed)) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null) {
                    if (this.Input.IsModifierKeyDown(ModifierKey.Shift)) {
                        // secondary action on element using space or enter
                        this.System.OnElementSecondaryPressed?.Invoke(this.SelectedElement);
                    } else {
                        // first action on element using space or enter
                        this.System.OnElementPressed?.Invoke(this.SelectedElement);
                    }
                }
            } else if (this.Input.IsKeyPressed(Keys.Tab)) {
                this.IsAutoNavMode = true;
                // tab or shift-tab to next or previous element
                var backward = this.Input.IsModifierKeyDown(ModifierKey.Shift);
                var next = this.GetTabNextElement(backward);
                if (this.SelectedElement?.Root != null)
                    next = this.SelectedElement.GetTabNextElement(backward, next);
                this.ActiveRoot?.SelectElement(next);
            }

            // TOUCH INPUT
            else if (this.Input.GetGesture(GestureType.Tap, out var tap)) {
                this.IsAutoNavMode = false;
                var tapped = this.GetElementUnderPos(tap.Position.ToPoint());
                this.ActiveRoot?.SelectElement(tapped);
                if (tapped != null)
                    this.System.OnElementPressed?.Invoke(tapped);
            } else if (this.Input.GetGesture(GestureType.Hold, out var hold)) {
                this.IsAutoNavMode = false;
                var held = this.GetElementUnderPos(hold.Position.ToPoint());
                this.ActiveRoot?.SelectElement(held);
                if (held != null)
                    this.System.OnElementSecondaryPressed?.Invoke(held);
            }

            // GAMEPAD INPUT
            else if (this.GamepadButtons.Any(b => this.IsGamepadPressed(b))) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null)
                    this.System.OnElementPressed?.Invoke(this.SelectedElement);
            } else if (this.SecondaryGamepadButtons.Any(b => this.IsGamepadPressed(b))) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null)
                    this.System.OnElementSecondaryPressed?.Invoke(this.SelectedElement);
            } else if (this.DownButtons.Any(this.IsGamepadPressed)) {
                this.HandleGamepadNextElement(Direction2.Down);
            } else if (this.LeftButtons.Any(this.IsGamepadPressed)) {
                this.HandleGamepadNextElement(Direction2.Left);
            } else if (this.RightButtons.Any(this.IsGamepadPressed)) {
                this.HandleGamepadNextElement(Direction2.Right);
            } else if (this.UpButtons.Any(this.IsGamepadPressed)) {
                this.HandleGamepadNextElement(Direction2.Up);
            }
        }

        public virtual Element GetElementUnderPos(Point position, bool transform = true) {
            foreach (var root in this.System.GetRootElements()) {
                var pos = transform ? position.Transform(root.InvTransform) : position;
                var moused = root.Element.GetElementUnderPos(pos);
                if (moused != null)
                    return moused;
            }
            return null;
        }

        protected virtual Element GetTabNextElement(bool backward) {
            if (this.ActiveRoot == null)
                return null;
            var children = this.ActiveRoot.Element.GetChildren(c => !c.IsHidden, true, true).Append(this.ActiveRoot.Element);
            if (this.SelectedElement?.Root != this.ActiveRoot) {
                return backward ? children.LastOrDefault(c => c.CanBeSelected) : children.FirstOrDefault(c => c.CanBeSelected);
            } else {
                var foundCurr = false;
                Element lastFound = null;
                foreach (var child in children) {
                    if (!child.CanBeSelected)
                        continue;
                    if (child == this.SelectedElement) {
                        // when going backwards, return the last element found before the current one
                        if (backward)
                            return lastFound;
                        foundCurr = true;
                    } else {
                        // when going forwards, return the element after the current one
                        if (!backward && foundCurr)
                            return child;
                    }
                    lastFound = child;
                }
                return null;
            }
        }

        protected virtual Element GetGamepadNextElement(Rectangle searchArea) {
            if (this.ActiveRoot == null)
                return null;
            var children = this.ActiveRoot.Element.GetChildren(c => !c.IsHidden, true, true).Append(this.ActiveRoot.Element);
            if (this.SelectedElement?.Root != this.ActiveRoot) {
                return children.FirstOrDefault(c => c.CanBeSelected);
            } else {
                Element closest = null;
                float closestDist = 0;
                foreach (var child in children) {
                    if (!child.CanBeSelected || child == this.SelectedElement || !searchArea.Intersects(child.Area))
                        continue;
                    var dist = Vector2.Distance(child.Area.Center.ToVector2(), this.SelectedElement.Area.Center.ToVector2());
                    if (closest == null || dist < closestDist) {
                        closest = child;
                        closestDist = dist;
                    }
                }
                return closest;
            }
        }

        private bool IsGamepadPressed(object button) {
            return this.Input.IsPressed(button, this.GamepadIndex);
        }

        private void HandleGamepadNextElement(Direction2 dir) {
            this.IsAutoNavMode = true;
            Rectangle searchArea = default;
            if (this.SelectedElement?.Root != null) {
                searchArea = this.SelectedElement.Area;
                var (_, _, width, height) = this.System.Viewport;
                switch (dir) {
                    case Direction2.Down:
                        searchArea.Height += height;
                        break;
                    case Direction2.Left:
                        searchArea.X -= width;
                        searchArea.Width += width;
                        break;
                    case Direction2.Right:
                        searchArea.Width += width;
                        break;
                    case Direction2.Up:
                        searchArea.Y -= height;
                        searchArea.Height += height;
                        break;
                }
            }
            var next = this.GetGamepadNextElement(searchArea);
            if (this.SelectedElement != null)
                next = this.SelectedElement.GetGamepadNextElement(dir, next);
            if (next != null)
                this.ActiveRoot.SelectElement(next);
        }

    }
}