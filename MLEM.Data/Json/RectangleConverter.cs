using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <inheritdoc />
    public class RectangleConverter : JsonConverter<Rectangle> {

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, Rectangle value, JsonSerializer serializer) {
            writer.WriteValue(
                value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture) + " " +
                value.Width.ToString(CultureInfo.InvariantCulture) + " " + value.Height.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc />
        public override Rectangle ReadJson(JsonReader reader, Type objectType, Rectangle existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var value = reader.Value.ToString().Split(' ');
            return new Rectangle(
                int.Parse(value[0], CultureInfo.InvariantCulture), int.Parse(value[1], CultureInfo.InvariantCulture),
                int.Parse(value[2], CultureInfo.InvariantCulture), int.Parse(value[3], CultureInfo.InvariantCulture));
        }

    }
}