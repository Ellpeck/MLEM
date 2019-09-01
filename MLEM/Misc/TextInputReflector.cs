using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Misc {
    public class TextInputReflector {

        private readonly TextInputCallback callback;

        public TextInputReflector(TextInputCallback callback) {
            this.callback = callback;
        }

        public void AddToWindow(GameWindow window) {
            var evt = window.GetType().GetEvent("TextInput");
            var handler = this.GetType().GetMethod(nameof(this.OnTextInput), new[] {typeof(object), typeof(EventArgs)});
            evt.AddEventHandler(window, Delegate.CreateDelegate(evt.EventHandlerType, this, handler));
        }

        public void OnTextInput(object sender, EventArgs args) {
            var type = args.GetType();
            var key = (Keys) type.GetProperty("Key").GetValue(args);
            var character = (char) type.GetProperty("Character").GetValue(args);
            this.callback.Invoke(sender, key, character);
        }

        public delegate void TextInputCallback(object sender, Keys key, char character);

    }
}