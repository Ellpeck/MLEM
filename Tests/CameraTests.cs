using System.Numerics;
using MLEM.Cameras;
using NUnit.Framework;
using Tests.Stub;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Tests {
    public class CameraTests {

        [Test]
        public void TestConversions([Range(-4, 4, 4F)] float x, [Range(-4, 4, 4F)] float y) {
            var camera = new Camera(new StubGraphics());
            var pos = new Vector2(x, y);
            var cam = camera.ToCameraPos(pos);
            var ret = camera.ToWorldPos(cam);
            Assert.AreEqual(pos, ret);
        }

    }
}