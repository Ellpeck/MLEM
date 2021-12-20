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
        /// Sets this style property's value and marks it as being set by a <see cref="UiStyle"/> if it doesn't have a custom value yet.
        /// This allows this property to be overridden by custom style settings using <see cref="StyleProp{T}(T)"/> or a higher <paramref name="priority"/>.
        /// </summary>
        /// <param name="value">The style to apply</param>
        /// <param name="priority">The priority that this style value has. Higher priority style values will override lower priority style values.</param>
        ///<seealso cref="CopyFromStyle"/>
        public void SetFromStyle(T value, byte priority = 0) {
            if (priority >= this.lastSetPriority) {
                this.Value = value;
                this.lastSetPriority = priority;
            }
        }

        /// <summary>
        /// Creates a copy of this style property and sets its value and marks it as being set by a <see cref="UiStyle"/> if it doesn't have a custom value yet.
        /// This allows this property to be overridden by custom style settings using <see cref="StyleProp{T}(T)"/> or a higher <paramref name="priority"/>.
        /// </summary>
        /// <param name="value">The style to apply</param>
        /// <param name="priority">The priority that the style value has. Higher priority style values will override lower priority style values.</param>
        /// <returns>The new style</returns>
        /// <seealso cref="SetFromStyle"/>
        public StyleProp<T> CopyFromStyle(T value, byte priority = 0) {
            var ret = this;
            ret.SetFromStyle(value, priority);
            return ret;
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
        /// Returns whether this style property has a value assigned to it using <see cref="SetFromStyle"/> or <see cref="StyleProp{T}(T)"/>.
        /// </summary>
        /// <returns>Whether this style property has a value</returns>
        public bool HasValue() {
            return !EqualityComparer<T>.Default.Equals(this.Value, default);
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
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