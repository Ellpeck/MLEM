using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace MLEM.Sound {
    /// <summary>
    /// A simple class that handles automatically removing and disposing <see cref="SoundEffectInstance"/> objects once they are done playing to free up the audio source for new sounds.
    /// Additionally, a callback can be registered that is invoked when the <see cref="SoundEffectInstance"/> finishes playing.
    /// Note that an object of this class can be added to a <see cref="Game"/> using its <see cref="GameComponentCollection"/>.
    /// </summary>
    public class SoundEffectInstanceHandler : GameComponent, IEnumerable<SoundEffectInstanceHandler.Entry> {

        private readonly List<Entry> playingSounds = new List<Entry>();
        private AudioListener[] listeners;

        /// <summary>
        /// Creates a new sound effect instance handler with the given settings
        /// </summary>
        /// <param name="game">The game instance</param>
        public SoundEffectInstanceHandler(Game game) : base(game) {}

        /// <inheritdoc cref="Update()"/>
        public override void Update(GameTime gameTime) {
            this.Update();
        }

        /// <summary>
        /// Updates this sound effect handler and manages all of the <see cref="SoundEffectInstance"/> objects in it.
        /// If <see cref="SetListeners"/> has been called, all sounds will additionally be updated in 3D space.
        /// This should be called each update frame.
        /// </summary>
        public void Update() {
            for (var i = this.playingSounds.Count - 1; i >= 0; i--) {
                var entry = this.playingSounds[i];
                if (entry.Instance.IsDisposed || entry.Instance.State == SoundState.Stopped) {
                    entry.StopAndNotify();
                    this.playingSounds.RemoveAt(i);
                } else {
                    entry.TryApply3D(this.listeners);
                }
            }
        }

        /// <summary>
        /// Sets the collection <see cref="AudioListener"/> objects that should be listening to the sounds in this handler in 3D space.
        /// If there are one or more listeners, this handler applies 3d effects to all sound effect instances that have been added to this handler along with an <see cref="AudioEmitter"/> in <see cref="Update(Microsoft.Xna.Framework.GameTime)"/> automatically.
        /// </summary>
        public void SetListeners(params AudioListener[] listeners) {
            this.listeners = listeners;
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
        /// Stops all of the sound effect instances in this handler
        /// </summary>
        public void Stop() {
            this.playingSounds.RemoveAll(e => {
                e.StopAndNotify();
                return true;
            });
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
            var entry = new Entry(instance, onStopped, emitter);
            this.playingSounds.Add(entry);
            instance.Play();
            entry.TryApply3D(this.listeners);
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

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<Entry> GetEnumerator() {
            return this.playingSounds.GetEnumerator();
        }

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }

        /// <summary>
        /// An entry in a <see cref="SoundEffectInstanceHandler"/>.
        /// Each entry stores the <see cref="SoundEffectInstance"/> that is being played, as well as the additional data passed through <see cref="SoundEffectInstanceHandler.Add(Microsoft.Xna.Framework.Audio.SoundEffectInstance,System.Action{Microsoft.Xna.Framework.Audio.SoundEffectInstance},Microsoft.Xna.Framework.Audio.AudioEmitter)"/>.
        /// </summary>
        public readonly struct Entry {

            /// <summary>
            /// The sound effect instance that this entry is playing
            /// </summary>
            public readonly SoundEffectInstance Instance;
            /// <summary>
            /// An action that is invoked when this entry's <see cref="Instance"/> is stopped or after it finishes naturally.
            /// This action is invoked in <see cref="SoundEffectInstanceHandler.Update()"/>.
            /// </summary>
            public readonly Action<SoundEffectInstance> OnStopped;
            /// <summary>
            /// The <see cref="AudioEmitter"/> that this sound effect instance is linked to.
            /// If the underlying handler's <see cref="SoundEffectInstanceHandler.SetListeners"/> method has been called, 3D sound will be applied.
            /// </summary>
            public readonly AudioEmitter Emitter;

            internal Entry(SoundEffectInstance instance, Action<SoundEffectInstance> onStopped, AudioEmitter emitter) {
                this.Instance = instance;
                this.OnStopped = onStopped;
                this.Emitter = emitter;
            }

            internal void TryApply3D(AudioListener[] listeners) {
                if (listeners != null && listeners.Length > 0 && this.Emitter != null)
                    this.Instance.Apply3D(listeners, this.Emitter);
            }

            internal void StopAndNotify() {
                this.Instance.Stop(true);
                this.OnStopped?.Invoke(this.Instance);
            }

        }

    }
}