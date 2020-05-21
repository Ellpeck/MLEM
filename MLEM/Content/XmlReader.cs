using System;
using System.IO;
using System.Xml.Serialization;

namespace MLEM.Content {
    /// <inheritdoc />
    public class XmlReader : RawContentReader {

        /// <inheritdoc />
        public override bool CanRead(Type t) {
            return true;
        }

        /// <inheritdoc />
        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            return new XmlSerializer(t).Deserialize(stream);
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"xml"};
        }

    }
}