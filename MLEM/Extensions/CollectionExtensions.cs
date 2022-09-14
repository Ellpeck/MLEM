using System.Collections.Generic;
using System.Linq;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with collections of various kinds
    /// </summary>
    public static class CollectionExtensions {

        /// <summary>
        /// This method returns a set of possible combinations of n items from n different sets, where the order of the items in each combination is based on the order of the input sets.
        /// For a version of this method that returns indices rather than entries, see <see cref="IndexCombinations{T}"/>.
        /// <example>
        /// Given the input set <c>{{1, 2, 3}, {A, B}, {+, -}}</c>, the returned set would contain the following sets:
        /// <code>
        /// {1, A, +}, {1, A, -}, {1, B, +}, {1, B, -},
        /// {2, A, +}, {2, A, -}, {2, B, +}, {2, B, -},
        /// {3, A, +}, {3, A, -}, {3, B, +}, {3, B, -}
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="things">The different sets to be combined</param>
        /// <typeparam name="T">The type of the items in the sets</typeparam>
        /// <returns>All combinations of set items as described</returns>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<IEnumerable<T>> things) {
            var combos = Enumerable.Repeat(Enumerable.Empty<T>(), 1);
            foreach (var t in things)
                combos = combos.SelectMany(c => t.Select(o => c.Concat(Enumerable.Repeat(o, 1))));
            return combos;
        }

        /// <summary>
        /// This method returns a set of possible combinations of n indices of items from n different sets, where the order of the items' indices in each combination is based on the order of the input sets.
        /// For a version of this method that returns entries rather than indices, see <see cref="Combinations{T}"/>.
        /// <example>
        /// Given the input set <c>{{1, 2, 3}, {A, B}, {+, -}}</c>, the returned set would contain the following sets:
        /// <code>
        /// {0, 0, 0}, {0, 0, 1}, {0, 1, 0}, {0, 1, 1},
        /// {1, 0, 0}, {1, 0, 1}, {1, 1, 0}, {1, 1, 1},
        /// {2, 0, 0}, {2, 0, 1}, {2, 1, 0}, {2, 1, 1}
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="things">The different sets to be combined</param>
        /// <typeparam name="T">The type of the items in the sets</typeparam>
        /// <returns>All combinations of set items as described</returns>
        public static IEnumerable<IEnumerable<int>> IndexCombinations<T>(this IEnumerable<IEnumerable<T>> things) {
            var combos = Enumerable.Repeat(Enumerable.Empty<int>(), 1);
            foreach (var t in things)
                combos = combos.SelectMany(c => t.Select((o, i) => c.Concat(Enumerable.Repeat(i, 1))));
            return combos;
        }

    }
}
