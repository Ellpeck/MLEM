using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MLEM.Font {
    /// <summary>
    /// A code point source is a wrapper around a <see cref="string"/> or <see cref="StringBuilder"/> that allows retrieving UTF-32 code points at a given index using <see cref="GetCodePoint"/>. Additionally, it allows enumerating every code point in the underlying <see cref="string"/> or <see cref="StringBuilder"/>. This class also contains <see cref="ToString(int)"/>, which converts a code point into its <see cref="string"/> representation, but caches the result to avoid allocating excess memory.
    /// </summary>
    public readonly struct CodePointSource : IEnumerable<int> {

        private static readonly Dictionary<int, string> StringCache = new Dictionary<int, string>();

        private readonly string strg;
        private readonly StringBuilder builder;
        private char this[int index] => this.strg?[index] ?? this.builder[index];

        /// <summary>
        /// The length of this code point, in characters.
        /// Note that this is not representative of the amount of code points in this source.
        /// </summary>
        public int Length => this.strg?.Length ?? this.builder.Length;

        /// <summary>
        /// Creates a new code point source from the given <see cref="string"/>.
        /// </summary>
        /// <param name="strg">The <see cref="string"/> whose code points to inspect.</param>
        public CodePointSource(string strg) {
            this.strg = strg;
            this.builder = null;
        }

        /// <summary>
        /// Creates a new code point source from the given <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> whose code points to inspect.</param>
        public CodePointSource(StringBuilder builder) {
            this.strg = null;
            this.builder = builder;
        }

        /// <summary>
        /// Returns the code point at the given <paramref name="index"/> in this code point source's underlying string, where the index is measured in characters and not code points.
        /// The resulting code point will either be a single <see cref="char"/> cast to an <see cref="int"/>, at which point the returned length will be 1, or a UTF-32 <see cref="int"/> character made up of two <see cref="char"/> values, at which point the returned length will be 2.
        /// </summary>
        /// <param name="index">The index at which to return the code point, which is measured in characters.</param>
        /// <param name="indexLowSurrogate">Whether the <paramref name="index"/> represents a low surrogate. If this is <see langword="false"/>, the <paramref name="index"/> represents a high surrogate and the low surrogate will be looked for in the following character. If this is <see langword="true"/>, the <paramref name="index"/> represents a low surrogate and the high surrogate will be looked for in the previous character.</param>
        /// <returns>The code point at the given location, as well as its length.</returns>
        public (int CodePoint, int Length) GetCodePoint(int index, bool indexLowSurrogate = false) {
            var curr = this[index];
            if (indexLowSurrogate) {
                if (index > 0) {
                    var high = this[index - 1];
                    if (char.IsSurrogatePair(high, curr))
                        return (char.ConvertToUtf32(high, curr), 2);
                }
            } else {
                if (index < this.Length - 1) {
                    var low = this[index + 1];
                    if (char.IsSurrogatePair(curr, low))
                        return (char.ConvertToUtf32(curr, low), 2);
                }
            }
            return (curr, 1);
        }

        /// <summary>
        /// Returns an index in this code point source that is as close to <paramref name="index"/> as possible, but not between two members of a surrogate pair. If the <paramref name="index"/> is already not between surrogate pairs, it is returned unchanged.
        /// </summary>
        /// <param name="index">The index to ensure is not between surrogates.</param>
        /// <param name="increase">Whether the returned index should be increased by 1 (instead of decreased by 1) when it is between surrogates.</param>
        /// <returns>An index close to <paramref name="index"/>, but not between surrogates.</returns>
        public int EnsureSurrogateBoundary(int index, bool increase) {
            if (index < this.Length && char.IsLowSurrogate(this[index]))
                return increase || index <= 0 ? index + 1 : index - 1;
            return index;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.</returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<int> GetEnumerator() {
            var index = 0;
            while (index < this.Length) {
                var (codePoint, length) = this.GetCodePoint(index);
                yield return codePoint;
                index += length;
            }
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Converts the given UTF-32 <paramref name="codePoint"/> into a string using <see cref="char.ConvertFromUtf32"/>, but caches the result in a <see cref="Dictionary{TKey,TValue}"/> cache to avoid allocating excess memory.
        /// </summary>
        /// <param name="codePoint">The UTF-32 code point to convert.</param>
        /// <returns>The string representation of the code point.</returns>
        public static string ToString(int codePoint) {
            if (!CodePointSource.StringCache.TryGetValue(codePoint, out var ret)) {
                ret = char.ConvertFromUtf32(codePoint);
                CodePointSource.StringCache.Add(codePoint, ret);
            }
            return ret;
        }

    }
}
