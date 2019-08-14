using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Textures;

namespace MLEM.Animations {
    public class SpriteAnimationGroup {

        private readonly List<ConditionedAnimation> animations = new List<ConditionedAnimation>();
        private ConditionedAnimation currAnimation;
        public AnimationFrame CurrentFrame => this.currAnimation != null ? this.currAnimation.Animation.CurrentFrame : null;
        public TextureRegion CurrentRegion => this.currAnimation != null ? this.currAnimation.Animation.CurrentRegion : null;

        public SpriteAnimationGroup Add(SpriteAnimation anim, Func<bool> condition) {
            this.animations.Add(new ConditionedAnimation(anim, condition));
            return this;
        }

        public void Update(GameTime time) {
            if (this.currAnimation == null || !this.currAnimation.ShouldPlay()) {
                this.currAnimation = null;
                foreach (var anim in this.animations) {
                    if (anim.ShouldPlay()) {
                        this.currAnimation = anim;
                        anim.Animation.Restart();
                        break;
                    }
                }
            }
            if (this.currAnimation != null)
                this.currAnimation.Animation.Update(time);
        }

    }

    internal class ConditionedAnimation {

        public readonly SpriteAnimation Animation;
        public readonly Func<bool> ShouldPlay;

        public ConditionedAnimation(SpriteAnimation animation, Func<bool> shouldPlay) {
            this.Animation = animation;
            this.ShouldPlay = shouldPlay;
        }

    }
}