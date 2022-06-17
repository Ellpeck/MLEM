using Microsoft.Xna.Framework;
using MLEM.Misc;
using NUnit.Framework;

namespace Tests {
    public class DirectionTests {

        [Test]
        public void TestDirections() {
            Assert.AreEqual(new Vector2(0.5F, 0.5F).ToDirection(), Direction2.DownRight);
            Assert.AreEqual(new Vector2(0.25F, 0.5F).ToDirection(), Direction2.DownRight);
            Assert.AreEqual(new Vector2(0.15F, 0.5F).ToDirection(), Direction2.Down);
        }

        [Test]
        public void Test90Directions() {
            Assert.AreEqual(new Vector2(0.75F, 0.5F).To90Direction(), Direction2.Right);
            Assert.AreEqual(new Vector2(0.5F, 0.5F).To90Direction(), Direction2.Down);
            Assert.AreEqual(new Vector2(0.25F, 0.5F).To90Direction(), Direction2.Down);
        }

        [Test]
        public void TestRotations() {
            // rotate cw
            Assert.AreEqual(Direction2.Up.RotateCw(), Direction2.Right);
            Assert.AreEqual(Direction2.Up.RotateCw(true), Direction2.UpRight);
            Assert.AreEqual(Direction2.Left.RotateCw(), Direction2.Up);
            Assert.AreEqual(Direction2.UpLeft.RotateCw(), Direction2.UpRight);

            // rotate ccw
            Assert.AreEqual(Direction2.Up.RotateCcw(), Direction2.Left);
            Assert.AreEqual(Direction2.Up.RotateCcw(true), Direction2.UpLeft);
            Assert.AreEqual(Direction2.Left.RotateCcw(), Direction2.Down);
            Assert.AreEqual(Direction2.UpLeft.RotateCcw(), Direction2.DownLeft);

            // rotate 360 degrees
            foreach (var dir in Direction2Helper.AllExceptNone) {
                Assert.AreEqual(DirectionTests.RotateMultipleTimes(dir, true, false, 4), dir);
                Assert.AreEqual(DirectionTests.RotateMultipleTimes(dir, true, true, 8), dir);
                Assert.AreEqual(DirectionTests.RotateMultipleTimes(dir, false, false, 4), dir);
                Assert.AreEqual(DirectionTests.RotateMultipleTimes(dir, false, true, 8), dir);
            }

            // rotate by with start Up
            Assert.AreEqual(Direction2.Right.RotateBy(Direction2.Right), Direction2.Down);
            Assert.AreEqual(Direction2.Right.RotateBy(Direction2.Down), Direction2.Left);
            Assert.AreEqual(Direction2.Right.RotateBy(Direction2.Left), Direction2.Up);
            Assert.AreEqual(Direction2.Right.RotateBy(Direction2.Up), Direction2.Right);

            // rotate by with start Left
            Assert.AreEqual(Direction2.Up.RotateBy(Direction2.Right, Direction2.Left), Direction2.Down);
            Assert.AreEqual(Direction2.Up.RotateBy(Direction2.Down, Direction2.Left), Direction2.Left);
            Assert.AreEqual(Direction2.Up.RotateBy(Direction2.Left, Direction2.Left), Direction2.Up);
            Assert.AreEqual(Direction2.Up.RotateBy(Direction2.Up, Direction2.Left), Direction2.Right);
        }

        private static Direction2 RotateMultipleTimes(Direction2 dir, bool clockwise, bool fortyFiveDegrees, int times) {
            for (var i = 0; i < times; i++)
                dir = clockwise ? dir.RotateCw(fortyFiveDegrees) : dir.RotateCcw(fortyFiveDegrees);
            return dir;
        }

    }
}
