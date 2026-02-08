using System;
using System.Collections.Generic;
using System.Linq;

namespace MLEM.Maths {
    /// <summary>
    /// A set of extensions for dealing with <see cref="Random"/>
    /// </summary>
    public static class RandomExtensions {

        private const string IndexOutOfRangeString = "Reached end of the collection in GetRandomWeightedEntry. For each entry, the passed weight function should return the same value on each invocation.";

        /// <summary>
        /// Gets a random entry from the given collection with uniform chance.
        /// </summary>
        /// <param name="random">The random</param>
        /// <param name="entries">The entries to choose from</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry</returns>
        public static T GetRandomEntry<T>(this Random random, ICollection<T> entries) {
            return RandomExtensions.GetRandomEntry(entries, random.NextSingle());
        }

        /// <summary>
        /// Returns a random entry from the given collection based on the specified weight function.
        /// A higher weight for an entry increases its likeliness of being picked.
        /// </summary>
        /// <param name="random">The random</param>
        /// <param name="entries">The entries to choose from</param>
        /// <param name="weightFunc">A function that applies weight to each entry</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry, based on the entries' weight</returns>
        /// <exception cref="IndexOutOfRangeException">If the weight function returns different weights for the same entry</exception>
        public static T GetRandomWeightedEntry<T>(this Random random, ICollection<T> entries, Func<T, int> weightFunc) {
            return RandomExtensions.GetRandomWeightedEntry(entries, weightFunc, random.NextSingle());
        }

        /// <inheritdoc cref="GetRandomWeightedEntry{T}(System.Random,System.Collections.Generic.ICollection{T},System.Func{T,int})"/>
        public static T GetRandomWeightedEntry<T>(this Random random, ICollection<T> entries, Func<T, float> weightFunc) {
            return RandomExtensions.GetRandomWeightedEntry(entries, weightFunc, random.NextSingle());
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0, and less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="random">The random.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <returns>A single-precision floating point number that is greater than or equal to 0, and less than <paramref name="maxValue"/>.</returns>
        public static float NextSingle(this Random random, float maxValue) {
            return maxValue * random.NextSingle();
        }

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to <paramref name="minValue"/>, and less than <paramref name="maxValue"/>.
        /// </summary>
        /// <param name="random">The random.</param>
        /// <param name="minValue">The (inclusive) minimum value.</param>
        /// <param name="maxValue">The (exclusive) maximum value.</param>
        /// <returns>A single-precision floating point number that is greater than or equal to <paramref name="minValue"/>, and less than <paramref name="maxValue"/>.</returns>
        public static float NextSingle(this Random random, float minValue, float maxValue) {
            return (maxValue - minValue) * random.NextSingle() + minValue;
        }

#if !NET6_0_OR_GREATER
        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0, and less than 1.
        /// </summary>
        /// <param name="random">The random.</param>
        /// <returns>A single-precision floating point number that is greater than or equal to 0, and less than 1.</returns>
        public static float NextSingle(this Random random) {
            return (float) random.NextDouble();
        }
#endif

        internal static T GetRandomEntry<T>(ICollection<T> entries, float randomValue) {
            // ElementAt internally optimizes for IList access so we don't have to here
            return entries.ElementAt((int) (randomValue * entries.Count));
        }

        internal static T GetRandomWeightedEntry<T>(ICollection<T> entries, Func<T, int> weightFunc, float randomValue) {
            var goalWeight = randomValue * entries.Sum(weightFunc);
            var currWeight = 0;
            foreach (var entry in entries) {
                currWeight += weightFunc(entry);
                if (currWeight >= goalWeight)
                    return entry;
            }
            throw new IndexOutOfRangeException(RandomExtensions.IndexOutOfRangeString);
        }

        internal static T GetRandomWeightedEntry<T>(ICollection<T> entries, Func<T, float> weightFunc, float randomValue) {
            var goalWeight = randomValue * entries.Sum(weightFunc);
            var currWeight = 0F;
            foreach (var entry in entries) {
                currWeight += weightFunc(entry);
                if (currWeight >= goalWeight)
                    return entry;
            }
            throw new IndexOutOfRangeException(RandomExtensions.IndexOutOfRangeString);
        }

    }
}
