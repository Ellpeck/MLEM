using System;
using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    /// <summary>
    /// A ui animation is a simple timed event that an <see cref="Element"/> in a <see cref="UiSystem"/> can use to play a visual or other type of animation.
    /// To use ui animations, you can use <see cref="Element.PlayAnimation"/>, or one of the built-in style properties like <see cref="Element.MouseEnterAnimation"/> or <see cref="Element.MouseExitAnimation"/>.
    /// </summary>
    public class UiAnimation : GenericDataHolder {

        /// <summary>
        /// The total time that this ui animation plays for.
        /// </summary>
        public readonly TimeSpan TotalTime;
        /// <summary>
        /// The <see cref="AnimationFunction"/> that is invoked every <see cref="Update"/>.
        /// </summary>
        public readonly AnimationFunction Function;

        /// <summary>
        /// An event that is raised when this ui animation is (re)started in <see cref="Update"/>.
        /// </summary>
        public Action<UiAnimation, Element> Started;
        /// <summary>
        /// An event that is raised when this ui animation is stopped or finished through <see cref="OnFinished"/>.
        /// </summary>
        public Action<UiAnimation, Element> Finished;
        /// <summary>
        /// The current time that this ui animation has been playing for, out of the <see cref="TotalTime"/>.
        /// </summary>
        public TimeSpan CurrentTime { get; private set; }

        /// <summary>
        /// Creates a new ui animation with the given settings.
        /// </summary>
        /// <param name="seconds">The amount of seconds that this ui animation should play for.</param>
        /// <param name="function">The <see cref="AnimationFunction"/> that is invoked every <see cref="Update"/>.</param>
        public UiAnimation(double seconds, AnimationFunction function) : this(TimeSpan.FromSeconds(seconds), function) {}

        /// <summary>
        /// Creates a new ui animation with the given settings.
        /// </summary>
        /// <param name="totalTime">The <see cref="TotalTime"/> that this ui animation should play for.</param>
        /// <param name="function">The <see cref="AnimationFunction"/> that is invoked every <see cref="Update"/>.</param>
        public UiAnimation(TimeSpan totalTime, AnimationFunction function) {
            this.TotalTime = totalTime;
            this.Function = function;
        }

        /// <summary>
        /// Updates this ui animation, invoking its <see cref="Started"/> event if necessary, increasing its <see cref="CurrentTime"/> and invoking its <see cref="Function"/>.
        /// This method is called by an <see cref="Element"/> in <see cref="Element.Update"/>.
        /// </summary>
        /// <param name="element">The element that this ui animation is attached to.</param>
        /// <param name="time">The game's current time.</param>
        /// <returns>Whether this animation is ready to finish, that is, if its <see cref="CurrentTime"/> is greater than or equal to its <see cref="TotalTime"/>.</returns>
        public virtual bool Update(Element element, GameTime time) {
            if (this.CurrentTime <= TimeSpan.Zero)
                this.Started?.Invoke(this, element);

            this.CurrentTime += time.ElapsedGameTime;
            this.Function?.Invoke(this, element, this.CurrentTime.Ticks / (float) this.TotalTime.Ticks);

            return this.CurrentTime >= this.TotalTime;
        }

        /// <summary>
        /// Causes this ui animation's <see cref="Finished"/> event to be raised, and sets the <see cref="CurrentTime"/> to <see cref="TimeSpan.Zero"/>.
        /// This allows the animation to play from the start again.
        /// This method is invoked automatically when <see cref="Update"/> returns <see langword="true"/> in <see cref="Element.Update"/>, as well as in <see cref="Element.StopAnimation"/>.
        /// </summary>
        /// <param name="element"></param>
        public virtual void OnFinished(Element element) {
            this.Finished?.Invoke(this, element);
            this.CurrentTime = TimeSpan.Zero;
        }

        /// <summary>
        /// A delegate method used by <see cref="UiAnimation.Function"/>.
        /// </summary>
        public delegate void AnimationFunction(UiAnimation animation, Element element, float timePercentage);

    }
}
