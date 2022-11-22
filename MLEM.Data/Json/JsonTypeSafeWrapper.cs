using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// A json type-safe wrapper can be used to wrap any objects that have a custom <see cref="JsonConverter"/> which stores them as a primitive type and that are serialized using a <see cref="JsonSerializer"/> in cases where <see cref="JsonSerializer.TypeNameHandling"/> is not set to <see cref="TypeNameHandling.None"/>.
    /// If these objects are not wrapped in this manner, the value deserialized from it might not have the same type as the originally serialized object. This behavior can be observed, for example, when serializing a <see cref="List{T}"/> of <see cref="object"/> entries, one of which is a <see cref="TimeSpan"/>: The <see cref="TimeSpan"/> will be serialized as a <see cref="string"/> and, upon deserialization, will remain a <see cref="string"/>.
    /// In general, wrapping objects in this manner is only useful in rare cases, where custom data of an unexpected or unknown type is stored.
    /// See <see cref="JsonTypeSafeGenericDataHolder"/> for an example of how this class can be used, and see this stackoverflow answer for more information on the problem that this class solves: https://stackoverflow.com/a/38798114.
    /// </summary>
    public abstract class JsonTypeSafeWrapper {

        /// <summary>
        /// Returns this json type-safe wrapper's value as an <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

        /// <summary>
        /// Returns the current <see cref="Value"/> of this <see cref="JsonTypeSafeWrapper"/>, typecast to the given type <typeparamref name="T"/>.
        /// If this <see cref="Value"/>'s type is incompatible with the given type, the type's default value is returned instead.
        /// </summary>
        /// <typeparam name="T">The type of value to return</typeparam>
        /// <returns>The <see cref="Value"/>, castt to the given type if compatible, otherwise default</returns>
        public T GetValue<T>() {
            return this.Value is T t ? t : default;
        }

        /// <summary>
        /// Creates a new <see cref="JsonTypeSafeWrapper{T}"/> from the given value.
        /// The type parameter of the returned wrapper will be equal to the <see cref="Type"/> of the <paramref name="value"/> passed.
        /// If a <see cref="JsonTypeSafeWrapper{T}"/> for a specific type, known at comepile type, should be created, you can use <see cref="JsonTypeSafeWrapper{T}(T)"/>.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <returns>A <see cref="JsonTypeSafeWrapper{T}"/> with a type matching the type of <paramref name="value"/></returns>
        #if NET7_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("The native code for this instantiation might not be available at runtime.")]
        #endif
        public static JsonTypeSafeWrapper Of(object value) {
            var type = typeof(JsonTypeSafeWrapper<>).MakeGenericType(value.GetType());
            return (JsonTypeSafeWrapper) Activator.CreateInstance(type, value);
        }

    }

    /// <inheritdoc />
    public class JsonTypeSafeWrapper<T> : JsonTypeSafeWrapper {

        /// <inheritdoc />
        public override object Value => this.value;

        [JsonProperty]
        private readonly T value;

        /// <summary>
        /// Creates a new json type-safe wrapper instance that wraps the given <paramref name="value"/>.
        /// If the type of the value is unknown at compile time, <see cref="JsonTypeSafeWrapper.Of"/> can be used instead.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        public JsonTypeSafeWrapper(T value) {
            this.value = value;
        }

    }
}
