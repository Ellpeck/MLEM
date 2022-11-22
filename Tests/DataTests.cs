using System;
using System.IO;
using Microsoft.Xna.Framework;
using MLEM.Data.Json;
using MLEM.Misc;
using Newtonsoft.Json;
using NUnit.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace Tests;

public class DataTests {

    private readonly TestObject testObject = new() {
        Vec = new Vector2(10, 20),
        Point = new Point(20, 30),
        Dir = Direction2.Left,
        OtherTest = new TestObject {
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
    public void TestJsonTypeSafety() {
        var safeData = new JsonTypeSafeGenericDataHolder();
        // data holder should wrap the time span to ensure that it stays a time span
        safeData.SetData("Time", TimeSpan.FromMinutes(5));
        var safeRead = DataTests.SerializeAndDeserialize(safeData);
        Assert.IsInstanceOf<TimeSpan>(safeRead.GetData<object>("Time"));
        Assert.DoesNotThrow(() => safeRead.GetData<TimeSpan>("Time"));
    }

    private static T SerializeAndDeserialize<T>(T t) {
        var serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.Auto};
        var writer = new StringWriter();
        serializer.Serialize(writer, t);
        return serializer.Deserialize<T>(new JsonTextReader(new StringReader(writer.ToString())));
    }

    private class TestObject {

        public Vector2 Vec;
        public Point Point;
        public Direction2 Dir { get; set; }
        public TestObject OtherTest;

        protected bool Equals(TestObject other) {
            return this.Vec.Equals(other.Vec) && this.Point.Equals(other.Point) && object.Equals(this.OtherTest, other.OtherTest) && this.Dir == other.Dir;
        }

        public override bool Equals(object obj) {
            return object.ReferenceEquals(this, obj) || obj is TestObject other && this.Equals(other);
        }

        public override int GetHashCode() {
            return HashCode.Combine(this.Vec, this.Point, this.OtherTest, (int) this.Dir);
        }

    }

}
