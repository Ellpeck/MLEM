using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using MLEM.Misc;

namespace MLEM.Input {
    public static class KeysExtensions {

        public static readonly ModifierKey[] ModifierKeys = EnumHelper.GetValues<ModifierKey>().ToArray();

        public static IEnumerable<Keys> GetKeys(this ModifierKey modifier) {
            switch (modifier) {
                case ModifierKey.Shift:
                    yield return Keys.LeftShift;
                    yield return Keys.RightShift;
                    break;
                case ModifierKey.Control:
                    yield return Keys.LeftControl;
                    yield return Keys.RightControl;
                    break;
                case ModifierKey.Alt:
                    yield return Keys.LeftAlt;
                    yield return Keys.RightAlt;
                    break;
            }
        }

        public static ModifierKey GetModifier(this Keys key) {
            foreach (var mod in ModifierKeys) {
                if (GetKeys(mod).Contains(key))
                    return mod;
            }
            return ModifierKey.None;
        }

        public static bool IsModifier(this Keys key) {
            return GetModifier(key) != ModifierKey.None;
        }

    }

    public enum ModifierKey {

        None,
        Shift,
        Control,
        Alt

    }
}