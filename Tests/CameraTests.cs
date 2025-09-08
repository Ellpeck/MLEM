using Microsoft.Xna.Framework;
using MLEM.Cameras;
using NUnit.Framework;

namespace Tests;

public class CameraTests : GameTestFixture {

    [Test]
    public void TestConversions([Range(-4, 4, 4F)] float x, [Range(-4, 4, 4F)] float y) {
        var camera = new Camera(this.Game.GraphicsDevice);
        var pos = new Vector2(x, y);
        var cam = camera.ToCameraPos(pos);
        var ret = camera.ToWorldPos(cam);
        Assert.AreEqual(pos, ret);
    }

}
