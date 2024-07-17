using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using static MLEM.Input.GenericInput;

namespace MLEM.Input {
    /// <summary>
    /// An input handler is a more advanced wrapper around MonoGame's default input system.
    /// It includes keyboard, mouse, gamepad and touch handling through the <see cref="GenericInput"/> wrapper, as well as a new "pressed" state for inputs, the ability for keyboard and gamepad repeat events, and the ability to track down, up and press times for inputs.
    /// </summary>
    public class InputHandler : GameComponent {

        /// <summary>
        /// All values of the <see cref="Buttons"/> enum.
        /// </summary>
        public static readonly Buttons[] AllButtons =
#if NET6_0_OR_GREATER
            Enum.GetValues<Buttons>();
#else
            (Buttons[]) Enum.GetValues(typeof(Buttons));
#endif
        /// <summary>
        /// All values of the <see cref="Keys"/> enum.
        /// </summary>
        public static readonly Keys[] AllKeys =
#if NET6_0_OR_GREATER
            Enum.GetValues<Keys>();
#else
            (Keys[]) Enum.GetValues(typeof(Keys));
#endif

#if FNA
        private const int MaximumGamePadCount = 4;
#else
        private static readonly int MaximumGamePadCount = GamePad.MaximumGamePadCount;
#endif
        private static readonly TouchLocation[] EmptyTouchLocations = new TouchLocation[0];
        private static readonly GenericInput[] EmptyGenericInputs = new GenericInput[0];

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
        /// A deadzone is the percentage (between 0 and 1) that an analog value has to exceed for it to be considered down (<see cref="IsDown"/>) or pressed (<see cref="IsPressed"/>).
        /// Querying of analog values is done using <see cref="GamepadExtensions.GetAnalogValue"/>.
        /// </summary>
        public float GamepadButtonDeadzone;
        /// <summary>
        /// Set this field to true to invert the press behavior of <see cref="IsPressed"/>.
        /// Inverted behavior means that, instead of an input counting as pressed when it was up in the last frame and is now down, it will be counted as pressed when it was down in the last frame and is now up.
        /// </summary>
        public bool InvertPressBehavior;
        /// <summary>
        /// If your project already handles the processing of MonoGame's gestures elsewhere, you can set this field to true to ensure that this input handler's gesture handling does not override your own, since <see cref="GestureSample"/> objects can only be retrieved once and are then removed from the <see cref="TouchPanel"/>'s queue.
        /// If this value is set to true, but you still want to be able to use <see cref="Gestures"/>, <see cref="GetGesture"/>, and <see cref="GetViewportGesture"/>, you can make this input handler aware of a gesture for the duration of the update frame that you added it on by using <see cref="AddExternalGesture"/>.
        /// For more info, see https://mlem.ellpeck.de/articles/input.html#external-gesture-handling.
        /// </summary>
        public bool ExternalGestureHandling;

        /// <summary>
        /// An array of all <see cref="Keys"/>, <see cref="Buttons"/> and <see cref="MouseButton"/> values that are currently down.
        /// Additionally, <see cref="TryGetDownTime"/> or <see cref="GetDownTime"/> can be used to determine the amount of time that a given input has been down for.
        /// </summary>
        public GenericInput[] InputsDown { get; private set; } = InputHandler.EmptyGenericInputs;
        /// <summary>
        /// An array of all <see cref="Keys"/>, <see cref="Buttons"/> and <see cref="MouseButton"/> that are currently considered pressed.
        /// An input is considered pressed if it was up in the last update, and is up in the current one.
        /// </summary>
        public GenericInput[] InputsPressed { get; private set; } = InputHandler.EmptyGenericInputs;
        /// <summary>
        /// Contains the touch state from the last update call
        /// </summary>
        public TouchCollection LastTouchState { get; private set; } = new TouchCollection(InputHandler.EmptyTouchLocations);
        /// <summary>
        /// Contains the current touch state
        /// </summary>
        public TouchCollection TouchState { get; private set; } = new TouchCollection(InputHandler.EmptyTouchLocations);
        /// <summary>
        /// Contains the <see cref="LastTouchState"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public IList<TouchLocation> LastViewportTouchState { get; private set; } = new List<TouchLocation>();
        /// <summary>
        /// Contains the <see cref="TouchState"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public IList<TouchLocation> ViewportTouchState { get; private set; } = new List<TouchLocation>();
        /// <summary>
        /// Contains the amount of gamepads that are currently connected. Note that this value will be set to 0 if <see cref="HandleGamepads"/> is false.
        /// This field is automatically updated in <see cref="Update()"/>.
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
        public Point LastMousePosition => new Point(this.LastMouseState.X, this.LastMouseState.Y);
        /// <summary>
        /// Contains the <see cref="LastMousePosition"/>, but with the <see cref="GraphicsDevice.Viewport"/> taken into account.
        /// </summary>
        public Point LastViewportMousePosition => this.LastMousePosition + this.ViewportOffset;
        /// <summary>
        /// Contains the current position of the mouse, extracted from <see cref="MouseState"/>
        /// </summary>
        public Point MousePosition => new Point(this.MouseState.X, this.MouseState.Y);
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

        private readonly GamePadState[] lastGamepads = new GamePadState[InputHandler.MaximumGamePadCount];
        private readonly GamePadState[] gamepads = new GamePadState[InputHandler.MaximumGamePadCount];
        private readonly DateTime[] lastGamepadButtonRepeats = new DateTime[InputHandler.MaximumGamePadCount];
        private readonly bool[] triggerGamepadButtonRepeat = new bool[InputHandler.MaximumGamePadCount];
        private readonly Buttons?[] heldGamepadButtons = new Buttons?[InputHandler.MaximumGamePadCount];
        private readonly List<GestureSample> gestures = new List<GestureSample>();
        private readonly HashSet<(GenericInput, int)> consumedPresses = new HashSet<(GenericInput, int)>();
        private readonly Dictionary<(GenericInput, int), DateTime> inputUpTimes = new Dictionary<(GenericInput, int), DateTime>();
        private readonly Dictionary<(GenericInput, int), DateTime> inputDownTimes = new Dictionary<(GenericInput, int), DateTime>();
        private readonly Dictionary<(GenericInput, int), DateTime> inputPressedTimes = new Dictionary<(GenericInput, int), DateTime>();

        private Point ViewportOffset => new Point(-this.Game.GraphicsDevice.Viewport.X, -this.Game.GraphicsDevice.Viewport.Y);
        private Dictionary<(GenericInput, int), DateTime> inputsDownAccum = new Dictionary<(GenericInput, int), DateTime>();
        private Dictionary<(GenericInput, int), DateTime> inputsDown = new Dictionary<(GenericInput, int), DateTime>();
        private DateTime lastKeyRepeat;
        private bool triggerKeyRepeat;
        private Keys heldKey;

        /// <summary>
        /// Creates a new input handler with optional initial values.
        /// </summary>
        /// <param name="game">The game instance that this input handler belongs to.</param>
        /// <param name="handleKeyboard">The initial value for <see cref="HandleKeyboard"/>, which determines whether this input handler handles keyboard inputs.</param>
        /// <param name="handleMouse">The initial value for <see cref="HandleMouse"/>, which determines whether this input handler handles mouse inputs.</param>
        /// <param name="handleGamepads">The initial value for <see cref="HandleGamepads"/>, which determines whether this input handler handles gamepad inputs.</param>
        /// <param name="handleTouch">The initial value for <see cref="HandleTouch"/>, which determines whether this input handler handles touch inputs.</param>
        /// <param name="externalGestureHandling">The initial value for <see cref="ExternalGestureHandling"/>, which determines whether gestures will be supplied using <see cref="AddExternalGesture"/> (or this input handler should handle gestures itself).</param>
        public InputHandler(Game game, bool handleKeyboard = true, bool handleMouse = true, bool handleGamepads = true, bool handleTouch = true, bool externalGestureHandling = false) : base(game) {
            this.HandleKeyboard = handleKeyboard;
            this.HandleMouse = handleMouse;
            this.HandleGamepads = handleGamepads;
            this.HandleTouch = handleTouch;
            this.ExternalGestureHandling = externalGestureHandling;
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

            this.LastKeyboardState = this.KeyboardState;
            if (this.HandleKeyboard) {
                this.KeyboardState = active ? Keyboard.GetState() : default;
                var pressedKeys = this.KeyboardState.GetPressedKeys();
                foreach (var pressed in pressedKeys)
                    this.AccumulateDown(pressed, -1);

                this.triggerKeyRepeat = false;
                if (this.HandleKeyboardRepeats) {
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
                } else {
                    this.heldKey = Keys.None;
                }
            } else {
                this.KeyboardState = default;
                this.triggerKeyRepeat = false;
                this.heldKey = Keys.None;
            }

            this.LastMouseState = this.MouseState;
            if (this.HandleMouse) {
                var state = Mouse.GetState();
                if (active && this.Game.GraphicsDevice.Viewport.Bounds.Contains(state.X, state.Y)) {
                    this.MouseState = state;
                    foreach (var button in MouseExtensions.MouseButtons) {
                        if (state.GetState(button) == ButtonState.Pressed)
                            this.AccumulateDown(button, -1);
                    }
                } else {
                    // mouse position and scroll wheel value should be preserved when the mouse is out of bounds
#if FNA
                    this.MouseState = new MouseState(state.X, state.Y, state.ScrollWheelValue, 0, 0, 0, 0, 0);
#else
                    this.MouseState = new MouseState(state.X, state.Y, state.ScrollWheelValue, 0, 0, 0, 0, 0, state.HorizontalScrollWheelValue);
#endif
                }
            } else {
                this.MouseState = default;
            }

            if (this.HandleGamepads) {
                this.ConnectedGamepads = InputHandler.MaximumGamePadCount;
                for (var i = 0; i < InputHandler.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = default;
                    if (GamePad.GetCapabilities((PlayerIndex) i).IsConnected) {
                        if (active) {
                            this.gamepads[i] = GamePad.GetState((PlayerIndex) i);
                            foreach (var button in InputHandler.AllButtons) {
                                if (this.IsDown(button, i))
                                    this.AccumulateDown(button, i);
                            }
                        }
                    } else if (this.ConnectedGamepads > i) {
                        this.ConnectedGamepads = i;
                    }
                }

                for (var i = 0; i < this.ConnectedGamepads; i++) {
                    this.triggerGamepadButtonRepeat[i] = false;
                    if (this.HandleGamepadRepeats) {
                        this.heldGamepadButtons[i] = InputHandler.AllButtons
                            .Where(b => this.IsDown(b, i))
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
                    } else {
                        this.heldGamepadButtons[i] = null;
                    }
                }
            } else {
                this.ConnectedGamepads = 0;
                for (var i = 0; i < InputHandler.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = default;
                    this.triggerGamepadButtonRepeat[i] = false;
                    this.heldGamepadButtons[i] = null;
                }
            }

            this.LastTouchState = this.TouchState;
            this.LastViewportTouchState = this.ViewportTouchState;
            if (this.HandleTouch) {
                this.TouchState = active ? TouchPanel.GetState() : new TouchCollection(InputHandler.EmptyTouchLocations);
                if (this.TouchState.Count > 0 && this.ViewportOffset != Point.Zero) {
                    this.ViewportTouchState = new List<TouchLocation>();
                    foreach (var touch in this.TouchState) {
                        touch.TryGetPreviousLocation(out var previous);
                        var offset = new Vector2(this.ViewportOffset.X, this.ViewportOffset.Y);
                        this.ViewportTouchState.Add(new TouchLocation(touch.Id, touch.State, touch.Position + offset, previous.State, previous.Position + offset));
                    }
                } else {
                    this.ViewportTouchState = this.TouchState;
                }

                // we still want to clear gestures when handling externally to maintain the per-frame gesture system
                this.gestures.Clear();
                if (active && !this.ExternalGestureHandling) {
                    while (TouchPanel.IsGestureAvailable)
                        this.gestures.Add(TouchPanel.ReadGesture());
                }
            } else {
                this.TouchState = new TouchCollection(InputHandler.EmptyTouchLocations);
                this.ViewportTouchState = this.TouchState;
                this.gestures.Clear();
            }

            if (this.inputsDownAccum.Count <= 0 && this.inputsDown.Count <= 0) {
                this.InputsPressed = InputHandler.EmptyGenericInputs;
                this.InputsDown = InputHandler.EmptyGenericInputs;
            } else {
                // handle pressed inputs
                var pressed = new List<GenericInput>();
                // if we're inverting press behavior, we need to check the press state for the inputs that were down in the last frame
                foreach (var key in (this.InvertPressBehavior ? this.inputsDown : this.inputsDownAccum).Keys) {
                    if (this.IsPressed(key.Item1, key.Item2)) {
                        this.inputPressedTimes[key] = DateTime.UtcNow;
                        pressed.Add(key.Item1);
                    }
                }
                this.InputsPressed = pressed.ToArray();

                // handle inputs that changed between down and up
                foreach (var key in this.inputsDown.Keys) {
                    if (!this.inputsDownAccum.ContainsKey(key))
                        this.inputUpTimes[key] = DateTime.UtcNow;
                }
                foreach (var key in this.inputsDownAccum.Keys) {
                    if (!this.inputsDown.ContainsKey(key))
                        this.inputDownTimes[key] = DateTime.UtcNow;
                }

                // handle inputs that are currently down
                this.InputsDown = this.inputsDownAccum.Keys.Select(key => key.Item1).ToArray();
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

        /// <summary>
        /// Returns whether the given modifier key is down.
        /// </summary>
        /// <param name="modifier">The modifier key</param>
        /// <returns>If the modifier key is down</returns>
        public bool IsModifierKeyDown(ModifierKey modifier) {
            foreach (var key in modifier.GetKeys()) {
                if (this.IsDown(key))
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
                var offset = new Vector2(this.ViewportOffset.X, this.ViewportOffset.Y);
                sample = new GestureSample(original.GestureType, original.Timestamp, original.Position + offset, original.Position2 + offset, original.Delta, original.Delta2);
                return true;
            }
            sample = default;
            return false;
        }

        /// <summary>
        /// Adds a gesture to the <see cref="Gestures"/> collection and allows it to be queried using <see cref="GetGesture"/> and <see cref="GetViewportGesture"/> for the duration of the update frame that it was added on.
        /// This method should be used when <see cref="ExternalGestureHandling"/> is set to true, but <see cref="GetGesture"/> and <see cref="GetViewportGesture"/> should still be available.
        /// For more info, see https://mlem.ellpeck.de/articles/input.html#external-gesture-handling.
        /// </summary>
        /// <param name="sample">The gesture sample to add.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="ExternalGestureHandling"/> is false.</exception>
        public void AddExternalGesture(GestureSample sample) {
            if (!this.ExternalGestureHandling)
                throw new InvalidOperationException($"Cannot add external gestures if {nameof(this.ExternalGestureHandling)} is false");
            this.gestures.Add(sample);
        }

        /// <summary>
        /// Returns if a given control of any kind is down.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose down state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is down</returns>
        public bool IsDown(GenericInput control, int index = -1) {
            switch (control.Type) {
                case InputType.Keyboard:
                    return this.KeyboardState.IsKeyDown(control);
                case InputType.Gamepad:
                    if (index < 0) {
                        for (var i = 0; i < this.ConnectedGamepads; i++) {
                            if (this.GetGamepadState(i).GetAnalogValue(control) > this.GamepadButtonDeadzone)
                                return true;
                        }
                        return false;
                    }
                    return this.GetGamepadState(index).GetAnalogValue(control) > this.GamepadButtonDeadzone;
                case InputType.Mouse:
                    return this.MouseState.GetState(control) == ButtonState.Pressed;
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
        public bool IsUp(GenericInput control, int index = -1) {
            switch (control.Type) {
                case InputType.Keyboard:
                    return this.KeyboardState.IsKeyUp(control);
                case InputType.Gamepad:
                    if (index < 0) {
                        for (var i = 0; i < this.ConnectedGamepads; i++) {
                            if (this.GetGamepadState(i).GetAnalogValue(control) <= this.GamepadButtonDeadzone)
                                return true;
                        }
                        return false;
                    }
                    return this.GetGamepadState(index).GetAnalogValue(control) <= this.GamepadButtonDeadzone;
                case InputType.Mouse:
                    return this.MouseState.GetState(control) == ButtonState.Released;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind was down in the last update call.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose down state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control was down</returns>
        public bool WasDown(GenericInput control, int index = -1) {
            switch (control.Type) {
                case InputType.Keyboard:
                    return this.LastKeyboardState.IsKeyDown(control);
                case InputType.Gamepad:
                    if (index < 0) {
                        for (var i = 0; i < this.ConnectedGamepads; i++) {
                            if (this.GetLastGamepadState(i).GetAnalogValue(control) > this.GamepadButtonDeadzone)
                                return true;
                        }
                        return false;
                    }
                    return this.GetLastGamepadState(index).GetAnalogValue(control) > this.GamepadButtonDeadzone;
                case InputType.Mouse:
                    return this.LastMouseState.GetState(control) == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind was up in the last update call.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose up state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control was up.</returns>
        public bool WasUp(GenericInput control, int index = -1) {
            switch (control.Type) {
                case InputType.Keyboard:
                    return this.LastKeyboardState.IsKeyUp(control);
                case InputType.Gamepad:
                    if (index < 0) {
                        for (var i = 0; i < this.ConnectedGamepads; i++) {
                            if (this.GetLastGamepadState(i).GetAnalogValue(control) <= this.GamepadButtonDeadzone)
                                return true;
                        }
                        return false;
                    }
                    return this.GetLastGamepadState(index).GetAnalogValue(control) <= this.GamepadButtonDeadzone;
                case InputType.Mouse:
                    return this.LastMouseState.GetState(control) == ButtonState.Released;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed.
        /// If <see cref="HandleKeyboardRepeats"/> or <see cref="HandleGamepadRepeats"/> are true, this method will also return true to signify a key or gamepad button repeat.
        /// An input is considered pressed if it was not down the last update call, but is down the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// </summary>
        /// <param name="control">The control whose pressed state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is pressed.</returns>
        public bool IsPressed(GenericInput control, int index = -1) {
            // handle repeat events for specific inputs, and delegate to default "ignore repeats" behavior otherwise
            switch (control.Type) {
                case InputType.Keyboard:
                    if (this.HandleKeyboardRepeats && (Keys) control == this.heldKey && this.triggerKeyRepeat)
                        return true;
                    break;
                case InputType.Gamepad:
                    if (this.HandleGamepadRepeats) {
                        if (index < 0) {
                            for (var i = 0; i < this.ConnectedGamepads; i++) {
                                if (this.heldGamepadButtons[i] == (Buttons) control && this.triggerGamepadButtonRepeat[i])
                                    return true;
                            }
                        } else if (this.heldGamepadButtons[index] == (Buttons) control && this.triggerGamepadButtonRepeat[index]) {
                            return true;
                        }
                    }
                    break;
            }
            return this.IsPressedIgnoreRepeats(control, index);
        }

        /// <summary>
        /// An input is considered pressed if it was not down the last update call, but is down the current update call. If <see cref="InvertPressBehavior"/> is true, this behavior is inverted.
        /// This has the same behavior as <see cref="IsPressed"/>, but ignores keyboard and gamepad repeat events.
        /// If <see cref="HandleKeyboardRepeats"/> and <see cref="HandleGamepadRepeats"/> are false, this method does the same as <see cref="IsPressed"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is pressed, ignoring repeat events.</returns>
        public bool IsPressedIgnoreRepeats(GenericInput control, int index = -1) {
            if (this.InvertPressBehavior)
                return this.WasDown(control, index) && this.IsUp(control, index);
            return this.WasUp(control, index) && this.IsDown(control, index);
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed, and if the press has not been consumed yet using <see cref="TryConsumePressed"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control is pressed and the press is not consumed yet.</returns>
        public bool IsPressedAvailable(GenericInput control, int index = -1) {
            return this.IsPressed(control, index) && !this.IsPressConsumed(control, index);
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed and available, and marks the press as consumed if it is.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control is pressed and the press is not consumed yet.</returns>
        public bool TryConsumePressed(GenericInput control, int index = -1) {
            if (this.IsPressedAvailable(control, index)) {
                this.consumedPresses.Add((control, index));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns if a given control of any kind was just let go this frame, and had been down for less than the given <paramref name="time"/> before that. Essentially, this action signifies a short press action.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="time">The maximum time that the control should have been down for.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control was pressed for less than the given time.</returns>
        public bool WasPressedForLess(GenericInput control, TimeSpan time, int index = -1) {
            return this.WasDown(control, index) && this.IsUp(control, index) && this.GetDownTime(control, index) < time;
        }

        /// <summary>
        /// Returns if a given control of any kind was just let go this frame, and had been down for less than the given <paramref name="time"/> before that, and if the press has not been consumed yet using <see cref="TryConsumePressedForLess"/>. Essentially, this action signifies a short press action.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="time">The maximum time that the control should have been down for.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control was pressed for less than the given time, and the press has not been consumed yet.</returns>
        public bool WasPressedForLessAvailable(GenericInput control, TimeSpan time, int index = -1) {
            return this.WasPressedForLess(control, time, index) && !this.IsPressConsumed(control, index);
        }

        /// <summary>
        /// Returns if a given control of any kind was just let go this frame, and had been down for less than the given <paramref name="time"/> before that, and if the press has not been consumed yet using <see cref="TryConsumePressedForLess"/>, and marks the press as consumed if it is. Essentially, this action signifies a short press action.
        /// </summary>
        /// <param name="control">The control whose pressed state to query.</param>
        /// <param name="time">The maximum time that the control should have been down for.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the given control was pressed for less than the given time, and the press was successfully consumed.</returns>
        public bool TryConsumePressedForLess(GenericInput control, TimeSpan time, int index = -1) {
            if (this.WasPressedForLessAvailable(control, time, index)) {
                this.consumedPresses.Add((control, index));
                return true;
            }
            return false;
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
        /// If the input is currently down or has been down previously, this method returns true and the amount of time that it has currently or last been down for is stored in <paramref name="downTime"/>.
        /// </summary>
        /// <param name="input">The input whose down time to query.</param>
        /// <param name="downTime">The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the input is currently being held.</returns>
        public bool TryGetDownTime(GenericInput input, out TimeSpan downTime, int index = -1) {
            if (this.inputDownTimes.TryGetValue((input, index), out var wentDown)) {
                // if we're currently down, we return the amount of time we've been down for so far
                // if we're not currently down, we return the last amount of time we were down for
                downTime = (this.IsDown(input) || !this.inputUpTimes.TryGetValue((input, index), out var wentUp) ? DateTime.UtcNow : wentUp) - wentDown;
                return true;
            }
            downTime = default;
            return false;
        }

        /// <summary>
        /// Returns the current or last amount of time that a given <see cref="GenericInput"/> has been held down for.
        /// If this input isn't currently down and has not been down previously, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="input">The input whose down time to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</returns>
        public TimeSpan GetDownTime(GenericInput input, int index = -1) {
            this.TryGetDownTime(input, out var time, index);
            return time;
        }

        /// <summary>
        /// Tries to retrieve the amount of time that a given <see cref="GenericInput"/> has been up for since the last time it was down.
        /// If the input has previously been down, this method returns true and the amount of time that it has been up for is stored in <paramref name="upTime"/>.
        /// </summary>
        /// <param name="input">The input whose up time to query.</param>
        /// <param name="upTime">The resulting up time, or <see cref="TimeSpan.Zero"/> if the input is being held.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>Whether the input is currently up.</returns>
        public bool TryGetUpTime(GenericInput input, out TimeSpan upTime, int index = -1) {
            if (this.inputUpTimes.TryGetValue((input, index), out var wentUp)) {
                // if we're currently up, we return the amount of time we've been up for so far
                // if we're not currently up, we return the last amount of time we were up for
                upTime = (this.IsUp(input) || !this.inputDownTimes.TryGetValue((input, index), out var wentDown) ? DateTime.UtcNow : wentDown) - wentUp;
                return true;
            }
            upTime = default;
            return false;
        }

        /// <summary>
        /// Returns the amount of time that a given <see cref="GenericInput"/> has last been up for since the last time it was down.
        /// If this input hasn't been down previously, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="input">The input whose up time to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input is being held.</returns>
        public TimeSpan GetUpTime(GenericInput input, int index = -1) {
            this.TryGetUpTime(input, out var time, index);
            return time;
        }

        /// <summary>
        /// Tries to retrieve the amount of time that has passed since a given <see cref="GenericInput"/> last counted as pressed.
        /// If the input has previously been pressed, or is currently pressed, this method returns true and the amount of time that has passed since it was last pressed is stored in <paramref name="lastPressTime"/>.
        /// </summary>
        /// <param name="input">The input whose last press time to query.</param>
        /// <param name="lastPressTime">The resulting up time, or <see cref="TimeSpan.Zero"/> if the input was never pressed or is currently pressed.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns><see langword="true"/> if the input has previously been pressed or is currently pressed, <see langword="false"/> otherwise.</returns>
        public bool TryGetTimeSincePress(GenericInput input, out TimeSpan lastPressTime, int index = -1) {
            if (this.inputPressedTimes.TryGetValue((input, index), out var start)) {
                lastPressTime = DateTime.UtcNow - start;
                return true;
            }
            lastPressTime = default;
            return false;
        }

        /// <summary>
        /// Returns the amount of time that has passed since a given <see cref="GenericInput"/> last counted as pressed.
        /// If this input hasn't been pressed previously, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="input">The input whose up time to query.</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad.</param>
        /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input has never been pressed, or is currently pressed.</returns>
        public TimeSpan GetTimeSincePress(GenericInput input, int index = -1) {
            this.TryGetTimeSincePress(input, out var time, index);
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
                InputHandler.EnableGestures(gestures);
            } else {
                InputHandler.DisableGestures(gestures);
            }
        }

    }
}
