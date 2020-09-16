using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using MLEM.Data.Json;
using Newtonsoft.Json;

namespace MLEM.Data {
    /// <summary>
    /// A set of extensions for dealing with <see cref="ContentManager"/>
    /// </summary>
    public static class ContentExtensions {

        private static readonly Dictionary<ContentManager, JsonSerializer> Serializers = new Dictionary<ContentManager, JsonSerializer>();
        private static readonly string[] JsonExtensions = {".json", ".json5", ".jsonc"};

        /// <summary>
        /// Adds a <see cref="JsonSerializer"/> to the given content manager, which allows <see cref="LoadJson{T}"/> to load JSON-based content.
        /// Note that <see cref="GetJsonSerializer"/> calls this method implicitly if no serializer exists.
        /// </summary>
        /// <param name="content">The content manager to add the json serializer to</param>
        /// <param name="serializer">The json serializer to add</param>
        public static void SetJsonSerializer(this ContentManager content, JsonSerializer serializer) {
            Serializers[content] = serializer;
        }

        /// <summary>
        /// Returns the given content manager's json serializer.
        /// This method sets a new json serializer using <see cref="SetJsonSerializer"/> if the given content manager does not yet have one.
        /// </summary>
        /// <param name="content">The content manager whose serializer to get</param>
        /// <returns>The content manager's serializer</returns>
        public static JsonSerializer GetJsonSerializer(this ContentManager content) {
            if (!Serializers.TryGetValue(content, out var serializer)) {
                serializer = JsonConverters.AddAll(new JsonSerializer());
                content.SetJsonSerializer(serializer);
            }
            return serializer;
        }

        /// <summary>
        /// Adds a <see cref="JsonConverter"/> to the given content manager's <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="content">The content manager to add the converter to</param>
        /// <param name="converter">The converter to add</param>
        public static void AddJsonConverter(this ContentManager content, JsonConverter converter) {
            var serializer = GetJsonSerializer(content);
            serializer.Converters.Add(converter);
        }

        /// <summary>
        /// Loads any kind of JSON data using the given content manager's <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="content">The content manager to load content with</param>
        /// <param name="name">The name of the file to load</param>
        /// <param name="extensions">The file extensions that should be appended, or ".json", ".json5" and ".jsonc" by default.</param>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <returns>The loaded asset</returns>
        public static T LoadJson<T>(this ContentManager content, string name, string[] extensions = null) {
            foreach (var extension in extensions ?? JsonExtensions) {
                var file = Path.Combine(content.RootDirectory, name + extension);
                if (!File.Exists(file))
                    continue;
                using (var stream = File.OpenText(file)) {
                    using (var reader = new JsonTextReader(stream)) {
                        return GetJsonSerializer(content).Deserialize<T>(reader);
                    }
                }
            }
            return default;
        }

    }
}