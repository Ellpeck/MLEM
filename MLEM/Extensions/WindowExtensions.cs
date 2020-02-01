using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Extensions {
    public static class WindowExtensions {

        private static readonly bool TextInputSupported = typeof(GameWindow).GetEvent("TextInput") != null;

        public static bool AddTextInputListener(this GameWindow window, TextInputCallback callback) {
            if (!SupportsTextInput())
                return false;
            TextInputAdder.Add(window, callback);
            return true;
        }

        public static bool SupportsTextInput() {
            return TextInputSupported;
        }

        public delegate void TextInputCallback(object sender, Keys key, char character);

        private static class TextInputAdder {

            public static void Add(GameWindow window, TextInputCallback callback) {
                window.TextInput += (sender, args) => callback(sender, args.Key, args.Character);
            }

        }

    }
}