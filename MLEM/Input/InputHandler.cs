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
    public class InputHandler {

        public KeyboardState LastKeyboardState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }
        public Keys[] PressedKeys { get; private set; }
        public bool HandleKeyboard;

        public MouseState LastMouseState { get; private set; }
        public MouseState MouseState { get; private set; }
        public Point MousePosition => this.MouseState.Position;
        public Point LastMousePosition => this.LastMouseState.Position;
        public int ScrollWheel => this.MouseState.ScrollWheelValue;
        public int LastScrollWheel => this.LastMouseState.ScrollWheelValue;
        public bool HandleMouse;

        private readonly GamePadState[] lastGamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly GamePadState[] gamepads = new GamePadState[GamePad.MaximumGamePadCount];
        public int ConnectedGamepads { get; private set; }
        public bool HandleGamepads;

        public TouchCollection LastTouchState { get; private set; }
        public TouchCollection TouchState { get; private set; }
        public readonly ReadOnlyCollection<GestureSample> Gestures;
        private readonly List<GestureSample> gestures = new List<GestureSample>();
        public bool HandleTouch;

        public bool HandleKeyboardRepeats = true;
        public TimeSpan KeyRepeatDelay = TimeSpan.FromSeconds(0.65);
        public TimeSpan KeyRepeatRate = TimeSpan.FromSeconds(0.05);
        private DateTime heldKeyStart;
        private DateTime lastKeyRepeat;
        private bool triggerRepeat;
        private Keys heldKey;

        public InputHandler(bool handleKeyboard = true, bool handleMouse = true, bool handleGamepads = true, bool handleTouch = true) {
            this.HandleKeyboard = handleKeyboard;
            this.HandleMouse = handleMouse;
            this.HandleGamepads = handleGamepads;
            this.HandleTouch = handleTouch;
            this.Gestures = this.gestures.AsReadOnly();
        }

        public void Update() {
            this.triggerRepeat = false;
            if (this.HandleKeyboard) {
                this.LastKeyboardState = this.KeyboardState;
                this.KeyboardState = Keyboard.GetState();
                this.PressedKeys = this.KeyboardState.GetPressedKeys();

                if (this.HandleKeyboardRepeats) {
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
                                    this.triggerRepeat = true;
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
                for (var i = 0; i < GamePad.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = GamePad.GetState(i);
                    if (this.ConnectedGamepads > i && !this.gamepads[i].IsConnected)
                        this.ConnectedGamepads = i;
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

        public GamePadState GetLastGamepadState(int index) {
            return this.lastGamepads[index];
        }

        public GamePadState GetGamepadState(int index) {
            return this.gamepads[index];
        }

        public bool IsKeyDown(Keys key) {
            return this.KeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key) {
            return this.KeyboardState.IsKeyUp(key);
        }

        public bool WasKeyDown(Keys key) {
            return this.LastKeyboardState.IsKeyDown(key);
        }

        public bool WasKeyUp(Keys key) {
            return this.LastKeyboardState.IsKeyUp(key);
        }

        public bool IsKeyPressed(Keys key) {
            // if the queried key is the held key and a repeat should be triggered, return true
            if (key == this.heldKey && this.triggerRepeat)
                return true;
            return this.WasKeyUp(key) && this.IsKeyDown(key);
        }

        public bool IsModifierKeyDown(ModifierKey modifier) {
            return modifier.GetKeys().Any(this.IsKeyDown);
        }

        public bool IsMouseButtonDown(MouseButton button) {
            return this.MouseState.GetState(button) == ButtonState.Pressed;
        }

        public bool IsMouseButtonUp(MouseButton button) {
            return this.MouseState.GetState(button) == ButtonState.Released;
        }

        public bool WasMouseButtonDown(MouseButton button) {
            return this.LastMouseState.GetState(button) == ButtonState.Pressed;
        }

        public bool WasMouseButtonUp(MouseButton button) {
            return this.LastMouseState.GetState(button) == ButtonState.Released;
        }

        public bool IsMouseButtonPressed(MouseButton button) {
            return this.WasMouseButtonUp(button) && this.IsMouseButtonDown(button);
        }

        public bool IsGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetGamepadState(i).IsButtonDown(button))
                        return true;
                return false;
            }
            return this.GetGamepadState(index).IsButtonDown(button);
        }

        public bool IsGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetGamepadState(i).IsButtonUp(button))
                        return true;
                return false;
            }
            return this.GetGamepadState(index).IsButtonUp(button);
        }

        public bool WasGamepadButtonDown(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetLastGamepadState(i).IsButtonDown(button))
                        return true;
                return false;
            }
            return this.GetLastGamepadState(index).IsButtonDown(button);
        }

        public bool WasGamepadButtonUp(Buttons button, int index = -1) {
            if (index < 0) {
                for (var i = 0; i < this.ConnectedGamepads; i++)
                    if (this.GetLastGamepadState(i).IsButtonUp(button))
                        return true;
                return false;
            }
            return this.GetLastGamepadState(index).IsButtonUp(button);
        }

        public bool IsGamepadButtonPressed(Buttons button, int index = -1) {
            return this.WasGamepadButtonUp(button, index) && this.IsGamepadButtonDown(button, index);
        }

        public bool GetGesture(GestureType type, out GestureSample sample) {
            foreach (var gesture in this.Gestures) {
                if (gesture.GestureType == type) {
                    sample = gesture;
                    return true;
                }
            }
            return false;
        }

        public bool IsDown(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyDown(key);
            if (control is Buttons button)
                return this.IsGamepadButtonDown(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonDown(mouse);
            throw new ArgumentException(nameof(control));
        }

        public bool IsUp(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyUp(key);
            if (control is Buttons button)
                return this.IsGamepadButtonUp(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonUp(mouse);
            throw new ArgumentException(nameof(control));
        }

        public bool IsPressed(object control, int index = -1) {
            if (control is Keys key)
                return this.IsKeyPressed(key);
            if (control is Buttons button)
                return this.IsGamepadButtonPressed(button, index);
            if (control is MouseButton mouse)
                return this.IsMouseButtonPressed(mouse);
            throw new ArgumentException(nameof(control));
        }

        public static void EnableGestures(params GestureType[] gestures) {
            foreach (var gesture in gestures)
                TouchPanel.EnabledGestures |= gesture;
        }

    }
}