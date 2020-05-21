using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Content {
    /// <inheritdoc />
    public class SoundEffectReader : RawContentReader<SoundEffect> {

        /// <inheritdoc />
        protected override SoundEffect Read(RawContentManager manager, string assetPath, Stream stream, SoundEffect existing) {
            return SoundEffect.FromStream(stream);
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"ogg", "wav", "mp3"};
        }

    }
}