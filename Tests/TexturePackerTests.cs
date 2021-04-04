using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Extensions;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests {
    public class TexturePackerTests {

        private Texture2D testTexture;
        private TestGame game;

        [SetUp]
        public void SetUp() {
            this.game = TestGame.Create();
            this.testTexture = new Texture2D(this.game.GraphicsDevice, 256, 256);
        }

        [TearDown]
        public void TearDown() {
            this.game?.Dispose();
            this.testTexture?.Dispose();
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
        public void TestBounds() {
            // test forced max width
            using var packer = new RuntimeTexturePacker(128);
            Assert.Throws<InvalidOperationException>(() => {
                packer.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), StubResult);
            });

            // test auto-expanding width
            using var packer2 = new RuntimeTexturePacker(128, true);
            Assert.DoesNotThrow(() => {
                packer2.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), StubResult);
            });
            packer2.Pack(this.game.GraphicsDevice);

            // test power of two forcing
            using var packer3 = new RuntimeTexturePacker(128, forcePowerOfTwo: true);
            packer3.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), StubResult);
            packer3.Pack(this.game.GraphicsDevice);
            Assert.AreEqual(64, packer3.PackedTexture.Width);
            Assert.AreEqual(256, packer3.PackedTexture.Height);

            // test square forcing
            using var packer4 = new RuntimeTexturePacker(128, forceSquare: true);
            packer4.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), StubResult);
            packer4.Pack(this.game.GraphicsDevice);
            Assert.AreEqual(170, packer4.PackedTexture.Width);
            Assert.AreEqual(170, packer4.PackedTexture.Height);
        }

        private static void StubResult(TextureRegion region) {
        }

    }
}