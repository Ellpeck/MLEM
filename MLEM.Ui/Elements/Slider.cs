using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    public class Slider : ScrollBar {

        public Slider(Anchor anchor, Vector2 size, int scrollerSize, float maxValue) :
            base(anchor, size, scrollerSize, maxValue, true) {
            this.CanBeSelected = true;
            this.GetGamepadNextElement = (dir, next) => {
                if (dir == Direction2.Left || dir == Direction2.Right)
                    return null;
                return next;
            };
        }

        public override void Update(GameTime time) {
            base.Update(time);

            if (this.IsSelected) {
                if (this.Controls.LeftButtons.Any(b => this.Input.IsDown(b, this.Controls.GamepadIndex))) {
                    this.CurrentValue -= this.StepPerScroll;
                } else if (this.Controls.RightButtons.Any(b => this.Input.IsDown(b, this.Controls.GamepadIndex))) {
                    this.CurrentValue += this.StepPerScroll;
                }
            }
        }

    }
}