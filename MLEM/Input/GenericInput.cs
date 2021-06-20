using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    /// <summary>
    /// A generic input represents any kind of input key.
    /// This includes <see cref="Keys"/> for keyboard keys, <see cref="MouseButton"/> for mouse buttons and <see cref="Buttons"/> for gamepad buttons.
    /// For creating and extracting inputs from a generic input, the implicit operators and <see cref="Type"/> can be used.
    /// Note that this type is serializable using <see cref="DataContractAttribute"/>.
    /// </summary>
    [DataContract]
    public readonly struct GenericInput {

        /// <summary>
        /// The <see cref="InputType"/> of this generic input's current <see cref="value"/>.
        /// </summary>
        [DataMember]
        public readonly InputType Type;
        [DataMember]
        private readonly int value;

        private GenericInput(InputType type, int value) {
            this.Type = type;
            this.value = value;
        }

        /// <inheritdoc />
        public override string ToString() {
            var ret = this.Type.ToString();
            switch (this.Type) {
                case InputType.Mouse:
                    ret += ((MouseButton) this).ToString();
                    break;
                case InputType.Keyboard:
                    ret += ((Keys) this).ToString();
                    break;
                case InputType.Gamepad:
                    ret += ((Buttons) this).ToString();
                    break;
            }
            return ret;
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            return obj is GenericInput o && this.Type == o.Type && this.value == o.value;
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            return ((int) this.Type * 397) ^ this.value;
        }

        /// <summary>
        /// Compares the two generic input instances for equality using <see cref="Equals"/>
        /// </summary>
        /// <param name="left">The left input</param>
        /// <param name="right">The right input</param>
        /// <returns>Whether the two generic inputs are equal</returns>
        public static bool operator ==(GenericInput left, GenericInput right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the two generic input instances for inequality using <see cref="Equals"/>
        /// </summary>
        /// <param name="left">The left input</param>
        /// <param name="right">The right input</param>
        /// <returns>Whether the two generic inputs are not equal</returns>
        public static bool operator !=(GenericInput left, GenericInput right) {
            return !left.Equals(right);
        }

        /// <summary>
        /// Converts a <see cref="Keys"/> to a generic input.
        /// </summary>
        /// <param name="keys">The keys to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(Keys keys) {
            return new GenericInput(InputType.Keyboard, (int) keys);
        }

        /// <summary>
        /// Converts a <see cref="MouseButton"/> to a generic input.
        /// </summary>
        /// <param name="button">The button to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(MouseButton button) {
            return new GenericInput(InputType.Mouse, (int) button);
        }

        /// <summary>
        /// Converts a <see cref="Buttons"/> to a generic input.
        /// </summary>
        /// <param name="buttons">The buttons to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(Buttons buttons) {
            return new GenericInput(InputType.Gamepad, (int) buttons);
        }

        /// <summary>
        /// Converts a generic input to a <see cref="Keys"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting keys</returns>
        /// <exception cref="ArgumentException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Keyboard"/> or <see cref="InputType.None"/></exception>
        public static implicit operator Keys(GenericInput input) {
            if (input.Type == InputType.None)
                return Keys.None;
            return input.Type == InputType.Keyboard ? (Keys) input.value : throw new ArgumentException();
        }

        /// <summary>
        /// Converts a generic input to a <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting button</returns>
        /// <exception cref="ArgumentException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Mouse"/></exception>
        public static implicit operator MouseButton(GenericInput input) {
            return input.Type == InputType.Mouse ? (MouseButton) input.value : throw new ArgumentException();
        }

        /// <summary>
        /// Converts a generic input to a <see cref="Buttons"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting buttons</returns>
        /// <exception cref="ArgumentException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Gamepad"/></exception>
        public static implicit operator Buttons(GenericInput input) {
            return input.Type == InputType.Gamepad ? (Buttons) input.value : throw new ArgumentException();
        }

        /// <summary>
        /// A type of input button.
        /// </summary>
        [DataContract]
        public enum InputType {

            /// <summary>
            /// A type representing no value
            /// </summary>
            [EnumMember]
            None,
            /// <summary>
            /// A type representing <see cref="MouseButton"/>
            /// </summary>
            [EnumMember]
            Mouse,
            /// <summary>
            /// A type representing <see cref="Keys"/>
            /// </summary>
            [EnumMember]
            Keyboard,
            /// <summary>
            /// A type representing <see cref="Buttons"/>
            /// </summary>
            [EnumMember]
            Gamepad

        }

    }
}