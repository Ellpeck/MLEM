using System;
using System.IO;
using System.Xml.Serialization;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class XmlReader : RawContentReader {

        /// <inheritdoc />
        public override bool CanRead(Type t) {
            return true;
        }

        /// <inheritdoc />
        #if NET6_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Trimming", "IL2026",
            Justification = "RawContentManager does not support XmlReader in a trimmed or AOT context, so this method is not expected to be called.")]
        #endif
        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            return new XmlSerializer(t).Deserialize(stream);
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"xml"};
        }

    }
}
