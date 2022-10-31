using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="Keys"/> and <see cref="Keyboard"/>
    /// </summary>
    public static class KeysExtensions {

        /// <summary>
        /// All enum values of <see cref="ModifierKey"/>
        /// </summary>
        public static readonly ModifierKey[] ModifierKeys = (ModifierKey[]) Enum.GetValues(typeof(ModifierKey));
        private static readonly Dictionary<ModifierKey, Keys[]> KeysLookup = new Dictionary<ModifierKey, Keys[]> {
            {ModifierKey.Shift, new[] {Keys.LeftShift, Keys.RightShift}},
            {ModifierKey.Control, new[] {Keys.LeftControl, Keys.RightControl}},
            {ModifierKey.Alt, new[] {Keys.LeftAlt, Keys.RightAlt}}
        };
        private static readonly Dictionary<Keys, ModifierKey> ModifiersLookup = KeysExtensions.KeysLookup
            .SelectMany(kv => kv.Value.Select(v => (kv.Key, v)))
            .ToDictionary(kv => kv.Item2, kv => kv.Item1);

        /// <summary>
        /// Returns all of the keys that the given modifier key represents
        /// </summary>
        /// <param name="modifier">The modifier key</param>
        /// <returns>All of the keys the modifier key represents</returns>
        public static IEnumerable<Keys> GetKeys(this ModifierKey modifier) {
            return KeysExtensions.KeysLookup.TryGetValue(modifier, out var keys) ? keys : Enumerable.Empty<Keys>();
        }

        /// <summary>
        /// Returns the modifier key that the given key represents.
        /// If there is no matching modifier key, <see cref="ModifierKey.None"/> is returned.
        /// </summary>
        /// <param name="key">The key to convert to a modifier key</param>
        /// <returns>The modifier key, or <see cref="ModifierKey.None"/></returns>
        public static ModifierKey GetModifier(this Keys key) {
            return KeysExtensions.ModifiersLookup.TryGetValue(key, out var mod) ? mod : ModifierKey.None;
        }

        /// <inheritdoc cref="GetModifier(Microsoft.Xna.Framework.Input.Keys)"/>
        public static ModifierKey GetModifier(this GenericInput input) {
            return input.Type == GenericInput.InputType.Keyboard ? ((Keys) input).GetModifier() : ModifierKey.None;
        }

        /// <summary>
        /// Returns whether the given key is a modifier key or not.
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>If the key is a modifier key</returns>
        public static bool IsModifier(this Keys key) {
            return key.GetModifier() != ModifierKey.None;
        }

        /// <inheritdoc cref="IsModifier(Microsoft.Xna.Framework.Input.Keys)"/>
        public static bool IsModifier(this GenericInput input) {
            return input.GetModifier() != ModifierKey.None;
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
