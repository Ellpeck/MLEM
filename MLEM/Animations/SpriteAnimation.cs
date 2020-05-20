using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Animations {
    /// <summary>
    /// A sprite animation that allows for any number of frames that each last any number of seconds
    /// </summary>
    public class SpriteAnimation : GenericDataHolder {

        private AnimationFrame[] frames;
        /// <summary>
        /// Returns the <see cref="AnimationFrame"/> at the given index.
        /// Index ordering is based on the order that animation frames were added in.
        /// </summary>
        /// <param name="index">The index in the list of animation frames</param>
        public AnimationFrame this[int index] => this.frames[index];
        /// <summary>
        /// The frame that the animation is currently on.
        /// </summary>
        public AnimationFrame CurrentFrame {
            get {
                // we might have overshot the end time by a little bit, so just return the last frame
                if (this.TimeIntoAnimation >= this.TotalTime)
                    return this.frames[this.frames.Length - 1];
                var accum = 0D;
                foreach (var frame in this.frames) {
                    accum += frame.Seconds;
                    if (accum >= this.TimeIntoAnimation)
                        return frame;
                }
                // if we're here then the time is negative for some reason, so just return the first frame
                return this.frames[0];
            }
        }
        /// <summary>
        /// The texture region that the animation's <see cref="CurrentFrame"/> has
        /// </summary>
        public TextureRegion CurrentRegion => this.CurrentFrame.Region;
        /// <summary>
        /// The total amount of time that this animation has.
        /// This is auatomatically calculated based on the frame time of each frame.
        /// </summary>
        public readonly double TotalTime;
        /// <summary>
        /// The amount of seconds that the animation has been going on for.
        /// If <see cref="TotalTime"/> is reached, this value resets to 0.
        /// </summary>
        public double TimeIntoAnimation { get; private set; }
        /// <summary>
        /// The finished state of this animation.
        /// This is only true for longer than a frame if <see cref="IsLooping"/> is false.
        /// </summary>
        public bool IsFinished { get; private set; }
        /// <summary>
        /// The name of this animation. This is useful if used in combination with <see cref="SpriteAnimationGroup"/>.
        /// </summary>
        public string Name;
        /// <summary>
        /// The speed multiplier that this animation should run with.
        /// Numbers higher than 1 will increase the speed.
        /// </summary>
        public float SpeedMultiplier = 1;
        /// <summary>
        /// Set to false to stop this animation from looping.
        /// To check if the animation has finished playing, see <see cref="IsFinished"/>.
        /// </summary>
        public bool IsLooping = true;
        /// <summary>
        /// A callback that gets fired when the animation completes.
        /// </summary>
        public event Completed OnCompleted;
        /// <summary>
        /// Set this to true to pause the playback of the animation.
        /// <see cref="TimeIntoAnimation"/> will not continue and the <see cref="CurrentFrame"/> will not change.
        /// </summary>
        public bool IsPaused;

        /// <summary>
        /// Creates a new sprite animation that contains the given frames.
        /// </summary>
        /// <param name="frames">The frames this animation should have</param>
        public SpriteAnimation(params AnimationFrame[] frames) {
            this.frames = frames;
            foreach (var frame in frames)
                this.TotalTime += frame.Seconds;
        }

        /// <summary>
        /// Creates a new sprite animation that contains the given texture regions as frames.
        /// </summary>
        /// <param name="timePerFrame">The amount of time that each frame should last for</param>
        /// <param name="regions">The texture regions that should make up this animation</param>
        public SpriteAnimation(double timePerFrame, params TextureRegion[] regions)
            : this(Array.ConvertAll(regions, region => new AnimationFrame(region, timePerFrame))) {
        }

        /// <summary>
        /// Creates a new sprite animation based on the given texture regions in rectangle-based format.
        /// </summary>
        /// <param name="timePerFrame">The amount of time that each frame should last for</param>
        /// <param name="texture">The texture that the regions should come from</param>
        /// <param name="regions">The texture regions that should make up this animation</param>
        public SpriteAnimation(double timePerFrame, Texture2D texture, params Rectangle[] regions)
            : this(timePerFrame, Array.ConvertAll(regions, region => new TextureRegion(texture, region))) {
        }

        /// <summary>
        /// Updates this animation, causing <see cref="TimeIntoAnimation"/> to be increased and the <see cref="CurrentFrame"/> to be updated.
        /// </summary>
        /// <param name="time">The game's time</param>
        public void Update(GameTime time) {
            this.SetTime(this.TimeIntoAnimation + time.ElapsedGameTime.TotalSeconds * this.SpeedMultiplier);
        }
        
        internal void SetTime(double totalTime) {
            if (this.IsFinished || this.IsPaused)
                return;
            this.TimeIntoAnimation = totalTime;
            if (this.TimeIntoAnimation >= this.TotalTime) {
                if (!this.IsLooping) {
                    this.IsFinished = true;
                } else {
                    this.Restart();
                }
                this.OnCompleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Restarts this animation from the first frame.
        /// </summary>
        public void Restart() {
            this.TimeIntoAnimation = 0;
            this.IsFinished = false;
            this.IsPaused = false;
        }

        public delegate void Completed(SpriteAnimation animation);

    }

    /// <summary>
    /// Represents a single frame of a <see cref="SpriteAnimation"/>
    /// </summary>
    public class AnimationFrame {

        /// <summary>
        /// The texture region that this frame should render
        /// </summary>
        public readonly TextureRegion Region;
        /// <summary>
        /// The total amount of seconds that this frame should last for
        /// </summary>
        public readonly double Seconds;

        /// <summary>
        /// Creates a new animation frame based on a texture region and a time
        /// </summary>
        /// <param name="region">The texture region that this frame should render</param>
        /// <param name="seconds">The total amount of seconds that this frame should last for</param>
        public AnimationFrame(TextureRegion region, double seconds) {
            this.Region = region;
            this.Seconds = seconds;
        }

    }
}