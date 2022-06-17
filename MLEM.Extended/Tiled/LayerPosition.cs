using System;
using System.Runtime.Serialization;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    /// <summary>
    /// A struct that represents a position on a <see cref="TiledMap"/> with multiple layers, where the <see cref="X"/> and <see cref="Y"/> coordinates are 32-bit integer numbers.
    /// See <see cref="LayerPositionF"/> for a floating point position.
    /// </summary>
    [DataContract]
    public struct LayerPosition : IEquatable<LayerPosition> {

        /// <summary>
        /// The name of the layer that this position is on
        /// </summary>
        [DataMember]
        public string Layer;
        /// <summary>
        /// The x coordinate of this position
        /// </summary>
        [DataMember]
        public int X;
        /// <summary>
        /// The y coordinate of this position
        /// </summary>
        [DataMember]
        public int Y;

        /// <summary>
        /// Creates a new layer position with the given settings
        /// </summary>
        /// <param name="layerName">The layer name</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        public LayerPosition(string layerName, int x, int y) {
            this.Layer = layerName;
            this.X = x;
            this.Y = y;
        }

        /// <inheritdoc cref="Equals(object)"/>
        public bool Equals(LayerPosition other) {
            return this.Layer == other.Layer && this.X == other.X && this.Y == other.Y;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns>true if <paramref name="obj">obj</paramref> and this instance are the same type and represent the same value; otherwise, false.</returns>
        public override bool Equals(object obj) {
            return obj is LayerPosition other && this.Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            var hashCode = this.Layer.GetHashCode();
            hashCode = hashCode * 397 ^ this.X;
            hashCode = hashCode * 397 ^ this.Y;
            return hashCode;
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() {
            return $"{nameof(this.Layer)}: {this.Layer}, {nameof(this.X)}: {this.X}, {nameof(this.Y)}: {this.Y}";
        }

        /// <summary>
        /// Adds the given layer positions together, returning a new layer position with the sum of their coordinates.
        /// If the two layer positions' <see cref="Layer"/> differ, an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="left">The left position.</param>
        /// <param name="right">The right position.</param>
        /// <returns>The sum of the positions.</returns>
        /// <exception cref="ArgumentException">Thrown if the two positions' <see cref="Layer"/> are not the same.</exception>
        public static LayerPosition Add(LayerPosition left, LayerPosition right) {
            if (left.Layer != right.Layer)
                throw new ArgumentException("Cannot add layer positions on different layers");
            return new LayerPosition(left.Layer, left.X + right.X, left.Y + right.Y);
        }

        /// <inheritdoc cref="Equals(LayerPosition)"/>
        public static bool operator ==(LayerPosition left, LayerPosition right) {
            return left.Equals(right);
        }

        /// <inheritdoc cref="Equals(LayerPosition)"/>
        public static bool operator !=(LayerPosition left, LayerPosition right) {
            return !left.Equals(right);
        }

        /// <summary>
        /// Returns the negative of the given layer position.
        /// </summary>
        /// <param name="position">The position to negate.</param>
        /// <returns>The negative position.</returns>
        public static LayerPosition operator -(LayerPosition position) {
            return new LayerPosition(position.Layer, -position.X, -position.Y);
        }

        /// <summary>
        /// Returns the sum of the two layer positions using <see cref="Add"/>.
        /// </summary>
        /// <param name="left">The left position.</param>
        /// <param name="right">The right position.</param>
        /// <returns>The sum of the positions.</returns>
        public static LayerPosition operator +(LayerPosition left, LayerPosition right) {
            return LayerPosition.Add(left, right);
        }

        /// <summary>
        /// Subtracts the second from the first position using <see cref="Add"/>.
        /// </summary>
        /// <param name="left">The left position.</param>
        /// <param name="right">The right position.</param>
        /// <returns>The difference of the positions.</returns>
        public static LayerPosition operator -(LayerPosition left, LayerPosition right) {
            return LayerPosition.Add(left, -right);
        }

        /// <summary>
        /// Implicitly converts a <see cref="LayerPosition"/> to a <see cref="LayerPositionF"/>.
        /// </summary>
        /// <param name="position">The position to convert.</param>
        /// <returns>The converted position.</returns>
        public static implicit operator LayerPositionF(LayerPosition position) {
            return new LayerPositionF(position.Layer, position.X, position.Y);
        }

    }
}
