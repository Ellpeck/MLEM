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
using MLEM.Ui.Style;

namespace MLEM.Ui {
    /// <summary>
    /// UiControls holds and manages all of the controls for a <see cref="UiSystem"/>.
    /// UiControls supports keyboard, mouse, gamepad and touch input using an underlying <see cref="InputHandler"/>.
    /// </summary>
    public class UiControls {

        /// <summary>
        /// The input handler that is used for querying input
        /// </summary>
        public readonly InputHandler Input;
        /// <summary>
        /// This value ist true if the <see cref="InputHandler"/> was created by this ui controls instance, or if it was passed in.
        /// If the input handler was created by this instance, its <see cref="InputHandler.Update()"/> method should be called by us.
        /// </summary>
        protected readonly bool IsInputOurs;
        /// <summary>
        /// The <see cref="UiSystem"/> that this ui controls instance is controlling
        /// </summary>
        protected readonly UiSystem System;

        /// <summary>
        /// The <see cref="RootElement"/> that is currently active.
        /// The active root element is the one with the highest <see cref="RootElement.Priority"/> that whose <see cref="RootElement.CanSelectContent"/> property is true.
        /// </summary>
        public RootElement ActiveRoot { get; protected set; }
        /// <summary>
        /// The <see cref="Element"/> that the mouse is currently over.
        /// </summary>
        public Element MousedElement { get; protected set; }
        /// <summary>
        /// The <see cref="Element"/> that is currently touched.
        /// </summary>
        public Element TouchedElement { get; protected set; }
        private readonly Dictionary<string, Element> selectedElements = new Dictionary<string, Element>();
        /// <summary>
        /// The element that is currently selected.
        /// This is the <see cref="RootElement.SelectedElement"/> of the <see cref="ActiveRoot"/>.
        /// </summary>
        public Element SelectedElement => this.GetSelectedElement(this.ActiveRoot);

        /// <summary>
        /// A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons on the keyboard which perform the <see cref="Element.OnPressed"/> action.
        /// If the <see cref="ModifierKey.Shift"/> is held, these buttons perform <see cref="Element.OnSecondaryPressed"/>.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] KeyboardButtons = {Keys.Space, Keys.Enter};
        /// <summary>
        /// A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons on a gamepad that perform the <see cref="Element.OnPressed"/> action.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] GamepadButtons = {Buttons.A};
        /// <summary>
        /// A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons on a gamepad that perform the <see cref="Element.OnSecondaryPressed"/> action.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] SecondaryGamepadButtons = {Buttons.X};
        /// <summary>
        /// A list of A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons that select a <see cref="Element"/> that is above the currently selected element.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] UpButtons = {Buttons.DPadUp, Buttons.LeftThumbstickUp};
        /// <summary>
        /// A list of A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons that select a <see cref="Element"/> that is below the currently selected element.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] DownButtons = {Buttons.DPadDown, Buttons.LeftThumbstickDown};
        /// <summary>
        /// A list of A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons that select a <see cref="Element"/> that is to the left of the currently selected element.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] LeftButtons = {Buttons.DPadLeft, Buttons.LeftThumbstickLeft};
        /// <summary>
        /// A list of A list of <see cref="Keys"/>, <see cref="Buttons"/> and/or <see cref="MouseButton"/> that act as the buttons that select a <see cref="Element"/> that is to the right of the currently selected element.
        /// To easily add more elements to this list, use <see cref="AddButtons"/>.
        /// </summary>
        public object[] RightButtons = {Buttons.DPadRight, Buttons.LeftThumbstickRight};
        /// <summary>
        /// The zero-based index of the <see cref="GamePad"/> used for gamepad input.
        /// If this index is lower than 0, every connected gamepad will trigger input.
        /// </summary>
        public int GamepadIndex = -1;
        /// <summary>
        /// Set this to false to disable mouse input for these ui controls.
        /// Note that this does not disable mouse input for the underlying <see cref="InputHandler"/>.
        /// </summary>
        public bool HandleMouse = true;
        /// <summary>
        /// Set this to false to disable keyboard input for these ui controls.
        /// Note that this does not disable keyboard input for the underlying <see cref="InputHandler"/>.
        /// </summary>
        public bool HandleKeyboard = true;
        /// <summary>
        /// Set this to false to disable touch input for these ui controls.
        /// Note that this does not disable touch input for the underlying <see cref="InputHandler"/>.
        /// </summary>
        public bool HandleTouch = true;
        /// <summary>
        /// Set this to false to disable gamepad input for these ui controls.
        /// Note that this does not disable gamepad input for the underlying <see cref="InputHandler"/>.
        /// </summary>
        public bool HandleGamepad = true;
        /// <summary>
        /// If this value is true, the ui controls are in automatic navigation mode.
        /// This means that the <see cref="UiStyle.SelectionIndicator"/> will be drawn around the <see cref="SelectedElement"/>.
        /// To set this value, use <see cref="SelectElement"/> or <see cref="RootElement.SelectElement"/>
        /// </summary>
        public bool IsAutoNavMode { get; internal set; }

        /// <summary>
        /// Creates a new instance of the ui controls.
        /// You should rarely have to invoke this manually, since the <see cref="UiSystem"/> handles it.
        /// </summary>
        /// <param name="system">The ui system to control with these controls</param>
        /// <param name="inputHandler">The input handler to use for controlling, or null to create a new one.</param>
        public UiControls(UiSystem system, InputHandler inputHandler = null) {
            this.System = system;
            this.Input = inputHandler ?? new InputHandler();
            this.IsInputOurs = inputHandler == null;

            // enable all required gestures
            InputHandler.EnableGestures(GestureType.Tap, GestureType.Hold);
        }

        /// <summary>
        /// Update this ui controls instance, causing the underlying <see cref="InputHandler"/> to be updated, as well as ui input to be queried.
        /// </summary>
        public virtual void Update() {
            if (this.IsInputOurs)
                this.Input.Update();
            this.ActiveRoot = this.System.GetRootElements().FirstOrDefault(root => root.CanSelectContent && !root.Element.IsHidden);

            // MOUSE INPUT
            if (this.HandleMouse) {
                var mousedNow = this.GetElementUnderPos(this.Input.MousePosition.ToVector2());
                this.SetMousedElement(mousedNow);

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
                } else if (this.Input.TouchState.Count <= 0) {
                    this.SetTouchedElement(null);
                } else {
                    foreach (var location in this.Input.TouchState) {
                        var element = this.GetElementUnderPos(location.Position);
                        if (location.State == TouchLocationState.Pressed) {
                            // start touching an element if we just touched down on it
                            this.SetTouchedElement(element);
                        } else if (element != this.TouchedElement) {
                            // if we moved off of the touched element, we stop touching
                            this.SetTouchedElement(null);
                        }
                    }
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

        /// <summary>
        /// Returns the <see cref="Element"/> in the underlying <see cref="UiSystem"/> that is currently below the given position.
        /// Throughout the ui system, this is used for mouse input querying.
        /// </summary>
        /// <param name="position">The position to query</param>
        /// <param name="transform">If this value is true, the <see cref="RootElement.Transform"/> will be applied.</param>
        /// <returns>The element under the position, or null if there isn't one</returns>
        public virtual Element GetElementUnderPos(Vector2 position, bool transform = true) {
            foreach (var root in this.System.GetRootElements()) {
                var pos = transform ? Vector2.Transform(position, root.InvTransform) : position;
                var moused = root.Element.GetElementUnderPos(pos);
                if (moused != null)
                    return moused;
            }
            return null;
        }

        /// <summary>
        /// Selects the given element that is a child of the given root element.
        /// Optionally, automatic navigation can be forced on, causing the <see cref="UiStyle.SelectionIndicator"/> to be drawn around the element.
        /// A simpler version of this method is <see cref="RootElement.SelectElement"/>.
        /// </summary>
        /// <param name="root">The root element of the <see cref="Element"/></param>
        /// <param name="element">The element to select, or null to deselect the selected element.</param>
        /// <param name="autoNav">Whether automatic navigation should be forced on</param>
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

        /// <summary>
        /// Sets the <see cref="MousedElement"/> to the given value, calling the appropriate events.
        /// </summary>
        /// <param name="element">The element to set as moused</param>
        public void SetMousedElement(Element element) {
            if (element != this.MousedElement) {
                if (this.MousedElement != null)
                    this.System.OnElementMouseExit?.Invoke(this.MousedElement);
                if (element != null)
                    this.System.OnElementMouseEnter?.Invoke(element);
                this.MousedElement = element;
                this.System.OnMousedElementChanged?.Invoke(element);
            }
        }

        /// <summary>
        /// Sets the <see cref="TouchedElement"/> to the given value, calling the appropriate events.
        /// </summary>
        /// <param name="element">The element to set as touched</param>
        public void SetTouchedElement(Element element) {
            if (element != this.TouchedElement) {
                if (this.TouchedElement != null)
                    this.System.OnElementTouchExit?.Invoke(this.TouchedElement);
                if (element != null)
                    this.System.OnElementTouchEnter?.Invoke(element);
                this.TouchedElement = element;
                this.System.OnTouchedElementChanged?.Invoke(element);
            }
        }

        /// <summary>
        /// Returns the selected element for the given root element.
        /// A property equivalent to this method is <see cref="RootElement.SelectedElement"/>.
        /// </summary>
        /// <param name="root">The root element whose selected element to return</param>
        /// <returns>The given root's selected element, or null if the root doesn't exist, or if there is no selected element for that root.</returns>
        public Element GetSelectedElement(RootElement root) {
            if (root == null)
                return null;
            this.selectedElements.TryGetValue(root.Name, out var element);
            return element;
        }

        /// <summary>
        /// Returns the next element to select when pressing the <see cref="Keys.Tab"/> key during keyboard navigation.
        /// If the <c>backward</c> boolean is true, the previous element should be returned instead.
        /// </summary>
        /// <param name="backward">If we're going backwards (if <see cref="ModifierKey.Shift"/> is held)</param>
        /// <returns>The next or previous element to select</returns>
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

        /// <summary>
        /// Returns the next element that should be selected during gamepad navigation, based on the <see cref="RectangleF"/> that we're looking for elements in.
        /// </summary>
        /// <param name="searchArea">The area that we're looking for next elements in</param>
        /// <returns>The first element found in that area</returns>
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

        /// <summary>
        /// A helper function to add <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/> to an array of controls.
        /// </summary>
        /// <param name="controls">The controls to add to</param>
        /// <param name="additional">The additional controls to add to the controls list</param>
        public static void AddButtons(ref object[] controls, params object[] additional) {
            controls = controls.Concat(additional).ToArray();
        }

    }
}