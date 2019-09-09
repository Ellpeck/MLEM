using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace MLEM.Misc {
    public static class EnumHelper {

        public static readonly Buttons[] Buttons = GetValues<Buttons>().ToArray();
        public static readonly Keys[] Keys = GetValues<Keys>().ToArray();

        public static IEnumerable<T> GetValues<T>() {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

    }
}