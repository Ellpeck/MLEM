using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, Element> selectedElements = new Dictionary<string, Element>();
        public Element SelectedElement => this.GetSelectedElement(this.ActiveRoot);

        public object[] KeyboardButtons = {Keys.Space, Keys.Enter};
        public object[] GamepadButtons = {Buttons.A};
        public object[] SecondaryGamepadButtons = {Buttons.X};
        public object[] UpButtons = {Buttons.DPadUp, Buttons.LeftThumbstickUp};
        public object[] DownButtons = {Buttons.DPadDown, Buttons.LeftThumbstickDown};
        public object[] LeftButtons = {Buttons.DPadLeft, Buttons.LeftThumbstickLeft};
        public object[] RightButtons = {Buttons.DPadRight, Buttons.LeftThumbstickRight};
        public int GamepadIndex = -1;
        public bool HandleMouse = true;
        public bool HandleKeyboard = true;
        public bool HandleTouch = true;
        public bool HandleGamepad = true;
        public bool IsAutoNavMode;

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
            this.ActiveRoot = this.System.GetRootElements().FirstOrDefault(root => root.CanSelectContent && !root.Element.IsHidden);

            // MOUSE INPUT
            if (this.HandleMouse) {
                var mousedNow = this.GetElementUnderPos(this.Input.MousePosition.ToVector2());
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
                    this.SelectElement(this.ActiveRoot, selectedNow);
                    if (mousedNow != null && mousedNow.CanBePressed)
                        this.System.OnElementPressed?.Invoke(mousedNow);
                } else if (this.Input.IsMouseButtonPressed(MouseButton.Right)) {
                    this.IsAutoNavMode = false;
                    if (mousedNow != null && mousedNow.CanBePressed)
                        this.System.OnElementSecondaryPressed?.Invoke(mousedNow);
                }
            }

            // KEYBOARD INPUT
            if (this.HandleKeyboard) {
                if (this.KeyboardButtons.Any(this.IsAnyPressed)) {
                    if (this.SelectedElement?.Root != null && this.SelectedElement.CanBePressed) {
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
                    this.SelectElement(this.ActiveRoot, next);
                }
            }

            // TOUCH INPUT
            if (this.HandleTouch) {
                if (this.Input.GetGesture(GestureType.Tap, out var tap)) {
                    this.IsAutoNavMode = false;
                    var tapped = this.GetElementUnderPos(tap.Position);
                    this.SelectElement(this.ActiveRoot, tapped);
                    if (tapped != null && tapped.CanBePressed)
                        this.System.OnElementPressed?.Invoke(tapped);
                } else if (this.Input.GetGesture(GestureType.Hold, out var hold)) {
                    this.IsAutoNavMode = false;
                    var held = this.GetElementUnderPos(hold.Position);
                    this.SelectElement(this.ActiveRoot, held);
                    if (held != null && held.CanBePressed)
                        this.System.OnElementSecondaryPressed?.Invoke(held);
                }
            }

            // GAMEPAD INPUT
            if (this.HandleGamepad) {
                if (this.GamepadButtons.Any(this.IsAnyPressed)) {
                    if (this.SelectedElement?.Root != null && this.SelectedElement.CanBePressed)
                        this.System.OnElementPressed?.Invoke(this.SelectedElement);
                } else if (this.SecondaryGamepadButtons.Any(this.IsAnyPressed)) {
                    if (this.SelectedElement?.Root != null && this.SelectedElement.CanBePressed)
                        this.System.OnElementSecondaryPressed?.Invoke(this.SelectedElement);
                } else if (this.DownButtons.Any(this.IsAnyPressed)) {
                    this.HandleGamepadNextElement(Direction2.Down);
                } else if (this.LeftButtons.Any(this.IsAnyPressed)) {
                    this.HandleGamepadNextElement(Direction2.Left);
                } else if (this.RightButtons.Any(this.IsAnyPressed)) {
                    this.HandleGamepadNextElement(Direction2.Right);
                } else if (this.UpButtons.Any(this.IsAnyPressed)) {
                    this.HandleGamepadNextElement(Direction2.Up);
                }
            }
        }

        public virtual Element GetElementUnderPos(Vector2 position, bool transform = true) {
            foreach (var root in this.System.GetRootElements()) {
                var pos = transform ? Vector2.Transform(position, root.InvTransform) : position;
                var moused = root.Element.GetElementUnderPos(pos);
                if (moused != null)
                    return moused;
            }
            return null;
        }

        public void SelectElement(RootElement root, Element element, bool? autoNav = null) {
            if (root == null)
                return;
            var selected = this.GetSelectedElement(root);
            if (selected == element)
                return;

            if (selected != null)
                this.System.OnElementDeselected?.Invoke(selected);
            if (element != null) {
                this.System.OnElementSelected?.Invoke(element);
                this.selectedElements[root.Name] = element;
            } else {
                this.selectedElements.Remove(root.Name);
            }
            this.System.OnSelectedElementChanged?.Invoke(element);

            if (autoNav != null)
                this.IsAutoNavMode = autoNav.Value;
        }

        public Element GetSelectedElement(RootElement root) {
            if (root == null)
                return null;
            this.selectedElements.TryGetValue(root.Name, out var element);
            return element;
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

        protected virtual Element GetGamepadNextElement(RectangleF searchArea) {
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
                    var dist = Vector2.Distance(child.Area.Center, this.SelectedElement.Area.Center);
                    if (closest == null || dist < closestDist) {
                        closest = child;
                        closestDist = dist;
                    }
                }
                return closest;
            }
        }

        private bool IsAnyPressed(object button) {
            return this.Input.IsPressed(button, this.GamepadIndex);
        }

        private void HandleGamepadNextElement(Direction2 dir) {
            this.IsAutoNavMode = true;
            RectangleF searchArea = default;
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
                this.SelectElement(this.ActiveRoot, next);
        }

    }
}