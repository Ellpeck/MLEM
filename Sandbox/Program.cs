using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace Sandbox {
    internal static class Program {

        private static void Main() {
            //TextInputWrapper.Current = new TextInputWrapper.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            using (var game = new GameImpl())
                game.Run();
        }

    }
}