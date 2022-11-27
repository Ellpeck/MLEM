using System;
using System.Collections.Generic;
using MLEM.Misc;
using Newtonsoft.Json;

namespace MLEM.Data.Json {
    /// <summary>
    /// An <see cref="IGenericDataHolder"/> represents an object that can hold generic key-value based data.
    /// This class uses <see cref="JsonTypeSafeWrapper"/> for each object stored to ensure that objects with a custom <see cref="JsonConverter"/> get deserialized as an instance of their original type if <see cref="JsonSerializer.TypeNameHandling"/> is not set to <see cref="TypeNameHandling.None"/>.
    /// Note that, using <see cref="SetData"/>, adding <see cref="JsonTypeSafeWrapper{T}"/> instances directly is also possible.
    /// </summary>
    #if NET7_0_OR_GREATER
    [System.Diagnostics.CodeAnalysis.RequiresDynamicCode("The native code for instantiation of JsonTypeSafeWrapper instances might not be available at runtime.")]
    #endif
    public class JsonTypeSafeGenericDataHolder : IGenericDataHolder {

        private static readonly string[] EmptyStrings = new string[0];

        [JsonProperty]
        private Dictionary<string, JsonTypeSafeWrapper> data;

        /// <inheritdoc />
        [Obsolete("This method will be removed in a future update in favor of the generic SetData<T>.")]
        public void SetData(string key, object data) {
            this.SetData<object>(key, data);
        }

        /// <inheritdoc />
        public void SetData<T>(string key, T data) {
            if (EqualityComparer<T>.Default.Equals(data, default)) {
                if (this.data != null)
                    this.data.Remove(key);
            } else {
                if (this.data == null)
                    this.data = new Dictionary<string, JsonTypeSafeWrapper>();
                // if types already match exactly, we don't need to use Of (which requires dynamic code)
                this.data[key] = data.GetType() == typeof(T) ? new JsonTypeSafeWrapper<T>(data) : JsonTypeSafeWrapper.Of(data);
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
