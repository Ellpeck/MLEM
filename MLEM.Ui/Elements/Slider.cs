using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A slider element for use inside of a <see cref="UiSystem"/>.
    /// A slider is a horizontal <see cref="ScrollBar"/> whose value can additionally be controlled using the <see cref="UiControls.LeftButtons"/> and <see cref="UiControls.RightButtons"/>.
    /// </summary>
    public class Slider : ScrollBar {

        /// <summary>
        /// Creates a new slider with the given settings
        /// </summary>
        /// <param name="anchor">The slider's anchor</param>
        /// <param name="size">The slider's size</param>
        /// <param name="scrollerSize">The size of the slider's scroller indicator</param>
        /// <param name="maxValue">The slider's maximum value</param>
        public Slider(Anchor anchor, Vector2 size, int scrollerSize, float maxValue) :
            base(anchor, size, scrollerSize, maxValue, true) {
            this.CanBeSelected = true;
            this.GetGamepadNextElement = (dir, next) => {
                if (dir == Direction2.Left || dir == Direction2.Right)
                    return null;
                return next;
            };
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);

            if (this.IsSelected) {
                if (this.Controls.LeftButtons.IsPressed(this.Input, this.Controls.GamepadIndex)) {
                    this.CurrentValue -= this.StepPerScroll;
                } else if (this.Controls.RightButtons.IsPressed(this.Input, this.Controls.GamepadIndex)) {
                    this.CurrentValue += this.StepPerScroll;
                }
            }
        }

    }
}