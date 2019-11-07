using Microsoft.Xna.Framework.Audio;

namespace MLEM.Extensions {
    public static class SoundExtensions {

        public static void Replay(this SoundEffectInstance sound) {
            sound.Stop();
            sound.Play();
        }

    }
}