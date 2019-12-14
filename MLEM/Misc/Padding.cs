using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    public struct Padding {

        public float Left;
        public float Right;
        public float Top;
        public float Bottom;
        public float Width => this.Left + this.Right;
        public float Height => this.Top + this.Bottom;

        public Padding(float left, float right, float top, float bottom) {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public static implicit operator Padding(Vector2 vec) {
            return new Padding(vec.X, vec.X, vec.Y, vec.Y);
        }

        public static Padding operator *(Padding p, float scale) {
            return new Padding(p.Left * scale, p.Right * scale, p.Top * scale, p.Bottom * scale);
        }

        public static Padding operator +(Padding left, Padding right) {
            return new Padding(left.Left + right.Left, left.Right + right.Right, left.Top + right.Top, left.Bottom + right.Bottom);
        }

        public static Padding operator -(Padding left, Padding right) {
            return new Padding(left.Left - right.Left, left.Right - right.Right, left.Top - right.Top, left.Bottom - right.Bottom);
        }

        public static bool operator ==(Padding left, Padding right) {
            return left.Equals(right);
        }

        public static bool operator !=(Padding left, Padding right) {
            return !(left == right);
        }

        public bool Equals(Padding other) {
            return this.Left.Equals(other.Left) && this.Right.Equals(other.Right) && this.Top.Equals(other.Top) && this.Bottom.Equals(other.Bottom);
        }

        public override bool Equals(object obj) {
            return obj is Padding other && this.Equals(other);
        }

        public override int GetHashCode() {
            var hashCode = this.Left.GetHashCode();
            hashCode = (hashCode * 397) ^ this.Right.GetHashCode();
            hashCode = (hashCode * 397) ^ this.Top.GetHashCode();
            hashCode = (hashCode * 397) ^ this.Bottom.GetHashCode();
            return hashCode;
        }

    }
}