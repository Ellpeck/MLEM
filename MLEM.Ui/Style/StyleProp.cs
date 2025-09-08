using System.Collections.Generic;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Style {
    /// <summary>
    /// A struct used by <see cref="Element"/> to store style properties.
    /// This is a helper struct that allows default style settings from <see cref="UiStyle"/> to be overridden by custom user settings easily.
    /// Note that <c>T</c> implicitly converts to <c>StyleProp{T}</c> and vice versa.
    /// </summary>
    /// <typeparam name="T">The type of style setting that this property stores</typeparam>
    public readonly struct StyleProp<T> {

        /// <summary>
        /// The empty style property, with no <see cref="Value"/> and a priority of 0.
        /// </summary>
        public static StyleProp<T> None => default;

        /// <summary>
        /// The currently applied style
        /// </summary>
        public readonly T Value;
        private readonly byte priority;

        /// <summary>
        /// Creates a new style property with the given custom style and a priority of <see cref="byte.MaxValue"/>.
        /// To create a style property with a lower priority, use <see cref="OrStyle(T,byte)"/> on an existing priority, or use <see cref="None"/>.
        /// </summary>
        /// <param name="value">The custom style to apply</param>
        public StyleProp(T value) : this(value, byte.MaxValue) {}

        private StyleProp(T value, byte priority) {
            this.Value = value;
            this.priority = priority;
        }

        /// <summary>
        /// Creates a copy of this style property and sets its value and marks it as being set by a <see cref="UiStyle"/> if it doesn't have a custom value yet.
        /// This allows this property to be overridden by custom style settings using <see cref="StyleProp{T}(T)"/> or a higher <paramref name="priority"/>.
        /// </summary>
        /// <param name="value">The style to apply</param>
        /// <param name="priority">The priority that the style value has. Higher priority style values will override lower priority style values.</param>
        /// <returns>The style with the higher priority</returns>
        public StyleProp<T> OrStyle(T value, byte priority = 0) {
            return this.OrStyle(new StyleProp<T>(value, priority));
        }

        /// <summary>
        /// Chooses and returns the style property with the higher priority, out of this value and <paramref name="other"/>.
        /// This allows this property to be overridden by custom style settings using <see cref="StyleProp{T}(T)"/> or a higher priority.
        /// </summary>
        /// <param name="other">The style property to compare with</param>
        /// <returns>The style property with the higher priority</returns>
        public StyleProp<T> OrStyle(StyleProp<T> other) {
            return other.priority >= this.priority ? other : this;
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
        /// Returns whether this style property has a value assigned to it using <see cref="OrStyle(StyleProp{T})"/> or <see cref="StyleProp{T}(T)"/>.
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
