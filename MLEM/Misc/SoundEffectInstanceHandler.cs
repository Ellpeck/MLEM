using System;
using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    /// <inheritdoc cref="Sound.SoundEffectInstanceHandler"/>
    [Obsolete("This class has been moved to MLEM.Sound.SoundEffectInstanceHandler in 5.1.0")]
    public class SoundEffectInstanceHandler : Sound.SoundEffectInstanceHandler {

        /// <inheritdoc cref="Sound.SoundEffectInstanceHandler(Game)"/>
        public SoundEffectInstanceHandler(Game game) : base(game) {
        }

    }
}