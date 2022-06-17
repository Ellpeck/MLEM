using Coroutine;

namespace MLEM.Startup {
    /// <summary>
    /// This class contains a set of events for the coroutine system that are automatically fired in <see cref="MlemGame"/>.
    /// </summary>
    public static class CoroutineEvents {

        /// <summary>
        /// This event is fired in <see cref="MlemGame.Draw"/>, before <see cref="MlemGame.DoDraw"/> is called.
        /// </summary>
        public static readonly Event PreUpdate = new Event();
        /// <summary>
        /// This event is fired in <see cref="MlemGame.Update"/>, after <see cref="MlemGame.DoUpdate"/> is called.
        /// </summary>
        public static readonly Event Update = new Event();
        /// <summary>
        /// This event is fired in <see cref="MlemGame.Draw"/>, before <see cref="MlemGame.DoDraw"/> is called.
        /// </summary>
        public static readonly Event PreDraw = new Event();
        /// <summary>
        /// This event is fired in <see cref="MlemGame.Draw"/>, after <see cref="MlemGame.DoDraw"/> is called.
        /// </summary>
        public static readonly Event Draw = new Event();

    }
}
