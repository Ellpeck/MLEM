using System;
using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    /// <inheritdoc />
    [Obsolete("This class has been moved to MLEM.Sound.SoundEffectInstanceHandler in 5.1.0")]
    public class SoundEffectInstanceHandler : Sound.SoundEffectInstanceHandler {

        /// <inheritdoc />
        public SoundEffectInstanceHandler(Game game) : base(game) {
        }

    }
}