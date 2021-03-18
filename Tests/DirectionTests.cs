using System.Drawing;
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

    }
}