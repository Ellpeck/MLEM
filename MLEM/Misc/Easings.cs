using System;
using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    /// <summary>
    /// This class contains a set of easing functions, adapted from https://easings.net.
    /// These can be used for ui elements or any other kind of animations.
    /// By default, each function takes an input that ranges between 0 and 1, and produces an output that roughly ranges between 0 and 1. To change this behavior, you can use <see cref="ScaleInput"/> and <see cref="ScaleOutput"/>.
    /// </summary>
    public static class Easings {

        /// <summary>
        /// An easing function that constantly returns 0, regardless of the input percentage.
        /// This is useful for chaining using <see cref="AndThen(MLEM.Misc.Easings.Easing,MLEM.Misc.Easings.Easing)"/>.
        /// </summary>
        public static readonly Easing Zero = p => 0;
        /// <summary>
        /// An easing function that constantly returns 1, regardless of the input percentage.
        /// This is useful for chaining using <see cref="AndThen(MLEM.Misc.Easings.Easing,MLEM.Misc.Easings.Easing)"/>.
        /// </summary>
        public static readonly Easing One = p => 1;
        /// <summary>
        /// A linear easing function that returns the input percentage without modifying it.
        /// </summary>
        public static readonly Easing Linear = p => p;

        /// <summary>https://easings.net/#easeInSine</summary>
        public static readonly Easing InSine = p => 1 - (float) Math.Cos(p * Math.PI / 2);
        /// <summary>https://easings.net/#easeOutSine</summary>
        public static readonly Easing OutSine = p => (float) Math.Sin(p * Math.PI / 2);
        /// <summary>https://easings.net/#easeInOutSine</summary>
        public static readonly Easing InOutSine = p => -((float) Math.Cos(p * Math.PI) - 1) / 2;

        /// <summary>https://easings.net/#easeInQuad</summary>
        public static readonly Easing InQuad = p => p * p;
        /// <summary>https://easings.net/#easeOutQuad</summary>
        public static readonly Easing OutQuad = p => 1 - (1 - p) * (1 - p);
        /// <summary>https://easings.net/#easeInOutQuad</summary>
        public static readonly Easing InOutQuad = p => p < 0.5F ? 2 * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary>https://easings.net/#easeInCubic</summary>
        public static readonly Easing InCubic = p => p * p * p;
        /// <summary>https://easings.net/#easeOutCubic</summary>
        public static readonly Easing OutCubic = p => 1 - (1 - p) * (1 - p) * (1 - p);
        /// <summary>https://easings.net/#easeInOutCubic</summary>
        public static readonly Easing InOutCubic = p => p < 0.5F ? 4 * p * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary>https://easings.net/#easeInQuart</summary>
        public static readonly Easing InQuart = p => p * p * p * p;
        /// <summary>https://easings.net/#easeOutQuart</summary>
        public static readonly Easing OutQuart = p => 1 - (1 - p) * (1 - p) * (1 - p) * (1 - p);
        /// <summary>https://easings.net/#easeInOutQuart</summary>
        public static readonly Easing InOutQuart = p => p < 0.5F ? 8 * p * p * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary>https://easings.net/#easeInQuint</summary>
        public static readonly Easing InQuint = p => p * p * p * p * p;
        /// <summary>https://easings.net/#easeOutQuint</summary>
        public static readonly Easing OutQuint = p => 1 - (1 - p) * (1 - p) * (1 - p) * (1 - p) * (1 - p);
        /// <summary>https://easings.net/#easeInOutQuint</summary>
        public static readonly Easing InOutQuint = p => p < 0.5F ? 16 * p * p * p * p * p : 1 - (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) * (-2 * p + 2) / 2;

        /// <summary>https://easings.net/#easeInExpo</summary>
        public static readonly Easing InExpo = p => p == 0 ? 0 : (float) Math.Pow(2, 10 * p - 10);
        /// <summary>https://easings.net/#easeOutExpo</summary>
        public static readonly Easing OutExpo = p => p == 1 ? 1 : 1 - (float) Math.Pow(2, -10 * p);
        /// <summary>https://easings.net/#easeInOutExpo</summary>
        public static readonly Easing InOutExpo = p => p == 0 ? 0 : p == 1 ? 1 : p < 0.5F ? (float) Math.Pow(2, 20 * p - 10) / 2 : (2 - (float) Math.Pow(2, -20 * p + 10)) / 2;

        /// <summary>https://easings.net/#easeInCirc</summary>
        public static readonly Easing InCirc = p => 1 - (float) Math.Sqrt(1 - p * p);
        /// <summary>https://easings.net/#easeOutCirc</summary>
        public static readonly Easing OutCirc = p => (float) Math.Sqrt(1 - (p - 1) * (p - 1));
        /// <summary>https://easings.net/#easeInOutCirc</summary>
        public static readonly Easing InOutCirc = p => p < 0.5F ? (1 - (float) Math.Sqrt(1 - 2 * p * (2 * p))) / 2 : ((float) Math.Sqrt(1 - (-2 * p + 2) * (-2 * p + 2)) + 1) / 2;

        /// <summary>https://easings.net/#easeInBack</summary>
        public static readonly Easing InBack = p => 2.70158F * p * p * p - 1.70158F * p * p;
        /// <summary>https://easings.net/#easeOutBack</summary>
        public static readonly Easing OutBack = p => 1 + 2.70158F * (p - 1) * (p - 1) * (p - 1) + 1.70158F * (p - 1) * (p - 1);
        /// <summary>https://easings.net/#easeInOutBack</summary>
        public static readonly Easing InOutBack = p => p < 0.5 ? 2 * p * (2 * p) * ((2.594909F + 1) * 2 * p - 2.594909F) / 2 : ((2 * p - 2) * (2 * p - 2) * ((2.594909F + 1) * (p * 2 - 2) + 2.594909F) + 2) / 2;

        /// <summary>https://easings.net/#easeInElastic</summary>
        public static readonly Easing InElastic = p => p == 0 ? 0 : p == 1 ? 1 : -(float) Math.Pow(2, 10 * p - 10) * (float) Math.Sin((p * 10 - 10.75) * 2.094395F);
        /// <summary>https://easings.net/#easeOutElastic</summary>
        public static readonly Easing OutElastic = p => p == 0 ? 0 : p == 1 ? 1 : (float) Math.Pow(2, -10 * p) * (float) Math.Sin((p * 10 - 0.75) * 2.0943951023932F) + 1;
        /// <summary>https://easings.net/#easeInOutElastic</summary>
        public static readonly Easing InOutElastic = p => p == 0 ? 0 : p == 1 ? 1 : p < 0.5 ? -((float) Math.Pow(2, 20 * p - 10) * (float) Math.Sin((20 * p - 11.125) * 1.39626340159546F)) / 2 : (float) Math.Pow(2, -20 * p + 10) * (float) Math.Sin((20 * p - 11.125) * 1.39626340159546F) / 2 + 1;

        /// <summary>https://easings.net/#easeInBounce</summary>
        public static readonly Easing InBounce = p => 1 - Easings.OutBounce(1 - p);
        /// <summary>https://easings.net/#easeOutBounce</summary>
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
        /// <summary>https://easings.net/#easeInOutBounce</summary>
        public static readonly Easing InOutBounce = p => p < 0.5 ? (1 - Easings.OutBounce(1 - 2 * p)) / 2 : (1 + Easings.OutBounce(2 * p - 1)) / 2;

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
        /// Reverses the input of an easing function, which is usually between 0 and 1, to be passed into the easing function as if it were between 1 and 0.
        /// </summary>
        /// <param name="easing">The easing function whose output to reverse.</param>
        /// <returns>The reversed easing function.</returns>
        public static Easing ReverseInput(this Easing easing) {
            return p => easing(1 - p);
        }

        /// <summary>
        /// Reverses the output of an easing function, which is usually between 0 and 1, to return as if it were between 1 and 0.
        /// </summary>
        /// <param name="easing">The easing function whose input to reverse.</param>
        /// <returns>The reversed easing function.</returns>
        public static Easing ReverseOutput(this Easing easing) {
            return p => 1 - easing(p);
        }

        /// <summary>
        /// Causes the easing functino to play fully, and then play fully in reverse, in the span between an input of 0 and 1.
        /// In some places, this behavior is also called "auto-reversing".
        /// </summary>
        /// <param name="easing">The easing function to play in reverse as well</param>
        /// <returns>An auto-reversing version of the easing function</returns>
        public static Easing AndReverse(this Easing easing) {
            return p => p <= 0.5F ? easing(p * 2) : easing(1 - (p - 0.5F) * 2);
        }

        /// <summary>
        /// Causes the easing function to play fully, followed by <paramref name="other"/> playing fully, in the span between an input of 0 and 1.
        /// Note that <see cref="AndThen(MLEM.Misc.Easings.Easing,MLEM.Misc.Easings.Easing[])"/> provides a version of this method for any amount of follow-up easing functions.
        /// </summary>
        /// <param name="easing">The first easing function to play.</param>
        /// <param name="other">The second easing function to play.</param>
        /// <returns>A combined easing function of the two functions passed.</returns>
        public static Easing AndThen(this Easing easing, Easing other) {
            return p => p <= 0.5F ? easing(p * 2) : other((p - 0.5F) * 2);
        }

        /// <summary>
        /// Causes the easing function to play fully, followed by all elements of <paramref name="others"/> playing fully, in the span between an input of 0 and 1.
        /// This is an any-amount version of <see cref="AndThen(MLEM.Misc.Easings.Easing,MLEM.Misc.Easings.Easing)"/>.
        /// </summary>
        /// <param name="easing">The first easing function to play.</param>
        /// <param name="others">The next easing functions to play.</param>
        /// <returns>A combined easing function of all of the functions passed.</returns>
        public static Easing AndThen(this Easing easing, params Easing[] others) {
            var interval = 1F / (others.Length + 1);
            return p => {
                if (p <= interval)
                    return easing(p / interval);
                var index = (int) ((p - interval) * (others.Length + 1));
                return others[index]((p - (index + 1) * interval) / interval);
            };
        }

        /// <summary>
        /// Causes output from the easing function to be clamped between the <paramref name="min"/> and <paramref name="max"/> values passed.
        /// </summary>
        /// <param name="easing">The easing function to clamp.</param>
        /// <param name="min">The minimum output value to clamp to, defaults to 0.</param>
        /// <param name="max">The maximum output value to clamp to, defaults to 1.</param>
        /// <returns>A clamped easing function.</returns>
        public static Easing Clamp(this Easing easing, float min = 0, float max = 1) {
            return p => MathHelper.Clamp(easing(p), min, max);
        }

        /// <summary>
        /// A delegate method used by <see cref="Easings"/>.
        /// </summary>
        /// <param name="percentage">The percentage into the easing function. Either between 0 and 1, or, if <see cref="Easings.ScaleInput"/> was used, between an arbitary set of values.</param>
        public delegate float Easing(float percentage);

    }
}
