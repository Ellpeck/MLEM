using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// A json type-safe wrapper can be used to wrap any objects of any type before submitting them to a non-specifically typed <see cref="List{T}"/> or <see cref="Dictionary{T,T}"/> that will be serialized using a <see cref="JsonSerializer"/> in cases where <see cref="JsonSerializer.TypeNameHandling"/> is not set to <see cref="TypeNameHandling.None"/>.
    /// If any object is not wrapped in this manner, and it has a custom <see cref="JsonConverter"/>, the value deserialized from it might not have the same type as the originally serialized object. This behavior can be observed, for example, when serializing a <see cref="List{T}"/> of <see cref="object"/> entries, one of which is a <see cref="TimeSpan"/>: The <see cref="TimeSpan"/> will be serialized as a <see cref="string"/> and, upon deserialization, will remain a <see cref="string"/>.
    /// In general, wrapping objects in this manner is only useful in rare cases, where custom data of an unexpected or unknown type is stored.
    /// See <see cref="JsonTypeSafeGenericDataHolder"/> for an example of how this class and <see cref="JsonTypeSafeWrapper{T}"/> can be used, and see this stackoverflow answer for more information on the problem that this class solves: https://stackoverflow.com/a/38798114.
    /// </summary>
    public abstract class JsonTypeSafeWrapper {

        /// <summary>
        /// Returns this json type-safe wrapper's value as an <see cref="object"/>.
        /// </summary>
        public abstract object Value { get; }

    }

    /// <inheritdoc />
    [DataContract]
    public class JsonTypeSafeWrapper<T> : JsonTypeSafeWrapper {

        /// <inheritdoc />
        public override object Value => this.value;
        [DataMember]
        private readonly T value;

        /// <summary>
        /// Creates a new json type-safe wrapper instance that wraps the given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        public JsonTypeSafeWrapper(T value) {
            this.value = value;
        }

    }
}