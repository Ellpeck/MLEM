using System;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <inheritdoc />
    public class DynamicEnumConverter : JsonConverter<DynamicEnum> {

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, DynamicEnum value, JsonSerializer serializer) {
            writer.WriteValue(value.ToString());
        }

        /// <inheritdoc />
        public override DynamicEnum ReadJson(JsonReader reader, Type objectType, DynamicEnum existingValue, bool hasExistingValue, JsonSerializer serializer) {
            return DynamicEnum.Parse(objectType, reader.Value.ToString());
        }

    }
}