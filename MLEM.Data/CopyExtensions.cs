using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace MLEM.Data {
    /// <summary>
    /// A set of extensions for dealing with copying objects.
    /// </summary>
    [Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
#if NET6_0_OR_GREATER
    [UnconditionalSuppressMessage("Aot", "IL3050"), UnconditionalSuppressMessage("Aot", "IL2070"), UnconditionalSuppressMessage("Aot", "IL2090")]
#endif
    public static class CopyExtensions {

        private const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Dictionary<Type, ConstructorInfo> ConstructorCache = new Dictionary<Type, ConstructorInfo>();

        /// <summary>
        /// Creates a shallow copy of the object and returns it.
        /// Object creation occurs using a constructor with the <see cref="CopyConstructorAttribute"/> or, if none is present, the first constructor with the correct <see cref="BindingFlags"/>.
        /// </summary>
        /// <param name="obj">The object to create a shallow copy of</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        /// <returns>A shallow copy of the object</returns>
        [Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
        public static T Copy<T>(this T obj, BindingFlags flags = CopyExtensions.DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            var copy = (T) CopyExtensions.Construct(typeof(T), flags);
            obj.CopyInto(copy, flags, fieldInclusion);
            return copy;
        }

        /// <summary>
        /// Creates a deep copy of the object and returns it.
        /// Object creation occurs using a constructor with the <see cref="CopyConstructorAttribute"/> or, if none is present, the first constructor with the correct <see cref="BindingFlags"/>.
        /// </summary>
        /// <param name="obj">The object to create a deep copy of</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        /// <returns>A deep copy of the object</returns>
        [Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
        public static T DeepCopy<T>(this T obj, BindingFlags flags = CopyExtensions.DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            var copy = (T) CopyExtensions.Construct(typeof(T), flags);
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
        [Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
        public static void CopyInto<T>(this T obj, T otherObj, BindingFlags flags = CopyExtensions.DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
            foreach (var field in typeof(T).GetFields(flags)) {
                if (fieldInclusion == null || fieldInclusion(field))
                    field.SetValue(otherObj, field.GetValue(obj));
            }
        }

        /// <summary>
        /// Copies the given object <paramref name="obj"/> into the given object <paramref name="otherObj"/> in a deep manner.
        /// Object creation occurs using a constructor with the <see cref="CopyConstructorAttribute"/> or, if none is present, the first constructor with the correct <see cref="BindingFlags"/>.
        /// </summary>
        /// <param name="obj">The object to create a deep copy of</param>
        /// <param name="otherObj">The object to copy into</param>
        /// <param name="flags">The binding flags for field searching</param>
        /// <param name="fieldInclusion">A predicate that determines whether or not the given field should be copied. If null, all fields will be copied.</param>
        /// <typeparam name="T">The type of the object to copy</typeparam>
        [Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
        public static void DeepCopyInto<T>(this T obj, T otherObj, BindingFlags flags = CopyExtensions.DefaultFlags, Predicate<FieldInfo> fieldInclusion = null) {
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
                        otherVal = CopyExtensions.Construct(field.FieldType, flags);
                        field.SetValue(otherObj, otherVal);
                    }
                    val.DeepCopyInto(otherVal, flags);
                }
            }
        }

        private static object Construct(Type t, BindingFlags flags) {
            if (!CopyExtensions.ConstructorCache.TryGetValue(t, out var constructor)) {
                var constructors = t.GetConstructors(flags);
                // find a contructor with the correct attribute
                constructor = constructors.FirstOrDefault(c => c.GetCustomAttribute<CopyConstructorAttribute>() != null);
                // find a parameterless construcotr
                if (constructor == null)
                    constructor = t.GetConstructor(flags, null, Type.EmptyTypes, null);
                // fall back to the first constructor
                if (constructor == null)
                    constructor = constructors.FirstOrDefault();
                if (constructor == null)
                    throw new NullReferenceException($"Type {t} does not have a constructor with the required visibility");
                CopyExtensions.ConstructorCache.Add(t, constructor);
            }
            return constructor.Invoke(new object[constructor.GetParameters().Length]);
        }

    }

    /// <summary>
    /// An attribute that, when added to a constructor, will make that constructor the one used by <see cref="CopyExtensions.Copy{T}"/>, <see cref="CopyExtensions.DeepCopy{T}"/> and <see cref="CopyExtensions.DeepCopyInto{T}"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor), Obsolete("CopyExtensions has major flaws and insufficient speed compared to other libraries specifically designed for copying objects.")]
    public class CopyConstructorAttribute : Attribute {}
}
