using System;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Extensions {
    public static class NumberExtensions {

        public static int Floor(this float f) {
            return (int) Math.Floor(f);
        }

        public static int Ceil(this float f) {
            return (int) Math.Ceiling(f);
        }

        public static bool Equals(this float first, float second, float tolerance) {
            return Math.Abs(first- second) <= tolerance;
        }

        public static bool Equals(this Vector2 first, Vector2 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance;
        }

        public static bool Equals(this Vector3 first, Vector3 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance && Math.Abs(first.Z - second.Z) <= tolerance;
        }

        public static bool Equals(this Vector4 first, Vector4 second, float tolerance) {
            return Math.Abs(first.X - second.X) <= tolerance && Math.Abs(first.Y - second.Y) <= tolerance && Math.Abs(first.Z - second.Z) <= tolerance && Math.Abs(first.W - second.W) <= tolerance;
        }

        public static Vector2 Floor(this Vector2 vec) {
            return new Vector2(vec.X.Floor(), vec.Y.Floor());
        }

        public static Vector3 Floor(this Vector3 vec) {
            return new Vector3(vec.X.Floor(), vec.Y.Floor(), vec.Z.Floor());
        }

        public static Vector4 Floor(this Vector4 vec) {
            return new Vector4(vec.X.Floor(), vec.Y.Floor(), vec.Z.Floor(), vec.W.Floor());
        }

        public static Point Multiply(this Point point, float f) {
            return new Point((point.X * f).Floor(), (point.Y * f).Floor());
        }

        public static Point Divide(this Point point, float f) {
            return new Point((point.X / f).Floor(), (point.Y / f).Floor());
        }

        public static Point Transform(this Point position, Matrix matrix) {
            return new Point(
                (position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41).Floor(),
                (position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42).Floor());
        }

        public static Rectangle OffsetCopy(this Rectangle rect, Point offset) {
            rect.X += offset.X;
            rect.Y += offset.Y;
            return rect;
        }

        public static RectangleF OffsetCopy(this RectangleF rect, Vector2 offset) {
            rect.X += offset.X;
            rect.Y += offset.Y;
            return rect;
        }

        public static Rectangle Shrink(this Rectangle rect, Point padding) {
            rect.X += padding.X;
            rect.Y += padding.Y;
            rect.Width -= padding.X * 2;
            rect.Height -= padding.Y * 2;
            return rect;
        }

        public static RectangleF Shrink(this RectangleF rect, Vector2 padding) {
            rect.X += padding.X;
            rect.Y += padding.Y;
            rect.Width -= padding.X * 2;
            rect.Height -= padding.Y * 2;
            return rect;
        }

        public static RectangleF Shrink(this RectangleF rect, Padding padding) {
            rect.X += padding.Left;
            rect.Y += padding.Top;
            rect.Width -= padding.Width;
            rect.Height -= padding.Height;
            return rect;
        }

    }
}