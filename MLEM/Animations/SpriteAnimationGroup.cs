using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Textures;

namespace MLEM.Animations {
    public class SpriteAnimationGroup {

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
        public SpriteAnimation CurrentAnimation => this.CurrAnimation?.Animation;
        public AnimationFrame CurrentFrame => this.CurrentAnimation?.CurrentFrame;
        public TextureRegion CurrentRegion => this.CurrentAnimation?.CurrentRegion;
        public AnimationChanged OnAnimationChanged;
        private bool isDirty;

        public SpriteAnimationGroup Add(SpriteAnimation anim, Func<bool> condition, int priority = 0) {
            this.animations.Add(new ConditionedAnimation(anim, condition, priority));
            this.isDirty = true;
            return this;
        }

        public void Update(GameTime time) {
            this.FindAnimationToPlay();
            if (this.CurrAnimation != null)
                this.CurrAnimation.Animation.Update(time);
        }

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