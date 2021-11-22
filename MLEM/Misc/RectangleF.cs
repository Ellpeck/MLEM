using System;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using MLEM.Extensions;

namespace MLEM.Misc {
    /// <summary>
    /// Represents a float-based version of <see cref="Rectangle"/>
    /// </summary>
    [DataContract]
    public struct RectangleF : IEquatable<RectangleF> {

        /// <summary>
        /// The empty rectangle, with an x, y, width and height of 0.
        /// </summary>
        public static RectangleF Empty => default;

        /// <summary>
        /// The x position of the top left corner of this rectangle.
        /// </summary>
        [DataMember]
        public float X;
        /// <summary>
        /// The y position of the top left corner of this rectangle.
        /// </summary>
        [DataMember]
        public float Y;
        /// <summary>
        /// The width of this rectangle.
        /// </summary>
        [DataMember]
        public float Width;
        /// <summary>
        /// The height of this rectangle.
        /// </summary>
        [DataMember]
        public float Height;

        /// <inheritdoc cref="X"/>
        public float Left => this.X;
        /// <summary>
        /// The x position of the bottom right corner of this rectangle.
        /// </summary>
        public float Right => this.X + this.Width;
        /// <inheritdoc cref="Y"/>
        public float Top => this.Y;
        /// <summary>
        /// The y position of the bottom right corner of this rectangle.
        /// </summary>
        public float Bottom => this.Y + this.Height;
        /// <summary>
        /// A boolean that is true if this rectangle is empty.
        /// A rectangle is considered empty if the width or height is 0.
        /// </summary>
        public bool IsEmpty => this.Width <= 0 || this.Height <= 0;

        /// <summary>
        /// The top left corner of this rectangle
        /// </summary>
        public Vector2 Location {
            get => new Vector2(this.X, this.Y);
            set {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        /// <summary>
        /// The size, that is, the width and height of this rectangle.
        /// </summary>
        public Vector2 Size {
            get => new Vector2(this.Width, this.Height);
            set {
                this.Width = value.X;
                this.Height = value.Y;
            }
        }
        /// <summary>
        /// The center of this rectangle, based on the top left corner and the size.
        /// </summary>
        public Vector2 Center => new Vector2(this.X + this.Width / 2, this.Y + this.Height / 2);

        /// <summary>
        /// Creates a new rectangle with the specified location and size
        /// </summary>
        /// <param name="x">The x coordinate of the top left corner of the rectangle</param>
        /// <param name="y">The y coordinate of the top left corner of the rectangle</param>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        public RectangleF(float x, float y, float width, float height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Creates a new rectangle with the specified location and size vectors
        /// </summary>
        /// <param name="location">The top left corner of the rectangle</param>
        /// <param name="size">The size of the rectangle, where x represents width and the y represents height</param>
        public RectangleF(Vector2 location, Vector2 size) {
            this.X = location.X;
            this.Y = location.Y;
            this.Width = size.X;
            this.Height = size.Y;
        }

        /// <summary>
        /// Creates a new rectangle based on two corners that form a bounding box.
        /// The resulting rectangle will encompass both corners as well as all of the space between them.
        /// </summary>
        /// <param name="corner1">The first corner to use</param>
        /// <param name="corner2">The second corner to use</param>
        /// <returns></returns>
        public static RectangleF FromCorners(Vector2 corner1, Vector2 corner2) {
            var minX = Math.Min(corner1.X, corner2.X);
            var minY = Math.Min(corner1.Y, corner2.Y);
            var maxX = Math.Max(corner1.X, corner2.X);
            var maxY = Math.Max(corner1.Y, corner2.Y);
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        /// <summary>
        /// Converts a float-based rectangle to an int-based rectangle, flooring each value in the process.
        /// </summary>
        /// <param name="rect">The rectangle to convert</param>
        /// <returns>The resulting rectangle</returns>
        public static explicit operator Rectangle(RectangleF rect) {
            return new Rectangle(rect.X.Floor(), rect.Y.Floor(), rect.Width.Floor(), rect.Height.Floor());
        }

        /// <summary>
        /// Converts an int-based rectangle to a float-based rectangle.
        /// </summary>
        /// <param name="rect">The rectangle to convert</param>
        /// <returns>The resulting rectangle</returns>
        public static explicit operator RectangleF(Rectangle rect) {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <inheritdoc cref="Equals(RectangleF)"/>
        public static bool operator ==(RectangleF a, RectangleF b) {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        /// <inheritdoc cref="Equals(RectangleF)"/>
        public static bool operator !=(RectangleF a, RectangleF b) {
            return !(a == b);
        }

        /// <inheritdoc cref="Rectangle.Contains(float, float)"/>
        public bool Contains(float x, float y) {
            return this.X <= x && x < this.X + this.Width && this.Y <= y && y < this.Y + this.Height;
        }

        /// <inheritdoc cref="Rectangle.Contains(Vector2)"/>
        public bool Contains(Vector2 value) {
            return this.Contains(value.X, value.Y);
        }

        /// <inheritdoc cref="Rectangle.Contains(Rectangle)"/>
        public bool Contains(RectangleF value) {
            return this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) {
            return obj is RectangleF f && this == f;
        }

        /// <inheritdoc cref="Equals(object)"/>
        public bool Equals(RectangleF other) {
            return this == other;
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
        }

        /// <inheritdoc cref="Rectangle.Inflate(float,float)"/>
        public void Inflate(float horizontalAmount, float verticalAmount) {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }

        /// <inheritdoc cref="Rectangle.Intersects(Rectangle)"/>
        public bool Intersects(RectangleF value) {
            return value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
        }

        /// <inheritdoc cref="Rectangle.Intersect(Rectangle, Rectangle)"/>
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

        /// <inheritdoc cref="Rectangle.Offset(float, float)"/>
        public void Offset(float offsetX, float offsetY) {
            this.X += offsetX;
            this.Y += offsetY;
        }

        /// <inheritdoc cref="Rectangle.Offset(Vector2)"/>
        public void Offset(Vector2 amount) {
            this.X += amount.X;
            this.Y += amount.Y;
        }

        /// <inheritdoc/>
        public override string ToString() {
            return "{X:" + this.X + " Y:" + this.Y + " Width:" + this.Width + " Height:" + this.Height + "}";
        }

        /// <inheritdoc cref="Rectangle.Union(Rectangle, Rectangle)"/>
        public static RectangleF Union(RectangleF value1, RectangleF value2) {
            var x = Math.Min(value1.X, value2.X);
            var y = Math.Min(value1.Y, value2.Y);
            return new RectangleF(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
        }

        /// <inheritdoc cref="Rectangle.Deconstruct"/>
        public void Deconstruct(out float x, out float y, out float width, out float height) {
            x = this.X;
            y = this.Y;
            width = this.Width;
            height = this.Height;
        }

    }
}