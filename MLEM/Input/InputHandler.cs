using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Misc;

namespace MLEM.Input {
    /// <summary>
    /// An input handler is a more advanced wrapper around MonoGame's default input system.
    /// It includes keyboard, mouse, gamepad and touch states, as well as a new "pressed" state for keys and the ability for keyboard and gamepad repeat events.
    /// </summary>
    public class InputHandler : GameComponent {

        /// <summary>
        /// Contains all of the gestures that have finished during the last update call.
        /// To easily query these gestures, use <see cref="GetGesture"/> or <see cref="GetViewportGesture"/>.
        /// </summary>
        public readonly ReadOnlyCollection<GestureSample> Gestures;

        /// <summary>
        /// Set this field to false to disable keyboard handling for this input handler.
        /// </summary>
        public bool HandleKeyboard;
        /// <summary>
        /// Set this field to false to disable mouse handling for this input handler.
        /// </summary>
        public bool HandleMouse;
        /// <summary>
        /// Set this field to false to disable keyboard handling for this input handler.
        /// </summary>
        public bool HandleGamepads;
        /// <summary>
        /// Set this field to false to disable touch handling for this input handler.
        /// </summary>
        public bool HandleTouch;
        /// <summary>
        /// This is the amount of time that has to pass before the first keyboard repeat event is triggered.
        /// <seealso cref="KeyRepeatRate"/>
        /// </summary>
        public TimeSpan KeyRepeatDelay = TimeSpan.FromSeconds(0.65);
        /// <summary>
        /// This is the amount of time that has to pass between keyboard repeat events.
        /// <seealso cref="KeyRepeatDelay"/>
        /// </summary>
        public TimeSpan KeyRepeatRate = TimeSpan.FromSeconds(0.05);
        /// <summary>
        /// Set this field to false to disable keyboard repeat event handling.
        /// </summary>
        public bool HandleKeyboardRepeats = true;
        /// <summary>
        /// Set this field to false to disable gamepad repeat event handling.
        /// </summary>
        public bool HandleGamepadRepeats = true;
        /// <summary>
        /// This field represents the deadzone that gamepad <see cref="Buttons"/> have when input is queried for them using this input handler.
        /// A deadzone is the percentage (between 0 and 1) that an analog value has to exceed for it to be considered down (<see cref="IsGamepadButtonDown"/>) or pressed (<see cref="IsGamepadButtonPressed"/>).
        /// Querying of analog values is done using <see cref="GamepadExtensions.GetAnalogValue"/>.
        /// </summary>
        public float GamepadButtonDeadzone;
        /// <summary>
        /// Set this field to true to invert the press behavior of <see cref="IsKeyPressed"/>, <see cref="IsMouseButtonPressed"/>, <see cref="IsGamepadButtonPressed"/> and <see cref="IsPressed"/>.
        /// Inverted behavior means that, instead of an input counting as pressed when it was up in the last frame and is now down, it will be counted as pressed when it was down in the last frame and is now up.
        /// </summary>
        public bool InvertPressBehavior;

        /// <summary>
        /// An array of all <see cref="Keys"/>, <see cref="Buttons"/> and <see cref="MouseButton"/> values that are currently down.
        /// Additionally, <see cref="TryGetDownTime"/> or <see cref="GetDownTime"/> can be used to determine the amount of time that a given input has been down for.
        /// </summary>
        public GenericInput[] InputsDown { get; private set; } = Array.Empty<GenericInput>();
        /// <summary>
        /// An array of all <see cref="Keys"/>, <see cref="Buttons"/> and <see cref="MouseButton"/> that are currently considered pressed.
        /// An input is considered pressed if it was up in the last update, and is up in the current one.
        /// </summary>
        public GenericInput[] InputsPressed { get; private set; } = Array.Empty<GenericInput>();
        /// <summary>
        /// Contains the touch state from the last update call
        /// </summary>
        public TouchCollection LastTouchState { get; private set; }
        /// <summary>
        /// Contains the current touch state
        /// </summary>
        public TouchCollection TouchState { get; private set; }
        /// <summary>
        /// Contains the <see cref="LastTouchState"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public IList<TouchLocation> LastViewportTouchState { get; private set; }
        /// <summary>
        /// Contains the <see cref="TouchState"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public IList<TouchLocation> ViewportTouchState { get; private set; }
        /// <summary>
        /// Contains the amount of gamepads that are currently connected.
        /// This field is automatically updated in <see cref="Update()"/>
        /// </summary>
        public int ConnectedGamepads { get; private set; }
        /// <summary>
        /// Contains the mouse state from the last update call
        /// </summary>
        public MouseState LastMouseState { get; private set; }
        /// <summary>
        /// Contains the current mouse state
        /// </summary>
        public MouseState MouseState { get; private set; }
        /// <summary>
        /// Contains the position of the mouse from the last update call, extracted from <see cref="LastMouseState"/>
        /// </summary>
        public Point LastMousePosition => this.LastMouseState.Position;
        /// <summary>
        /// Contains the <see cref="LastMousePosition"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public Point LastViewportMousePosition => this.LastMousePosition + this.ViewportOffset;
        /// <summary>
        /// Contains the current position of the mouse, extracted from <see cref="MouseState"/>
        /// </summary>
        public Point MousePosition => this.MouseState.Position;
        /// <summary>
        /// Contains the <see cref="MousePosition"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public Point ViewportMousePosition => this.MousePosition + this.ViewportOffset;
        /// <summary>
        /// Contains the current scroll wheel value, in increments of 120
        /// </summary>
        public int ScrollWheel => this.MouseState.ScrollWheelValue;
        /// <summary>
        /// Contains the scroll wheel value from the last update call, in increments of 120
        /// </summary>
        public int LastScrollWheel => this.LastMouseState.ScrollWheelValue;
        /// <summary>
        /// Contains the keyboard state from the last update call
        /// </summary>
        public KeyboardState LastKeyboardState { get; private set; }
        /// <summary>
        /// Contains the current keyboard state
        /// </summary>
        public KeyboardState KeyboardState { get; private set; }

        private readonly GamePadState[] lastGamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly GamePadState[] gamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly DateTime[] lastGamepadButtonRepeats = new DateTime[GamePad.MaximumGamePadCount];
        private readonly bool[] triggerGamepadButtonRepeat = new bool[GamePad.MaximumGamePadCount];
        private readonly Buttons?[] heldGamepadButtons = new Buttons?[GamePad.MaximumGamePadCount];
        private readonly List<GestureSample> gestures = new List<GestureSample>();
        private readonly HashSet<(GenericInput, int)> consumedPresses = new HashSet<(GenericInput, int)>();

        private Point ViewportOffset => new Point(-this.Game.GraphicsDevice.Viewport.X, -this.Game.GraphicsDevice.Viewport.Y);
        private Dictionary<(GenericInput, int), DateTime> inputsDownAccum = new Dictionary<(GenericInput, int), DateTime>();
        private Dictionary<(GenericInput, int), DateTime> inputsDown = new Dictionary<(GenericInput, int), DateTime>();
        private DateTime lastKeyRepeat;
        private bool triggerKeyRepeat;
        private Keys heldKey;

        /// <summary>
        /// Creates a new input handler with optional initial values.
        /// </summary>
        /// <param name="game">The game instance that this input handler belongs to</param>
        /// <param name="handleKeyboard">If keyboard input should be handled</param>
        /// <param name="handleMouse">If mouse input should be handled</param>
        /// <param name="handleGamepads">If gamepad input should be handled</param>
        /// <param name="handleTouch">If touch input should be handled</param>
        public InputHandler(Game game, bool handleKeyboard = true, bool handleMouse = true, bool handleGamepads = true, bool handleTouch = true) : base(game) {
            this.HandleKeyboard = handleKeyboard;
            this.HandleMouse = handleMouse;
            this.HandleGamepads = handleGamepads;
            this.HandleTouch = handleTouch;
            this.Gestures = this.gestures.AsReadOnly();
        }

        /// <summary>
        /// Updates this input handler, querying pressed and released keys and calculating repeat events.
        /// Call this in your <see cref="Game.Update"/> method.
        /// </summary>
        public void Update() {
            var now = DateTime.UtcNow;
            var active = this.Game.IsActive;

            this.consumedPresses.Clear();

            if (this.HandleKeyboard) {
                this.LastKeyboardState = this.KeyboardState;
                this.KeyboardState = active ? Keyboard.GetState() : default;
                var pressedKeys = this.KeyboardState.GetPressedKeys();
                foreach (var pressed in pressedKeys)
                    this.AccumulateDown(pressed, -1);

                if (this.HandleKeyboardRepeats) {
                    this.triggerKeyRepeat = false;
                    // the key that started being held most recently should be the one being repeated
                    this.heldKey = pressedKeys.OrderBy(k => this.GetDownTime(k)).FirstOrDefault();
                    if (this.TryGetDownTime(this.heldKey, out var heldTime)) {
                        // if we've been holding the key longer than the initial delay...
                        if (heldTime >= this.KeyRepeatDelay) {
                            var diff = now - this.lastKeyRepeat;
                            // and we've been holding it for longer than a repeat...
                            if (diff >= this.KeyRepeatRate) {
                                this.lastKeyRepeat = now;
                                // then trigger a repeat, causing IsKeyPressed to be true once
                                this.triggerKeyRepeat = true;
                            }
                        }
                    }
                }
            }

            if (this.HandleMouse) {
                this.LastMouseState = this.MouseState;
                var state = Mouse.GetState();
                if (active && this.Game.GraphicsDevice.Viewport.Bounds.Contains(state.Position)) {
                    this.MouseState = state;
                    foreach (var button in MouseExtensions.MouseButtons) {
                        if (state.GetState(button) == ButtonState.Pressed)
                            this.AccumulateDown(button, -1);
                    }
                } else {
                    // mouse position and scroll wheel value should be preserved when the mouse is out of bounds
                    this.MouseState = new MouseState(state.X, state.Y, state.ScrollWheelValue, 0, 0, 0, 0, 0, state.HorizontalScrollWheelValue);
                }
            }

            if (this.HandleGamepads) {
                this.ConnectedGamepads = GamePad.MaximumGamePadCount;
                for (var i = 0; i < GamePad.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = GamePadState.Default;
                    if (GamePad.GetCapabilities(i).IsConnected) {
                        if (active) {
                            this.gamepads[i] = GamePad.GetState(i);
                            foreach (var button in EnumHelper.Buttons) {
                                if (this.IsGamepadButtonDown(button, i))
                                    this.AccumulateDown(button, i);
                            }
                        }
                    } else if (this.ConnectedGamepads > i) {
                        this.ConnectedGamepads = i;
                    }
                }

                if (this.HandleGamepadRepeats) {
                    for (var i = 0; i < this.ConnectedGamepads; i++) {
                        this.triggerGamepadButtonRepeat[i] = false;
                        this.heldGamepadButtons[i] = EnumHelper.Buttons
                            .Where(b => this.IsGamepadButtonDown(b, i))
                            .OrderBy(b => this.GetDownTime(b, i))
                            .Cast<Buttons?>().FirstOrDefault();
                        if (this.heldGamepadButtons[i].HasValue && this.TryGetDownTime(this.heldGamepadButtons[i].Value, out var heldTime, i)) {
                            if (heldTime >= this.KeyRepeatDelay) {
                                var diff = now - this.lastGamepadButtonRepeats[i];
                                if (diff >= this.KeyRepeatRate) {
                                    this.lastGamepadButtonRepeats[i] = now;
                                    this.triggerGamepadButtonRepeat[i] = true;
                                }
                            }
                        }
                    }
                }
            }

            if (this.HandleTouch) {
                this.LastTouchState = this.TouchState;
                this.LastViewportTouchState = this.ViewportTouchState;

                this.TouchState = active ? TouchPanel.GetState() : default;
                if (this.TouchState.Count > 0 && this.ViewportOffset != Point.Zero) {
                    this.ViewportTouchState = new List<TouchLocation>();
                    foreach (var touch in this.TouchState) {
                        touch.TryGetPreviousLocation(out var previous);
                        this.ViewportTouchState.Add(new TouchLocation(touch.Id, touch.State, touch.Position + this.ViewportOffset.ToVector2(), previous.State, previous.Position + this.ViewportOffset.ToVector2()));
                    }
                } else {
                    this.ViewportTouchState = this.TouchState;
                }

                this.gestures.Clear();
                while (active && TouchPanel.IsGestureAvailable)
                    this.gestures.Add(TouchPanel.ReadGesture());
            }

            if (this.inputsDownAccum.Count <= 0 && this.inputsDown.Count <= 0) {
                this.InputsPressed = Array.Empty<GenericInput>();
                this.InputsDown = Array.Empty<GenericInput>();
                this.inputsDown.Clear();
            } else {
                // if we're inverting press behavior, we need to check the press state for the inputs that were down in the last frame
                var down = this.InvertPressBehavior ? this.inputsDown : this.inputsDownAccum;
                this.InputsPressed = down.Keys.Where(kv => this.IsPressed(kv.Item1, kv.Item2)).Select(kv => kv.Item1).ToArray();
                this.InputsDown = this.inputsDownAccum.Keys.Select(kv => kv.Item1).ToArray();
                // swapping these collections means that we don't have to keep moving entries between them
                (this.inputsDown, this.inputsDownAccum) = (this.inputsDownAccum, this.inputsDown);
                this.inputsDownAccum.Clear();
            }
        }

        /// <inheritdoc cref="Update()"/>
        public override void Update(GameTime gameTime) {
            this.Update();
        }

        /// <summary>
        /// Returns the state of the <c>index</c>th gamepad from the last update call
        /// </summary>
        /// <param name="index">The zero-based gamepad index</param>
        /// <returns>The state of the gamepad last update</returns>
        public GamePadState GetLastGamepadState(int index) {
            return this.lastGamepads[index];
        }

        /// <summary>
        /// Returns the current state of the <c>index</c>th gamepad
        /// </summary>
        /// <param name="index">The zero-based gamepad index</param>
        /// <returns>The current state of the gamepad</returns>
        public GamePadState GetGamepadState(int index) {
            return this.gamepads[index];
        }

        /// <inheritdoc cref="Microsoft.Xna.Framework.Input.KeyboardState.IsKeyDown"/>
        public bool IsKeyDown(Keys key) {
            return this.KeyboardState.IsKeyDown(key);
        }

        /// <inheritdoc cref="Microsoft.Xna.Framework.Input.KeyboardState.IsKeyUp"/>
        public bool IsKeyUp(Keys key) {
            return this.KeyboardState.IsKeyUp(key);
        }

        /// <inheritdoc cref="Microsoft.Xna.Framework.Input.KeyboardState.IsKeyDown"/>
        public bool WasKeyDown(Keys key) {
            return this.LastKeyboardState.IsKeyDown(key);
        }

        /// <inheritdoc cref="Microsoft.Xna.Framework.Input.KeyboardState.IsKeyUp"/>
        public bool WasKeyUp(Keys key) {
            return this.LastKeyboardState.IsKeyUp(key);
        }

        /// <summary>
        /// Returns whether the given key is considered pressed.
        /// A key is considered pressed if it was not down the last update call, but is down the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// If <see cref="HandleKeyboardRepeats"/> is true, this method will also return true to signify a key repeat.
        /// </summary>
        /// <param name="key">The key to query</param>
        /// <returns>If the key is pressed</returns>
        public bool IsKeyPressed(Keys key) {
            // if the queried key is the held key and a repeat should be triggered, return true
            if (this.HandleKeyboardRepeats && key == this.heldKey && this.triggerKeyRepeat)
                return true;
            return this.IsKeyPressedIgnoreRepeats(key);
        }

        /// <summary>
        /// Returns whether the given key is considered pressed.
        /// A key is considered pressed if it was not down the last update call, but is down the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// This has the same behavior as <see cref="IsKeyPressed"/>, but ignores keyboard repeat events.
        /// If <see cref="HandleKeyboardRepeats"/> is false, this method does the same as <see cref="IsKeyPressed"/>.
        /// </summary>
        /// <param name="key">The key to query</param>
        /// <returns>If the key is pressed</returns>
        public bool IsKeyPressedIgnoreRepeats(Keys key) {
            if (this.InvertPressBehavior)
                return this.WasKeyDown(key) && this.IsKeyUp(key);
            return this.WasKeyUp(key) && this.IsKeyDown(key);
        }

        /// <summary>
        /// Returns if the given key is considered pressed, and if the press has not been consumed yet using <see cref="TryConsumeKeyPressed"/>.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>If the key is pressed and the press is not consumed yet.</returns>
        public bool IsKeyPressedAvailable(Keys key) {
            return this.IsKeyPressed(key) && !this.IsPressConsumed(key);
        }

        /// <summary>
        /// Returns whether the given key is considered pressed, and marks the press as consumed if it is.
        /// A key is considered pressed if it was not down the last update call, but is down the current update call.
        /// A key press is considered consumed if this method has already returned true previously since the last <see cref="Update()"/> call.
        /// If <see cref="HandleKeyboardRepeats"/> is true, this method will also return true to signify a key repeat.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>If the key is pressed and the press is not consumed yet.</returns>
        public bool TryConsumeKeyPressed(Keys key) {
            if (this.IsKeyPressedAvailable(key)) {
                this.consumedPresses.Add((key, -1));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether the given modifier key is down.
        /// </summary>
        /// <param name="modifier">The modifier key</param>
        /// <returns>If the modifier key is down</returns>
        public bool IsModifierKeyDown(ModifierKey modifier) {
            foreach (var key in modifier.GetKeys()) {
                if (this.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether the given mouse button is currently down.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether or not the queried button is down</returns>
        public bool IsMouseButtonDown(MouseButton button) {
            return this.MouseState.GetState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Returns whether the given mouse button is currently up.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether or not the queried button is up</returns>
        public bool IsMouseButtonUp(MouseButton button) {
            return this.MouseState.GetState(button) == ButtonState.Released;
        }

        /// <summary>
        /// Returns whether the given mouse button was down the last update call.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether or not the queried button was down</returns>
        public bool WasMouseButtonDown(MouseButton button) {
            return this.LastMouseState.GetState(button) == ButtonState.Pressed;
        }

        /// <summary>
        /// Returns whether the given mouse button was up the last update call.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether or not the queried button was up</returns>
        public bool WasMouseButtonUp(MouseButton button) {
            return this.LastMouseState.GetState(button) == ButtonState.Released;
        }

        /// <summary>
        /// Returns whether the given mouse button is considered pressed.
        /// A mouse button is considered pressed if it was up the last update call, and is down the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether the button is pressed</returns>
        public bool IsMouseButtonPressed(MouseButton button) {
            if (this.InvertPressBehavior)
                return this.WasMouseButtonDown(button) && this.IsMouseButtonUp(button);
            return this.WasMouseButtonUp(button) && this.IsMouseButtonDown(button);
        }

        /// <summary>
        /// Returns if the given mouse button is considered pressed, and if the press has not been consumed yet using <see cref="TryConsumeMouseButtonPressed"/>.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>If the button is pressed and the press is not consumed yet.</returns>
        public bool IsMouseButtonPressedAvailable(MouseButton button) {
            return this.IsMouseButtonPressed(button) && !this.IsPressConsumed(button);
        }

        /// <summary>
        /// Returns whether the given mouse button is considered pressed, and marks the press as consumed if it is.
        /// A mouse button is considered pressed if it was up the last update call, and is down the current update call.
        /// A mouse button press is considered consumed if this method has already returned true previously since the last <see cref="Update()"/> call.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <returns>If the button is pressed and the press is not consumed yet.</returns>
        public bool TryConsumeMouseButtonPressed(MouseButton button) {
            if (this.IsMouseButtonPressedAvailable(button)) {
                this.consumedPresses.Add((button, -1));
                return true;
            }
            return false;
        }

        /// <inheritdoc cref="GamePadState.IsButtonDown"/>
        public bool IsGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++) {
                    if (this.GetGamepadState(i).GetAnalogValue(button) > this.GamepadButtonDeadzone)
                        return true;
                }
                return false;
            }
            return this.GetGamepadState(index).GetAnalogValue(button) > this.GamepadButtonDeadzone;
        }

        /// <inheritdoc cref="GamePadState.IsButtonUp"/>
        public bool IsGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++) {
                    if (this.GetGamepadState(i).GetAnalogValue(button) <= this.GamepadButtonDeadzone)
                        return true;
                }
                return false;
            }
            return this.GetGamepadState(index).GetAnalogValue(button) <= this.GamepadButtonDeadzone;
        }

        /// <inheritdoc cref="GamePadState.IsButtonDown"/>
        public bool WasGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++) {
                    if (this.GetLastGamepadState(i).GetAnalogValue(button) > this.GamepadButtonDeadzone)
                        return true;
                }
                return false;
            }
            return this.GetLastGamepadState(index).GetAnalogValue(button) > this.GamepadButtonDeadzone;
        }

        /// <inheritdoc cref="GamePadState.IsButtonUp"/>
        public bool WasGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++) {
                    if (this.GetLastGamepadState(i).GetAnalogValue(button) <= this.GamepadButtonDeadzone)
                        return true;
                }
                return false;
            }
            return this.GetLastGamepadState(index).GetAnalogValue(button) <= this.GamepadButtonDeadzone;
        }

        /// <summary>
        /// Returns whether the given gamepad button on the given index is considered pressed.
        /// A gamepad button is considered pressed if it was down the last update call, and is up the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// If <see cref="HandleGamepadRepeats"/> is true, this method will also return true to signify a gamepad button repeat.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <param name="index">The zero-based index of the gamepad, or -1 for any gamepad</param>
        /// <returns>Whether the given button is pressed</returns>
        public bool IsGamepadButtonPressed(Buttons button, int index = -1) {
            if (this.HandleGamepadRepeats) {
                if (index < 0) {
                    for (var i = 0; i < this.ConnectedGamepads; i++) {
                        if (this.heldGamepadButtons[i] == button && this.triggerGamepadButtonRepeat[i])
                            return true;
                    }
                } else if (this.heldGamepadButtons[index] == button && this.triggerGamepadButtonRepeat[index]) {
                    return true;
                }
            }
            return this.IsGamepadButtonPressedIgnoreRepeats(button, index);
        }

        /// <summary>
        /// Returns whether the given key is considered pressed.
        /// A gamepad button is considered pressed if it was down the last update call, and is up the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// This has the same behavior as <see cref="IsGamepadButtonPressed"/>, but ignores gamepad repeat events. 
        /// If <see cref="HandleGamepadRepeats"/> is false, this method does the same as <see cref="IsGamepadButtonPressed"/>.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <param name="index">The zero-based index of the gamepad, or -1 for any gamepad</param>
        /// <returns>Whether the given button is pressed</returns>
        public bool IsGamepadButtonPressedIgnoreRepeats(Buttons button, int index = -1) {
            if (this.InvertPressBehavior)
                return this.WasGamepadButtonDown(button, index) && this.IsGamepadButtonUp(button, index);
            return this.WasGamepadButtonUp(button, index) && this.IsGamepadButtonDown(button, index);
        }

        /// <summary>
        /// Returns if the given gamepad button is considered pressed, and if the press has not been consumed yet using <see cref="TryConsumeMouseButtonPressed"/>.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <param name="index">The zero-based index of the gamepad, or -1 for any gamepad.</param>
        /// <returns>Whether the given button is pressed and the press is not consumed yet.</returns>
        public bool IsGamepadButtonPressedAvailable(Buttons button, int index = -1) {
            return this.IsGamepadButtonPressed(button) && !this.IsPressConsumed(button, index) && (index < 0 || !this.IsPressConsumed(button));
        }

        /// <summary>
        /// Returns whether the given gamepad button on the given index is considered pressed, and marks the press as consumed if it is.
        /// A gamepad button is considered pressed if it was down the last update call, and is up the current update call.
        /// A gamepad button press is considered consumed if this method has already returned true previously since the last <see cref="Update()"/> call.
        /// If <see cref="HandleGamepadRepeats"/> is true, this method will also return true to signify a gamepad button repeat.
        /// </summary>
        /// <param name="button">The button to query.</param>
        /// <param name="index">The zero-based index of the gamepad, or -1 for any gamepad.</param>
        /// <returns>Whether the given button is pressed and the press is not consumed yet.</returns>
        public bool TryConsumeGamepadButtonPressed(Buttons button, int index = -1) {
            if (this.IsGamepadButtonPressedAvailable(button, index)) {
                this.consumedPresses.Add((button, index));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Queries for a gesture of a given type that finished during the current update call.
        /// </summary>
        /// <param name="type">The type of gesture to query for</param>
        /// <param name="sample">The resulting gesture sample, or default if there isn't one</param>
        /// <returns>True if a gesture of the type was found, otherwise false</returns>
        public bool GetGesture(GestureType type, out GestureSample sample) {
            foreach (var gesture in this.Gestures) {
                if (type.HasFlag(gesture.GestureType)) {
                    sample = gesture;
                    return true;
                }
            }
            sample = default;
            return false;
        }

        /// <summary>
        /// Queries for a gesture of the given type that finished during the current update call.
        /// Unlike <see cref="GetGesture"/>, the return value of this method takes the <see cref="GraphicsDevice.Viewport"/> into account.
        /// </summary>
        /// <param name="type">The type of gesture to query for</param>
        /// <param name="sample">The resulting gesture sample with the <see cref="GraphicsDevice.Viewport"/> taken into account, or default if there isn't one</param>
        /// <returns>True if a gesture of the type was found, otherwise false</returns>
        public bool GetViewportGesture(GestureType type, out GestureSample sample) {
            if (this.GetGesture(type, out var original)) {
                sample = new GestureSample(original.GestureType, original.Timestamp, original.Position + this.ViewportOffset.ToVector2(), original.Position2 + this.ViewportOffset.ToVector2(), original.Delta, original.Delta2);
                return true;
            }
            sample = default;
            return false;
        }

        /// <summary>
        /// Returns if a given control of any kind is down.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose down state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is down</returns>
        /// <exception cref="ArgumentException">If the passed control isn't of a supported type</exception>
        public bool IsDown(GenericInput control, int index = -1) {
            switch (control.Type) {
                case GenericInput.InputType.Keyboard:
                    return this.IsKeyDown(control);
                case GenericInput.InputType.Gamepad:
                    return this.IsGamepadButtonDown(control, index);
                case GenericInput.InputType.Mouse:
                    return this.IsMouseButtonDown(control);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind is up.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose up state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is up.</returns>
        /// <exception cref="ArgumentException">If the passed control isn't of a supported type</exception>
        public bool IsUp(GenericInput control, int index = -1) {
            switch (control.Type) {
                case GenericInput.InputType.Keyboard:
                    return this.IsKeyUp(control);
                case GenericInput.InputType.Gamepad:
                    return this.IsGamepadButtonUp(control, index);
                case GenericInput.InputType.Mouse:
                    return this.IsMouseButtonUp(control);
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is pressed.</returns>
        /// <exception cref="ArgumentException">If the passed control isn't of a supported type</exception>
        public bool IsPressed(GenericInput control, int index = -1) {
            switch (control.Type) {
                case GenericInput.InputType.Keyboard:
                    return this.IsKeyPressed(control);
                case GenericInput.InputType.Gamepad:
                    return this.IsGamepadButtonPressed(control, index);
                case GenericInput.InputType.Mouse:
                    return this.IsMouseButtonPressed(control);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed, and if the press has not been consumed yet using <see cref="TryConsumePressed"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control is pressed and the press is not consumed yet.</returns>
        public bool IsPressedAvailable(GenericInput control, int index = -1) {
            switch (control.Type) {
                case GenericInput.InputType.Keyboard:
                    return this.IsKeyPressedAvailable(control);
                case GenericInput.InputType.Gamepad:
                    return this.IsGamepadButtonPressedAvailable(control, index);
                case GenericInput.InputType.Mouse:
                    return this.IsMouseButtonPressedAvailable(control);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed and available, and marks the press as consumed if it is.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control is pressed and the press is not consumed yet.</returns>
        public bool TryConsumePressed(GenericInput control, int index = -1) {
            switch (control.Type) {
                case GenericInput.InputType.Keyboard:
                    return this.TryConsumeKeyPressed(control);
                case GenericInput.InputType.Gamepad:
                    return this.TryConsumeGamepadButtonPressed(control, index);
                case GenericInput.InputType.Mouse:
                    return this.TryConsumeMouseButtonPressed(control);
                default:
                    return false;
            }
        }

        /// <inheritdoc cref="IsDown"/>
        public bool IsAnyDown(params GenericInput[] controls) {
            foreach (var control in controls) {
                if (this.IsDown(control))
                    return true;
            }
            return false;
        }

        /// <inheritdoc cref="IsUp"/>
        public bool IsAnyUp(params GenericInput[] controls) {
            foreach (var control in controls) {
                if (this.IsUp(control))
                    return true;
            }
            return false;
        }

        /// <inheritdoc cref="IsPressed"/>
        public bool IsAnyPressed(params GenericInput[] controls) {
            foreach (var control in controls) {
                if (this.IsPressed(control))
                    return true;
            }
            return false;
        }

        /// <inheritdoc cref="IsPressedAvailable"/>
        public bool IsAnyPressedAvailable(params GenericInput[] controls) {
            foreach (var control in controls) {
                if (this.IsPressedAvailable(control))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to retrieve the amount of time that a given <see cref="GenericInput"/> has been held down for.
        /// If the input is currently down, this method returns true and the amount of time that it has been down for is stored in <paramref name="downTime"/>.
        /// </summary>
        /// <param name="input">The input whose down time to query.</param>
        /// <param name="downTime">The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the input is currently being held.</returns>
        public bool TryGetDownTime(GenericInput input, out TimeSpan downTime, int index = -1) {
            if (this.inputsDown.TryGetValue((input, index), out var start)) {
                downTime = DateTime.UtcNow - start;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the amount of time that a given <see cref="GenericInput"/> has been held down for.
        /// If this input isn't currently own, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="input">The input whose down time to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</returns>
        public TimeSpan GetDownTime(GenericInput input, int index = -1) {
            this.TryGetDownTime(input, out var time, index);
            return time;
        }

        /// <summary>
        /// Returns whether the given <see cref="GenericInput"/>'s press state has been consumed using <see cref="TryConsumePressed"/>.
        /// If an input has been consumed, <see cref="IsPressedAvailable"/> and <see cref="TryConsumePressed"/> will always return false for that input.
        /// </summary>
        /// <param name="input">The input to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether a press has been consumed for the given input.</returns>
        public bool IsPressConsumed(GenericInput input, int index = -1) {
            return this.consumedPresses.Contains((input, index));
        }

        private void AccumulateDown(GenericInput input, int index) {
            this.inputsDownAccum.Add((input, index), this.inputsDown.TryGetValue((input, index), out var start) ? start : DateTime.UtcNow);
        }

        /// <summary>
        /// Helper function to enable gestures for a <see cref="TouchPanel"/> easily.
        /// Note that, if other gestures were previously enabled, they will not get overridden.
        /// </summary>
        /// <param name="gestures">The gestures to enable</param>
        public static void EnableGestures(params GestureType[] gestures) {
            foreach (var gesture in gestures)
                TouchPanel.EnabledGestures |= gesture;
        }

        /// <summary>
        /// Helper function to disable gestures for a <see cref="TouchPanel"/> easily.
        /// </summary>
        /// <param name="gestures">The gestures to disable</param>
        public static void DisableGestures(params GestureType[] gestures) {
            foreach (var gesture in gestures)
                TouchPanel.EnabledGestures &= ~gesture;
        }

        /// <summary>
        /// Helper function to enable or disable the given gestures for a <see cref="TouchPanel"/> easily.
        /// This method is equivalent to calling <see cref="EnableGestures"/> if the <c>enabled</c> value is true and calling <see cref="DisableGestures"/> if it is false.
        /// Note that, if other gestures were previously enabled, they will not get overridden.
        /// </summary>
        /// <param name="enabled">Whether to enable or disable the gestures</param>
        /// <param name="gestures">The gestures to enable or disable</param>
        public static void SetGesturesEnabled(bool enabled, params GestureType[] gestures) {
            if (enabled) {
                EnableGestures(gestures);
            } else {
                DisableGestures(gestures);
            }
        }

    }
}