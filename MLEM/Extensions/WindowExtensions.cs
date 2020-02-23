using System;
using System.Linq;
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
            if (TextInput == null)
                return false;
            // The newest version of DesktopGL.Core made a change where TextInputEventArgs doesn't extend EventArgs
            // anymore, making this reflection system incompatible with it. For now, this just disables text input
            // meaning that MLEM.Ui text boxes won't work, but at least it won't crash either.
            // Let's hope there'll be one last update to DesktopGL that also introduces this change so we can fix this.
            if (!typeof(EventArgs).IsAssignableFrom(TextInput.EventHandlerType.GenericTypeArguments.FirstOrDefault()))
                return false;
            return true;
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
                if (!SupportsTextInput())
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