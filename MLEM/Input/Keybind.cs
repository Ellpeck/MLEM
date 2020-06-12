using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    /// <summary>
    /// A keybind represents a generic way to trigger input.
    /// A keybind is made up of multiple key combinations, one of which has to be pressed for the keybind to be triggered.
    /// Note that this type is serializable using <see cref="DataContractAttribute"/>.
    /// </summary>
    [DataContract]
    public class Keybind {

        [DataMember]
        private readonly List<Combination> combinations = new List<Combination>();

        /// <summary>
        /// Adds a new key combination to this keybind that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Add(GenericInput key, params GenericInput[] modifiers) {
            this.combinations.Add(new Combination(key, modifiers));
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
            this.combinations.Clear();
            return this;
        }

        /// <summary>
        /// Copies all of the combinations from the given keybind into this keybind.
        /// Note that this doesn't <see cref="Clear"/> this keybind, so combinations will be merged rather than replaced.
        /// </summary>
        /// <param name="other">The keybind to copy from</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind CopyFrom(Keybind other) {
            this.combinations.AddRange(other.combinations);
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

        [DataContract]
        private class Combination {

            [DataMember]
            private readonly GenericInput[] modifiers;
            [DataMember]
            private readonly GenericInput key;

            public Combination(GenericInput key, GenericInput[] modifiers) {
                this.modifiers = modifiers;
                this.key = key;
            }

            internal bool IsDown(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsDown(this.key, gamepadIndex);
            }

            internal bool IsPressed(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsPressed(this.key, gamepadIndex);
            }

            private bool IsModifierDown(InputHandler handler, int gamepadIndex = -1) {
                return this.modifiers.Length <= 0 || this.modifiers.Any(m => handler.IsDown(m, gamepadIndex));
            }

        }

    }
}