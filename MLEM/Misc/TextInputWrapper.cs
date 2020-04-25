using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;

namespace MLEM.Misc {
    public abstract class TextInputWrapper {

        private static TextInputWrapper current;
        public static TextInputWrapper Current {
            get {
                if (current == null)
                    throw new InvalidOperationException("The TextInputWrapper was not initialized. For more information, see https://github.com/Ellpeck/MLEM/wiki/MLEM.Ui#text-input");
                return current;
            }
            set => current = value;
        }

        public abstract bool RequiresOnScreenKeyboard();

        public abstract void AddListener(GameWindow window, TextInputCallback callback);

        public delegate void TextInputCallback(object sender, Keys key, char character);

        public class DesktopGl<T> : TextInputWrapper {

            private MemberInfo key;
            private MemberInfo character;
            private readonly Action<GameWindow, EventHandler<T>> addListener;

            public DesktopGl(Action<GameWindow, EventHandler<T>> addListener) {
                this.addListener = addListener;
            }

            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            public override void AddListener(GameWindow window, TextInputCallback callback) {
                this.addListener(window, (sender, args) => {
                    // the old versions of DesktopGL use a property here, while the 
                    // core version uses a field. So much for "no breaking changes"
                    if (this.key == null)
                        this.key = GetMember(args, "Key");
                    if (this.character == null)
                        this.character = GetMember(args, "Character");
                    callback.Invoke(sender, GetValue<Keys>(this.key, args), GetValue<char>(this.character, args));
                });
            }

            private static MemberInfo GetMember(object args, string name) {
                var ret = args.GetType().GetProperty(name);
                if (ret != null)
                    return ret;
                return args.GetType().GetField(name);
            }

            private static U GetValue<U>(MemberInfo member, object args) {
                switch (member) {
                    case PropertyInfo p:
                        return (U) p.GetValue(args);
                    case FieldInfo f:
                        return (U) f.GetValue(args);
                }
                throw new ArgumentException();
            }

        }

        public class Mobile : TextInputWrapper {

            public override bool RequiresOnScreenKeyboard() {
                return true;
            }

            public override void AddListener(GameWindow window, TextInputCallback callback) {
            }

        }

        public class None : TextInputWrapper {

            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            public override void AddListener(GameWindow window, TextInputCallback callback) {
            }

        }

        public class Primitive : TextInputWrapper {

            private TextInputCallback callback;

            public void Update(InputHandler handler) {
                var pressed = handler.KeyboardState.GetPressedKeys().Except(handler.LastKeyboardState.GetPressedKeys());
                var shift = handler.IsModifierKeyDown(ModifierKey.Shift);
                foreach (var key in pressed) {
                    var c = GetChar(key, shift);
                    if (c.HasValue)
                        this.callback?.Invoke(this, key, c.Value);
                }
            }

            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

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