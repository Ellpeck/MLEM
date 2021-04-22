using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace Demos.DesktopGL {
    public static class Program {

        public static void Main() {
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            using var game = new GameImpl();
            game.Run();
        }

    }
}