using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    public class UiControls {

        public readonly InputHandler Input;
        private readonly bool isInputOurs;
        private readonly UiSystem system;

        public Element MousedElement { get; private set; }
        public Element SelectedElement { get; private set; }
        public bool ShowSelectionIndicator;

        public UiControls(UiSystem system, InputHandler inputHandler = null) {
            this.system = system;
            this.Input = inputHandler ?? new InputHandler();
            this.isInputOurs = inputHandler == null;
        }

        public void Update() {
            if (this.isInputOurs)
                this.Input.Update();

            var mousedNow = this.GetMousedElement();
            // mouse new element
            if (mousedNow != this.MousedElement) {
                if (this.MousedElement != null)
                    this.MousedElement.OnMouseExit?.Invoke(this.MousedElement);
                if (mousedNow != null)
                    mousedNow.OnMouseEnter?.Invoke(mousedNow);
                this.MousedElement = mousedNow;
            }

            if (this.Input.IsMouseButtonPressed(MouseButton.Left)) {
                // select element
                var selectedNow = mousedNow != null && mousedNow.CanBeSelected ? mousedNow : null;
                if (this.SelectedElement != selectedNow)
                    this.SelectElement(selectedNow, false);

                // first action on element
                if (mousedNow != null)
                    mousedNow.OnPressed?.Invoke(mousedNow);
            } else if (this.Input.IsMouseButtonPressed(MouseButton.Right)) {
                // secondary action on element
                if (mousedNow != null)
                    mousedNow.OnSecondaryPressed?.Invoke(mousedNow);
            } else if (this.Input.IsKeyPressed(Keys.Enter) || this.Input.IsKeyPressed(Keys.Space)) {
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
                this.SelectElement(this.GetNextElement(this.Input.IsModifierKeyDown(ModifierKey.Shift)), true);
            }
        }

        public void SelectElement(Element element, bool show) {
            if (this.SelectedElement != null)
                this.SelectedElement.OnDeselected?.Invoke(this.SelectedElement);
            if (element != null)
                element.OnSelected?.Invoke(element);
            this.SelectedElement = element;
            this.ShowSelectionIndicator = show;
        }

        public Element GetMousedElement() {
            foreach (var root in this.system.GetRootElements()) {
                var moused = root.Element.GetMousedElement();
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