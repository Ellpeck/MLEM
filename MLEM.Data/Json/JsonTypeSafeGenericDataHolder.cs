using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// An <see cref="IGenericDataHolder"/> represents an object that can hold generic key-value based data.
    /// This class uses <see cref="JsonTypeSafeWrapper"/> for each object stored to ensure that objects with a custom <see cref="JsonConverter"/> get deserialized as an instance of their original type if <see cref="JsonSerializer.TypeNameHandling"/> is not set to <see cref="TypeNameHandling.None"/>.
    /// </summary>
    [DataContract]
    public class JsonTypeSafeGenericDataHolder : IGenericDataHolder {

        [DataMember(EmitDefaultValue = false)]
        private Dictionary<string, JsonTypeSafeWrapper> data;

        /// <inheritdoc />
        public void SetData<T>(string key, T data) {
            if (EqualityComparer<T>.Default.Equals(data, default)) {
                if (this.data != null)
                    this.data.Remove(key);
            } else {
                if (this.data == null)
                    this.data = new Dictionary<string, JsonTypeSafeWrapper>();
                this.data[key] = new JsonTypeSafeWrapper<T>(data);
            }
        }

        /// <inheritdoc />
        public T GetData<T>(string key) {
            if (this.data != null && this.data.TryGetValue(key, out var val))
                return (T) val.Value;
            return default;
        }

        /// <inheritdoc />
        public IEnumerable<string> GetDataKeys() {
            if (this.data == null)
                return Array.Empty<string>();
            return this.data.Keys;
        }

    }
}