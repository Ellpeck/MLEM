using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace Demos.DesktopGL {
    public static class Program {

        public static void Main() {
            #if FNA
            MlemPlatform.Current = new MlemPlatform.DesktopFna(a => TextInputEXT.TextInput += a);
            #else
            MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
            #endif
            using var game = new GameImpl();
            game.Run();
        }

    }
}
