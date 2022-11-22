using System.Collections.Generic;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// An <see cref="IGenericDataHolder"/> represents an object that can hold generic key-value based data.
    /// This class uses <see cref="JsonTypeSafeWrapper"/> for each object stored to ensure that objects with a custom <see cref="JsonConverter"/> get deserialized as an instance of their original type if <see cref="JsonSerializer.TypeNameHandling"/> is not set to <see cref="TypeNameHandling.None"/>.
    /// </summary>
    #if NET7_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("The native code for instantiation of JsonTypeSafeWrapper instances might not be available at runtime.")]
    #endif
    public class JsonTypeSafeGenericDataHolder : IGenericDataHolder {

        private static readonly string[] EmptyStrings = new string[0];

        [JsonProperty]
        private Dictionary<string, JsonTypeSafeWrapper> data;

        /// <inheritdoc />
        public void SetData(string key, object data) {
            if (data == default) {
                if (this.data != null)
                    this.data.Remove(key);
            } else {
                if (this.data == null)
                    this.data = new Dictionary<string, JsonTypeSafeWrapper>();
                this.data[key] = JsonTypeSafeWrapper.Of(data);
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
                return JsonTypeSafeGenericDataHolder.EmptyStrings;
            return this.data.Keys;
        }

    }
}
