using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace MLEM.Content {
    /// <summary>
    /// Represents a way for any kind of raw content file to be read using a <see cref="RawContentManager"/>
    /// </summary>
    public abstract class RawContentReader {

        /// <summary>
        /// Returns if the given type can be loaded by this content reader
        /// </summary>
        /// <param name="t">The type of asset</param>
        /// <returns>If the type can be loaded by this content reader</returns>
        public abstract bool CanRead(Type t);

        /// <summary>
        /// Reads the content file from disk and returns it.
        /// </summary>
        /// <param name="manager">The <see cref="RawContentManager"/> that is loading the asset</param>
        /// <param name="assetPath">The full path to the asset, starting from the <see cref="ContentManager.RootDirectory"/></param>
        /// <param name="stream">A stream that leads to this asset</param>
        /// <param name="t">The type of asset to load</param>
        /// <param name="existing">If this asset is being reloaded, this value contains the previous version of the asset.</param>
        /// <returns>The loaded asset</returns>
        public abstract object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing);

        /// <summary>
        /// Represents the list of file extensions that this reader can read from.
        /// </summary>
        /// <returns>The list of valid extensions</returns>
        public abstract string[] GetFileExtensions();

    }

    /// <inheritdoc/>
    public abstract class RawContentReader<T> : RawContentReader {

        /// <inheritdoc/>
        public override bool CanRead(Type t) {
            return typeof(T).IsAssignableFrom(t);
        }

        /// <inheritdoc/>
        public override object Read(RawContentManager manager, string assetPath, Stream stream, Type t, object existing) {
            return this.Read(manager, assetPath, stream, (T) existing);
        }

        /// <summary>
        /// Reads the content file that is represented by our generic type from disk.
        /// </summary>
        /// <param name="manager">The <see cref="RawContentManager"/> that is loading the asset</param>
        /// <param name="assetPath">The full path to the asset, starting from the <see cref="ContentManager.RootDirectory"/></param>
        /// <param name="stream">A stream that leads to this asset</param>
        /// <param name="existing">If this asset is being reloaded, this value contains the previous version of the asset.</param>
        /// <returns>The loaded asset</returns>
        protected abstract T Read(RawContentManager manager, string assetPath, Stream stream, T existing);

    }
}