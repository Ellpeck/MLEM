using Coroutine;

namespace MLEM.Startup {
    /// <summary>
    /// This class contains a set of events for the coroutine system that are automatically fired in <see cref="MlemGame"/>.
    /// </summary>
    public static class CoroutineEvents {

        /// <summary>
        /// This event is fired in <see cref="MlemGame.Update"/>
        /// </summary>
        public static readonly Event Update = new Event();
        /// <summary>
        /// This event is fired in <see cref="MlemGame.Draw"/>
        /// </summary>
        public static readonly Event Draw = new Event();

    }
}