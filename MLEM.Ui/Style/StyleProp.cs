using System;
using System.Collections.Generic;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Style {
    /// <summary>
    /// A struct used by <see cref="Element"/> to store style properties.
    /// This is a helper struct that allows default style settings from <see cref="UiStyle"/> to be overridden by custom user settings easily.
    /// Note that <c>T</c> implicitly converts to <c>StyleProp{T}</c> and vice versa.
    /// </summary>
    /// <typeparam name="T">The type of style setting that this property stores</typeparam>
    public struct StyleProp<T> : IEquatable<StyleProp<T>> {

        /// <summary>
        /// The currently applied style
        /// </summary>
        public T Value { get; private set; }
        private byte priority;

        /// <summary>
        /// Creates a new style property with the given custom style and a priority of <see cref="byte.MaxValue"/>.
        /// </summary>
        /// <param name="value">The custom style to apply</param>
        public StyleProp(T value) {
            this.priority = byte.MaxValue;
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
            if (priority >= this.priority) {
                this.Value = value;
                this.priority = priority;
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

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(StyleProp<T> other) {
            return EqualityComparer<T>.Default.Equals(this.Value, other.Value);
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) {
            return obj is StyleProp<T> other && this.Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(this.Value);
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

        /// <summary>
        /// Compares the two style properties and returns whether they are equal using <see cref="Equals(MLEM.Ui.Style.StyleProp{T})"/>.
        /// </summary>
        /// <param name="left">The left style property.</param>
        /// <param name="right">The right style property.</param>
        /// <returns>Whether the two style properties are equal.</returns>
        public static bool operator ==(StyleProp<T> left, StyleProp<T> right) {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares the two style properties and returns whether they are not equal using <see cref="Equals(MLEM.Ui.Style.StyleProp{T})"/>.
        /// </summary>
        /// <param name="left">The left style property.</param>
        /// <param name="right">The right style property.</param>
        /// <returns>Whether the two style properties are not equal.</returns>
        public static bool operator !=(StyleProp<T> left, StyleProp<T> right) {
            return !left.Equals(right);
        }

    }
}