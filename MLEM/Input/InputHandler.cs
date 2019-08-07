using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    public class InputHandler {

        public KeyboardState LastKeyboardState { get; private set; }
        public KeyboardState KeyboardState { get; private set; }
        private readonly bool handleKeyboard;

        public MouseState LastMouseState { get; private set; }
        public MouseState MouseState { get; private set; }
        public Point MousePosition => this.MouseState.Position;
        public Point LastMousePosition => this.LastMouseState.Position;
        public int ScrollWheel => this.MouseState.ScrollWheelValue;
        public int LastScrollWheel => this.LastMouseState.ScrollWheelValue;
        private readonly bool handleMouse;

        private readonly GamePadState[] lastGamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly GamePadState[] gamepads = new GamePadState[GamePad.MaximumGamePadCount];
        private readonly bool handleGamepads;

        public InputHandler(bool handleKeyboard = true, bool handleMouse = true, bool handleGamepads = true) {
            this.handleKeyboard = handleKeyboard;
            this.handleMouse = handleMouse;
            this.handleGamepads = handleGamepads;
        }

        public void Update() {
            if (this.handleKeyboard) {
                this.LastKeyboardState = this.KeyboardState;
                this.KeyboardState = Keyboard.GetState();
            }
            if (this.handleMouse) {
                this.LastMouseState = this.MouseState;
                this.MouseState = Mouse.GetState();
            }
            if (this.handleGamepads) {
                for (var i = 0; i < GamePad.MaximumGamePadCount; i++) {
                    this.lastGamepads[i] = this.gamepads[i];
                    this.gamepads[i] = GamePad.GetState(i);
                }
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
            return this.WasKeyUp(key) && this.IsKeyDown(key);
        }

        public bool IsMouseButtonDown(MouseButton button) {
            return GetState(this.MouseState, button) == ButtonState.Pressed;
        }

        public bool IsMouseButtonUp(MouseButton button) {
            return GetState(this.MouseState, button) == ButtonState.Released;
        }

        public bool WasMouseButtonDown(MouseButton button) {
            return GetState(this.LastMouseState, button) == ButtonState.Pressed;
        }

        public bool WasMouseButtonUp(MouseButton button) {
            return GetState(this.LastMouseState, button) == ButtonState.Released;
        }

        public bool IsMouseButtonPressed(MouseButton button) {
            return this.WasMouseButtonUp(button) && this.IsMouseButtonDown(button);
        }

        public bool IsGamepadButtonDown(int index, Buttons button) {
            return this.GetGamepadState(index).IsButtonDown(button);
        }

        public bool IsGamepadButtonUp(int index, Buttons button) {
            return this.GetGamepadState(index).IsButtonUp(button);
        }

        public bool WasGamepadButtonDown(int index, Buttons button) {
            return this.GetLastGamepadState(index).IsButtonDown(button);
        }

        public bool WasGamepadButtonUp(int index, Buttons button) {
            return this.GetLastGamepadState(index).IsButtonUp(button);
        }

        public bool IsGamepadButtonPressed(int index, Buttons button) {
            return this.WasGamepadButtonUp(index, button) && this.IsGamepadButtonDown(index, button);
        }

        private static ButtonState GetState(MouseState state, MouseButton button) {
            switch (button) {
                case MouseButton.Left:
                    return state.LeftButton;
                case MouseButton.Middle:
                    return state.MiddleButton;
                case MouseButton.Right:
                    return state.RightButton;
                case MouseButton.Extra1:
                    return state.XButton1;
                case MouseButton.Extra2:
                    return state.XButton2;
                default:
                    throw new ArgumentException(nameof(button));
            }
        }

    }

    public enum MouseButton {

        Left,
        Middle,
        Right,
        Extra1,
        Extra2

    }
}