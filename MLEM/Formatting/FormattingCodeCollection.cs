using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MLEM.Formatting {
    public class FormattingCodeCollection : Dictionary<int, FormattingCode> {

        public void Update(GameTime time) {
            foreach (var code in this.Values)
                code.Update(time);
        }

        public void Reset() {
            foreach (var code in this.Values)
                code.Reset();
        }

    }
}