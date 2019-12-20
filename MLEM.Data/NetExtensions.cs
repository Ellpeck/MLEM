using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Lidgren.Network;
using Microsoft.Xna.Framework;
using MLEM.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace MLEM.Data {
    public static class NetExtensions {

        public static void Write(this NetBuffer buffer, Vector2 vector) {
            buffer.Write(vector.X);
            buffer.Write(vector.Y);
        }

        public static Vector2 ReadVector2(this NetBuffer buffer) {
            return new Vector2(buffer.ReadFloat(), buffer.ReadFloat());
        }

        public static void Write(this NetBuffer buffer, Guid guid) {
            buffer.Write(guid.ToByteArray());
        }

        public static Guid ReadGuid(this NetBuffer buffer) {
            return new Guid(buffer.ReadBytes(16));
        }

        public static void Write(this NetBuffer buffer, Direction2 direction) {
            buffer.Write((short) direction);
        }

        public static Direction2 ReadDirection(this NetBuffer buffer) {
            return (Direction2) buffer.ReadInt16();
        }

        public static void WriteObject<T>(this NetBuffer buffer, T obj, JsonSerializer serializer) {
            if (EqualityComparer<T>.Default.Equals(obj, default))
                buffer.Write(0);
            using (var memory = new MemoryStream()) {
                using (var gzip = new DeflateStream(memory, CompressionLevel.Fastest, true))
                    serializer.Serialize(new BsonDataWriter(gzip), obj, typeof(T));
                var arr = memory.ToArray();
                buffer.Write(arr.Length);
                buffer.Write(arr);
            }
        }

        public static T ReadObject<T>(this NetBuffer buffer, JsonSerializer serializer) {
            var length = buffer.ReadInt32();
            if (length <= 0)
                return default;
            var arr = buffer.ReadBytes(length);
            using (var memory = new MemoryStream(arr)) {
                using (var gzip = new DeflateStream(memory, CompressionMode.Decompress, true))
                    return serializer.Deserialize<T>(new BsonDataReader(gzip));
            }
        }

    }
}