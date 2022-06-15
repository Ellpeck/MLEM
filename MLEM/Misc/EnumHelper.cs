using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Misc {
    /// <summary>
    /// A helper class that allows easier usage of <see cref="Enum"/> values.
    /// </summary>
    public static class EnumHelper {

        /// <summary>
        /// All values of the <see cref="Buttons"/> enum.
        /// </summary>
        public static readonly Buttons[] Buttons = EnumHelper.GetValues<Buttons>().ToArray();
        /// <summary>
        /// All values of the <see cref="Keys"/> enum.
        /// </summary>
        public static readonly Keys[] Keys = EnumHelper.GetValues<Keys>().ToArray();

        /// <summary>
        /// Returns all of the values of the given enum type.
        /// </summary>
        /// <typeparam name="T">The type whose enum to get</typeparam>
        /// <returns>An enumerable of the values of the enum, in declaration order.</returns>
        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

    }
}