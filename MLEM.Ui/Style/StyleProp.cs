using System.Collections.Generic;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Style {
    /// <summary>
    /// A struct used by <see cref="Element"/> to store style properties.
    /// This is a helper struct that allows default style settings from <see cref="UiStyle"/> to be overridden by custom user settings easily.
    /// Note that <c>T</c> implicitly converts to <c>StyleProp{T}</c> and vice versa.
    /// </summary>
    /// <typeparam name="T">The type of style setting that this property stores</typeparam>
    public struct StyleProp<T> {

        /// <summary>
        /// The currently applied style
        /// </summary>
        public T Value { get; private set; }
        private byte lastSetPriority;

        /// <summary>
        /// Creates a new style property with the given custom style.
        /// </summary>
        /// <param name="value">The custom style to apply</param>
        public StyleProp(T value) {
            this.lastSetPriority = byte.MaxValue;
            this.Value = value;
        }

        /// <summary>
        /// Sets this style property's value and marks it as being set by a <see cref="UiStyle"/>.
        /// This allows this property to be overridden by custom style settings using <see cref="Set"/>.
        /// </summary>
        /// <param name="value">The style to apply</param>
        /// <param name="priority">The priority that this style value has. Higher priority style values will override lower priority style values.</param>
        public void SetFromStyle(T value, byte priority = 0) {
            if (priority >= this.lastSetPriority) {
                this.Value = value;
                this.lastSetPriority = priority;
            }
        }

        /// <summary>
        /// Sets this style property's value and marks it as being custom.
        /// This causes <see cref="SetFromStyle"/> not to override the style value through a <see cref="UiStyle"/>.
        /// </summary>
        /// <param name="value"></param>
        public void Set(T value) {
            this.lastSetPriority = byte.MaxValue;
            this.Value = value;
        }

        /// <summary>
        /// Returns the current style value or, if <see cref="HasValue"/> is false, the given default value.
        /// </summary>
        /// <param name="def">The default to return if this style property has no value</param>
        /// <returns>The current value, or the default</returns>
        public T OrDefault(T def) {
            return this.HasValue() ? this.Value : def;
        }

        /// <summary>
        /// Returns whether this style property has a value assigned to it using <see cref="SetFromStyle"/> or <see cref="Set"/>.
        /// </summary>
        /// <returns>Whether this style property has a value</returns>
        public bool HasValue() {
            return !EqualityComparer<T>.Default.Equals(this.Value, default);
        }

        /// <inheritdoc />
        public override string ToString() {
            return this.Value?.ToString();
        }

        /// <summary>
        /// Implicitly converts a style property to its value.
        /// </summary>
        /// <param name="prop">The property to convert</param>
        /// <returns>The style that the style property holds</returns>
        public static implicit operator T(StyleProp<T> prop) {
            return prop.Value;
        }

        /// <summary>
        /// Implicitly converts a style to a style property.
        /// </summary>
        /// <param name="prop">The property to convert</param>
        /// <returns>A style property with the given style value</returns>
        public static implicit operator StyleProp<T>(T prop) {
            return new StyleProp<T>(prop);
        }

    }
}