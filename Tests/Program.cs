using System;

namespace Tests {
    public static class Program {

        public static void Main() {
            using (var game = new GameImpl())
                game.Run();
        }

    }
}