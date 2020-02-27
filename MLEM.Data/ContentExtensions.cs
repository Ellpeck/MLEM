using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using MLEM.Data.Json;
using Newtonsoft.Json;

namespace MLEM.Data {
    public static class ContentExtensions {

        private static readonly Dictionary<ContentManager, JsonSerializer> Serializers = new Dictionary<ContentManager, JsonSerializer>();

        public static void SetJsonSerializer(this ContentManager content, JsonSerializer serializer) {
            Serializers[content] = serializer;
        }

        public static JsonSerializer GetJsonSerializer(this ContentManager content) {
            if (!Serializers.TryGetValue(content, out var serializer)) {
                serializer = JsonConverters.AddAll(new JsonSerializer());
                content.SetJsonSerializer(serializer);
            }
            return serializer;
        }

        public static void AddJsonConverter<T>(this ContentManager content, JsonConverter<T> converter) {
            var serializer = GetJsonSerializer(content);
            serializer.Converters.Add(converter);
        }

        public static T LoadJson<T>(this ContentManager content, string name, string extension = ".json") {
            using (var stream = File.OpenText(Path.Combine(content.RootDirectory, name + extension))) {
                using (var reader = new JsonTextReader(stream)) {
                    return GetJsonSerializer(content).Deserialize<T>(reader);
                }
            }
        }

    }
}