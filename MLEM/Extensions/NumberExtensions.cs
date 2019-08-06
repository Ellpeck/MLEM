using System;

namespace MLEM.Extensions {
    public static class NumberExtensions {

        public static int Floor(this float f) {
            return (int) Math.Floor(f);
        }

        public static int Ceil(this float f) {
            return (int) Math.Ceiling(f);
        }


    }
}