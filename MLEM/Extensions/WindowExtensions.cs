using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Extensions {
    public static class WindowExtensions {

        private static readonly EventInfo TextInput = typeof(GameWindow).GetEvent("TextInput");

        public static bool AddTextInputListener(this GameWindow window, TextInputCallback callback) {
            return new TextInputReflector(callback).AddToWindow(window);
        }

        public static bool SupportsTextInput() {
            return TextInput != null;
        }

        public delegate void TextInputCallback(object sender, Keys key, char character);

        private class TextInputReflector {

            private static readonly MethodInfo Callback = typeof(TextInputReflector).GetMethod(nameof(OnTextInput), BindingFlags.Instance | BindingFlags.NonPublic);
            private static PropertyInfo key;
            private static PropertyInfo character;
            private readonly TextInputCallback callback;

            public TextInputReflector(TextInputCallback callback) {
                this.callback = callback;
            }

            public bool AddToWindow(GameWindow window) {
                if (TextInput == null)
                    return false;
                TextInput.AddEventHandler(window, Delegate.CreateDelegate(TextInput.EventHandlerType, this, Callback));
                return true;
            }

            private void OnTextInput(object sender, object args) {
                if (key == null)
                    key = args.GetType().GetProperty("Key");
                if (character == null)
                    character = args.GetType().GetProperty("Character");
                this.callback.Invoke(sender, (Keys) key.GetValue(args), (char) character.GetValue(args));
            }

        }

    }
}