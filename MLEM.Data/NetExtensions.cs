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
    /// <summary>
    /// A set of extensions for dealing with <see cref="NetBuffer"/>.
    /// </summary>
    public static class NetExtensions {

        /// <summary>
        /// Writes a <see cref="Vector2"/> to the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="vector">The vector to write</param>
        public static void Write(this NetBuffer buffer, Vector2 vector) {
            buffer.Write(vector.X);
            buffer.Write(vector.Y);
        }

        /// <summary>
        /// Reads a <see cref="Vector2"/> from the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <returns>The read vector</returns>
        public static Vector2 ReadVector2(this NetBuffer buffer) {
            return new Vector2(buffer.ReadFloat(), buffer.ReadFloat());
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> to the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="guid">The guid to write</param>
        public static void Write(this NetBuffer buffer, Guid guid) {
            buffer.Write(guid.ToByteArray());
        }

        /// <summary>
        /// Reads a <see cref="Guid"/> from the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <returns>The read guid</returns>
        public static Guid ReadGuid(this NetBuffer buffer) {
            return new Guid(buffer.ReadBytes(16));
        }

        /// <summary>
        /// Writes a <see cref="Direction2"/> to the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="direction">The direction to write</param>
        public static void Write(this NetBuffer buffer, Direction2 direction) {
            buffer.Write((short) direction);
        }

        /// <summary>
        /// Reads a <see cref="Direction2"/> from the given net buffer
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <returns>The read direction</returns>
        public static Direction2 ReadDirection(this NetBuffer buffer) {
            return (Direction2) buffer.ReadInt16();
        }

        /// <summary>
        /// Writes a generic object to the given net buffer using a <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to write to</param>
        /// <param name="obj">The object to write</param>
        /// <param name="serializer">The JSON serializer to use</param>
        /// <typeparam name="T">The type of object written</typeparam>
        public static void WriteObject<T>(this NetBuffer buffer, T obj, JsonSerializer serializer) {
            if (EqualityComparer<T>.Default.Equals(obj, default)) {
                buffer.Write(0);
                return;
            }
            using (var memory = new MemoryStream()) {
                using (var stream = new DeflateStream(memory, CompressionLevel.Fastest, true))
                    serializer.Serialize(new BsonDataWriter(stream), obj, typeof(T));
                var arr = memory.ToArray();
                buffer.Write(arr.Length);
                buffer.Write(arr);
            }
        }

        /// <summary>
        /// Reads a generic object from the given buffer using a <see cref="JsonSerializer"/>.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="serializer">The JSON serializer to use</param>
        /// <typeparam name="T">The type of object read</typeparam>
        /// <returns>The read object</returns>
        public static T ReadObject<T>(this NetBuffer buffer, JsonSerializer serializer) {
            var length = buffer.ReadInt32();
            if (length <= 0)
                return default;
            var arr = buffer.ReadBytes(length);
            using (var memory = new MemoryStream(arr)) {
                using (var stream = new DeflateStream(memory, CompressionMode.Decompress, true))
                    return serializer.Deserialize<T>(new BsonDataReader(stream));
            }
        }

    }
}