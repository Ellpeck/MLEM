using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Animations {
    /// <summary>
    /// Represents a list of <see cref="SpriteAnimation"/> objects with a condition and priority attached to them.
    /// Sprite animation groups can be used if any single entity should have multiple animations (like up, down, left, right standing and running animations) that should be automatically managed.
    /// </summary>
    public class SpriteAnimationGroup : GenericDataHolder {

        /// <summary>
        /// Returns the animation that is currently playing.
        /// </summary>
        public SpriteAnimation CurrentAnimation {
            get {
                this.SortAnimationsIfDirty(true);
                return this.currentAnimation?.Animation;
            }
        }
        /// <summary>
        /// Returns the frame that <see cref="CurrentAnimation"/> is displaying.
        /// </summary>
        public AnimationFrame CurrentFrame => this.CurrentAnimation?.CurrentFrame;
        /// <summary>
        /// Returns the <see cref="CurrentAnimation"/>'s <see cref="SpriteAnimation.CurrentRegion"/>.
        /// </summary>
        public TextureRegion CurrentRegion => this.CurrentAnimation?.CurrentRegion;
        /// <summary>
        /// Returns the <see cref="CurrentAnimation"/>'s <see cref="SpriteAnimation.CurrentRegions"/>.
        /// </summary>
        public IList<TextureRegion> CurrentRegions => this.CurrentAnimation?.CurrentRegions;
        /// <inheritdoc cref="SpriteAnimation.SpeedMultiplier"/>
        public float SpeedMultiplier {
            set {
                for (var i = 0; i < this.Count; i++)
                    this[i].SpeedMultiplier = value;
            }
        }
        /// <summary>
        /// Returns the amount of <see cref="SpriteAnimation"/> entries that this sprite animation group has.
        /// </summary>
        public int Count;
        /// <summary>
        /// Returns the <see cref="SpriteAnimation"/> at the given index.
        /// </summary>
        /// <param name="index">The index.</param>
        public SpriteAnimation this[int index] => this.animations[index].Animation;
        /// <summary>
        /// Returns the <see cref="SpriteAnimation"/> in this animation group with the given <see cref="SpriteAnimation.Name"/>, if it exists, and <see langword="null"/> otherwise.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        public SpriteAnimation this[string name] => this.animations.Find(anim => anim.Animation.Name == name)?.Animation;

        /// <summary>
        /// A callback for when the currently displaying animation has changed due to a condition with a higher priority being met.
        /// </summary>
        public event AnimationChanged OnAnimationChanged;

        private readonly List<ConditionedAnimation> animations = new List<ConditionedAnimation>();
        private ConditionedAnimation currentAnimation;
        private bool animationsDirty;

        /// <summary>
        /// Adds a <see cref="SpriteAnimation"/> to this group.
        /// </summary>
        /// <param name="anim">The animation to add</param>
        /// <param name="condition">The condition that needs to be met for this animation to play</param>
        /// <param name="priority">The priority of this animation. The higher the priority, the earlier it is picked for playing.</param>
        /// <returns>This group, for chaining</returns>
        public SpriteAnimationGroup Add(SpriteAnimation anim, Func<bool> condition, int priority = 0) {
            this.animations.Add(new ConditionedAnimation(anim, condition, priority));
            this.animationsDirty = true;
            return this;
        }

        /// <inheritdoc cref="SpriteAnimation.Update(GameTime)"/>
        public void Update(GameTime time) {
            this.Update(time.ElapsedGameTime);
        }

        /// <inheritdoc cref="SpriteAnimation.Update(TimeSpan)"/>
        public void Update(TimeSpan elapsed) {
            this.FindAnimationToPlay();
            if (this.CurrentAnimation != null)
                this.CurrentAnimation.Update(elapsed);
        }

        /// <summary>
        /// Find an animation in this group by name and returns it.
        /// </summary>
        /// <param name="name">The <see cref="SpriteAnimation.Name"/> of the animation</param>
        /// <returns>The animation by that name, or <c>null</c> if there is none</returns>
        [Obsolete("Use the name-based indexer instead")]
        public SpriteAnimation ByName(string name) {
            return this[name];
        }

        private void FindAnimationToPlay() {
            this.SortAnimationsIfDirty(false);

            ConditionedAnimation animToPlay = null;
            if (this.currentAnimation != null && this.currentAnimation.ShouldPlay())
                animToPlay = this.currentAnimation;

            for (var i = 0; i < this.Count; i++) {
                var anim = this.animations[i];
                // if we find an animation with a lower priority then it means we can break since the list is sorted by priority
                if (animToPlay != null && anim.Priority <= animToPlay.Priority)
                    break;
                if (anim.ShouldPlay())
                    animToPlay = anim;
            }

            if (animToPlay != this.currentAnimation) {
                this.OnAnimationChanged?.Invoke(this.currentAnimation?.Animation, animToPlay?.Animation);
                this.currentAnimation = animToPlay;
                if (animToPlay != null)
                    animToPlay.Animation.Restart();
            }
        }

        private void SortAnimationsIfDirty(bool findAnimationToPlay) {
            if (this.animationsDirty) {
                this.animationsDirty = false;
                this.animations.Sort((a1, a2) => a2.Priority.CompareTo(a1.Priority));
                if (findAnimationToPlay)
                    this.FindAnimationToPlay();
            }
        }

        /// <summary>
        /// A callback delegate for when a <see cref="SpriteAnimationGroup"/>'s current animation changed.
        /// </summary>
        /// <param name="oldAnim">The previous animation</param>
        /// <param name="newAnim">The new animation</param>
        public delegate void AnimationChanged(SpriteAnimation oldAnim, SpriteAnimation newAnim);

        private class ConditionedAnimation {

            public readonly SpriteAnimation Animation;
            public readonly Func<bool> ShouldPlay;
            public readonly int Priority;

            public ConditionedAnimation(SpriteAnimation animation, Func<bool> shouldPlay, int priority) {
                this.Animation = animation;
                this.ShouldPlay = shouldPlay;
                this.Priority = priority;
            }

        }

    }
}
