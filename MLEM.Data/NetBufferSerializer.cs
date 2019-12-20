using System;
using System.Collections.Generic;
using System.Reflection;
using Lidgren.Network;

namespace MLEM.Data {
    public class NetBufferSerializer {

        private readonly Dictionary<Type, Action<NetBuffer, object>> writeFunctions = new Dictionary<Type, Action<NetBuffer, object>>();
        private readonly Dictionary<Type, Func<NetBuffer, object>> readFunctions = new Dictionary<Type, Func<NetBuffer, object>>();
        private readonly Dictionary<Type, FieldInfo[]> fieldCache = new Dictionary<Type, FieldInfo[]>();

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

        public void Serialize(NetBuffer buffer, object o, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) {
            foreach (var field in this.GetFields(o.GetType(), flags)) {
                if (!this.writeFunctions.TryGetValue(field.FieldType, out var func))
                    throw new ArgumentException($"The type {field.FieldType} doesn't have a writer");
                func(buffer, field.GetValue(o));
            }
        }

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

        public void AddHandler<T>(Action<NetBuffer, T> write, Func<NetBuffer, T> read) {
            this.writeFunctions.Add(typeof(T), (buffer, o) => write(buffer, (T) o));
            this.readFunctions.Add(typeof(T), buffer => read(buffer));
        }

    }
}