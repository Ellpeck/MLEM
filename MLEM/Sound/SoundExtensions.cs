using Microsoft.Xna.Framework.Audio;

namespace MLEM.Sound {
    /// <summary>
    /// A set of extensions for dealing with <see cref="SoundEffect"/> and <see cref="SoundEffectInstance"/>
    /// </summary>
    public static class SoundExtensions {

        /// <summary>
        /// Creates a new <see cref="SoundEffectInstance"/> from the given <see cref="SoundEffect"/>, allowing optional instance data to be supplied as part of the method call
        /// </summary>
        /// <param name="effect">The sound effect to create an instance from</param>
        /// <param name="volume">The value to set the returned instance's <see cref="SoundEffectInstance.Volume"/> to. Defaults to 1.</param>
        /// <param name="pitch">The value to set the returned instance's <see cref="SoundEffectInstance.Pitch"/> to. Defaults to 0.</param>
        /// <param name="pan">The value to set the returned instance's <see cref="SoundEffectInstance.Pan"/> to. Defaults to 0.</param>
        /// <param name="isLooped">The value to set the returned instance's <see cref="SoundEffectInstance.IsLooped"/> to. Defaults to false.</param>
        /// <returns></returns>
        public static SoundEffectInstance CreateInstance(this SoundEffect effect, float volume = 1, float pitch = 0, float pan = 0, bool isLooped = false) {
            var instance = effect.CreateInstance();
            instance.Volume = volume;
            instance.Pitch = pitch;
            instance.Pan = pan;
            instance.IsLooped = isLooped;
            return instance;
        }

    }
}
