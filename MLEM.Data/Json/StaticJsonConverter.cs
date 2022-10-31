using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace MLEM.Data.Json {
    /// <summary>
    /// A <see cref="JsonConverter{T}"/> that doesn't actually serialize the object, but instead serializes the name given to it by the underlying <see cref="Dictionary{T,T}"/>.
    /// Optionally, the name of a <see cref="Dictionary{TKey,TValue}"/> can be passed to this converter when used in the <see cref="JsonConverterAttribute"/> by passing the arguments for the <see cref="StaticJsonConverter{T}(Type,string)"/> constructor as <see cref="JsonConverterAttribute.ConverterParameters"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to convert</typeparam>
    public class StaticJsonConverter<T> : JsonConverter<T> {

        private readonly Dictionary<string, T> entries;
        private readonly Dictionary<T, string> inverse;

        /// <summary>
        /// Creates a new static json converter using the given underlying <see cref="Dictionary{T,T}"/>.
        /// </summary>
        /// <param name="entries">The dictionary to use</param>
        public StaticJsonConverter(Dictionary<string, T> entries) {
            this.entries = entries;
            this.inverse = entries.ToDictionary(kv => kv.Value, kv => kv.Key);
        }

        /// <summary>
        /// Creates a new static json converter by finding the underlying <see cref="Dictionary{TKey,TValue}"/> from the given type and member name
        /// </summary>
        /// <param name="type">The type that the dictionary is declared in</param>
        /// <param name="memberName">The name of the dictionary itself</param>
        public StaticJsonConverter(
            #if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
            #endif
            Type type, string memberName) : this(StaticJsonConverter<T>.GetEntries(type, memberName)) {}

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
            if (!this.inverse.TryGetValue(value, out var key))
                throw new InvalidOperationException($"Cannot write {value} that is not a registered entry");
            writer.WriteValue(key);
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var val = reader.Value?.ToString();
            if (val == null)
                return default;
            this.entries.TryGetValue(val, out var ret);
            return ret;
        }

        private static Dictionary<string, T> GetEntries(
            #if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties | DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
            #endif
            Type type, string memberName) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var value = type.GetProperty(memberName, flags)?.GetValue(null) ?? type.GetField(memberName, flags)?.GetValue(null);
            if (value == null)
                throw new ArgumentException($"There is no property or field value for name {memberName}", nameof(memberName));
            return value as Dictionary<string, T> ?? throw new InvalidCastException($"{value} is not of expected type {typeof(T)}");
        }

    }
}
