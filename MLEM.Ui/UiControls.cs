using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Input;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    public class UiControls {

        public readonly InputHandler Input;
        private readonly bool isInputOurs;
        private readonly UiSystem system;

        public Element MousedElement { get; private set; }
        public Element SelectedElement { get; private set; }
        public bool SelectedLastElementWithMouse { get; private set; }

        public UiControls(UiSystem system, InputHandler inputHandler = null) {
            this.system = system;
            this.Input = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;

            // enable all required gestures
            this.Input.EnableGestures(GestureType.Tap, GestureType.Hold);
        }

        public void Update() {
            if (this.isInputOurs)
                this.Input.Update();

            // MOUSE INPUT
            var mousedNow = this.GetElementUnderPos(this.Input.MousePosition);
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow);
                this.MousedElement = mousedNow;
                this.system.Propagate(e => e.OnMousedElementChanged?.Invoke(e, mousedNow));
            }
            
            if (this.Input.IsMouseButtonPressed(MouseButton.Left)) {
                var selectedNow = mousedNow != null && mousedNow.CanBeSelected ? mousedNow : null;
                this.SelectElement(selectedNow, true);
                if (mousedNow != null)
                    mousedNow.OnPressed?.Invoke(mousedNow);
            } else if (this.Input.IsMouseButtonPressed(MouseButton.Right)) {
                if (mousedNow != null)
                    mousedNow.OnSecondaryPressed?.Invoke(mousedNow);
            }
            
            // KEYBOARD INPUT
            else if (this.Input.IsKeyPressed(Keys.Enter) || this.Input.IsKeyPressed(Keys.Space)) {
                if (this.SelectedElement != null) {
                    if (this.Input.IsModifierKeyDown(ModifierKey.Shift)) {
                        // secondary action on element using space or enter
                        this.SelectedElement.OnSecondaryPressed?.Invoke(this.SelectedElement);
                    } else {
                        // first action on element using space or enter
                        this.SelectedElement.OnPressed?.Invoke(this.SelectedElement);
                    }
                }
            } else if (this.Input.IsKeyPressed(Keys.Tab)) {
                // tab or shift-tab to next or previous element
                this.SelectElement(this.GetNextElement(this.Input.IsModifierKeyDown(ModifierKey.Shift)), false);
            }
            
            // TOUCH INPUT
            else if (this.Input.GetGesture(GestureType.Tap, out var tap)) {
                var tapped = this.GetElementUnderPos(tap.Position.ToPoint());
                this.SelectElement(tapped, true);
                if (tapped != null)
                    tapped.OnPressed?.Invoke(tapped);
            } else if (this.Input.GetGesture(GestureType.Hold, out var hold)) {
                var held = this.GetElementUnderPos(hold.Position.ToPoint());
                this.SelectElement(held, true);
                if (held != null)
                    held.OnSecondaryPressed?.Invoke(held);
            }
        }

        public void SelectElement(Element element, bool mouse) {
            if (this.SelectedElement == element)
                return;

            if (this.SelectedElement != null)
                this.SelectedElement.OnDeselected?.Invoke(this.SelectedElement);
            if (element != null)
                element.OnSelected?.Invoke(element);
            this.SelectedElement = element;
            this.SelectedLastElementWithMouse = mouse;
            this.system.Propagate(e => e.OnSelectedElementChanged?.Invoke(e, element));
        }

        public Element GetElementUnderPos(Point position) {
            foreach (var root in this.system.GetRootElements()) {
                var moused = root.Element.GetElementUnderPos(position);
                if (moused != null)
                    return moused;
            }
            return null;
        }

        private Element GetNextElement(bool backward) {
            var currRoot = this.system.GetRootElements().FirstOrDefault(root => root.CanSelectContent);
            if (currRoot == null)
                return null;
            var children = currRoot.Element.GetChildren(regardChildrensChildren: true);
            if (this.SelectedElement == null || this.SelectedElement.Root != currRoot) {
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

    }
}