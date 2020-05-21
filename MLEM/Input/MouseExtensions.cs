using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Input {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="MouseButton"/> and <see cref="Mouse"/>
    /// </summary>
    public static class MouseExtensions {

        /// <summary>
        /// All enum values of <see cref="MouseButton"/>
        /// </summary>
        public static readonly MouseButton[] MouseButtons = EnumHelper.GetValues<MouseButton>().ToArray();

        /// <summary>
        /// Returns the <see cref="ButtonState"/> of the given mouse button.
        /// </summary>
        /// <param name="state">The mouse's current state</param>
        /// <param name="button">The button whose state to query</param>
        /// <returns>The state of the button</returns>
        /// <exception cref="ArgumentException">If a mouse button out of range is passed</exception>
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

    /// <summary>
    /// This enum is a list of possible mouse buttons.
    /// It serves as a wrapper around <see cref="MouseState"/>'s button properties.
    /// </summary>
    public enum MouseButton {

        /// <summary>
        /// The left mouse button, or <see cref="MouseState.LeftButton"/>
        /// </summary>
        Left,
        /// <summary>
        /// The middle mouse button, or <see cref="MouseState.MiddleButton"/>
        /// </summary>
        Middle,
        /// <summary>
        /// The right mouse button, or <see cref="MouseState.RightButton"/>
        /// </summary>
        Right,
        /// <summary>
        /// The first extra mouse button, or <see cref="MouseState.XButton1"/>
        /// </summary>
        Extra1,
        /// <summary>
        /// The second extra mouse button, or <see cref="MouseState.XButton2"/>
        /// </summary>
        Extra2

    }
}