namespace MLEM.Misc {
    /// <summary>
    /// The SingleRandom class allows generating single, one-off pseudorandom numbers based on a seed or a <see cref="SeedSource"/>.
    /// The types of numbers that can be generated are <see cref="int"/> and <see cref="float"/>, both of which can be generated with specific minimum and maximum values if desired.
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
        /// Generates a single, one-off pseudorandom integer between 0 and <see cref="int.MaxValue"/> based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(SeedSource source) {
            return (int) (SingleRandom.Single(source) * int.MaxValue);
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
        /// Generates a single, one-off pseudorandom integer between 0 and <paramref name="maxValue"/> based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int maxValue, SeedSource source) {
            return (int) (maxValue * SingleRandom.Single(source));
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
        /// Generates a single, one-off pseudorandom integer between <paramref name="minValue"/> and <paramref name="maxValue"/> based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static int Int(int minValue, int maxValue, SeedSource source) {
            return (int) ((maxValue - minValue) * SingleRandom.Single(source)) + minValue;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and 1 based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(int seed) {
            return (new SeedSource(seed).Get() / (float) int.MaxValue + 1) / 2;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and 1 based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(SeedSource source) {
            return (source.Get() / (float) int.MaxValue + 1) / 2;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and <paramref name="maxValue"/> based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(float maxValue, int seed) {
            return maxValue * SingleRandom.Single(seed);
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between 0 and <paramref name="maxValue"/> based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(float maxValue, SeedSource source) {
            return maxValue * SingleRandom.Single(source);
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between <paramref name="minValue"/> and <paramref name="maxValue"/> based on a given <paramref name="seed"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="seed"/>.
        /// </summary>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="seed">The seed to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(float minValue, float maxValue, int seed) {
            return (maxValue - minValue) * SingleRandom.Single(seed) + minValue;
        }

        /// <summary>
        /// Generates a single, one-off pseudorandom floating point number between <paramref name="minValue"/> and <paramref name="maxValue"/> based on a given <paramref name="source"/>.
        /// This method is guaranteed to return the same result for the same <paramref name="source"/>.
        /// </summary>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <returns>The generated number.</returns>
        public static float Single(float minValue, float maxValue, SeedSource source) {
            return (maxValue - minValue) * SingleRandom.Single(source) + minValue;
        }

    }
}
