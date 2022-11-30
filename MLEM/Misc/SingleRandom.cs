using System;

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

    /// <summary>
    /// A seed source contains an <see cref="int"/> value which can be used as a seed for a <see cref="System.Random"/> or <see cref="SingleRandom"/>. Seed sources feature a convenient way to add multiple seeds using <see cref="Add(int)"/>, which will be sufficiently scrambled to be deterministically semi-random and combined into a single <see cref="int"/>.
    /// This struct behaves similarly to <c>System.HashCode</c> in many ways, with an important distinction being that <see cref="SeedSource"/>'s scrambling procedure is not considered an implementation detail, and will stay consistent between process executions.
    /// </summary>
    /// <example>
    /// For example, a seed source can be used to create a new <see cref="System.Random"/> based on an object's <c>x</c> and <c>y</c> coordinates by combining them into a <see cref="SeedSource"/> using <see cref="Add(int)"/>. The values generated by the <see cref="System.Random"/> created using <see cref="Random()"/> will then be determined by the specific pair of <c>x</c> and <c>y</c> values used.
    /// </example>
    public readonly struct SeedSource {

        private readonly int? value;

        /// <summary>
        /// Creates a new seed source from the given seed, which will be added automatically using <see cref="Add(int)"/>.
        /// </summary>
        /// <param name="seed">The initial seed to use.</param>
        public SeedSource(int seed) : this() {
            this = this.Add(seed);
        }

        /// <summary>
        /// Creates a new seed source from the given set of seeds, which will be added automatically using <see cref="Add(int)"/>.
        /// </summary>
        /// <param name="seeds">The initial seeds to use.</param>
        public SeedSource(params int[] seeds) : this() {
            foreach (var seed in seeds)
                this = this.Add(seed);
        }

        private SeedSource(int? value) {
            this.value = value;
        }

        /// <summary>
        /// Adds the given seed to this seed source's value and returns the result as a new seed source.
        /// The algorithm used for adding involves various scrambling operations that sufficiently semi-randomize the seed and final value.
        /// </summary>
        /// <param name="seed">The seed to add.</param>
        /// <returns>A new seed source with the seed added.</returns>
        public SeedSource Add(int seed) {
            return new SeedSource(new int?(SeedSource.Scramble(this.Get()) + SeedSource.Scramble(seed)));
        }

        /// <summary>
        /// Adds the given seed to this seed source's value and returns the result as a new seed source.
        /// Floating point values are scrambled by invoking <see cref="Add(int)"/> using a typecast version, followed by invoking <see cref="Add(int)"/> using the decimal value multiplied by <see cref="int.MaxValue"/>.
        /// </summary>
        /// <param name="seed">The seed to add.</param>
        /// <returns>A new seed source with the seed added.</returns>
        public SeedSource Add(float seed) {
            return this.Add((int) seed).Add((seed - (int) seed) * int.MaxValue);
        }

        /// <summary>
        /// Adds the given seed to this seed source's value and returns the result as a new seed source.
        /// Strings are scrambled by invoking <see cref="Add(int)"/> using every character contained in the string, in order.
        /// </summary>
        /// <param name="seed">The seed to add.</param>
        /// <returns>A new seed source with the seed added.</returns>
        public SeedSource Add(string seed) {
            var ret = this;
            foreach (var c in seed)
                ret = ret.Add(c);
            return ret;
        }

        /// <summary>
        /// Returns this seed source's seed value, which can then be used in <see cref="SingleRandom"/> or elsewhere.
        /// </summary>
        /// <returns>This seed source's value.</returns>
        public int Get() {
            return this.value ?? 1623487;
        }

        /// <summary>
        /// Returns a new <see cref="Random"/> instance using this source seed's value, retrieved using <see cref="Get"/>.
        /// </summary>
        /// <returns>A new <see cref="Random"/> using this seed source's value.</returns>
        public Random Random() {
            return new Random(this.Get());
        }

        private static int Scramble(int x) {
            x += 84317;
            x ^= x << 7;
            x *= 207398809;
            x ^= x << 17;
            x *= 928511849;
            return x;
        }

    }
}
