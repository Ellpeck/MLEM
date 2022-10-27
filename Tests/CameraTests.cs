using Microsoft.Xna.Framework;
using MLEM.Cameras;
using NUnit.Framework;

namespace Tests; 

public class CameraTests {

    private TestGame game;

    [SetUp]
    public void SetUp() {
        this.game = TestGame.Create();
    }

    [TearDown]
    public void TearDown() {
        this.game?.Dispose();
    }

    [Test]
    public void TestConversions([Range(-4, 4, 4F)] float x, [Range(-4, 4, 4F)] float y) {
        var camera = new Camera(this.game.GraphicsDevice);
        var pos = new Vector2(x, y);
        var cam = camera.ToCameraPos(pos);
        var ret = camera.ToWorldPos(cam);
        Assert.AreEqual(pos, ret);
    }

}
