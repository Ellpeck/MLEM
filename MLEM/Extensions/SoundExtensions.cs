using System;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Extensions {
    public static class SoundExtensions {

        [Obsolete("When using the .NET Core version of MonoGame, the replay issue has been fixed. Just call Play() instead.")]
        public static void Replay(this SoundEffectInstance sound) {
            sound.Stop();
            sound.Play();
        }

    }
}