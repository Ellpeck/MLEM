using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extension methods for dealing with <see cref="Random"/>
    /// </summary>
    public static class RandomExtensions {

        /// <summary>
        /// Returns a random number in the given range.
        /// </summary>
        /// <param name="random">The random to use for generation</param>
        /// <param name="range">The range in which numbers will be generated</param>
        /// <returns>A number in the given range</returns>
        public static int Range(this Random random, Range<int> range) {
            return random.Next(range.Min, range.Max);
        }

        /// <summary>
        /// Returns a random number in the given range.
        /// </summary>
        /// <param name="random">The random to use for generation</param>
        /// <param name="range">The range in which numbers will be generated</param>
        /// <returns>A number in the given range</returns>
        public static float Range(this Random random, Range<float> range) {
            return random.NextSingle(range.Min, range.Max);
        }

        /// <summary>
        /// Returns a random vector whose x and y values are in the given range.
        /// </summary>
        /// <param name="random">The random to use for generation</param>
        /// <param name="min">The minimum value for each coordinate</param>
        /// <param name="max">The maximum value for each coordinate</param>
        /// <returns>A random vector in the given range</returns>
        public static Vector2 NextVector2(this Random random, float min, float max) {
            return new Vector2(random.NextSingle(min, max), random.NextSingle(min, max));
        }

    }
}