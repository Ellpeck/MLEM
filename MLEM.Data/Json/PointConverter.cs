using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    public class PointConverter : JsonConverter<Point> {

        public override void WriteJson(JsonWriter writer, Point value, JsonSerializer serializer) {
            writer.WriteValue(value.X.ToString(CultureInfo.InvariantCulture) + " " + value.Y.ToString(CultureInfo.InvariantCulture));
        }

        public override Point ReadJson(JsonReader reader, Type objectType, Point existingValue, bool hasExistingValue, JsonSerializer serializer) {
            var value = reader.Value.ToString().Split(' ');
            return new Point(int.Parse(value[0], CultureInfo.InvariantCulture), int.Parse(value[1], CultureInfo.InvariantCulture));
        }

    }
}