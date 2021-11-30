using System.Runtime.Serialization;
using MonoGame.Extended.Tiled;

namespace MLEM.Extended.Tiled {
    /// <summary>
    /// A struct that represents a position on a <see cref="TiledMap"/> with multiple layers.
    /// </summary>
    [DataContract]
    public struct LayerPosition {

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

        /// <inheritdoc cref="Equals(LayerPosition)"/>
        public static bool operator ==(LayerPosition left, LayerPosition right) {
            return left.Equals(right);
        }

        /// <inheritdoc cref="Equals(LayerPosition)"/>
        public static bool operator !=(LayerPosition left, LayerPosition right) {
            return !left.Equals(right);
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
            hashCode = (hashCode * 397) ^ this.X;
            hashCode = (hashCode * 397) ^ this.Y;
            return hashCode;
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() {
            return $"{nameof(this.Layer)}: {this.Layer}, {nameof(this.X)}: {this.X}, {nameof(this.Y)}: {this.Y}";
        }

    }
}