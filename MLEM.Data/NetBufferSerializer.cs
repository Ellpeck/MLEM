using System;
using System.Collections.Generic;
using System.Reflection;
using Lidgren.Network;
using Newtonsoft.Json;

namespace MLEM.Data {
    /// <summary>
    /// A net buffer serializer allows easily writing generic objects into a Lidgren.Network <see cref="NetBuffer"/>.
    /// It can be used both for serialization of outgoing packets, and deserialization of incoming packets.
    /// Before serializing and deserializing an object, each of the object's fields has to have a handler. New handlers can be added using <see cref="AddHandler{T}(System.Action{Lidgren.Network.NetBuffer,T},System.Func{Lidgren.Network.NetBuffer,T})"/> or <see cref="AddHandler{T}(Newtonsoft.Json.JsonSerializer)"/>.
    /// </summary>
    [Obsolete("Lidgren.Network support is deprecated. Consider using LiteNetLib or a custom implementation instead.")]
#if NET6_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessage("Aot", "IL2070")]
#endif
    public class NetBufferSerializer {

        private readonly Dictionary<Type, Action<NetBuffer, object>> writeFunctions = new Dictionary<Type, Action<NetBuffer, object>>();
        private readonly Dictionary<Type, Func<NetBuffer, object>> readFunctions = new Dictionary<Type, Func<NetBuffer, object>>();
        private readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();

        /// <summary>
        /// Create a new net buffer serializer with some default serialization and deserialization implementations for various types.
        /// </summary>
        public NetBufferSerializer() {
            foreach (var method in typeof(NetBuffer).GetMethods(BindingFlags.Instance | BindingFlags.Public)) {
                if (method.GetParameters().Length == 0 && method.Name.StartsWith("Read", StringComparison.Ordinal) && method.Name.Substring(4) == method.ReturnType.Name)
                    this.readFunctions[method.ReturnType] = buffer => method.Invoke(buffer, null);
            }
            foreach (var method in typeof(NetBuffer).GetMethods(BindingFlags.Instance | BindingFlags.Public)) {
                if (method.Name.Equals("Write", StringComparison.InvariantCulture)) {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1)
                        this.writeFunctions[parameters[0].ParameterType] = (buffer, o) => method.Invoke(buffer, new[] {o});
                }
            }
            this.AddHandler((buffer, o) => buffer.Write(o), buffer => buffer.ReadVector2());
            this.AddHandler((buffer, o) => buffer.Write(o), buffer => buffer.ReadGuid());
            this.AddHandler((buffer, o) => buffer.Write(o), buffer => buffer.ReadDirection());
        }

        /// <summary>
        /// Serializes the given object into the given net buffer.
        /// Note that each field in the object has to have a handler (<see cref="AddHandler{T}(System.Action{Lidgren.Network.NetBuffer,T},System.Func{Lidgren.Network.NetBuffer,T})"/>)
        /// </summary>
        /// <param name="buffer">The buffer to serialize into</param>
        /// <param name="o">The object to serialize</param>
        /// <param name="flags">The binding flags to search for fields in the object by</param>
        /// <exception cref="ArgumentException">If any of the object's fields has no writer</exception>
        public void Serialize(NetBuffer buffer, object o, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) {
            foreach (var field in this.GetFields(o.GetType(), flags)) {
                if (!this.writeFunctions.TryGetValue(field.FieldType, out var func))
                    throw new ArgumentException($"The type {field.FieldType} doesn't have a writer");
                func(buffer, field.GetValue(o));
            }
        }

        /// <summary>
        /// Deserializes the net buffer's content into the given object.
        /// If this is used for packet serialization, a new instance of the required type has to be created before this method is called.
        /// </summary>
        /// <param name="buffer">The buffer to read the data from</param>
        /// <param name="o">The object to serialize into</param>
        /// <param name="flags">The binding flags to search for fields in the object by</param>
        /// <exception cref="ArgumentException">If any of the object's fields has no reader</exception>
        public void Deserialize(NetBuffer buffer, object o, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) {
            foreach (var field in this.GetFields(o.GetType(), flags)) {
                if (!this.readFunctions.TryGetValue(field.FieldType, out var func))
                    throw new ArgumentException($"The type {field.FieldType} doesn't have a reader");
                field.SetValue(o, func(buffer));
            }
        }

        private IEnumerable<FieldInfo> GetFields(Type type, BindingFlags flags) {
            if (!this.fieldCache.TryGetValue(type, out var fields)) {
                fields = type.GetFields(flags);
                Array.Sort(fields, (f1, f2) => string.Compare(f1.Name, f2.Name, StringComparison.Ordinal));
                this.fieldCache.Add(type, fields);
            }
            return fields;
        }

        /// <summary>
        /// Adds a manually created deserialization and serialization handler to this net buffer serializer.
        /// </summary>
        /// <param name="write">The function to write the given object into the net buffer</param>
        /// <param name="read">The function to read the given object out of the net buffer</param>
        /// <typeparam name="T">The type that will be serialized and deserialized</typeparam>
        public void AddHandler<T>(Action<NetBuffer, T> write, Func<NetBuffer, T> read) {
            this.writeFunctions.Add(typeof(T), (buffer, o) => write(buffer, (T) o));
            this.readFunctions.Add(typeof(T), buffer => read(buffer));
        }

        /// <summary>
        /// Adds a JSON-based deserialization and serialization handler to this net buffer serializer.
        /// Objects that are serialized in this way are converted to JSON, and the resulting JSON is compressed.
        /// </summary>
        /// <param name="serializer">The JSON serializer to use</param>
        /// <typeparam name="T">The type that will be serialized and deserialized</typeparam>
        public void AddHandler<T>(JsonSerializer serializer) {
            this.AddHandler((buffer, o) => buffer.WriteObject(o, serializer), buffer => buffer.ReadObject<T>(serializer));
        }

    }
}
