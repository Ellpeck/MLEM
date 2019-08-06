using System;
using System.Collections.Generic;

namespace MLEM.Extensions {
    public static class RandomExtensions {

        public static T GetRandomEntry<T>(this Random random, params T[] entries) {
            return entries[random.Next(entries.Length)];
        }

        public static T GetRandomEntry<T>(this Random random, IList<T> entries) {
            return entries[random.Next(entries.Count)];
        }

    }
}