using System.Text.RegularExpressions;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class ResetFormattingCode : Code {

        /// <inheritdoc />
        public ResetFormattingCode(Match match, Regex regex) : base(match, regex) {}

        /// <inheritdoc />
        public override bool EndsHere(Code other) {
            return true;
        }

    }
}