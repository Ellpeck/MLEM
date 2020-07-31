using System;
using System.Reflection;

namespace MLEM.Data {
    /// <summary>
    /// A set of extensions for dealing with copying objects.
    /// </summary>
    public static class CopyExtensions {

        /// <summary>
        /// Creates a shallow copy of the object and returns it.
        /// Note that, for this to work correctly, <typeparamref name="T"/> needs to contain a parameterless constructor.
        /// </summary>
        /// <param name="obj">The object to create a shallow copy of</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        /// <returns>A shallow copy of the object</returns>
        public static T Copy<T>(this T obj, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) {
            var copy = (T) typeof(T).GetConstructor(Type.EmptyTypes).Invoke(null);
            obj.CopyInto(copy, flags);
            return copy;
        }

        /// <summary>
        /// Copies the given object <paramref name="obj"/> into the given object <see cref="otherObj"/>.
        /// </summary>
        /// <param name="obj">The object to create a shallow copy of</param>
        /// <param name="otherObj">The object to copy into</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        public static void CopyInto<T>(this T obj, T otherObj, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) {
            foreach (var field in typeof(T).GetFields(flags))
                field.SetValue(otherObj, field.GetValue(obj));
        }

    }
}