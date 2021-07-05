using System;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Misc {
    /// <inheritdoc />
    [Obsolete("This class has been moved to MLEM.Sound.SoundEffectInfo in 5.1.0")]
    public class SoundEffectInfo : Sound.SoundEffectInfo {

        /// <inheritdoc />
        public SoundEffectInfo(SoundEffect sound, float volume = 1, float pitch = 0, float pan = 0) : base(sound, volume, pitch, pan) {
        }

    }
}