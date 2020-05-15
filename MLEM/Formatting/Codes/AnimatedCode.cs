using System.Text.RegularExpressions;

namespace MLEM.Formatting.Codes {
    public class AnimatedCode : Code {

        public AnimatedCode(Match match) : base(match) {
        }

        public override bool EndsHere(Code other) {
            return other is AnimatedCode;
        }

    }
}