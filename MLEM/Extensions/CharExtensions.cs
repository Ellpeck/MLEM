using System;
using System.Collections.Generic;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="char"/>
    /// </summary>
    [Obsolete("ToCachedString is deprecated. Consider using a more robust, custom implementation for text caching.")]
    public static class CharExtensions {

        private static readonly Dictionary<char, string> Cache = new Dictionary<char, string>();

        /// <summary>
        /// Returns the string representation of this character which will be stored and retrieved from a dictionary cache.
        /// This method reduces string allocations, making it trade in processor efficiency for memory efficiency.
        /// </summary>
        /// <param name="c">The character to turn into a string</param>
        /// <returns>A string representing the character</returns>
        [Obsolete("ToCachedString is deprecated. Consider using a more robust, custom implementation for text caching.")]
        public static string ToCachedString(this char c) {
            if (!CharExtensions.Cache.TryGetValue(c, out var ret)) {
                ret = c.ToString();
                CharExtensions.Cache.Add(c, ret);
            }
            return ret;
        }

    }
}
