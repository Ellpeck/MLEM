using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;

namespace MLEM.Misc {
    /// <summary>
    /// A text input wrapper is a wrapper around MonoGame's built-in text input event.
    /// Since said text input event does not exist on non-Desktop devices, we want to wrap it in a wrapper that is platform-independent for MLEM.
    /// See subclasses of this wrapper or <see href="https://mlem.ellpeck.de/articles/ui.html#text-input"/> for more info.
    /// </summary>
    public abstract class TextInputWrapper {

        /// <summary>
        /// The current text input wrapper.
        /// Set this value before starting your game if you want to use text input wrapping.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public static TextInputWrapper Current;

        /// <summary>
        /// Ensures that <see cref="Current"/> is set to a valid <see cref="TextInputWrapper"/> value by throwing an <see cref="InvalidOperationException"/> exception if <see cref="Current"/> is null.
        /// </summary>
        /// <exception cref="InvalidOperationException">If <see cref="Current"/> is null</exception>
        public static void EnsureExists() {
            if (Current == null)
                throw new InvalidOperationException("The TextInputWrapper was not initialized. For more information, see https://mlem.ellpeck.de/articles/ui.html#text-input");
        }

        /// <summary>
        /// Determines if this text input wrapper requires an on-screen keyboard.
        /// </summary>
        /// <returns>If this text input wrapper requires an on-screen keyboard</returns>
        public abstract bool RequiresOnScreenKeyboard();

        /// <summary>
        /// Adds a text input listener to this text input wrapper.
        /// The supplied listener will be called whenever a character is input.
        /// </summary>
        /// <param name="window">The game's window</param>
        /// <param name="callback">The callback that should be called whenever a character is pressed</param>
        public abstract void AddListener(GameWindow window, TextInputCallback callback);

        /// <summary>
        /// A delegate method that can be used for <see cref="TextInputWrapper.AddListener"/>
        /// </summary>
        /// <param name="sender">The object that sent the event. The <see cref="TextInputWrapper"/> used in most cases.</param>
        /// <param name="key">The key that was pressed</param>
        /// <param name="character">The character that corresponds to that key</param>
        public delegate void TextInputCallback(object sender, Keys key, char character);

        /// <summary>
        /// A text input wrapper for DesktopGL devices.
        /// This wrapper uses the built-in MonoGame TextInput event, which makes this listener work with any keyboard localization natively.
        /// </summary>
        /// <example>
        /// This listener is initialized as follows:
        /// <code>
        /// new TextInputWrapper.DesktopGl{TextInputEventArgs}((w, c) => w.TextInput += c);
        /// </code>
        /// </example>
        /// <typeparam name="T"></typeparam>
        public class DesktopGl<T> : TextInputWrapper {

            private FieldInfo key;
            private FieldInfo character;
            private readonly Action<GameWindow, EventHandler<T>> addListener;

            /// <summary>
            /// Creates a new DesktopGL-based text input wrapper
            /// </summary>
            /// <param name="addListener">The function that is used to add a text input listener</param>
            public DesktopGl(Action<GameWindow, EventHandler<T>> addListener) {
                this.addListener = addListener;
            }

            /// <inheritdoc />
            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            /// <inheritdoc />
            public override void AddListener(GameWindow window, TextInputCallback callback) {
                this.addListener(window, (sender, args) => {
                    if (this.key == null)
                        this.key = args.GetType().GetField("Key");
                    if (this.character == null)
                        this.character = args.GetType().GetField("Character");
                    callback.Invoke(sender, (Keys) this.key.GetValue(args), (char) this.character.GetValue(args));
                });
            }

        }

        /// <summary>
        /// A text input wrapper for mobile platforms as well as consoles.
        /// This text input wrapper performs no actions itself, as it signals that an on-screen keyboard is required.
        /// </summary>
        public class Mobile : TextInputWrapper {

            /// <inheritdoc />
            public override bool RequiresOnScreenKeyboard() {
                return true;
            }

            /// <inheritdoc />
            public override void AddListener(GameWindow window, TextInputCallback callback) {
            }

        }

        /// <summary>
        /// A text input wrapper that does nothing.
        /// This can be used if no text input is required for the game.
        /// </summary>
        public class None : TextInputWrapper {

            /// <inheritdoc />
            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            /// <inheritdoc />
            public override void AddListener(GameWindow window, TextInputCallback callback) {
            }

        }

        /// <summary>
        /// A primitive text input wrapper that is locked to the American keyboard localization.
        /// Only use this text input wrapper if <see cref="TextInputWrapper.DesktopGl{T}"/> is unavailable for some reason.
        ///
        /// Note that, when using this text input wrapper, its <see cref="Update"/> method has to be called periodically.
        /// </summary>
        public class Primitive : TextInputWrapper {

            private TextInputCallback callback;

            /// <summary>
            /// Updates this text input wrapper by querying pressed keys and sending corresponding input events.
            /// </summary>
            /// <param name="handler">The input handler to use for text input querying</param>
            public void Update(InputHandler handler) {
                var pressed = handler.KeyboardState.GetPressedKeys().Except(handler.LastKeyboardState.GetPressedKeys());
                var shift = handler.IsModifierKeyDown(ModifierKey.Shift);
                foreach (var key in pressed) {
                    var c = GetChar(key, shift);
                    if (c.HasValue)
                        this.callback?.Invoke(this, key, c.Value);
                }
            }

            /// <inheritdoc />
            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            /// <inheritdoc />
            public override void AddListener(GameWindow window, TextInputCallback callback) {
                this.callback += callback;
            }

            private static char? GetChar(Keys key, bool shift) {
                if (key == Keys.A) return shift ? 'A' : 'a';
                if (key == Keys.B) return shift ? 'B' : 'b';
                if (key == Keys.C) return shift ? 'C' : 'c';
                if (key == Keys.D) return shift ? 'D' : 'd';
                if (key == Keys.E) return shift ? 'E' : 'e';
                if (key == Keys.F) return shift ? 'F' : 'f';
                if (key == Keys.G) return shift ? 'G' : 'g';
                if (key == Keys.H) return shift ? 'H' : 'h';
                if (key == Keys.I) return shift ? 'I' : 'i';
                if (key == Keys.J) return shift ? 'J' : 'j';
                if (key == Keys.K) return shift ? 'K' : 'k';
                if (key == Keys.L) return shift ? 'L' : 'l';
                if (key == Keys.M) return shift ? 'M' : 'm';
                if (key == Keys.N) return shift ? 'N' : 'n';
                if (key == Keys.O) return shift ? 'O' : 'o';
                if (key == Keys.P) return shift ? 'P' : 'p';
                if (key == Keys.Q) return shift ? 'Q' : 'q';
                if (key == Keys.R) return shift ? 'R' : 'r';
                if (key == Keys.S) return shift ? 'S' : 's';
                if (key == Keys.T) return shift ? 'T' : 't';
                if (key == Keys.U) return shift ? 'U' : 'u';
                if (key == Keys.V) return shift ? 'V' : 'v';
                if (key == Keys.W) return shift ? 'W' : 'w';
                if (key == Keys.X) return shift ? 'X' : 'x';
                if (key == Keys.Y) return shift ? 'Y' : 'y';
                if (key == Keys.Z) return shift ? 'Z' : 'z';
                if (key == Keys.D0 && !shift || key == Keys.NumPad0) return '0';
                if (key == Keys.D1 && !shift || key == Keys.NumPad1) return '1';
                if (key == Keys.D2 && !shift || key == Keys.NumPad2) return '2';
                if (key == Keys.D3 && !shift || key == Keys.NumPad3) return '3';
                if (key == Keys.D4 && !shift || key == Keys.NumPad4) return '4';
                if (key == Keys.D5 && !shift || key == Keys.NumPad5) return '5';
                if (key == Keys.D6 && !shift || key == Keys.NumPad6) return '6';
                if (key == Keys.D7 && !shift || key == Keys.NumPad7) return '7';
                if (key == Keys.D8 && !shift || key == Keys.NumPad8) return '8';
                if (key == Keys.D9 && !shift || key == Keys.NumPad9) return '9';
                if (key == Keys.D0 && shift) return ')';
                if (key == Keys.D1 && shift) return '!';
                if (key == Keys.D2 && shift) return '@';
                if (key == Keys.D3 && shift) return '#';
                if (key == Keys.D4 && shift) return '$';
                if (key == Keys.D5 && shift) return '%';
                if (key == Keys.D6 && shift) return '^';
                if (key == Keys.D7 && shift) return '&';
                if (key == Keys.D8 && shift) return '*';
                if (key == Keys.D9 && shift) return '(';
                if (key == Keys.Space) return ' ';
                if (key == Keys.Tab) return '\t';
                if (key == Keys.Add) return '+';
                if (key == Keys.Decimal) return '.';
                if (key == Keys.Divide) return '/';
                if (key == Keys.Multiply) return '*';
                if (key == Keys.OemBackslash) return '\\';
                if (key == Keys.OemComma && !shift) return ',';
                if (key == Keys.OemComma && shift) return '<';
                if (key == Keys.OemOpenBrackets && !shift) return '[';
                if (key == Keys.OemOpenBrackets && shift) return '{';
                if (key == Keys.OemCloseBrackets && !shift) return ']';
                if (key == Keys.OemCloseBrackets && shift) return '}';
                if (key == Keys.OemPeriod && !shift) return '.';
                if (key == Keys.OemPeriod && shift) return '>';
                if (key == Keys.OemPipe && !shift) return '\\';
                if (key == Keys.OemPipe && shift) return '|';
                if (key == Keys.OemPlus && !shift) return '=';
                if (key == Keys.OemPlus && shift) return '+';
                if (key == Keys.OemMinus && !shift) return '-';
                if (key == Keys.OemMinus && shift) return '_';
                if (key == Keys.OemQuestion && !shift) return '/';
                if (key == Keys.OemQuestion && shift) return '?';
                if (key == Keys.OemQuotes && !shift) return '\'';
                if (key == Keys.OemQuotes && shift) return '"';
                if (key == Keys.OemSemicolon && !shift) return ';';
                if (key == Keys.OemSemicolon && shift) return ':';
                if (key == Keys.OemTilde && !shift) return '`';
                if (key == Keys.OemTilde && shift) return '~';
                if (key == Keys.Subtract) return '-';
                return null;
            }

        }

    }
}