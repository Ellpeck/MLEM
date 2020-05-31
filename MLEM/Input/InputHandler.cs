using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Input {
    /// <summary>
    /// An input handler is a more advanced wrapper around MonoGame's default input system.
    /// It includes keyboard, mouse, gamepad and touch states, as well as a new "pressed" state for keys and the ability for keyboard and gamepad repeat events.
    /// </summary>
    public class InputHandler : GameComponent {

        /// <summary>
        /// Contains the keyboard state from the last update call
        /// </summary>
        public KeyboardState LastKeyboardState { get; private set; }
        /// <summary>
        /// Contains the current keyboard state
        /// </summary>
        public KeyboardState KeyboardState { get; private set; }
        /// <summary>
        /// Contains the keyboard keys that are currently being pressed
        /// </summary>
        public Keys[] PressedKeys { get; private set; }
        /// <summary>
        /// Set this property to false to disable keyboard handling for this input handler.
        /// </summary>
        public bool HandleKeyboard;

        /// <summary>
        /// Contains the mouse state from the last update call
        /// </summary>
        public MouseState LastMouseState { get; private set; }
        /// <summary>
        /// Contains the current mouse state
        /// </summary>
        public MouseState MouseState { get; private set; }
        /// <summary>
        /// Contains the current position of the mouse, extracted from <see cref="MouseState"/>
        /// </summary>
        public Point MousePosition => this.MouseState.Position;
        /// <summary>
        /// Contains the position of the mouse from the last update call, extracted from <see cref="LastMouseState"/>
        /// </summary>
        public Point LastMousePosition => this.LastMouseState.Position;
        /// <summary>
        /// Contains the current scroll wheel value, in increments of 120
        /// </summary>
        public int ScrollWheel => this.MouseState.ScrollWheelValue;
        /// <summary>
        /// Contains the scroll wheel value from the last update call, in increments of 120
        /// </summary>
        public int LastScrollWheel => this.LastMouseState.ScrollWheelValue;
        /// <summary>
        /// Set this property to false to disable mouse handling for this input handler.
        /// </summary>
        public bool HandleMouse;

        private readonly GamePadState[] lastGamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly GamePadState[] gamepads = new GamePadState[GamePad.MaximumGamePadCount];
        /// <summary>
        /// Contains the amount of gamepads that are currently connected.
        /// This property is automatically updated in <see cref="Update()"/>
        /// </summary>
        public int ConnectedGamepads { get; private set; }
        /// <summary>
        /// Set this property to false to disable keyboard handling for this input handler.
        /// </summary>
        public bool HandleGamepads;

        /// <summary>
        /// Contains the touch state from the last update call
        /// </summary>
        public TouchCollection LastTouchState { get; private set; }
        /// <summary>
        /// Contains the current touch state
        /// </summary>
        public TouchCollection TouchState { get; private set; }
        /// <summary>
        /// Contains all of the gestures that have finished during the last update call.
        /// To easily query these gestures, use <see cref="GetGesture"/>
        /// </summary>
        public readonly ReadOnlyCollection<GestureSample> Gestures;
        private readonly List<GestureSample> gestures = new List<GestureSample>();
        /// <summary>
        /// Set this property to false to disable touch handling for this input handler.
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
        /// Set this property to false to disable keyboard repeat event handling.
        /// </summary>
        public bool HandleKeyboardRepeats = true;
        private DateTime heldKeyStart;
        private DateTime lastKeyRepeat;
        private bool triggerKeyRepeat;
        private Keys heldKey;

        /// <summary>
        /// Set this property to false to disable gamepad repeat event handling.
        /// </summary>
        public bool HandleGamepadRepeats = true;
        private readonly DateTime[] heldGamepadButtonStarts = new DateTime[GamePad.MaximumGamePadCount];
        private readonly DateTime[] lastGamepadButtonRepeats = new DateTime[GamePad.MaximumGamePadCount];
        private readonly bool[] triggerGamepadButtonRepeat = new bool[GamePad.MaximumGamePadCount];
        private readonly Buttons?[] heldGamepadButtons = new Buttons?[GamePad.MaximumGamePadCount];

        /// <summary>
        /// Creates a new input handler with optional initial values.
        /// </summary>
        /// <param name="handleKeyboard">If keyboard input should be handled</param>
        /// <param name="handleMouse">If mouse input should be handled</param>
        /// <param name="handleGamepads">If gamepad input should be handled</param>
        /// <param name="handleTouch">If touch input should be handled</param>
        public InputHandler(bool handleKeyboard = true, bool handleMouse = true, bool handleGamepads = true, bool handleTouch = true) : base(null) {
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
            if (this.HandleKeyboard) {
                this.LastKeyboardState = this.KeyboardState;
                this.KeyboardState = Keyboard.GetState();
                this.PressedKeys = this.KeyboardState.GetPressedKeys();

                if (this.HandleKeyboardRepeats) {
                    this.triggerKeyRepeat = false;
                    if (this.heldKey == Keys.None) {
                        // if we're not repeating a key, set the first key being held to the repeat key
                        // note that modifier keys don't count as that wouldn't really make sense
                        var key = this.PressedKeys.FirstOrDefault(k => !k.IsModifier());
                        if (key != Keys.None) {
                            this.heldKey = key;
                            this.heldKeyStart = DateTime.UtcNow;
                        }
                    } else {
                        // if the repeating key isn't being held anymore, reset
                        if (!this.IsKeyDown(this.heldKey)) {
                            this.heldKey = Keys.None;
                        } else {
                            var now = DateTime.UtcNow;
                            var holdTime = now - this.heldKeyStart;
                            // if we've been holding the key longer than the initial delay...
                            if (holdTime >= this.KeyRepeatDelay) {
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
            }

            if (this.HandleMouse) {
                this.LastMouseState = this.MouseState;
                this.MouseState = Mouse.GetState();
            }

            if (this.HandleGamepads) {
                this.ConnectedGamepads = GamePad.MaximumGamePadCount;
                for (var i = 0; i < GamePad.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = GamePad.GetState(i);
                    if (this.ConnectedGamepads > i && !this.gamepads[i].IsConnected)
                        this.ConnectedGamepads = i;
                }

                if (this.HandleGamepadRepeats) {
                    for (var i = 0; i < this.ConnectedGamepads; i++) {
                        this.triggerGamepadButtonRepeat[i] = false;

                        if (!this.heldGamepadButtons[i].HasValue) {
                            foreach (var b in EnumHelper.Buttons) {
                                if (this.IsGamepadButtonDown(b, i)) {
                                    this.heldGamepadButtons[i] = b;
                                    this.heldGamepadButtonStarts[i] = DateTime.UtcNow;
                                    break;
                                }
                            }
                        } else {
                            if (!this.IsGamepadButtonDown(this.heldGamepadButtons[i].Value, i)) {
                                this.heldGamepadButtons[i] = null;
                            } else {
                                var now = DateTime.UtcNow;
                                var holdTime = now - this.heldGamepadButtonStarts[i];
                                if (holdTime >= this.KeyRepeatDelay) {
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
            }

            if (this.HandleTouch) {
                this.LastTouchState = this.TouchState;
                this.TouchState = TouchPanel.GetState();

                this.gestures.Clear();
                while (TouchPanel.IsGestureAvailable)
                    this.gestures.Add(TouchPanel.ReadGesture());
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
        /// A key is considered pressed if it was not down the last update call, but is down the current update call.
        /// </summary>
        /// <param name="key">The key to query</param>
        /// <returns>If the key is pressed</returns>
        public bool IsKeyPressed(Keys key) {
            // if the queried key is the held key and a repeat should be triggered, return true
            if (this.HandleKeyboardRepeats && key == this.heldKey && this.triggerKeyRepeat)
                return true;
            return this.WasKeyUp(key) && this.IsKeyDown(key);
        }

        /// <summary>
        /// Returns whether the given modifier key is down.
        /// </summary>
        /// <param name="modifier">The modifier key</param>
        /// <returns>If the modifier key is down</returns>
        public bool IsModifierKeyDown(ModifierKey modifier) {
            return modifier.GetKeys().Any(this.IsKeyDown);
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
        /// A mouse button is considered pressed if it was up the last update call, and is down the current update call.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <returns>Whether the button is pressed</returns>
        public bool IsMouseButtonPressed(MouseButton button) {
            return this.WasMouseButtonUp(button) && this.IsMouseButtonDown(button);
        }

        /// <inheritdoc cref="GamePadState.IsButtonDown"/>
        public bool IsGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetGamepadState(i).IsButtonDown(button))
                        return true;
                return false;
            }
            return this.GetGamepadState(index).IsButtonDown(button);
        }

        /// <inheritdoc cref="GamePadState.IsButtonUp"/>
        public bool IsGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetGamepadState(i).IsButtonUp(button))
                        return true;
                return false;
            }
            return this.GetGamepadState(index).IsButtonUp(button);
        }

        /// <inheritdoc cref="GamePadState.IsButtonDown"/>
        public bool WasGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetLastGamepadState(i).IsButtonDown(button))
                        return true;
                return false;
            }
            return this.GetLastGamepadState(index).IsButtonDown(button);
        }

        /// <inheritdoc cref="GamePadState.IsButtonUp"/>
        public bool WasGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetLastGamepadState(i).IsButtonUp(button))
                        return true;
                return false;
            }
            return this.GetLastGamepadState(index).IsButtonUp(button);
        }

        /// <summary>
        /// Returns whether the given gamepad button on the given index is considered pressed.
        /// A gamepad button is considered pressed if it was down the last update call, and is up the current update call.
        /// </summary>
        /// <param name="button">The button to query</param>
        /// <param name="index">The zero-based index of the gamepad, or -1 for any gamepad</param>
        /// <returns>Whether the given button is pressed</returns>
        public bool IsGamepadButtonPressed(Buttons button, int index = -1) {
            if (this.HandleGamepadRepeats) {
                if (index < 0) {
                    for (var i = 0; i < this.ConnectedGamepads; i++)
                        if (this.heldGamepadButtons[i] == button && this.triggerGamepadButtonRepeat[i])
                            return true;
                } else if (this.heldGamepadButtons[index] == button && this.triggerGamepadButtonRepeat[index]) {
                    return true;
                }
            }
            return this.WasGamepadButtonUp(button, index) && this.IsGamepadButtonDown(button, index);
        }

        /// <summary>
        /// Queries for a gesture of a given type that finished during the current update call.
        /// </summary>
        /// <param name="type">The type of gesture to query for</param>
        /// <param name="sample">The resulting gesture sample, or default if there isn't one</param>
        /// <returns>True if a gesture of the type was found, otherwise false</returns>
        public bool GetGesture(GestureType type, out GestureSample sample) {
            foreach (var gesture in this.Gestures) {
                if ((gesture.GestureType & type) != 0) {
                    sample = gesture;
                    return true;
                }
            }
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
        public bool IsDown(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyDown(key);
            if (control is Buttons button)
                return this.IsGamepadButtonDown(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonDown(mouse);
            throw new ArgumentException(nameof(control));
        }

        /// <summary>
        /// Returns if a given control of any kind is up.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose up state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is down</returns>
        /// <exception cref="ArgumentException">If the passed control isn't of a supported type</exception>
        public bool IsUp(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyUp(key);
            if (control is Buttons button)
                return this.IsGamepadButtonUp(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonUp(mouse);
            throw new ArgumentException(nameof(control));
        }

        /// <summary>
        /// Returns if a given control of any kind is pressed.
        /// This is a helper function that can be passed a <see cref="Keys"/>, <see cref="Buttons"/> or <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="control">The control whose pressed state to query</param>
        /// <param name="index">The index of the gamepad to query (if applicable), or -1 for any gamepad</param>
        /// <returns>Whether the given control is down</returns>
        /// <exception cref="ArgumentException">If the passed control isn't of a supported type</exception>
        public bool IsPressed(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyPressed(key);
            if (control is Buttons button)
                return this.IsGamepadButtonPressed(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonPressed(mouse);
            throw new ArgumentException(nameof(control));
        }

        /// <inheritdoc cref="IsDown"/>
        public bool IsAnyDown(params object[] control) {
            return control.Any(c => this.IsDown(c));
        }

        /// <inheritdoc cref="IsUp"/>
        public bool IsAnyUp(params object[] control) {
            return control.Any(c => this.IsUp(c));
        }

        /// <inheritdoc cref="IsPressed"/>
        public bool IsAnyPressed(params object[] control) {
            return control.Any(c => this.IsPressed(c));
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

    }
}