using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    /// <summary>
    /// Represents a generic padding.
    /// A padding is an object of data that stores an offset from each side of a rectangle or square.
    /// </summary>
    [DataContract]
    public struct Padding : IEquatable<Padding> {

        /// <summary>
        /// The empty padding, with all borders set to 0
        /// </summary>
        public static Padding Empty => default;

        /// <summary>
        /// The amount of padding on the left side
        /// </summary>
        [DataMember]
        public float Left;
        /// <summary>
        /// The amount of padding on the right side
        /// </summary>
        [DataMember]
        public float Right;
        /// <summary>
        /// The amount of padding on the top side
        /// </summary>
        [DataMember]
        public float Top;
        /// <summary>
        /// The amount of padding on the bottom side
        /// </summary>
        [DataMember]
        public float Bottom;
        /// <summary>
        /// The total width of this padding, a sum of the left and right padding.
        /// </summary>
        public float Width => this.Left + this.Right;
        /// <summary>
        /// The total height of this padding, a sum of the top and bottom padding.
        /// </summary>
        public float Height => this.Top + this.Bottom;

        /// <summary>
        /// Create a new padding with the specified borders.
        /// </summary>
        /// <param name="left">The amount of padding on the left side</param>
        /// <param name="right">The amount of padding on the right side</param>
        /// <param name="top">The amount of padding on the top side</param>
        /// <param name="bottom">The amount of padding on the bottom side</param>
        public Padding(float left, float right, float top, float bottom) {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        /// <summary>
        /// Creates a new padding with the specified value, which will be applied to each edge.
        /// </summary>
        /// <param name="value">The padding to apply to each edge</param>
        public Padding(float value) : this(value, value) {}

        /// <summary>
        /// Creates a new padding with the specified x and y values, applying them to both edges.
        /// </summary>
        /// <param name="x">The x padding, which will turn into the left and right padding</param>
        /// <param name="y">The y padding, which till turn into the top and bottom padding</param>
        public Padding(float x, float y) : this(x, x, y, y) {}

        /// <summary>
        /// Creates a new padding from an existing padding, modifying it by growing or shrinking it.
        /// </summary>
        /// <param name="padding">The padding whose initial values to use</param>
        /// <param name="growth">The amount to grow each border by. Negative values will shrink the padding.</param>
        public Padding(Padding padding, float growth) : this(padding.Left + growth, padding.Right + growth, padding.Top + growth, padding.Bottom + growth) {}

        /// <summary>
        /// Implicitly creates a padding from the given two-dimensional vector.
        /// The left and right padding will both be the vector's x value, and the top and bottom padding will both be the vector's y value.
        /// </summary>
        /// <param name="vec">The vector to convert</param>
        /// <returns>A padding based on the vector's values</returns>
        public static implicit operator Padding(Vector2 vec) {
            return new Padding(vec.X, vec.X, vec.Y, vec.Y);
        }

        /// <summary>
        /// Scales a padding by a scalar.
        /// </summary>
        public static Padding operator *(Padding p, float scale) {
            return new Padding(p.Left * scale, p.Right * scale, p.Top * scale, p.Bottom * scale);
        }

        /// <summary>
        /// Adds two paddings together in a memberwise fashion.
        /// </summary>
        public static Padding operator +(Padding left, Padding right) {
            return new Padding(left.Left + right.Left, left.Right + right.Right, left.Top + right.Top, left.Bottom + right.Bottom);
        }

        /// <summary>
        /// Subtracts two paddings in a memberwise fashion.
        /// </summary>
        public static Padding operator -(Padding left, Padding right) {
            return new Padding(left.Left - right.Left, left.Right - right.Right, left.Top - right.Top, left.Bottom - right.Bottom);
        }

        /// <inheritdoc cref="Equals(Padding)"/>
        public static bool operator ==(Padding left, Padding right) {
            return left.Equals(right);
        }

        /// <inheritdoc cref="Equals(Padding)"/>
        public static bool operator !=(Padding left, Padding right) {
            return !(left == right);
        }

        /// <inheritdoc cref="Equals(object)"/>
        public bool Equals(Padding other) {
            return this.Left.Equals(other.Left) && this.Right.Equals(other.Right) && this.Top.Equals(other.Top) && this.Bottom.Equals(other.Bottom);
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) {
            return obj is Padding other && this.Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            var hashCode = this.Left.GetHashCode();
            hashCode = hashCode * 397 ^ this.Right.GetHashCode();
            hashCode = hashCode * 397 ^ this.Top.GetHashCode();
            hashCode = hashCode * 397 ^ this.Bottom.GetHashCode();
            return hashCode;
        }

    }
}
