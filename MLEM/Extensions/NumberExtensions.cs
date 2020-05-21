using System;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="float"/>, <see cref="Vector2"/>, <see cref="Vector3"/>, <see cref="Vector4"/>, <see cref="Point"/>, <see cref="Rectangle"/> and <see cref="RectangleF"/>
    /// </summary>
    public static class NumberExtensions {

        /// <inheritdoc cref="Math.Floor(decimal)"/>
        public static int Floor(this float f) {
            return (int) Math.Floor(f);
        }

        /// <inheritdoc cref="Math.Ceiling(decimal)"/>
        public static int Ceil(this float f) {
            return (int) Math.Ceiling(f);
        }

        /// <summary>
        /// Checks for decimal equality with a given tolerance.
        /// </summary>
        /// <param name="first">The first number to equate</param>
        /// <param name="second">The second number to equate</param>
        /// <param name="tolerance">The equality tolerance</param>
        /// <returns>Whether or not <see cref="first"/> and <see cref="second"/> are different by at most <see cref="tolerance"/></returns>
        public static bool Equals(this float first, float second, float tolerance) {
            return Math.Abs(first - second) <= tolerance;
        }

        /// <inheritdoc cref="Equals(float,float,float)"/>
        public static bool Equals(this Vector2 first, Vector2 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance;
        }

        /// <inheritdoc cref="Equals(float,float,float)"/>
        public static bool Equals(this Vector3 first, Vector3 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance && Math.Abs(first.Z - second.Z) <= tolerance;
        }

        /// <inheritdoc cref="Equals(float,float,float)"/>
        public static bool Equals(this Vector4 first, Vector4 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance && Math.Abs(first.Z - second.Z) <= tolerance && Math.Abs(first.W - second.W) <= tolerance;
        }

        /// <inheritdoc cref="Math.Floor(decimal)"/>
        public static Vector2 Floor(this Vector2 vec) {
            return new Vector2(vec.X.Floor(), vec.Y.Floor());
        }

        /// <inheritdoc cref="Math.Floor(decimal)"/>
        public static Vector3 Floor(this Vector3 vec) {
            return new Vector3(vec.X.Floor(), vec.Y.Floor(), vec.Z.Floor());
        }

        /// <inheritdoc cref="Math.Floor(decimal)"/>
        public static Vector4 Floor(this Vector4 vec) {
            return new Vector4(vec.X.Floor(), vec.Y.Floor(), vec.Z.Floor(), vec.W.Floor());
        }

        /// <summary>
        /// Multiplies a point by a given scalar.
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="f">The scalar</param>
        /// <returns>The point, multiplied by the scalar memberwise</returns>
        public static Point Multiply(this Point point, float f) {
            return new Point((point.X * f).Floor(), (point.Y * f).Floor());
        }

        /// <summary>
        /// Divides a point by a given scalar.
        /// </summary>
        /// <param name="point">The point</param>
        /// <param name="f">The scalar</param>
        /// <returns>The point, divided by the scalar memberwise</returns>
        public static Point Divide(this Point point, float f) {
            return new Point((point.X / f).Floor(), (point.Y / f).Floor());
        }

        /// <summary>
        /// Transforms a point by a given matrix.
        /// </summary>
        /// <param name="position">The point</param>
        /// <param name="matrix">The matrix</param>
        /// <returns>The point, transformed by the matrix</returns>
        public static Point Transform(this Point position, Matrix matrix) {
            return new Point(
                (position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41).Floor(),
                (position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42).Floor());
        }

        /// <summary>
        /// Returns a copy of the given rectangle, moved by the given point.
        /// The rectangle's size remains unchanged.
        /// </summary>
        /// <param name="rect">The rectangle to move</param>
        /// <param name="offset">The amount to move by</param>
        /// <returns>The moved rectangle</returns>
        public static Rectangle OffsetCopy(this Rectangle rect, Point offset) {
            rect.X += offset.X;
            rect.Y += offset.Y;
            return rect;
        }

        /// <inheritdoc cref="OffsetCopy(Microsoft.Xna.Framework.Rectangle,Microsoft.Xna.Framework.Point)"/>
        public static RectangleF OffsetCopy(this RectangleF rect, Vector2 offset) {
            rect.X += offset.X;
            rect.Y += offset.Y;
            return rect;
        }

        /// <summary>
        /// Shrinks the rectangle by the given padding, causing its size to decrease by twice the amount and its position to be moved inwards by the amount.
        /// </summary>
        /// <param name="rect">The rectangle to shrink</param>
        /// <param name="padding">The padding to shrink by</param>
        /// <returns>The shrunk rectangle</returns>
        public static Rectangle Shrink(this Rectangle rect, Point padding) {
            rect.X += padding.X;
            rect.Y += padding.Y;
            rect.Width -= padding.X * 2;
            rect.Height -= padding.Y * 2;
            return rect;
        }

        /// <inheritdoc cref="Shrink(Microsoft.Xna.Framework.Rectangle,Microsoft.Xna.Framework.Point)"/>
        public static RectangleF Shrink(this RectangleF rect, Vector2 padding) {
            rect.X += padding.X;
            rect.Y += padding.Y;
            rect.Width -= padding.X * 2;
            rect.Height -= padding.Y * 2;
            return rect;
        }

        /// <inheritdoc cref="Shrink(Microsoft.Xna.Framework.Rectangle,Microsoft.Xna.Framework.Point)"/>
        public static RectangleF Shrink(this RectangleF rect, Padding padding) {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Width;
            rect.Height -= padding.Height;
            return rect;
        }

    }
}