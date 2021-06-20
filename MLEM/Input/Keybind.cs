using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace MLEM.Input {
    /// <summary>
    /// A keybind represents a generic way to trigger input.
    /// A keybind is made up of multiple key combinations, one of which has to be pressed for the keybind to be triggered.
    /// Note that this type is serializable using <see cref="DataContractAttribute"/>.
    /// </summary>
    [DataContract]
    public class Keybind {

        [DataMember]
        private Combination[] combinations = Array.Empty<Combination>();

        /// <summary>
        /// Creates a new keybind and adds the given key and modifiers using <see cref="Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/>
        /// </summary>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        public Keybind(GenericInput key, params GenericInput[] modifiers) {
            this.Add(key, modifiers);
        }

        /// <inheritdoc cref="Keybind(GenericInput, GenericInput[])"/>
        public Keybind(GenericInput key, ModifierKey modifier) {
            this.Add(key, modifier);
        }

        /// <summary>
        /// Creates a new keybind with no default combinations
        /// </summary>
        public Keybind() {
        }

        /// <summary>
        /// Adds a new key combination to this keybind that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Add(GenericInput key, params GenericInput[] modifiers) {
            this.combinations = this.combinations.Append(new Combination(key, modifiers)).ToArray();
            return this;
        }

        /// <inheritdoc cref="Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/>
        public Keybind Add(GenericInput key, ModifierKey modifier) {
            return this.Add(key, modifier.GetKeys().Select(m => (GenericInput) m).ToArray());
        }

        /// <summary>
        /// Clears this keybind, removing all active combinations.
        /// </summary>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Clear() {
            this.combinations = Array.Empty<Combination>();
            return this;
        }

        /// <summary>
        /// Copies all of the combinations from the given keybind into this keybind.
        /// Note that this doesn't <see cref="Clear"/> this keybind, so combinations will be merged rather than replaced.
        /// </summary>
        /// <param name="other">The keybind to copy from</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind CopyFrom(Keybind other) {
            this.combinations = this.combinations.Concat(other.combinations).ToArray();
            return this;
        }

        /// <summary>
        /// Returns whether this keybind is considered to be down.
        /// See <see cref="InputHandler.IsDown"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be down</returns>
        public bool IsDown(InputHandler handler, int gamepadIndex = -1) {
            return this.combinations.Any(c => c.IsDown(handler, gamepadIndex));
        }

        /// <summary>
        /// Returns whether this keybind is considered to be pressed.
        /// See <see cref="InputHandler.IsPressed"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be pressed</returns>
        public bool IsPressed(InputHandler handler, int gamepadIndex = -1) {
            return this.combinations.Any(c => c.IsPressed(handler, gamepadIndex));
        }

        /// <summary>
        /// Returns an enumerable of all of the combinations that this keybind currently contains
        /// </summary>
        /// <returns>This keybind's combinations</returns>
        public IEnumerable<Combination> GetCombinations() {
            foreach (var combination in this.combinations)
                yield return combination;
        }

        /// <summary>
        /// A key combination is a combination of a set of modifier keys and a key.
        /// All of the keys are <see cref="GenericInput"/> instances, so they can be keyboard-, mouse- or gamepad-based.
        /// </summary>
        [DataContract]
        public class Combination {

            /// <summary>
            /// The inputs that have to be held down for this combination to be valid.
            /// If this collection is empty, there are no required modifier keys.
            /// </summary>
            [DataMember]
            public readonly GenericInput[] Modifiers;
            /// <summary>
            /// The input that has to be down (or pressed) for this combination to be considered down (or pressed).
            /// Note that <see cref="Modifiers"/> needs to be empty, or all of its values need to be down, as well.
            /// </summary>
            [DataMember]
            public readonly GenericInput Key;

            /// <summary>
            /// Creates a new combination with the given settings.
            /// To add a combination to a <see cref="Keybind"/>, use <see cref="Keybind.Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/> instead.
            /// </summary>
            /// <param name="key">The key</param>
            /// <param name="modifiers">The modifiers</param>
            public Combination(GenericInput key, GenericInput[] modifiers) {
                this.Modifiers = modifiers;
                this.Key = key;
            }

            internal bool IsDown(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsDown(this.Key, gamepadIndex);
            }

            internal bool IsPressed(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsPressed(this.Key, gamepadIndex);
            }

            private bool IsModifierDown(InputHandler handler, int gamepadIndex = -1) {
                return this.Modifiers.Length <= 0 || this.Modifiers.Any(m => handler.IsDown(m, gamepadIndex));
            }

        }

    }
}