using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace Sandbox {
    internal static class Program {

        private static void Main() {
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            using var game = new GameImpl();
            game.Run();
        }

    }
}