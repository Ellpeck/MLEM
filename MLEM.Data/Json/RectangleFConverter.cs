using System;
using System.Globalization;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// Converts a <see cref="RectangleF"/> to and from JSON
    /// </summary>
    public class RectangleFConverter : JsonConverter<RectangleF> {

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, RectangleF value, JsonSerializer serializer) {
            writer.WriteValue(
                value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture) + " " +
                value.Width.ToString(CultureInfo.InvariantCulture) + " " + value.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override RectangleF ReadJson(JsonReader reader, Type objectType, RectangleF existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var value = reader.Value.ToString().Split(' ');
            return new RectangleF(
                float.Parse(value[0], CultureInfo.InvariantCulture), float.Parse(value[1], CultureInfo.InvariantCulture),
                float.Parse(value[2], CultureInfo.InvariantCulture), float.Parse(value[3], CultureInfo.InvariantCulture));
        }

    }
}
