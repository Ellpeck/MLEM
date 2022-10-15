using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace MLEM.Font {
    /// <summary>
    /// A code point source is a wrapper around a <see cref="string"/> or <see cref="StringBuilder"/> that allows retrieving UTF-32 code points at a given index using <see cref="GetCodePoint"/>. Additionally, it allows enumerating every code point in the underlying <see cref="string"/> or <see cref="StringBuilder"/>.
    /// </summary>
    public readonly struct CodePointSource : IEnumerable<int> {

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
        /// <returns></returns>
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

    }
}
