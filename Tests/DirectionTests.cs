using Microsoft.Xna.Framework;
using MLEM.Misc;
using NUnit.Framework;
using static MLEM.Misc.Direction2;

namespace Tests {
    public class DirectionTests {

        [Test]
        public void TestDirections() {
            Assert.AreEqual(new Vector2(0.5F, 0.5F).ToDirection(), DownRight);
            Assert.AreEqual(new Vector2(0.25F, 0.5F).ToDirection(), DownRight);
            Assert.AreEqual(new Vector2(0.15F, 0.5F).ToDirection(), Down);
        }

        [Test]
        public void Test90Directions() {
            Assert.AreEqual(new Vector2(0.75F, 0.5F).To90Direction(), Right);
            Assert.AreEqual(new Vector2(0.5F, 0.5F).To90Direction(), Down);
            Assert.AreEqual(new Vector2(0.25F, 0.5F).To90Direction(), Down);
        }

        [Test]
        public void TestRotations() {
            // rotate cw
            Assert.AreEqual(Up.RotateCw(), Right);
            Assert.AreEqual(Up.RotateCw(true), UpRight);
            Assert.AreEqual(Left.RotateCw(), Up);
            Assert.AreEqual(UpLeft.RotateCw(), UpRight);

            // rotate ccw
            Assert.AreEqual(Up.RotateCcw(), Left);
            Assert.AreEqual(Up.RotateCcw(true), UpLeft);
            Assert.AreEqual(Left.RotateCcw(), Down);
            Assert.AreEqual(UpLeft.RotateCcw(), DownLeft);

            // rotate 360 degrees
            foreach (var dir in Direction2Helper.AllExceptNone) {
                Assert.AreEqual(RotateMultipleTimes(dir, true, false, 4), dir);
                Assert.AreEqual(RotateMultipleTimes(dir, true, true, 8), dir);
                Assert.AreEqual(RotateMultipleTimes(dir, false, false, 4), dir);
                Assert.AreEqual(RotateMultipleTimes(dir, false, true, 8), dir);
            }

            // rotate by with start Up
            Assert.AreEqual(Right.RotateBy(Right), Down);
            Assert.AreEqual(Right.RotateBy(Down), Left);
            Assert.AreEqual(Right.RotateBy(Left), Up);
            Assert.AreEqual(Right.RotateBy(Up), Right);

            // rotate by with start Left
            Assert.AreEqual(Up.RotateBy(Right, Left), Down);
            Assert.AreEqual(Up.RotateBy(Down, Left), Left);
            Assert.AreEqual(Up.RotateBy(Left, Left), Up);
            Assert.AreEqual(Up.RotateBy(Up, Left), Right);
        }

        private static Direction2 RotateMultipleTimes(Direction2 dir, bool clockwise, bool fortyFiveDegrees, int times) {
            for (var i = 0; i < times; i++)
                dir = clockwise ? dir.RotateCw(fortyFiveDegrees) : dir.RotateCcw(fortyFiveDegrees);
            return dir;
        }

    }
}