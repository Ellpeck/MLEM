using System.Collections.Generic;

namespace MLEM.Misc {
    public class GenericDataHolder {

        private Dictionary<string, object> data;

        public void SetData(string key, object data) {
            if (this.data == null)
                this.data = new Dictionary<string, object>();
            if (data == default) {
                this.data.Remove(key);
            } else {
                this.data[key] = data;
            }
        }

        public T GetData<T>(string key) {
            if (this.data != null && this.data.TryGetValue(key, out var val) && val is T t)
                return t;
            return default;
        }

        public IReadOnlyCollection<string> GetDataKeys() {
            return this.data.Keys;
        }

    }
}