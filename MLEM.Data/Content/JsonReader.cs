using System;
using System.IO;
using Newtonsoft.Json;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class JsonReader : RawContentReader {

        /// <inheritdoc />
        public override bool CanRead(Type t) {
            return true;
        }

        /// <inheritdoc />
        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            using (var reader = new JsonTextReader(new StreamReader(stream)))
                return manager.GetJsonSerializer().Deserialize(reader);
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"json", "json5", "jsonc"};
        }

    }
}