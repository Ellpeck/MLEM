using System.Runtime.Serialization;

namespace MLEM.Extended.Tiled {
    [DataContract]
    public struct LayerPosition {

        [DataMember]
        public string Layer;
        [DataMember]
        public int X;
        [DataMember]
        public int Y;

        public LayerPosition(string layerName, int x, int y) {
            this.Layer = layerName;
            this.X = x;
            this.Y = y;
        }

        public static bool operator ==(LayerPosition left, LayerPosition right) {
            return left.Equals(right);
        }

        public static bool operator !=(LayerPosition left, LayerPosition right) {
            return !left.Equals(right);
        }

        public bool Equals(LayerPosition other) {
            return this.Layer == other.Layer && this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj) {
            return obj is LayerPosition other && this.Equals(other);
        }

        public override int GetHashCode() {
            var hashCode = this.Layer.GetHashCode();
            hashCode = (hashCode * 397) ^ this.X;
            hashCode = (hashCode * 397) ^ this.Y;
            return hashCode;
        }

        public override string ToString() {
            return $"{nameof(this.Layer)}: {this.Layer}, {nameof(this.X)}: {this.X}, {nameof(this.Y)}: {this.Y}";
        }

    }
}