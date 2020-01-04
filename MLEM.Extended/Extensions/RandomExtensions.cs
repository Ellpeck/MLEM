using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MLEM.Extended.Extensions {
    public static class RandomExtensions {

        public static int Range(this Random random, Range<int> range) {
            return random.Next(range.Min, range.Max);
        }

        public static float Range(this Random random, Range<float> range) {
            return random.NextSingle(range.Min, range.Max);
        }

        public static Vector2 Vector2(this Random random, float min, float max) {
            return new Vector2(random.NextSingle(min, max), random.NextSingle(min, max));
        }

    }
}