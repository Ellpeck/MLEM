using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// A helper class that stores all of the <see cref="JsonConverter"/> types that are part of MLEM.Data.
    /// </summary>
    public class JsonConverters {

        /// <summary>
        /// An array of all of the <see cref="JsonConverter"/>s that are part of MLEM.Data
        /// </summary>
        public static readonly JsonConverter[] Converters = typeof(JsonConverters).Assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(typeof(JsonConverter)))
            .Select(t => t.GetConstructor(Type.EmptyTypes).Invoke(null)).Cast<JsonConverter>().ToArray();

        /// <summary>
        /// Adds all of the <see cref="JsonConverter"/> objects that are part of MLEM.Data to the given <see cref="JsonSerializer"/>
        /// </summary>
        /// <param name="serializer">The serializer to add the converters to</param>
        /// <returns>The given serializer, for chaining</returns>
        public static JsonSerializer AddAll(JsonSerializer serializer) {
            foreach (var converter in Converters)
                serializer.Converters.Add(converter);
            return serializer;
        }

    }
}