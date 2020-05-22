using System;
using System.Globalization;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <inheritdoc />
    public class RectangleFConverter : JsonConverter<RectangleF> {

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, RectangleF value, JsonSerializer serializer) {
            writer.WriteValue(
                value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture) + " " +
                value.Width.ToString(CultureInfo.InvariantCulture) + " " + value.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public override RectangleF ReadJson(JsonReader reader, Type objectType, RectangleF existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var value = reader.Value.ToString().Split(' ');
            return new RectangleF(
                float.Parse(value[0], CultureInfo.InvariantCulture), float.Parse(value[1], CultureInfo.InvariantCulture),
                float.Parse(value[2], CultureInfo.InvariantCulture), float.Parse(value[3], CultureInfo.InvariantCulture));
        }

    }
}