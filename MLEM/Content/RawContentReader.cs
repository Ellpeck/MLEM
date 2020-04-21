using System;
using System.IO;

namespace MLEM.Content {
    public abstract class RawContentReader {

        public abstract bool CanRead(Type t);

        public abstract object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing);

        public abstract string[] GetFileExtensions();

    }

    public abstract class RawContentReader<T> : RawContentReader {

        public override bool CanRead(Type t) {
            return typeof(T).IsAssignableFrom(t);
        }

        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            return this.Read(manager, assetPath, stream, (T) existing);
        }

        protected abstract T Read(RawContentManager manager, string assetPath, Stream stream, T existing);

    }
}