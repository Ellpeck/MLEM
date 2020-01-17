using System;
using System.Collections.Generic;
using System.Linq;

namespace MLEM.Extensions {
    public static class RandomExtensions {

        public static T GetRandomEntry<T>(this Random random, params T[] entries) {
            return entries[random.Next(entries.Length)];
        }

        public static T GetRandomEntry<T>(this Random random, IList<T> entries) {
            return entries[random.Next(entries.Count)];
        }

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

    }
}