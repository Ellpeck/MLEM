using System;
using System.Collections.Generic;
using System.Linq;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="Random"/>
    /// </summary>
    public static class RandomExtensions {

        /// <summary>
        /// Gets a random entry from the given list with uniform chance.
        /// </summary>
        /// <param name="random">The random</param>
        /// <param name="entries">The entries to choose from</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry</returns>
        public static T GetRandomEntry<T>(this Random random, IList<T> entries) {
            return entries[random.Next(entries.Count)];
        }

        /// <summary>
        /// Returns a random entry from the given list based on the specified weight function.
        /// A higher weight for an entry increases its likeliness of being picked.
        /// </summary>
        /// <param name="random">The random</param>
        /// <param name="entries">The entries to choose from</param>
        /// <param name="weightFunc">A function that applies weight to each entry</param>
        /// <typeparam name="T">The entries' type</typeparam>
        /// <returns>A random entry, based on the entries' weight</returns>
        /// <exception cref="IndexOutOfRangeException">If the weight function returns different weights for the same entry</exception>
        public static T GetRandomWeightedEntry<T>(this Random random, IList<T> entries, Func<T, int> weightFunc) {
            var totalWeight = entries.Sum(weightFunc);
            var goalWeight = random.Next(totalWeight);
            var currWeight = 0;
            foreach (var entry in entries) {
                currWeight += weightFunc(entry);
                if (currWeight >= goalWeight)
                    return entry;
            }
            throw new IndexOutOfRangeException();
        }

        /// <inheritdoc cref="GetRandomWeightedEntry{T}(System.Random,System.Collections.Generic.IList{T},System.Func{T,int})"/>
        public static T GetRandomWeightedEntry<T>(this Random random, IList<T> entries, Func<T, float> weightFunc) {
            var totalWeight = entries.Sum(weightFunc);
            var goalWeight = random.NextDouble() * totalWeight;
            var currWeight = 0F;
            foreach (var entry in entries) {
                currWeight += weightFunc(entry);
                if (currWeight >= goalWeight)
                    return entry;
            }
            throw new IndexOutOfRangeException();
        }

    }
}