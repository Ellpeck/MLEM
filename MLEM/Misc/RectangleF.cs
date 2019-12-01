using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using MLEM.Extensions;

namespace MLEM.Misc {
    [DataContract]
    public struct RectangleF : IEquatable<RectangleF> {

        public static RectangleF Empty => default;

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Width;
        [DataMember]
        public float Height;

        public float Left => this.X;
        public float Right => this.X + this.Width;
        public float Top => this.Y;
        public float Bottom => this.Y + this.Height;
        public bool IsEmpty => this.Width <= 0 || this.Height <= 0;

        public Vector2 Location {
            get => new Vector2(this.X, this.Y);
            set {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        public Vector2 Size {
            get => new Vector2(this.Width, this.Height);
            set {
                this.Width = value.X;
                this.Height = value.Y;
            }
        }
        public Vector2 Center => new Vector2(this.X + this.Width / 2, this.Y + this.Height / 2);

        public RectangleF(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public RectangleF(Vector2 location, Vector2 size) {
            this.X = location.X;
            this.Y = location.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        public static explicit operator Rectangle(RectangleF rect) {
            return new Rectangle(rect.X.Floor(), rect.Y.Floor(), rect.Width.Floor(), rect.Height.Floor());
        }

        public static explicit operator RectangleF(Rectangle rect) {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static bool operator ==(RectangleF a, RectangleF b) {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        public static bool operator !=(RectangleF a, RectangleF b) {
            return !(a == b);
        }

        public bool Contains(float x, float y) {
            return this.X <= x && x < this.X + this.Width && this.Y <= y && y < this.Y + this.Height;
        }

        public bool Contains(Vector2 value) {
            return this.Contains(value.X, value.Y);
        }

        public bool Contains(RectangleF value) {
            return this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
        }

        public override bool Equals(object obj) {
            return obj is RectangleF f && this == f;
        }

        public bool Equals(RectangleF other) {
            return this == other;
        }

        public override int GetHashCode() {
            return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
        }

        public void Inflate(float horizontalAmount, float verticalAmount) {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }

        public bool Intersects(RectangleF value) {
            return value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
        }

        public static RectangleF Intersect(RectangleF value1, RectangleF value2) {
            if (value1.Intersects(value2)) {
                var num1 = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
                var x = Math.Max(value1.X, value2.X);
                var y = Math.Max(value1.Y, value2.Y);
                var num2 = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
                return new RectangleF(x, y, num1 - x, num2 - y);
            } else {
                return Empty;
            }
        }

        public void Offset(float offsetX, float offsetY) {
            this.X += offsetX;
            this.Y += offsetY;
        }

        public void Offset(Vector2 amount) {
            this.X += amount.X;
            this.Y += amount.Y;
        }

        public override string ToString() {
            return "{X:" + this.X + " Y:" + this.Y + " Width:" + this.Width + " Height:" + this.Height + "}";
        }

        public static RectangleF Union(RectangleF value1, RectangleF value2) {
            var x = Math.Min(value1.X, value2.X);
            var y = Math.Min(value1.Y, value2.Y);
            return new RectangleF(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        public void Deconstruct(out float x, out float y, out float width, out float height) {
            x = this.X;
            y = this.Y;
            width = this.Width;
            height = this.Height;
        }

    }
}