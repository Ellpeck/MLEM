using System;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing wiht <see cref="SoundEffectInstance"/>
    /// </summary>
    public static class SoundExtensions {

        /// <summary>
        /// Stops and plays a sound effect instance in one call
        /// </summary>
        /// <param name="sound">The sound to stop and play</param>
        [Obsolete("When using the .NET Core version of MonoGame, the replay issue has been fixed. Just call Play() instead.")]
        public static void Replay(this SoundEffectInstance sound) {
            sound.Stop();
            sound.Play();
        }

    }
}