using System;
using System.IO;
using System.Xml.Serialization;

namespace MLEM.Content {
    public class XmlReader : RawContentReader {

        public override bool CanRead(Type t) {
            return true;
        }

        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            return new XmlSerializer(t).Deserialize(stream);
        }

        public override string[] GetFileExtensions() {
            return new[] {"xml"};
        }

    }
}