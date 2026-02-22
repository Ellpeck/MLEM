using System;
using System.Collections.Generic;
using MLEM.Animations;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    /// <summary>
    /// A ui animation is a simple timed event that an <see cref="Element"/> in a <see cref="UiSystem"/> can use to play a visual or other type of animation.
    /// Ui animations use <see cref="StepAnimator{Element}"/> as their underlying animation player.
    /// To use ui animations, you can use <see cref="Element.PlayAnimation"/>, or one of the built-in style properties like <see cref="Element.MouseEnterAnimation"/> or <see cref="Element.MouseExitAnimation"/>.
    /// </summary>
    public class UiAnimation : StepAnimator<Element> {

        /// <inheritdoc />
        public UiAnimation(double seconds, AnimationFunction function) : this(new Step(TimeSpan.FromSeconds(seconds), function)) {}

        /// <inheritdoc />
        public UiAnimation(IList<Step> steps, bool loop = false) : base(steps, loop) {}

        /// <inheritdoc />
        public UiAnimation(params Step[] steps) : base(steps) {}

    }
}
