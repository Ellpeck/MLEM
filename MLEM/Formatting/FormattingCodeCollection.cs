using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLEM.Misc;

namespace MLEM.Formatting {
    public class FormattingCodeCollection : Dictionary<int, List<FormattingCodeData>> {

    }

    public class FormattingCodeData : GenericDataHolder {

        public readonly FormattingCode Code;
        public readonly Match Match;
        public readonly int UnformattedIndex;

        public FormattingCodeData(FormattingCode code, Match match, int unformattedIndex) {
            this.Code = code;
            this.Match = match;
            this.UnformattedIndex = unformattedIndex;
        }

    }
}