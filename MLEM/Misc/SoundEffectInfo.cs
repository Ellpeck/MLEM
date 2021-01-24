using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Misc {
    /// <summary>
    /// A sound effect info is a wrapper around <see cref="SoundEffect"/> that additionally stores <see cref="Volume"/>, <see cref="Pitch"/> and <see cref="Pan"/> information.
    /// Additionally, a <see cref="SoundEffectInstance"/> can be created using <see cref="CreateInstance"/> and a 3D sound can be played using <see cref="Play3D"/>.
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

        private AudioEmitter emitter;

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
        /// Plays this info's <see cref="Sound"/> once, with 3d positioning applied, to the given <see cref="listener"/>.
        /// The required <see cref="AudioEmitter"/> is automatically created and cached for future use.
        /// </summary>
        /// <param name="listener">Data about the listener</param>
        /// <param name="pos">The position to play the sound at</param>
        /// <param name="loop">Whether to loop the sound effect instance</param>
        /// <param name="dopplerScale">The emitter's doppler scale, defaults to 1</param>
        /// <param name="forward">The emitter's forward vector, defaults to <see cref="Vector3.Forward"/></param>
        /// <param name="up">The emitter's up vector, defaults to <see cref="Vector3.Up"/></param>
        /// <param name="velocity">The emitter's velocity, defaults to <see cref="Vector3.Zero"/></param>
        public SoundEffectInstance Play3D(AudioListener listener, Vector3 pos, bool loop = false, float? dopplerScale = null, Vector3? forward = null, Vector3? up = null, Vector3? velocity = null) {
            if (this.emitter == null)
                this.emitter = new AudioEmitter();
            this.emitter.Position = pos;
            this.emitter.DopplerScale = dopplerScale ?? 1;
            this.emitter.Forward = forward ?? Vector3.Forward;
            this.emitter.Up = up ?? Vector3.Up;
            this.emitter.Velocity = velocity ?? Vector3.Zero;

            var inst = this.CreateInstance();
            inst.IsLooped = loop;
            inst.Apply3D(listener, this.emitter);
            inst.Play();
            return inst;
        }

        /// <summary>
        /// Creates a new <see cref="SoundEffectInstance"/> with this sound effect info's data.
        /// </summary>
        /// <returns>A new sound effect instance, with this info's data applied</returns>
        public SoundEffectInstance CreateInstance() {
            var instance = this.Sound.CreateInstance();
            instance.Volume = this.Volume;
            instance.Pitch = this.Pitch;
            instance.Pan = this.Pan;
            return instance;
        }

    }
}