using System.Text.RegularExpressions;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class AnimatedCode : Code {

        /// <inheritdoc />
        public AnimatedCode(Match match, Regex regex) : base(match, regex) {}

        /// <inheritdoc />
        public override bool EndsHere(Code other) {
            return other is AnimatedCode;
        }

    }
}
