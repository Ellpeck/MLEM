using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Textures;

namespace MLEM.Animations {
    public class SpriteAnimationGroup {

        private readonly List<ConditionedAnimation> animations = new List<ConditionedAnimation>();
        private ConditionedAnimation currAnimation;
        public SpriteAnimation CurrentAnimation => this.currAnimation?.Animation;
        public AnimationFrame CurrentFrame => this.CurrentAnimation?.CurrentFrame;
        public TextureRegion CurrentRegion => this.CurrentAnimation?.CurrentRegion;
        public AnimationChanged OnAnimationChanged;

        public SpriteAnimationGroup Add(SpriteAnimation anim, Func<bool> condition, int priority = 0) {
            this.animations.Add(new ConditionedAnimation(anim, condition, priority));
            this.animations.Sort((a1, a2) => a1.Priority.CompareTo(a2.Priority));
            return this;
        }

        public void Update(GameTime time) {
            ConditionedAnimation animToPlay = null;
            if (this.currAnimation != null && this.currAnimation.ShouldPlay())
                animToPlay = this.currAnimation;
            foreach (var anim in this.animations) {
                // if we find an animation with a lower priority then it means we can break
                // because the list is sorted by priority
                if (animToPlay != null && anim.Priority < animToPlay.Priority)
                    break;
                if (anim.ShouldPlay())
                    animToPlay = anim;
            }
            if (animToPlay != this.currAnimation) {
                this.OnAnimationChanged?.Invoke(this.currAnimation?.Animation, animToPlay?.Animation);
                this.currAnimation = animToPlay;
                if (animToPlay != null)
                    animToPlay.Animation.Restart();
            }

            if (this.currAnimation != null)
                this.currAnimation.Animation.Update(time);
        }

        public SpriteAnimation ByName(string name) {
            return this.animations.Find(anim => anim.Animation.Name == name)?.Animation;
        }

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