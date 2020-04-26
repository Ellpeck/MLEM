using System;
using System.Collections.Generic;

namespace MLEM.Misc {
    public class GenericDataHolder {

        private Dictionary<string, object> data;

        public void SetData(string key, object data) {
            if (data == default) {
                if (this.data != null)
                    this.data.Remove(key);
            } else {
                if (this.data == null)
                    this.data = new Dictionary<string, object>();
                this.data[key] = data;
            }
        }

        public T GetData<T>(string key) {
            if (this.data != null && this.data.TryGetValue(key, out var val) && val is T t)
                return t;
            return default;
        }

        public IReadOnlyCollection<string> GetDataKeys() {
            if (this.data == null)
                return Array.Empty<string>();
            return this.data.Keys;
        }

    }
}