using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    public class Slider : ScrollBar {

        public Slider(Anchor anchor, Vector2 size, int scrollerSize, float maxValue) :
            base(anchor, size, scrollerSize, maxValue, true) {
            this.CanBeSelected = true;
        }

        public override void Update(GameTime time) {
            base.Update(time);

            if (this.IsSelected) {
                if (this.Input.IsGamepadButtonDown(Buttons.DPadLeft) || this.Input.IsGamepadButtonDown(Buttons.LeftThumbstickLeft)) {
                    this.CurrentValue -= this.StepPerScroll;
                } else if (this.Input.IsGamepadButtonDown(Buttons.DPadRight) || this.Input.IsGamepadButtonDown(Buttons.LeftThumbstickRight)) {
                    this.CurrentValue += this.StepPerScroll;
                }
            }
        }

        public override Element GetGamepadNextElement(Direction2 dir, Element usualNext) {
            if (dir == Direction2.Left || dir == Direction2.Right)
                return null;
            return base.GetGamepadNextElement(dir, usualNext);
        }

    }
}