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

            private readonly TextInputCallback callback;

            public TextInputReflector(TextInputCallback callback) {
                this.callback = callback;
            }

            public bool AddToWindow(GameWindow window) {
                if (TextInput == null)
                    return false;
                var handler = this.GetType().GetMethod(nameof(this.OnTextInput), new[] {typeof(object), typeof(EventArgs)});
                if (handler == null)
                    return false;
                TextInput.AddEventHandler(window, Delegate.CreateDelegate(TextInput.EventHandlerType, this, handler));
                return true;
            }

            public void OnTextInput(object sender, EventArgs args) {
                var type = args.GetType();
                var key = (Keys) type.GetProperty("Key").GetValue(args);
                var character = (char) type.GetProperty("Character").GetValue(args);
                this.callback.Invoke(sender, key, character);
            }

        }

    }
}