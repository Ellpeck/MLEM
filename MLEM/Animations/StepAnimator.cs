using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MLEM.Maths;
using MLEM.Misc;

namespace MLEM.Animations {
    /// <summary>
    /// A step animator is a simple class to facilitate multi-step, timed, and optionally looping "animations" in the form of event callbacks.
    /// A step animator supports multiple <see cref="Steps"/> which are executed in order in <see cref="Update"/>.
    /// </summary>
    /// <typeparam name="TObject">The object to animate, which is passed to all relevant events.</typeparam>
    public class StepAnimator<TObject> : GenericDataHolder {

        /// <summary>
        /// This step animator's steps.
        /// </summary>
        public readonly ReadOnlyCollection<Step> Steps;
        /// <summary>
        /// Whether this step animator is looping.
        /// </summary>
        public readonly bool Loop;

        /// <summary>
        /// The index of the <see cref="CurrentStep"/> in the <see cref="Steps"/> collection.
        /// This is -1 if the animator has not started yet or is finished.
        /// </summary>
        public int CurrentStepIndex { get; private set; }
        /// <summary>
        /// The currently executed step, or <see langword="null"/> if the animator has not started yet or is finished.
        /// </summary>
        public Step CurrentStep => this.CurrentStepIndex >= 0 && this.CurrentStepIndex < this.Steps.Count ? this.Steps[this.CurrentStepIndex] : null;
        /// <summary>
        /// The time into the <see cref="CurrentStep"/>.
        /// </summary>
        public TimeSpan CurrentStepTime { get; private set; }
        /// <summary>
        /// The total time that the <see cref="CurrentStep"/> should play for.
        /// </summary>
        public TimeSpan CurrentStepTotalTime { get; private set; }

        private readonly Random random;

        /// <summary>
        /// Creates a new step animator with the given settings.
        /// </summary>
        /// <param name="random">The random instance to use for determining the time for each <see cref="Step"/> with differing <see cref="Step.MinTime"/> and <see cref="Step.MaxTime"/> to play for.</param>
        /// <param name="steps">The steps to play.</param>
        /// <param name="loop">Whether the <paramref name="steps"/> should loop.</param>
        public StepAnimator(Random random, IList<Step> steps, bool loop = false) {
            this.random = random;
            this.Steps = new ReadOnlyCollection<Step>(steps);
            this.Loop = loop;
            this.CurrentStepIndex = -1;
        }

        /// <summary>
        /// Creates a new step animator with the given settings.
        /// </summary>
        /// <param name="steps">The steps to play.</param>
        /// <param name="loop">Whether the <paramref name="steps"/> should loop.</param>
        public StepAnimator(IList<Step> steps, bool loop = false) : this(new Random(), steps, loop) {}

        /// <summary>
        /// Creates a new step animator with the given settings.
        /// </summary>
        /// <param name="steps">The steps to play.</param>
        public StepAnimator(params Step[] steps) : this(steps, false) {}

        /// <summary>
        /// Updates this step animator, starting, updating and finishing its <see cref="Steps"/>.
        /// If this method returns <see langword="true"/>, this step animator has finished playing (which will never occur if there are any steps and <see cref="Loop"/> is <see langword="true"/>).
        /// </summary>
        /// <param name="obj">The object to animate, which is passed to all relevant events.</param>
        /// <param name="passed">The amount of time that has passed since the last time this method was called.</param>
        /// <returns>Whether this step animator has finished playing.</returns>
        public bool Update(TObject obj, TimeSpan passed) {
            if (this.CurrentStepIndex >= this.Steps.Count)
                return true;

            if (this.CurrentStepIndex < 0)
                this.Advance(obj);

            this.CurrentStepTime += passed;

            var timePercentage = Math.Min(1, this.CurrentStepTime.Ticks / (float) this.CurrentStepTotalTime.Ticks);
            this.CurrentStep?.Animation?.Invoke(this, this.CurrentStep, obj, timePercentage);

            return timePercentage >= 1 && this.Advance(obj);
        }

        /// <summary>
        /// Resets this step animator, returning back to the first <see cref="Step"/> in its <see cref="Steps"/> collection.
        /// </summary>
        /// <param name="obj">The object to animate, which is passed to all relevant events.</param>
        public void Reset(TObject obj) {
            this.CurrentStep?.Finished?.Invoke(this, this.CurrentStep, obj);
            this.CurrentStepIndex = -1;
        }

        private bool Advance(TObject obj) {
            this.CurrentStep?.Finished?.Invoke(this, this.CurrentStep, obj);

            this.CurrentStepIndex++;
            if (this.CurrentStepIndex >= this.Steps.Count) {
                if (!this.Loop)
                    return true;
                this.CurrentStepIndex = 0;
            }

            this.CurrentStepTime = TimeSpan.Zero;
            this.CurrentStepTotalTime = this.CurrentStep == null ? TimeSpan.Zero :
                this.CurrentStep.MinTime == this.CurrentStep.MaxTime ? this.CurrentStep.MinTime :
                TimeSpan.FromTicks((long) this.random.NextSingle(this.CurrentStep.MinTime.Ticks, this.CurrentStep.MaxTime.Ticks));

            this.CurrentStep?.Started?.Invoke(this, this.CurrentStep, obj);

            return false;
        }

        /// <summary>
        /// A step in a <see cref="StepAnimator{TObject}"/>.
        /// </summary>
        public class Step : GenericDataHolder {

            /// <summary>
            /// The minimum time that this step should be able to play for.
            /// If this is different from <see cref="MaxTime"/>, a random time between the two is selected each time the step is played.
            /// </summary>
            public readonly TimeSpan MinTime;
            /// <summary>
            /// The maximum time that this step should be able to play for.
            /// If this is different from <see cref="MinTime"/>, a random time between the two is selected each time the step is played.
            /// </summary>
            public readonly TimeSpan MaxTime;

            /// <summary>
            /// The animation function that is called every <see cref="Update"/> while this step is active.
            /// </summary>
            public AnimationFunction Animation;
            /// <summary>
            /// A function that is called when this step starts, before the first time <see cref="Animation"/> is called.
            /// </summary>
            public Action<StepAnimator<TObject>, Step, TObject> Started;
            /// <summary>
            /// A function that is called when this step finishes, after the last time <see cref="Animation"/> is called, or when this step is currently playing while the <see cref="StepAnimator{TObject}"/> is <see cref="Reset"/>.
            /// </summary>
            public Action<StepAnimator<TObject>, Step, TObject> Finished;

            /// <summary>
            /// Creates a new animator step with the given settings.
            /// </summary>
            /// <param name="time">The time that this step should play for.</param>
            /// <param name="varianceFactor">An optional variance factor, which is used to determine a <see cref="MinTime"/> and <see cref="MaxTime"/> by multiplying the <paramref name="time"/> with it and subtracting the result from the <see cref="MinTime"/> and adding the result to the <see cref="MaxTime"/>.</param>
            /// <param name="animation">The animation function that is called every <see cref="Update"/> while this step is active.</param>
            public Step(TimeSpan time, float varianceFactor, AnimationFunction animation = null) : this(
                time - TimeSpan.FromTicks((long) (time.Ticks * varianceFactor)),
                time + TimeSpan.FromTicks((long) (time.Ticks * varianceFactor)),
                animation) {}

            /// <summary>
            /// Creates a new animator step with the given settings.
            /// </summary>
            /// <param name="time">The time that this step should play for.</param>
            /// <param name="animation">The animation function that is called every <see cref="Update"/> while this step is active.</param>
            public Step(TimeSpan time, AnimationFunction animation = null) : this(time, time, animation) {}

            /// <summary>
            /// Creates a new animator step with the given settings.
            /// </summary>
            /// <param name="minTime">The minimum time that this step should be able to play for. If this is different from <paramref name="maxTime"/>, a random time between the two is selected each time the step is played.</param>
            /// <param name="maxTime">The maximum time that this step should be able to play for. If this is different from <paramref name="minTime"/>, a random time between the two is selected each time the step is played.</param>
            /// <param name="animation">The animation function that is called every <see cref="Update"/> while this step is active.</param>
            public Step(TimeSpan minTime, TimeSpan maxTime, AnimationFunction animation = null) {
                this.MinTime = minTime;
                this.MaxTime = maxTime;
                this.Animation = animation;
            }

        }

        /// <summary>
        /// An animation function used by <see cref="Step"/>.
        /// The animation function that is called every <see cref="Update"/> while a <see cref="Step"/> is active.
        /// </summary>
        public delegate void AnimationFunction(StepAnimator<TObject> animator, Step step, TObject obj, float timePercentage);

    }
}
