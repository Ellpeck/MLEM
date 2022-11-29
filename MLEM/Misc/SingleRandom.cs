using System;

namespace MLEM.Misc {
    /// <summary>
    /// The SingleRandom class allows generating single, one-off pseudorandom numbers based on a seed, or a set of seeds.
    /// The types of numbers that can be generated are <see cref="int"/> and <see cref="float"/>, the former of which can be generated with specific minimum and maximum values.
    /// Methods in this class are tested to be sufficiently "random", that is, to be sufficiently distributed throughout their range, as well as sufficiently different for neighboring seeds.
    /// </summary>
    public class SingleRandom {

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between 0 and <see cref="int.MaxValue"/> based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int seed) {
            return (int) (SingleRandom.Single(seed) * int.MaxValue);
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between 0 and <see cref="int.MaxValue"/> based on a given set of <paramref name="seeds"/>.
        /// This method is guaranteed to return the same result for the same set of <paramref name="seeds"/>.
        /// </summary>
        /// <param name="seeds">The seeds to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(params int[] seeds) {
            return (int) (SingleRandom.Single(seeds) * int.MaxValue);
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between 0 and <paramref name="maxValue"/> based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int maxValue, int seed) {
            return (int) (maxValue * SingleRandom.Single(seed));
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between 0 and <paramref name="maxValue"/> based on a given set of <paramref name="seeds"/>.
        /// This method is guaranteed to return the same result for the same set of <paramref name="seeds"/>.
        /// </summary>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seeds">The seeds to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int maxValue, params int[] seeds) {
            return (int) (maxValue * SingleRandom.Single(seeds));
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between <paramref name="minValue"/> and <paramref name="maxValue"/> based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int minValue, int maxValue, int seed) {
            return (int) ((maxValue - minValue) * SingleRandom.Single(seed)) + minValue;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom integer between <paramref name="minValue"/> and <paramref name="maxValue"/> based on a given set of <paramref name="seeds"/>.
        /// This method is guaranteed to return the same result for the same set of <paramref name="seeds"/>.
        /// </summary>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seeds">The seeds to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int minValue, int maxValue, params int[] seeds) {
            return (int) ((maxValue - minValue) * SingleRandom.Single(seeds)) + minValue;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and 1 based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(int seed) {
            return (SingleRandom.Scramble(seed) / (float) int.MaxValue + 1) / 2;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and 1 based on a given set of <paramref name="seeds"/>.
        /// This method is guaranteed to return the same result for the same set of <paramref name="seeds"/>.
        /// </summary>
        /// <param name="seeds">The seeds to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(params int[] seeds) {
            return (SingleRandom.Scramble(seeds) / (float) int.MaxValue + 1) / 2;
        }

        private static int Scramble(int[] seeds) {
            if (seeds == null || seeds.Length <= 0)
                throw new ArgumentOutOfRangeException(nameof(seeds));
            var ret = 1;
            for (var i = 0; i < seeds.Length; i++)
                ret *= SingleRandom.Scramble(seeds[i]);
            return ret;
        }

        private static int Scramble(int seed) {
            seed ^= (seed << 7);
            seed *= 207398809;
            seed ^= (seed << 17);
            seed *= 928511849;
            seed ^= (seed << 12);
            return seed;
        }

    }
}
