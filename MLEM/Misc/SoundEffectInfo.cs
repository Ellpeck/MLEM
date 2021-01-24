using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns>A new sound effect instance, with this info's data applied</returns>
        public SoundEffectInstance CreateInstance() {
            var instance = this.Sound.CreateInstance();
            instance.Volume = this.Volume;
            instance.Pitch = this.Pitch;
            instance.Pan = this.Pan;
            return instance;
        }

    }

    /// <summary>
    /// A simple class that handles automatically removing and disposing <see cref="SoundEffectInstance"/> objects once they are done playing to free up the audio source for new sounds.
    /// Additionally, a callback can be registered that is invoked when the <see cref="SoundEffectInstance"/> finishes playing.
    /// Note that an object of this class can be added to a <see cref="Game"/> using <see cref="GameComponentCollection.Add"/>.
    /// </summary>
    public class SoundEffectInstanceHandler : GameComponent, IEnumerable<SoundEffectInstance> {

        private readonly List<Entry> playingSounds = new List<Entry>();

        /// <summary>
        /// Creates a new sound effect instance handler with the given settings
        /// </summary>
        /// <param name="game">The game instance</param>
        public SoundEffectInstanceHandler(Game game) : base(game) {
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime) {
            this.Update();
        }

        /// <summary>
        /// Updates this sound effect handler and manages all of the <see cref="SoundEffectInstance"/> objects in it.
        /// This should be called each update frame.
        /// </summary>
        public void Update() {
            for (var i = this.playingSounds.Count - 1; i >= 0; i--) {
                var entry = this.playingSounds[i];
                if (entry.Instance.IsDisposed || entry.Instance.State == SoundState.Stopped) {
                    entry.Instance.Stop(true);
                    entry.OnStopped?.Invoke(entry.Instance);
                    this.playingSounds.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffectInstance"/> to this handler.
        /// This also starts playing the instance.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <returns>The passed instance, for chaining</returns>
        public SoundEffectInstance Add(SoundEffectInstance instance, Action<SoundEffectInstance> onStopped = null) {
            this.playingSounds.Add(new Entry(instance, onStopped));
            instance.Play();
            return instance;
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffectInfo"/> to this handler.
        /// This also starts playing the created instance.
        /// </summary>
        /// <param name="info">The info for which to add a <see cref="SoundEffectInstance"/></param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <returns>The newly created <see cref="SoundEffectInstance"/></returns>
        public SoundEffectInstance Add(SoundEffectInfo info, Action<SoundEffectInstance> onStopped = null) {
            return this.Add(info.CreateInstance(), onStopped);
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffect"/> to this handler.
        /// This also starts playing the created instance.
        /// </summary>
        /// <param name="effect">The sound for which to add a <see cref="SoundEffectInstance"/></param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <returns>The newly created <see cref="SoundEffectInstance"/></returns>
        public SoundEffectInstance Add(SoundEffect effect, Action<SoundEffectInstance> onStopped = null) {
            return this.Add(effect.CreateInstance(), onStopped);
        }

        /// <inheritdoc />
        public IEnumerator<SoundEffectInstance> GetEnumerator() {
            foreach (var sound in this.playingSounds)
                yield return sound.Instance;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        private readonly struct Entry {

            public readonly SoundEffectInstance Instance;
            public readonly Action<SoundEffectInstance> OnStopped;

            public Entry(SoundEffectInstance instance, Action<SoundEffectInstance> onStopped) {
                this.Instance = instance;
                this.OnStopped = onStopped;
            }

        }

    }
}