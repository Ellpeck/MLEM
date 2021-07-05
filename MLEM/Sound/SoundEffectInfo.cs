using Microsoft.Xna.Framework.Audio;
using MLEM.Extensions;

namespace MLEM.Sound {
    /// <summary>
    /// A sound effect info is a wrapper around <see cref="SoundEffect"/> that additionally stores <see cref="Volume"/>, <see cref="Pitch"/> and <see cref="Pan"/> information.
    /// Additionally, a <see cref="SoundEffectInstance"/> can be created using <see cref="CreateInstance"/>.
    /// </summary>
    public class SoundEffectInfo {

        /// <summary>
        /// The <see cref="SoundEffect"/> that is played by this info.
        /// </summary>
        public readonly SoundEffect Sound;
        /// <summary>
        /// Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.
        /// </summary>
        public float Volume;
        /// <summary>
        /// Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave).
        /// </summary>
        public float Pitch;
        /// <summary>
        /// Pan value ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker).
        /// </summary>
        public float Pan;

        /// <summary>
        /// Creates a new sound effect info with the given values.
        /// </summary>
        /// <param name="sound">The sound to play</param>
        /// <param name="volume">The volume to play the sound with</param>
        /// <param name="pitch">The pitch to play the sound with</param>
        /// <param name="pan">The pan to play the sound with</param>
        public SoundEffectInfo(SoundEffect sound, float volume = 1, float pitch = 0, float pan = 0) {
            this.Sound = sound;
            this.Volume = volume;
            this.Pitch = pitch;
            this.Pan = pan;
        }

        /// <summary>
        /// Plays this info's <see cref="Sound"/> once.
        /// </summary>
        /// <returns>False if more sounds are currently playing than the platform allows</returns>
        public bool Play() {
            return this.Sound.Play(this.Volume, this.Pitch, this.Pan);
        }

        /// <summary>
        /// Creates a new <see cref="SoundEffectInstance"/> with this sound effect info's data.
        /// </summary>
        /// <param name="isLooped">The value to set the returned instance's <see cref="SoundEffectInstance.IsLooped"/> to. Defaults to false.</param>
        /// <returns>A new sound effect instance, with this info's data applied</returns>
        public SoundEffectInstance CreateInstance(bool isLooped = false) {
            return this.Sound.CreateInstance(this.Volume, this.Pitch, this.Pan, isLooped);
        }

    }
}