using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

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
        public StaticJsonConverter(Type type, string memberName) :
            this(GetEntries(type, memberName)) {
        }

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) {
            if (!this.inverse.TryGetValue(value, out var key))
                throw new InvalidOperationException($"Cannot write {value} that is not a registered entry");
            writer.WriteValue(key);
        }

        /// <inheritdoc />
        public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var val = reader.Value?.ToString();
            if (val == null)
                return default;
            this.entries.TryGetValue(val, out var ret);
            return ret;
        }

        private static Dictionary<string, T> GetEntries(Type type, string memberName) {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var value = type.GetProperty(memberName, flags)?.GetValue(null) ?? type.GetField(memberName, flags)?.GetValue(null);
            if (value == null)
                throw new ArgumentException($"There is no property or field value for name {memberName}", nameof(memberName));
            return value as Dictionary<string, T> ?? throw new InvalidCastException($"{value} is not of expected type {typeof(T)}");
        }

    }
}