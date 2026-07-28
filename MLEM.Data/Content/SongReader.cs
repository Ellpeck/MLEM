using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class SongReader : RawContentReader<Song> {

        /// <inheritdoc />
        protected override Song Read(RawContentManager manager, string assetPath, Stream stream, Song existing) {
            if (!Path.IsPathRooted(assetPath))
                throw new ContentLoadException($"Cannot read song from non-rooted path {assetPath}");
            return Song.FromUri(Path.GetFileNameWithoutExtension(assetPath), new Uri(assetPath));
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"ogg", "wav", "mp3"};
        }

    }
}
