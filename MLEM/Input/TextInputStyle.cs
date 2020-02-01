using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Input {
    public abstract class TextInputStyle {

        private InputHandler handler;

        public abstract bool RequiresOnScreenKeyboard();

        public abstract void Update();

        public virtual void Initialize(InputHandler handler) {
            this.handler = handler;
        }

        public class DesktopGl : TextInputStyle {

            private static readonly EventInfo TextInput = typeof(GameWindow).GetEvent("TextInput");
            private static readonly MethodInfo Callback = typeof(DesktopGl).GetMethod(nameof(OnTextInput));
            private static PropertyInfo key;
            private static PropertyInfo character;

            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            public override void Update() {
            }

            public override void Initialize(InputHandler handler) {
                base.Initialize(handler);
                if (TextInput != null)
                    TextInput.AddEventHandler(handler.Game.Window, Delegate.CreateDelegate(TextInput.EventHandlerType, this, Callback));
            }

            public void OnTextInput(object sender, EventArgs args) {
                if (key == null)
                    key = args.GetType().GetProperty("Key");
                if (character == null)
                    character = args.GetType().GetProperty("Character");
                this.handler.OnTextInput?.Invoke((Keys) key.GetValue(args), (char) character.GetValue(args));
            }

        }

        public class Mobile : TextInputStyle {

            public override bool RequiresOnScreenKeyboard() {
                return true;
            }

            public override void Update() {
            }

        }

        public class American : TextInputStyle {

            public override bool RequiresOnScreenKeyboard() {
                return false;
            }

            public override void Update() {
                var pressed = this.handler.KeyboardState.GetPressedKeys().Except(this.handler.LastKeyboardState.GetPressedKeys());
                var shift = this.handler.IsModifierKeyDown(ModifierKey.Shift);
                foreach (var key in pressed) {
                    var c = GetChar(key, shift);
                    if (c.HasValue)
                        this.handler.OnTextInput?.Invoke(key, c.Value);
                }
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