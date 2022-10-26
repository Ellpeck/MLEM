using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Misc {
    /// <summary>
    /// A helper class that allows easier usage of <see cref="Enum"/> values.
    /// </summary>
    public static class EnumHelper {

        /// <summary>
        /// All values of the <see cref="Buttons"/> enum.
        /// </summary>
        public static readonly Buttons[] Buttons = EnumHelper.GetValues<Buttons>();
        /// <summary>
        /// All values of the <see cref="Keys"/> enum.
        /// </summary>
        public static readonly Keys[] Keys = EnumHelper.GetValues<Keys>();

        /// <summary>
        /// Returns an array containing all of the values of the given enum type.
        /// Note that this method is a version-independent equivalent of .NET 5's <c>Enum.GetValues&lt;TEnum&gt;</c>.
        /// </summary>
        /// <typeparam name="T">The type whose enum to get</typeparam>
        /// <returns>An enumerable of the values of the enum, in declaration order.</returns>
        public static T[] GetValues<T>() where T : struct, Enum {
            #if NET6_0_OR_GREATER
            return Enum.GetValues<T>();
            #else
            return (T[]) Enum.GetValues(typeof(T));
            #endif
        }

        /// <summary>
        /// Returns all of the defined values from the given enum type <typeparamref name="T"/> which are contained in <paramref name="combinedFlag"/>.
        /// Note that, if combined flags are defined in <typeparamref name="T"/>, and <paramref name="combinedFlag"/> contains them, they will also be returned.
        /// </summary>
        /// <param name="combinedFlag">The combined flags whose individual flags to return.</param>
        /// <param name="includeZero">Whether the enum value 0 should also be returned, if <typeparamref name="T"/> contains one.</param>
        /// <typeparam name="T">The type of enum.</typeparam>
        /// <returns>All of the flags that make up <paramref name="combinedFlag"/>.</returns>
        public static IEnumerable<T> GetFlags<T>(T combinedFlag, bool includeZero = true) where T : struct, Enum {
            foreach (var flag in EnumHelper.GetValues<T>()) {
                if (combinedFlag.HasFlag(flag) && (includeZero || Convert.ToInt64(flag) != 0))
                    yield return flag;
            }
        }

    }
}
