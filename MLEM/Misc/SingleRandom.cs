using System;
using System.Collections.Generic;
using MLEM.Extensions;

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
            return (new SeedSource().Add(seed).Get() / (float) int.MaxValue + 1) / 2;
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

        /// <summary>
        /// Gets a random entry from the given collection with uniform chance.
        /// </summary>
        /// <param name="entries">The entries to choose from</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry</returns>
        public static T GetRandomEntry<T>(ICollection<T> entries, SeedSource source) {
            return RandomExtensions.GetRandomEntry(entries, SingleRandom.Single(source));
        }

        /// <summary>
        /// Returns a random entry from the given collection based on the specified weight function.
        /// A higher weight for an entry increases its likeliness of being picked.
        /// </summary>
        /// <param name="entries">The entries to choose from</param>
        /// <param name="weightFunc">A function that applies weight to each entry</param>
        /// <param name="source">The <see cref="SeedSource"/> to use.</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry, based on the entries' weight</returns>
        /// <exception cref="IndexOutOfRangeException">If the weight function returns different weights for the same entry</exception>
        public static T GetRandomWeightedEntry<T>(ICollection<T> entries, Func<T, int> weightFunc, SeedSource source) {
            return RandomExtensions.GetRandomWeightedEntry(entries, weightFunc, SingleRandom.Single(source));
        }

        /// <inheritdoc cref="GetRandomWeightedEntry{T}(System.Collections.Generic.ICollection{T},System.Func{T,int},MLEM.Misc.SeedSource)"/>
        public static T GetRandomWeightedEntry<T>(ICollection<T> entries, Func<T, float> weightFunc, SeedSource source) {
            return RandomExtensions.GetRandomWeightedEntry(entries, weightFunc, SingleRandom.Single(source));
        }

    }
}
