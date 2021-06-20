using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Input {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="Keys"/> and <see cref="Keyboard"/>
    /// </summary>
    public static class KeysExtensions {

        /// <summary>
        /// All enum values of <see cref="ModifierKey"/>
        /// </summary>
        public static readonly ModifierKey[] ModifierKeys = EnumHelper.GetValues<ModifierKey>().ToArray();

        /// <summary>
        /// Returns all of the keys that the given modifier key represents
        /// </summary>
        /// <param name="modifier">The modifier key</param>
        /// <returns>All of the keys the modifier key represents</returns>
        public static IEnumerable<Keys> GetKeys(this ModifierKey modifier) {
            switch (modifier) {
                case ModifierKey.Shift:
                    yield return Keys.LeftShift;
                    yield return Keys.RightShift;
                    break;
                case ModifierKey.Control:
                    yield return Keys.LeftControl;
                    yield return Keys.RightControl;
                    break;
                case ModifierKey.Alt:
                    yield return Keys.LeftAlt;
                    yield return Keys.RightAlt;
                    break;
            }
        }

        /// <summary>
        /// Returns the modifier key that the given key represents.
        /// If there is no matching modifier key, <see cref="ModifierKey.None"/> is returned.
        /// </summary>
        /// <param name="key">The key to convert to a modifier key</param>
        /// <returns>The modifier key, or <see cref="ModifierKey.None"/></returns>
        public static ModifierKey GetModifier(this Keys key) {
            foreach (var mod in ModifierKeys) {
                if (GetKeys(mod).Contains(key))
                    return mod;
            }
            return ModifierKey.None;
        }

        /// <inheritdoc cref="GetModifier(Microsoft.Xna.Framework.Input.Keys)"/>
        public static ModifierKey GetModifier(this GenericInput input) {
            return input.Type == GenericInput.InputType.Keyboard ? GetModifier(input) : ModifierKey.None;
        }

        /// <summary>
        /// Returns whether the given key is a modifier key or not.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>If the key is a modifier key</returns>
        public static bool IsModifier(this Keys key) {
            return GetModifier(key) != ModifierKey.None;
        }

        /// <inheritdoc cref="IsModifier(Microsoft.Xna.Framework.Input.Keys)"/>
        public static bool IsModifier(this GenericInput input) {
            return GetModifier(input) != ModifierKey.None;
        }

    }

    /// <summary>
    /// An enum representing modifier keys.
    /// A modifier key is a key that is usually pressed as part of key combination to change the function of a regular key.
    /// </summary>
    public enum ModifierKey {

        /// <summary>
        /// No modifier key. Only used for <see cref="KeysExtensions.GetModifier(Keys)"/>
        /// </summary>
        None,
        /// <summary>
        /// The shift modifier key. This represents Left Shift and Right Shift keys.
        /// </summary>
        Shift,
        /// <summary>
        /// The control modifier key. This represents Left Control and Right Control.
        /// </summary>
        Control,
        /// <summary>
        /// The alt modifier key. This represents Alt and Alt Graph.
        /// </summary>
        Alt

    }
}