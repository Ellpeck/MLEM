using System.Collections.Generic;

namespace MLEM.Ui.Style {
    public struct StyleProp<T> {

        public T Value { get; private set; }
        private bool isCustom;

        public StyleProp(T value) {
            this.isCustom = true;
            this.Value = value;
        }

        public void SetFromStyle(T value) {
            if (!this.isCustom)
                this.Value = value;
        }

        public void Set(T value) {
            this.isCustom = true;
            this.Value = value;
        }

        public T OrDefault(T def) {
            return EqualityComparer<T>.Default.Equals(this.Value, default(T)) ? def : this.Value;
        }

        public static implicit operator T(StyleProp<T> prop) {
            return prop.Value;
        }

        public static implicit operator StyleProp<T>(T prop) {
            return new StyleProp<T>(prop);
        }

        public static bool operator ==(StyleProp<T> left, StyleProp<T> right) {
            return EqualityComparer<T>.Default.Equals(left.Value, right.Value);
        }

        public static bool operator !=(StyleProp<T> left, StyleProp<T> right) {
            return !(left == right);
        }

    }
}