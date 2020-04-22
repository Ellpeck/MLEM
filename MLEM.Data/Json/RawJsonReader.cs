using System;
using System.IO;
using MLEM.Content;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    public class RawJsonReader : RawContentReader {

        public override bool CanRead(Type t) {
            return true;
        }

        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            using (var reader = new JsonTextReader(new StreamReader(stream)))
                return manager.GetJsonSerializer().Deserialize(reader);
        }

        public override string[] GetFileExtensions() {
            return new[] {"json"};
        }

    }
}