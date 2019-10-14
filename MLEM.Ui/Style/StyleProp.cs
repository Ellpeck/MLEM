namespace MLEM.Ui.Style {
    public struct StyleProp<T> {

        public T Value { get; private set; }
        private bool isCustom;

        public void SetFromStyle(T value) {
            if (!this.isCustom) {
                this.Value = value;
            }
        }

        public void Set(T value) {
            this.isCustom = true;
            this.Value = value;
        }

        public static implicit operator T(StyleProp<T> prop) {
            return prop.Value;
        }

    }
}