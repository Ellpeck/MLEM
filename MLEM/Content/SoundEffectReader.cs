using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Content {
    public class SoundEffectReader : RawContentReader<SoundEffect> {

        protected override SoundEffect Read(RawContentManager manager, string assetPath, Stream stream, SoundEffect existing) {
            return SoundEffect.FromStream(stream);
        }

        public override string[] GetFileExtensions() {
            return new[] {"ogg", "wav", "mp3"};
        }

    }
}