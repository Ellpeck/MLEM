using System;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// Converts a <see cref="DynamicEnum"/> to and from JSON
    /// </summary>
    [Obsolete("DynamicEnum has been moved into the DynamicEnums library: https://www.nuget.org/packages/DynamicEnums"), JsonConverter(typeof(DynamicEnumConverter))]
    public class DynamicEnumConverter : JsonConverter<DynamicEnum> {

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, DynamicEnum value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override DynamicEnum ReadJson(JsonReader reader, Type objectType, DynamicEnum existingValue, bool hasExistingValue, JsonSerializer serializer) {
            return DynamicEnum.Parse(objectType, reader.Value.ToString());
        }

    }
}
