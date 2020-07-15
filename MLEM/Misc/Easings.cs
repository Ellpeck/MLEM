using System;

namespace MLEM.Misc {
    /// <summary>
    /// This class contains a set of easing functions, adapted from <see href="https://easings.net"/>.
    /// These can be used for ui elements or any other kind of animations.
    /// By default, each function takes an input that ranges between 0 and 1, and produces an output that roughly ranges between 0 and 1. To change this behavior, you can use <see cref="ScaleInput"/> and <see cref="ScaleOutput"/>.
    /// </summary>
    public static class Easings {

        /// <summary><see href="https://easings.net/#easeInSine"/></summary>
        public static readonly Easing InSine = p => 1 - (float) Math.Cos(p * Math.PI / 2);
        /// <summary><see href="https://easings.net/#easeOutSine"/></summary>
        public static readonly Easing OutSine = p => (float) Math.Sin(p * Math.PI / 2);
        /// <summary><see href="https://easings.net/#easeInOutSine"/></summary>
        public static readonly Easing InOutSine = p => -((float) Math.Cos(p * Math.PI) - 1) / 2;

        /// <summary><see href="https://easings.net/#easeInQuad"/></summary>
        public static readonly Easing InQuad = p => p * p;
        /// <summary><see href="https://easings.net/#easeOutQuad"/></summary>
        public static readonly Easing OutQuad = p => 1 - (1 - p) * (1 - p);
        /// <summary><see href="https://easings.net/#easeInOutQuad"/></summary>
        public static readonly Easing InOutQuad = p => p < 0.5F ? 2 * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary><see href="https://easings.net/#easeInCubic"/></summary>
        public static readonly Easing InCubic = p => p * p * p;
        /// <summary><see href="https://easings.net/#easeOutCubic"/></summary>
        public static readonly Easing OutCubic = p => 1 - (1 - p) * (1 - p) * (1 - p);
        /// <summary><see href="https://easings.net/#easeInOutCubic"/></summary>
        public static readonly Easing InOutCubic = p => p < 0.5F ? 4 * p * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary><see href="https://easings.net/#easeInExpo"/></summary>
        public static readonly Easing InExpo = p => p == 0 ? 0 : (float) Math.Pow(2, 10 * p - 10);
        /// <summary><see href="https://easings.net/#easeOutExpo"/></summary>
        public static readonly Easing OutExpo = p => p == 1 ? 1 : 1 - (float) Math.Pow(2, -10 * p);
        /// <summary><see href="https://easings.net/#easeInOutExpo"/></summary>
        public static readonly Easing InOutExpo = p => p == 0 ? 0 : p == 1 ? 1 : p < 0.5F ? (float) Math.Pow(2, 20 * p - 10) / 2 : (2 - (float) Math.Pow(2, -20 * p + 10)) / 2;

        /// <summary><see href="https://easings.net/#easeInCirc"/></summary>
        public static readonly Easing InCirc = p => 1 - (float) Math.Sqrt(1 - p * p);
        /// <summary><see href="https://easings.net/#easeOutCirc"/></summary>
        public static readonly Easing OutCirc = p => (float) Math.Sqrt(1 - (p - 1) * (p - 1));
        /// <summary><see href="https://easings.net/#easeInOutCirc"/></summary>
        public static readonly Easing InOutCirc = p => p < 0.5F ? (1 - (float) Math.Sqrt(1 - 2 * p * (2 * p))) / 2 : ((float) Math.Sqrt(1 - (-2 * p + 2) * (-2 * p + 2)) + 1) / 2;

        /// <summary><see href="https://easings.net/#easeInBack"/></summary>
        public static readonly Easing InBack = p => 2.70158F * p * p * p - 1.70158F * p * p;
        /// <summary><see href="https://easings.net/#easeOutBack"/></summary>
        public static readonly Easing OutBack = p => 1 + 2.70158F * (p - 1) * (p - 1) * (p - 1) + 1.70158F * (p - 1) * (p - 1);
        /// <summary><see href="https://easings.net/#easeInOutBack"/></summary>
        public static readonly Easing InOutBack = p => p < 0.5 ? 2 * p * (2 * p) * ((2.594909F + 1) * 2 * p - 2.594909F) / 2 : ((2 * p - 2) * (2 * p - 2) * ((2.594909F + 1) * (p * 2 - 2) + 2.594909F) + 2) / 2;

        /// <summary><see href="https://easings.net/#easeInElastic"/></summary>
        public static readonly Easing InElastic = p => p == 0 ? 0 : p == 1 ? 1 : -(float) Math.Pow(2, 10 * p - 10) * (float) Math.Sin((p * 10 - 10.75) * 2.094395F);
        /// <summary><see href="https://easings.net/#easeOutElastic"/></summary>
        public static readonly Easing OutElastic = p => p == 0 ? 0 : p == 1 ? 1 : (float) Math.Pow(2, -10 * p) * (float) Math.Sin((p * 10 - 0.75) * 2.0943951023932F) + 1;
        /// <summary><see href="https://easings.net/#easeInOutElastic"/></summary>
        public static readonly Easing InOutElastic = p => p == 0 ? 0 : p == 1 ? 1 : p < 0.5 ? -((float) Math.Pow(2, 20 * p - 10) * (float) Math.Sin((20 * p - 11.125) * 1.39626340159546F)) / 2 : (float) Math.Pow(2, -20 * p + 10) * (float) Math.Sin((20 * p - 11.125) * 1.39626340159546F) / 2 + 1;

        /// <summary><see href="https://easings.net/#easeInBounce"/></summary>
        public static readonly Easing InBounce = p => 1 - OutBounce(1 - p);
        /// <summary><see href="https://easings.net/#easeOutBounce"/></summary>
        public static readonly Easing OutBounce = p => {
            const float n1 = 7.5625F;
            const float d1 = 2.75F;
            if (p < 1 / d1) {
                return n1 * p * p;
            } else if (p < 2 / d1) {
                return n1 * (p -= 1.5F / d1) * p + 0.75F;
            } else if (p < 2.5 / d1) {
                return n1 * (p -= 2.25F / d1) * p + 0.9375F;
            } else {
                return n1 * (p -= 2.625F / d1) * p + 0.984375F;
            }
        };
        /// <summary><see href="https://easings.net/#easeInOutBounce"/></summary>
        public static readonly Easing InOutBounce = p => p < 0.5 ? (1 - OutBounce(1 - 2 * p)) / 2 : (1 + OutBounce(2 * p - 1)) / 2;

        /// <summary>
        /// Scales the input of an easing function, which is usually between 0 and 1, to a given minimum and maximum.
        /// Note that the minimum needs to be smaller than the maximum.
        /// </summary>
        /// <param name="easing">The easing function to scale</param>
        /// <param name="min">The minimum input value</param>
        /// <param name="max">The maximum input value</param>
        /// <returns>The scaled easing function</returns>
        public static Easing ScaleInput(this Easing easing, float min, float max) {
            return p => easing((p - min) / (max - min));
        }

        /// <summary>
        /// Scales the output of an easing function, which is usually between 0 and 1, to a given minimum and maximum.
        /// Note that the minimum needs to be smaller than the maximum.
        /// </summary>
        /// <param name="easing">The easing function to scale</param>
        /// <param name="min">The minimum output value</param>
        /// <param name="max">The maximum output value</param>
        /// <returns>The scaled easing function</returns>
        public static Easing ScaleOutput(this Easing easing, float min, float max) {
            return p => easing(p) * (max - min) + min;
        }

        /// <summary>
        /// A delegate method used by <see cref="Easings"/>.
        /// </summary>
        /// <param name="percentage">The percentage into the easing function. Either between 0 and 1, or, if <see cref="Easings.ScaleInput"/> was used, between an arbitary set of values.</param>
        public delegate float Easing(float percentage);

    }
}