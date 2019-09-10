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
        private readonly bool isInputOurs;
        private readonly UiSystem system;

        public RootElement ActiveRoot { get; private set; }
        public Element MousedElement { get; private set; }
        public Element SelectedElement => this.ActiveRoot.SelectedElement;

        public Buttons[] GamepadButtons = {Buttons.A};
        public Buttons[] SecondaryGamepadButtons = {Buttons.X};
        public int GamepadIndex = -1;

        public bool IsAutoNavMode { get; private set; }

        public UiControls(UiSystem system, InputHandler inputHandler = null) {
            this.system = system;
            this.Input = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;

            // enable all required gestures
            InputHandler.EnableGestures(GestureType.Tap, GestureType.Hold);
        }

        public void Update() {
            if (this.isInputOurs)
                this.Input.Update();
            this.ActiveRoot = this.system.GetRootElements().FirstOrDefault(root => root.CanSelectContent);

            // MOUSE INPUT
            var mousedNow = this.GetElementUnderPos(this.Input.MousePosition);
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow);
                this.MousedElement = mousedNow;
                this.system.ApplyToAll(e => e.OnMousedElementChanged?.Invoke(e, mousedNow));
            }

            if (this.Input.IsMouseButtonPressed(MouseButton.Left)) {
                this.IsAutoNavMode = false;
                var selectedNow = mousedNow != null && mousedNow.CanBeSelected ? mousedNow : null;
                this.ActiveRoot.SelectElement(selectedNow);
                if (mousedNow != null)
                    mousedNow.OnPressed?.Invoke(mousedNow);
            } else if (this.Input.IsMouseButtonPressed(MouseButton.Right)) {
                this.IsAutoNavMode = false;
                if (mousedNow != null)
                    mousedNow.OnSecondaryPressed?.Invoke(mousedNow);
            }

            // KEYBOARD INPUT
            else if (this.Input.IsKeyPressed(Keys.Enter) || this.Input.IsKeyPressed(Keys.Space)) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null) {
                    if (this.Input.IsModifierKeyDown(ModifierKey.Shift)) {
                        // secondary action on element using space or enter
                        this.SelectedElement.OnSecondaryPressed?.Invoke(this.SelectedElement);
                    } else {
                        // first action on element using space or enter
                        this.SelectedElement.OnPressed?.Invoke(this.SelectedElement);
                    }
                }
            } else if (this.Input.IsKeyPressed(Keys.Tab)) {
                this.IsAutoNavMode = true;
                // tab or shift-tab to next or previous element
                var backward = this.Input.IsModifierKeyDown(ModifierKey.Shift);
                var next = this.GetTabNextElement(backward);
                if (this.SelectedElement?.Root != null)
                    next = this.SelectedElement.GetTabNextElement(backward, next);
                this.ActiveRoot.SelectElement(next);
            }

            // TOUCH INPUT
            else if (this.Input.GetGesture(GestureType.Tap, out var tap)) {
                this.IsAutoNavMode = false;
                var tapped = this.GetElementUnderPos(tap.Position.ToPoint());
                this.ActiveRoot.SelectElement(tapped);
                if (tapped != null)
                    tapped.OnPressed?.Invoke(tapped);
            } else if (this.Input.GetGesture(GestureType.Hold, out var hold)) {
                this.IsAutoNavMode = false;
                var held = this.GetElementUnderPos(hold.Position.ToPoint());
                this.ActiveRoot.SelectElement(held);
                if (held != null)
                    held.OnSecondaryPressed?.Invoke(held);
            }

            // GAMEPAD INPUT
            else if (this.GamepadButtons.Any(this.IsGamepadPressed)) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null)
                    this.SelectedElement.OnPressed?.Invoke(this.SelectedElement);
            } else if (this.SecondaryGamepadButtons.Any(this.IsGamepadPressed)) {
                this.IsAutoNavMode = true;
                if (this.SelectedElement?.Root != null)
                    this.SelectedElement.OnSecondaryPressed?.Invoke(this.SelectedElement);
            } else if (this.IsGamepadPressed(Buttons.DPadDown) || this.IsGamepadPressed(Buttons.LeftThumbstickDown)) {
                this.HandleGamepadNextElement(Direction2.Down);
            } else if (this.IsGamepadPressed(Buttons.DPadLeft) || this.IsGamepadPressed(Buttons.LeftThumbstickLeft)) {
                this.HandleGamepadNextElement(Direction2.Left);
            } else if (this.IsGamepadPressed(Buttons.DPadRight) || this.IsGamepadPressed(Buttons.LeftThumbstickRight)) {
                this.HandleGamepadNextElement(Direction2.Right);
            } else if (this.IsGamepadPressed(Buttons.DPadUp) || this.IsGamepadPressed(Buttons.LeftThumbstickUp)) {
                this.HandleGamepadNextElement(Direction2.Up);
            }
        }

        public Element GetElementUnderPos(Point position, bool transform = true) {
            foreach (var root in this.system.GetRootElements()) {
                var pos = transform ? position.Transform(root.InvTransform) : position;
                var moused = root.Element.GetElementUnderPos(pos);
                if (moused != null)
                    return moused;
            }
            return null;
        }

        private Element GetTabNextElement(bool backward) {
            if (this.ActiveRoot == null)
                return null;
            var children = this.ActiveRoot.Element.GetChildren(regardChildrensChildren: true).Append(this.ActiveRoot.Element);
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

        private void HandleGamepadNextElement(Direction2 dir) {
            this.IsAutoNavMode = true;
            Rectangle searchArea = default;
            if (this.SelectedElement?.Root != null) {
                searchArea = this.SelectedElement.Area;
                var (_, _, width, height) = this.system.Viewport;
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

        private Element GetGamepadNextElement(Rectangle searchArea) {
            if (this.ActiveRoot == null)
                return null;
            var children = this.ActiveRoot.Element.GetChildren(regardChildrensChildren: true).Append(this.ActiveRoot.Element);
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

        private bool IsGamepadPressed(Buttons button) {
            return this.Input.IsGamepadButtonPressed(button, this.GamepadIndex);
        }

    }
}