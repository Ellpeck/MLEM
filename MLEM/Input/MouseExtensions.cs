using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Input {
    public static class MouseExtensions {

        public static readonly MouseButton[] MouseButtons = EnumHelper.GetValues<MouseButton>().ToArray();

        public static ButtonState GetState(this MouseState state, MouseButton button) {
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