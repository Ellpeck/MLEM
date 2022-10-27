using System;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests; 

public class TexturePackerTests {

    private Texture2D testTexture;
    private Texture2D disposedTestTexture;
    private TestGame game;

    [SetUp]
    public void SetUp() {
        this.game = TestGame.Create();
        this.testTexture = new Texture2D(this.game.GraphicsDevice, 256, 256);
        this.disposedTestTexture = new Texture2D(this.game.GraphicsDevice, 16, 16);
    }

    [TearDown]
    public void TearDown() {
        this.game?.Dispose();
        this.testTexture?.Dispose();
        this.disposedTestTexture?.Dispose();
    }

    [Test]
    public void TestPacking() {
        using var packer = new RuntimeTexturePacker();
        for (var i = 0; i < 5; i++) {
            var width = 16 * (i + 1);
            packer.Add(new TextureRegion(this.testTexture, 0, 0, width, 64), r => {
                Assert.AreEqual(r.Width, width);
                Assert.AreEqual(r.Height, 64);
            });
        }
        packer.Pack(this.game.GraphicsDevice);
        Assert.AreEqual(packer.PackedTexture.Width, 16 + 32 + 48 + 64 + 80);
        Assert.AreEqual(packer.PackedTexture.Height, 64);
    }

    [Test]
    public void TestDisposal() {
        using var packer = new RuntimeTexturePacker(128, disposeTextures: true);
        packer.Add(new TextureRegion(this.disposedTestTexture), TexturePackerTests.StubResult);
        packer.Add(new TextureRegion(this.disposedTestTexture, 0, 0, 8, 8), TexturePackerTests.StubResult);
        packer.Pack(this.game.GraphicsDevice);
        Assert.True(this.disposedTestTexture.IsDisposed);
        Assert.False(packer.PackedTexture.IsDisposed);
    }

    [Test]
    public void TestBounds() {
        // test forced max width
        using var packer = new RuntimeTexturePacker(128);
        Assert.Throws<InvalidOperationException>(() => {
            packer.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), TexturePackerTests.StubResult);
        });

        // test auto-expanding width
        using var packer2 = new RuntimeTexturePacker(128, true);
        Assert.DoesNotThrow(() => {
            packer2.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), TexturePackerTests.StubResult);
        });
        packer2.Pack(this.game.GraphicsDevice);

        // test power of two forcing
        using var packer3 = new RuntimeTexturePacker(128, forcePowerOfTwo: true);
        packer3.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), TexturePackerTests.StubResult);
        packer3.Pack(this.game.GraphicsDevice);
        Assert.AreEqual(64, packer3.PackedTexture.Width);
        Assert.AreEqual(256, packer3.PackedTexture.Height);

        // test square forcing
        using var packer4 = new RuntimeTexturePacker(128, forceSquare: true);
        packer4.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), TexturePackerTests.StubResult);
        packer4.Pack(this.game.GraphicsDevice);
        Assert.AreEqual(170, packer4.PackedTexture.Width);
        Assert.AreEqual(170, packer4.PackedTexture.Height);
    }

    [Test]
    public void TestPackMultipleTimes() {
        using var packer = new RuntimeTexturePacker(1024);

        // pack the first time
        var results = 0;
        for (var i = 0; i < 10; i++)
            packer.Add(new TextureRegion(this.testTexture, 0, 0, 64, 64), _ => results++);
        packer.Pack(this.game.GraphicsDevice);
        Assert.AreEqual(10, results);

        // pack without resizing
        packer.Add(new TextureRegion(this.testTexture, 0, 0, 0, 0), _ => results++);
        packer.Pack(this.game.GraphicsDevice);
        Assert.AreEqual(11, results);

        // pack and force a resize
        packer.Add(new TextureRegion(this.testTexture, 0, 0, 64, 64), _ => results++);
        packer.Pack(this.game.GraphicsDevice);
        // all callbacks are called again, so we add 11 again, as well as the callback we just added
        Assert.AreEqual(2 * 11 + 1, results);
    }

    private static void StubResult(TextureRegion region) {}

}
