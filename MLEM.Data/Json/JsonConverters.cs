using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    public class JsonConverters {

        public static readonly JsonConverter[] Converters = Assembly.GetExecutingAssembly().GetExportedTypes()
            .Where(t => t.Namespace == typeof(JsonConverters).Namespace && t.IsSubclassOf(typeof(JsonConverter)))
            .Select(t => t.GetConstructor(Type.EmptyTypes).Invoke(null)).Cast<JsonConverter>().ToArray();

        public static JsonSerializer AddAll(JsonSerializer serializer) {
            foreach (var converter in Converters)
                serializer.Converters.Add(converter);
            return serializer;
        }

    }
}