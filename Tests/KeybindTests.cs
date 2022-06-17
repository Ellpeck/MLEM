using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using NUnit.Framework;

namespace Tests {
    public class KeybindTests {

        [Test]
        public void TestCombinationOrder() {
            Assert.AreEqual(0, new Keybind.Combination(Keys.A).CompareTo(new Keybind.Combination(Keys.B)));

            Assert.AreEqual(1, new Keybind.Combination(Keys.A, Keys.LeftShift).CompareTo(new Keybind.Combination(Keys.B)));
            Assert.AreEqual(1, new Keybind.Combination(Keys.A, Keys.LeftShift, Keys.RightShift).CompareTo(new Keybind.Combination(Keys.B)));
            Assert.AreEqual(1, new Keybind.Combination(Keys.A, Keys.LeftShift, Keys.RightShift).CompareTo(new Keybind.Combination(Keys.B, Keys.LeftShift)));

            Assert.AreEqual(-1, new Keybind.Combination(Keys.A).CompareTo(new Keybind.Combination(Keys.B, Keys.LeftShift)));
            Assert.AreEqual(-1, new Keybind.Combination(Keys.A).CompareTo(new Keybind.Combination(Keys.B, Keys.LeftShift, Keys.RightShift)));
            Assert.AreEqual(-1, new Keybind.Combination(Keys.A, Keys.LeftShift).CompareTo(new Keybind.Combination(Keys.B, Keys.LeftShift, Keys.RightShift)));
        }

        [Test]
        public void TestKeybindOrder() {
            Assert.AreEqual(0, new Keybind(Keys.A).CompareTo(new Keybind(Keys.B)));

            Assert.AreEqual(2, new Keybind(Keys.A, Keys.LeftShift).Add(Keys.B, Keys.RightShift).CompareTo(new Keybind(Keys.B)));
            Assert.AreEqual(2, new Keybind(Keys.A, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C).CompareTo(new Keybind(Keys.B)));
            Assert.AreEqual(3, new Keybind(Keys.A, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl).CompareTo(new Keybind(Keys.B)));
            Assert.AreEqual(0, new Keybind(Keys.A, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl).CompareTo(new Keybind(Keys.B, Keys.LeftAlt)));
            Assert.AreEqual(1, new Keybind(Keys.A, Keys.LeftShift, Keys.RightShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl).CompareTo(new Keybind(Keys.B, Keys.LeftAlt)));

            Assert.AreEqual(-2, new Keybind(Keys.A).CompareTo(new Keybind(Keys.B, Keys.LeftShift).Add(Keys.B, Keys.RightShift)));
            Assert.AreEqual(-2, new Keybind(Keys.A).CompareTo(new Keybind(Keys.B, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C)));
            Assert.AreEqual(-3, new Keybind(Keys.A).CompareTo(new Keybind(Keys.B, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl)));
            Assert.AreEqual(0, new Keybind(Keys.A, Keys.LeftAlt).CompareTo(new Keybind(Keys.B, Keys.LeftShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl)));
            Assert.AreEqual(-1, new Keybind(Keys.A, Keys.LeftAlt).CompareTo(new Keybind(Keys.B, Keys.LeftShift, Keys.RightShift).Add(Keys.B, Keys.RightShift).Add(Keys.C, Keys.RightControl)));
        }

    }
}
