using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="GamePad"/>, <see cref="GamePadState"/> and <see cref="Buttons"/>.
    /// </summary>
    public static class GamepadExtensions {

        /// <summary>
        /// Returns the given <see cref="Buttons"/>'s value as an analog value between 0 and 1, where 1 is fully down and 0 is not down at all.
        /// For non-analog buttons, like <see cref="Buttons.X"/> or <see cref="Buttons.Start"/>, only 0 and 1 will be returned and no inbetween values are possible.
        /// </summary>
        /// <param name="state">The gamepad state to query.</param>
        /// <param name="button">The button to query.</param>
        /// <returns>The button's state as an analog value.</returns>
        public static float GetAnalogValue(this GamePadState state, Buttons button) {
            switch (button) {
                case Buttons.LeftThumbstickDown:
                    return -MathHelper.Clamp(state.ThumbSticks.Left.Y, -1, 0);
                case Buttons.LeftThumbstickUp:
                    return MathHelper.Clamp(state.ThumbSticks.Left.Y, 0, 1);
                case Buttons.LeftThumbstickLeft:
                    return -MathHelper.Clamp(state.ThumbSticks.Left.X, -1, 0);
                case Buttons.LeftThumbstickRight:
                    return MathHelper.Clamp(state.ThumbSticks.Left.X, 0, 1);
                case Buttons.RightTrigger:
                    return state.Triggers.Right;
                case Buttons.LeftTrigger:
                    return state.Triggers.Left;
                case Buttons.RightThumbstickDown:
                    return -MathHelper.Clamp(state.ThumbSticks.Right.Y, -1, 0);
                case Buttons.RightThumbstickUp:
                    return MathHelper.Clamp(state.ThumbSticks.Right.Y, 0, 1);
                case Buttons.RightThumbstickLeft:
                    return -MathHelper.Clamp(state.ThumbSticks.Right.X, -1, 0);
                case Buttons.RightThumbstickRight:
                    return MathHelper.Clamp(state.ThumbSticks.Right.X, 0, 1);
                default:
                    return state.IsButtonDown(button) ? 1 : 0;
            }
        }

    }
}