using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Animations {
    public class SpriteAnimation : GenericDataHolder {

        private AnimationFrame[] frames;
        public AnimationFrame this[int index] => this.frames[index];
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
        public TextureRegion CurrentRegion => this.CurrentFrame.Region;
        public readonly double TotalTime;
        public double TimeIntoAnimation { get; private set; }
        public bool IsFinished { get; private set; }
        public string Name;
        public float SpeedMultiplier = 1;

        public bool IsLooping = true;
        public Completed OnCompleted;
        public bool IsPaused;

        public SpriteAnimation(params AnimationFrame[] frames) {
            this.frames = frames;
            foreach (var frame in frames)
                this.TotalTime += frame.Seconds;
        }

        public SpriteAnimation(double timePerFrame, params TextureRegion[] regions)
            : this(Array.ConvertAll(regions, region => new AnimationFrame(region, timePerFrame))) {
        }

        public SpriteAnimation(double timePerFrame, Texture2D texture, params Rectangle[] regions)
            : this(timePerFrame, Array.ConvertAll(regions, region => new TextureRegion(texture, region))) {
        }

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

        public void Restart() {
            this.TimeIntoAnimation = 0;
            this.IsFinished = false;
            this.IsPaused = false;
        }

        public delegate void Completed(SpriteAnimation animation);

    }

    public class AnimationFrame {

        public readonly TextureRegion Region;
        public readonly double Seconds;

        public AnimationFrame(TextureRegion region, double seconds) {
            this.Region = region;
            this.Seconds = seconds;
        }

    }
}