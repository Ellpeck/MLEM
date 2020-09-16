using System;
using System.IO;
using Microsoft.Xna.Framework.Media;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class SongReader : RawContentReader<Song> {

        /// <inheritdoc />
        protected override Song Read(RawContentManager manager, string assetPath, Stream stream, Song existing) {
            return Song.FromUri(Path.GetFileNameWithoutExtension(assetPath), new Uri(assetPath));
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"ogg", "wav", "mp3"};
        }

    }
}