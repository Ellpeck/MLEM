using System;
using Microsoft.Xna.Framework;

namespace MLEM.Extensions {
    public static class NumberExtensions {

        public static int Floor(this float f) {
            return (int) Math.Floor(f);
        }

        public static int Ceil(this float f) {
            return (int) Math.Ceiling(f);
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

    }
}