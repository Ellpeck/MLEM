using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using MLEM.Data;
using MLEM.Data.Json;
using MLEM.Misc;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Tests {
    public class DataTests {

        private readonly TestObject testObject = new(Vector2.One, "test") {
            Vec = new Vector2(10, 20),
            Point = new Point(20, 30),
            Dir = Direction2.Left,
            OtherTest = new TestObject(Vector2.One, "other") {
                Vec = new Vector2(70, 30),
                Dir = Direction2.Right
            }
        };

        [Test]
        public void TestJsonSerializers() {
            var serializer = JsonConverters.AddAll(new JsonSerializer());

            var writer = new StringWriter();
            serializer.Serialize(writer, this.testObject);
            var ret = writer.ToString();

            Assert.AreEqual(ret, "{\"Vec\":\"10 20\",\"Point\":\"20 30\",\"OtherTest\":{\"Vec\":\"70 30\",\"Point\":\"0 0\",\"OtherTest\":null,\"Dir\":\"Right\"},\"Dir\":\"Left\"}");

            var read = serializer.Deserialize<TestObject>(new JsonTextReader(new StringReader(ret)));
            Assert.AreEqual(this.testObject, read);
        }

        [Test]
        public void TestCopy() {
            var copy = this.testObject.Copy();
            Assert.AreEqual(this.testObject, copy);
            Assert.AreSame(this.testObject.OtherTest, copy.OtherTest);

            var deepCopy = this.testObject.DeepCopy();
            Assert.AreEqual(this.testObject, deepCopy);
            Assert.AreNotSame(this.testObject.OtherTest, deepCopy.OtherTest);
        }

        [Test]
        public void TestCopySpeed() {
            const int count = 1000000;
            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
                this.testObject.Copy();
            stopwatch.Stop();
            TestContext.WriteLine($"Copy took {stopwatch.Elapsed.TotalMilliseconds / count * 1000000}ns on average");

            stopwatch.Restart();
            for (var i = 0; i < count; i++)
                this.testObject.DeepCopy();
            stopwatch.Stop();
            TestContext.WriteLine($"DeepCopy took {stopwatch.Elapsed.TotalMilliseconds / count * 1000000}ns on average");
        }

        private class TestObject {

            public Vector2 Vec;
            public Point Point;
            public Direction2 Dir { get; set; }
            public TestObject OtherTest;

            public TestObject(Vector2 test, string test2) {
            }

            protected bool Equals(TestObject other) {
                return this.Vec.Equals(other.Vec) && this.Point.Equals(other.Point) && Equals(this.OtherTest, other.OtherTest) && this.Dir == other.Dir;
            }

            public override bool Equals(object obj) {
                return ReferenceEquals(this, obj) || obj is TestObject other && this.Equals(other);
            }

            public override int GetHashCode() {
                return HashCode.Combine(this.Vec, this.Point, this.OtherTest, (int) this.Dir);
            }

        }

    }
}