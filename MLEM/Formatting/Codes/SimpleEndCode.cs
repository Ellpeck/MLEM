using System.Text.RegularExpressions;

namespace MLEM.Formatting.Codes {
    /// <inheritdoc />
    public class SimpleEndCode : Code {

        private readonly Regex codeToEnd;

        /// <inheritdoc />
        public SimpleEndCode(Match match, Regex regex, string codeNameToEnd) : base(match, regex) {
            this.codeToEnd = new Regex($"<{codeNameToEnd}.*>");
        }

        /// <inheritdoc />
        public override bool EndsOther(Code other) {
            return this.codeToEnd.IsMatch(other.Regex.ToString());
        }

    }
}
