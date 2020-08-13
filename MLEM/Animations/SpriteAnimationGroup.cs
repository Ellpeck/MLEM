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

        private readonly List<ConditionedAnimation> animations = new List<ConditionedAnimation>();
        private ConditionedAnimation currAnimation;
        private ConditionedAnimation CurrAnimation {
            get {
                if (this.isDirty) {
                    this.isDirty = false;
                    this.animations.Sort((a1, a2) => a2.Priority.CompareTo(a1.Priority));
                    this.FindAnimationToPlay();
                }
                return this.currAnimation;
            }
            set => this.currAnimation = value;
        }
        /// <summary>
        /// The animation that is currently playing
        /// </summary>
        public SpriteAnimation CurrentAnimation => this.CurrAnimation?.Animation;
        /// <summary>
        /// The frame that <see cref="CurrentAnimation"/> is displaying
        /// </summary>
        public AnimationFrame CurrentFrame => this.CurrentAnimation?.CurrentFrame;
        /// <summary>
        /// The region that <see cref="CurrentFrame"/> has
        /// </summary>
        public TextureRegion CurrentRegion => this.CurrentAnimation?.CurrentRegion;
        /// <summary>
        /// A callback for when the currently displaying animation has changed due to a condition with a higher priority being met. 
        /// </summary>
        public event AnimationChanged OnAnimationChanged;
        private bool isDirty;
        /// <inheritdoc cref="SpriteAnimation.SpeedMultiplier"/>
        public float SpeedMultiplier {
            set {
                foreach (var anim in this.animations)
                    anim.Animation.SpeedMultiplier = value;
            }
        }

        /// <summary>
        /// Adds a <see cref="SpriteAnimation"/> to this group.
        /// </summary>
        /// <param name="anim">The animation to add</param>
        /// <param name="condition">The condition that needs to be met for this animation to play</param>
        /// <param name="priority">The priority of this animation. The higher the priority, the earlier it is picked for playing.</param>
        /// <returns>This group, for chaining</returns>
        public SpriteAnimationGroup Add(SpriteAnimation anim, Func<bool> condition, int priority = 0) {
            this.animations.Add(new ConditionedAnimation(anim, condition, priority));
            this.isDirty = true;
            return this;
        }

        /// <inheritdoc cref="SpriteAnimation.Update(GameTime)"/>
        public void Update(GameTime time) {
            this.Update(time.ElapsedGameTime);
        }

        /// <inheritdoc cref="SpriteAnimation.Update(TimeSpan)"/>
        public void Update(TimeSpan elapsed) {
            this.FindAnimationToPlay();
            if (this.CurrAnimation != null)
                this.CurrAnimation.Animation.Update(elapsed);
        }

        /// <summary>
        /// Find an animation in this group by name and returns it.
        /// </summary>
        /// <param name="name">The <see cref="SpriteAnimation.Name"/> of the animation</param>
        /// <returns>The animation by that name, or <c>null</c> if there is none</returns>
        public SpriteAnimation ByName(string name) {
            return this.animations.Find(anim => anim.Animation.Name == name)?.Animation;
        }

        private void FindAnimationToPlay() {
            ConditionedAnimation animToPlay = null;
            if (this.CurrAnimation != null && this.CurrAnimation.ShouldPlay())
                animToPlay = this.CurrAnimation;
            foreach (var anim in this.animations) {
                // if we find an animation with a lower priority then it means we can break
                // because the list is sorted by priority
                if (animToPlay != null && anim.Priority < animToPlay.Priority)
                    break;
                if (anim.ShouldPlay())
                    animToPlay = anim;
            }
            if (animToPlay != this.CurrAnimation) {
                this.OnAnimationChanged?.Invoke(this.CurrAnimation?.Animation, animToPlay?.Animation);
                this.CurrAnimation = animToPlay;
                if (animToPlay != null)
                    animToPlay.Animation.Restart();
            }
        }

        /// <summary>
        /// A callback delegate for when a <see cref="SpriteAnimationGroup"/>'s current animation changed.
        /// </summary>
        /// <param name="oldAnim">The previous animation</param>
        /// <param name="newAnim">The new animation</param>
        public delegate void AnimationChanged(SpriteAnimation oldAnim, SpriteAnimation newAnim);

    }

    internal class ConditionedAnimation {

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