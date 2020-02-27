using System;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    public class Direction2Converter : JsonConverter<Direction2> {

        public override void WriteJson(JsonWriter writer, Direction2 value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        public override Direction2 ReadJson(JsonReader reader, Type objectType, Direction2 existingValue, bool hasExistingValue, JsonSerializer serializer) {
            Enum.TryParse<Direction2>(reader.Value.ToString(), out var dir);
            return dir;
        }

    }
}