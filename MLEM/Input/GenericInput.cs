using System;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    /// <summary>
    /// A generic input represents any kind of input key.
    /// This includes <see cref="Keys"/> for keyboard keys, <see cref="MouseButton"/> for mouse buttons and <see cref="Buttons"/> for gamepad buttons.
    /// For creating and extracting inputs from a generic input, the implicit operators and <see cref="Type"/> can additionally be used.
    /// Note that this type is serializable using <see cref="DataContractAttribute"/>.
    /// </summary>
    [DataContract]
    public readonly struct GenericInput : IEquatable<GenericInput> {

        /// <summary>
        /// All <see cref="GenericInput"/> values created from all values of the <see cref="Keys"/> enum.
        /// </summary>
        public static readonly GenericInput[] AllKeys = InputHandler.AllKeys.Select(k => (GenericInput) k).ToArray();
        /// <summary>
        /// All <see cref="GenericInput"/> values created from all values of the <see cref="Input.MouseButton"/> enum.
        /// </summary>
        public static readonly GenericInput[] AllMouseButtons = MouseExtensions.MouseButtons.Select(k => (GenericInput) k).ToArray();
        /// <summary>
        /// All <see cref="GenericInput"/> values created from all values of the <see cref="Buttons"/> enum.
        /// </summary>
        public static readonly GenericInput[] AllButtons = InputHandler.AllButtons.Select(k => (GenericInput) k).ToArray();
        /// <summary>
        /// All <see cref="GenericInput"/> values created from all values of the <see cref="Keys"/>, <see cref="Input.MouseButton"/> and <see cref="Buttons"/> enums.
        /// This collection represents all possible valid, non-default <see cref="GenericInput"/> values.
        /// </summary>
        public static readonly GenericInput[] AllInputs = GenericInput.AllKeys.Concat(GenericInput.AllMouseButtons).Concat(GenericInput.AllButtons).ToArray();

        /// <summary>
        /// The <see cref="InputType"/> of this generic input's current <see cref="value"/>.
        /// </summary>
        [DataMember]
        public readonly InputType Type;
        [DataMember]
        private readonly int value;

        /// <summary>
        /// Returns this generic input's <see cref="Keys"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this generic input's <see cref="Type"/> is not <see cref="InputType.Keyboard"/> or <see cref="InputType.None"/>.</exception>
        public Keys Key => this.Type == InputType.None ? 0 : this.Type == InputType.Keyboard ? (Keys) this.value : throw new InvalidOperationException();
        /// <summary>
        /// Returns this generic input's <see cref="MouseButton"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this generic input's <see cref="Type"/> is not <see cref="InputType.Mouse"/>.</exception>
        public MouseButton MouseButton => this.Type == InputType.Mouse ? (MouseButton) this.value : throw new InvalidOperationException();
        /// <summary>
        /// Returns this generic input's <see cref="Buttons"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">If this generic input's <see cref="Type"/> is not <see cref="InputType.Gamepad"/> or <see cref="InputType.None"/>.</exception>
        public Buttons Button => this.Type == InputType.None ? 0 : this.Type == InputType.Gamepad ? (Buttons) this.value : throw new InvalidOperationException();

        /// <summary>
        /// Creates a new generic input from the given keyboard <see cref="Keys"/>.
        /// </summary>
        /// <param name="key">The key to convert.</param>
        public GenericInput(Keys key) : this(InputType.Keyboard, (int) key) {}

        /// <summary>
        /// Creates a new generic input from the given <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="button">The button to convert.</param>
        public GenericInput(MouseButton button) : this(InputType.Mouse, (int) button) {}

        /// <summary>
        /// Creates a new generic input from the given gamepad <see cref="Buttons"/>.
        /// </summary>
        /// <param name="button">The button to convert.</param>
        public GenericInput(Buttons button) : this(InputType.Gamepad, (int) button) {}

        /// <summary>
        /// Creates a new generic input from the given <see cref="Enum"/> value.
        /// If the value is a <see cref="MouseButton"/>, <see cref="Keys"/> or <see cref="Buttons"/>, the appropriate <see cref="Type"/> and value will be set. Otherwise, the <see cref="Type"/> will be set to <see cref="InputType.None"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public GenericInput(Enum value) {
            this.Type = value is MouseButton ? InputType.Mouse : value is Keys ? InputType.Keyboard : value is Buttons ? InputType.Gamepad : InputType.None;
            this.value = Convert.ToInt32(value);
        }

        private GenericInput(InputType type, int value) {
            this.Type = type;
            this.value = value;
        }

        /// <summary>Returns this generic input, converted to a string.</summary>
        /// <returns>This generic input, converted to a string.</returns>
        public override string ToString() {
            switch (this.Type) {
                case InputType.Mouse:
                    return $"Mouse{(MouseButton) this}";
                case InputType.Keyboard:
                    return ((Keys) this).ToString();
                case InputType.Gamepad:
                    return $"Gamepad{(Buttons) this}";
                default:
                    return this.Type.ToString();
            }
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(GenericInput other) {
            return this.Type == other.Type && this.value == other.value;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) {
            return obj is GenericInput other && this.Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            return (int) this.Type * 397 ^ this.value;
        }

        /// <summary>
        /// Compares the two generic input instances for equality using <see cref="Equals(GenericInput)"/>
        /// </summary>
        /// <param name="left">The left input</param>
        /// <param name="right">The right input</param>
        /// <returns>Whether the two generic inputs are equal</returns>
        public static bool operator ==(GenericInput left, GenericInput right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the two generic input instances for inequality using <see cref="Equals(GenericInput)"/>
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
        /// <param name="key">The keys to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(Keys key) {
            return new GenericInput(key);
        }

        /// <summary>
        /// Converts a <see cref="MouseButton"/> to a generic input.
        /// </summary>
        /// <param name="button">The button to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(MouseButton button) {
            return new GenericInput(button);
        }

        /// <summary>
        /// Converts a <see cref="Buttons"/> to a generic input.
        /// </summary>
        /// <param name="button">The buttons to convert</param>
        /// <returns>The resulting generic input</returns>
        public static implicit operator GenericInput(Buttons button) {
            return new GenericInput(button);
        }

        /// <summary>
        /// Converts a generic input to a <see cref="Keys"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting keys</returns>
        /// <exception cref="InvalidOperationException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Keyboard"/> or <see cref="InputType.None"/></exception>
        public static implicit operator Keys(GenericInput input) {
            return input.Key;
        }

        /// <summary>
        /// Converts a generic input to a <see cref="MouseButton"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting button</returns>
        /// <exception cref="InvalidOperationException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Mouse"/></exception>
        public static implicit operator MouseButton(GenericInput input) {
            return input.MouseButton;
        }

        /// <summary>
        /// Converts a generic input to a <see cref="Buttons"/>.
        /// </summary>
        /// <param name="input">The input to convert</param>
        /// <returns>The resulting buttons</returns>
        /// <exception cref="InvalidOperationException">If the given generic input's <see cref="Type"/> is not <see cref="InputType.Gamepad"/></exception>
        public static implicit operator Buttons(GenericInput input) {
            return input.Button;
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
