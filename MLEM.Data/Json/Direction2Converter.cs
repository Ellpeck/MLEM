using System;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// Converts a <see cref="Direction2"/> to and from JSON
    /// </summary>
    public class Direction2Converter : JsonConverter<Direction2> {

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, Direction2 value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override Direction2 ReadJson(JsonReader reader, Type objectType, Direction2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Enum.TryParse<Direction2>(reader.Value.ToString(), out var dir);
            return dir;
        }

    }
}