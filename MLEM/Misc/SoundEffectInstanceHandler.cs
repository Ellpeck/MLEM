using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Misc {
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
        /// Pauses all of the sound effect instances that are currently playing
        /// </summary>
        public void Pause() {
            foreach (var entry in this.playingSounds)
                entry.Instance.Pause();
        }

        /// <summary>
        /// Resumes all of the sound effect instances in this handler
        /// </summary>
        public void Resume() {
            foreach (var entry in this.playingSounds)
                entry.Instance.Resume();
        }

        /// <summary>
        /// Applies 3d effects to all sound effect instances that have been added to this handler along with an <see cref="AudioEmitter"/> 
        /// </summary>
        /// <param name="listener">The audio listener that 3d sound should be applied for</param>
        public void Apply3D(AudioListener listener) {
            foreach (var entry in this.playingSounds) {
                if (entry.Emitter != null)
                    entry.Instance.Apply3D(listener, entry.Emitter);
            }
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffectInstance"/> to this handler.
        /// This also starts playing the instance.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <param name="emitter">An optional audio emitter with which 3d sound can be applied</param>
        /// <returns>The passed instance, for chaining</returns>
        public SoundEffectInstance Add(SoundEffectInstance instance, Action<SoundEffectInstance> onStopped = null, AudioEmitter emitter = null) {
            this.playingSounds.Add(new Entry(instance, onStopped, emitter));
            instance.Play();
            return instance;
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffectInfo"/> to this handler.
        /// This also starts playing the created instance.
        /// </summary>
        /// <param name="info">The info for which to add a <see cref="SoundEffectInstance"/></param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <param name="emitter">An optional audio emitter with which 3d sound can be applied</param>
        /// <returns>The newly created <see cref="SoundEffectInstance"/></returns>
        public SoundEffectInstance Add(SoundEffectInfo info, Action<SoundEffectInstance> onStopped = null, AudioEmitter emitter = null) {
            return this.Add(info.CreateInstance(), onStopped, emitter);
        }

        /// <summary>
        /// Adds a new <see cref="SoundEffect"/> to this handler.
        /// This also starts playing the created instance.
        /// </summary>
        /// <param name="effect">The sound for which to add a <see cref="SoundEffectInstance"/></param>
        /// <param name="onStopped">The function that should be invoked when this instance stops playing, defaults to null</param>
        /// <param name="emitter">An optional audio emitter with which 3d sound can be applied</param>
        /// <returns>The newly created <see cref="SoundEffectInstance"/></returns>
        public SoundEffectInstance Add(SoundEffect effect, Action<SoundEffectInstance> onStopped = null, AudioEmitter emitter = null) {
            return this.Add(effect.CreateInstance(), onStopped, emitter);
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
            public readonly AudioEmitter Emitter;

            public Entry(SoundEffectInstance instance, Action<SoundEffectInstance> onStopped, AudioEmitter emitter) {
                this.Instance = instance;
                this.OnStopped = onStopped;
                this.Emitter = emitter;
            }

        }

    }
}