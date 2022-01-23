using System;
using System.IO;
using System.Numerics;
using Microsoft.Xna.Framework;
using MLEM.Data;
using MLEM.Data.Json;
using MLEM.Misc;
using Newtonsoft.Json;
using NUnit.Framework;
using static MLEM.Data.DynamicEnum;
using Vector2 = Microsoft.Xna.Framework.Vector2;

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
        public void TestDynamicEnum() {
            var flags = new TestEnum[100];
            for (var i = 0; i < flags.Length; i++)
                flags[i] = AddFlag<TestEnum>("Flag" + i);

            Assert.AreEqual(GetValue(flags[7]), BigInteger.One << 7);
            Assert.AreEqual(GetEnumValue<TestEnum>(BigInteger.One << 75), flags[75]);

            Assert.AreEqual(GetValue(Or(flags[2], flags[17])), (BigInteger.One << 2) | (BigInteger.One << 17));
            Assert.AreEqual(GetValue(And(flags[2], flags[3])), BigInteger.Zero);
            Assert.AreEqual(And(Or(flags[24], flags[52]), Or(flags[52], flags[75])), flags[52]);
            Assert.AreEqual(Xor(Or(flags[85], flags[73]), flags[73]), flags[85]);
            Assert.AreEqual(Xor(Or(flags[85], Or(flags[73], flags[12])), flags[73]), Or(flags[85], flags[12]));
            Assert.AreEqual(GetValue(Neg(flags[74])), ~(BigInteger.One << 74));

            Assert.AreEqual(Or(flags[24], flags[52]).HasFlag(flags[24]), true);
            Assert.AreEqual(Or(flags[24], flags[52]).HasAnyFlag(flags[24]), true);
            Assert.AreEqual(Or(flags[24], flags[52]).HasFlag(Or(flags[24], flags[26])), false);
            Assert.AreEqual(Or(flags[24], flags[52]).HasAnyFlag(Or(flags[24], flags[26])), true);

            Assert.AreEqual(Parse<TestEnum>("Flag24"), flags[24]);
            Assert.AreEqual(Parse<TestEnum>("Flag24 | Flag43"), Or(flags[24], flags[43]));
            Assert.AreEqual(flags[24].ToString(), "Flag24");
            Assert.AreEqual(Or(flags[24], flags[43]).ToString(), "Flag24 | Flag43");
        }

        [Test]
        public void TestJsonTypeSafety() {
            var serializer = new JsonSerializer {TypeNameHandling = TypeNameHandling.Auto};

            // normal generic data holder should crush the time span down to a string due to its custom serializer
            var data = new GenericDataHolder();
            data.SetData("Time", TimeSpan.FromMinutes(5));
            var read = SerializeAndDeserialize(serializer, data);
            Assert.IsNotInstanceOf<TimeSpan>(read.GetData<object>("Time"));
            Assert.Throws<InvalidCastException>(() => read.GetData<TimeSpan>("Time"));

            // json type safe generic data holder should wrap the time span to ensure that it stays a time span
            var safeData = new JsonTypeSafeGenericDataHolder();
            safeData.SetData("Time", TimeSpan.FromMinutes(5));
            var safeRead = SerializeAndDeserialize(serializer, safeData);
            Assert.IsInstanceOf<TimeSpan>(safeRead.GetData<object>("Time"));
            Assert.DoesNotThrow(() => safeRead.GetData<TimeSpan>("Time"));
        }

        private static T SerializeAndDeserialize<T>(JsonSerializer serializer, T t) {
            var writer = new StringWriter();
            serializer.Serialize(writer, t);
            return serializer.Deserialize<T>(new JsonTextReader(new StringReader(writer.ToString())));
        }

        private class TestObject {

            public Vector2 Vec;
            public Point Point;
            public Direction2 Dir { get; set; }
            public TestObject OtherTest;

            public TestObject(Vector2 test, string test2) {}

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

        private class TestEnum : DynamicEnum {

            public TestEnum(string name, BigInteger value) : base(name, value) {}

        }

    }
}