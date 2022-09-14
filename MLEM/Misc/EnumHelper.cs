using System;
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

    }
}
