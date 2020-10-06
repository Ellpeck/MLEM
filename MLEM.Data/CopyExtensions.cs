using System;
using System.Reflection;

namespace MLEM.Data {
    /// <summary>
    /// A set of extensions for dealing with copying objects.
    /// </summary>
    public static class CopyExtensions {

        private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        /// <summary>
        /// Creates a shallow copy of the object and returns it.
        /// Note that, for this to work correctly, <typeparamref name="T"/> needs to contain a parameterless constructor.
        /// </summary>
        /// <param name="obj">The object to create a shallow copy of</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        /// <returns>A shallow copy of the object</returns>
        public static T Copy<T>(this T obj, BindingFlags flags = DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            var copy = (T) Construct(typeof(T), flags);
            obj.CopyInto(copy, flags, fieldInclusion);
            return copy;
        }

        /// <summary>
        /// Creates a deep copy of the object and returns it.
        /// Note that, for this to work correctly, <typeparamref name="T"/> needs to contain a parameterless constructor.
        /// </summary>
        /// <param name="obj">The object to create a deep copy of</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        /// <returns>A deep copy of the object</returns>
        public static T DeepCopy<T>(this T obj, BindingFlags flags = DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            var copy = (T) Construct(typeof(T), flags);
            obj.DeepCopyInto(copy, flags, fieldInclusion);
            return copy;
        }

        /// <summary>
        /// Copies the given object <paramref name="obj"/> into the given object <paramref name="otherObj"/> in a shallow manner.
        /// </summary>
        /// <param name="obj">The object to create a shallow copy of</param>
        /// <param name="otherObj">The object to copy into</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        public static void CopyInto<T>(this T obj, T otherObj, BindingFlags flags = DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            foreach (var field in typeof(T).GetFields(flags)) {
                if (fieldInclusion == null || fieldInclusion(field))
                    field.SetValue(otherObj, field.GetValue(obj));
            }
        }

        /// <summary>
        /// Copies the given object <paramref name="obj"/> into the given object <paramref name="otherObj"/> in a deep manner.
        /// Note that, for this to work correctly, each type that should be constructed below the topmost level needs to contanin a parameterless constructor.
        /// </summary>
        /// <param name="obj">The object to create a deep copy of</param>
        /// <param name="otherObj">The object to copy into</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        public static void DeepCopyInto<T>(this T obj, T otherObj, BindingFlags flags = DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            foreach (var field in obj.GetType().GetFields(flags)) {
                if (fieldInclusion != null && !fieldInclusion(field))
                    continue;
                var val = field.GetValue(obj);
                if (val == null || field.FieldType.IsValueType) {
                    // if we're a value type (struct or primitive) or null, we can just set the value
                    field.SetValue(otherObj, val);
                } else {
                    var otherVal = field.GetValue(otherObj);
                    // if the object we want to copy into doesn't have a value yet, we create one
                    if (otherVal == null) {
                        otherVal = Construct(field.FieldType, flags);
                        field.SetValue(otherObj, otherVal);
                    }
                    val.DeepCopyInto(otherVal, flags);
                }
            }
        }

        private static object Construct(Type t, BindingFlags flags) {
            var constructor = t.GetConstructor(flags, null, Type.EmptyTypes, null);
            if (constructor == null)
                throw new NullReferenceException($"Type {t} does not have a parameterless constructor with the required visibility");
            return constructor.Invoke(null);
        }

    }
}