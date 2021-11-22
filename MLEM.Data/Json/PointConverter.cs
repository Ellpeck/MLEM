using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// Converts a <see cref="Point"/> to and from JSON
    /// </summary>
    public class PointConverter : JsonConverter<Point> {

        /// <summary>Writes the JSON representation of the object.</summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) {
            writer.WriteValue(value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
        /// <param name="hasExistingValue">The existing value has a value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var value = reader.Value.ToString().Split(' ');
            return new Point(int.Parse(value[0], CultureInfo.InvariantCulture), int.Parse(value[1], CultureInfo.InvariantCulture));
        }

    }
}