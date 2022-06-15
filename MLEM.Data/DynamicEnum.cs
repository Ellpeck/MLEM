using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using MLEM.Data.Json;
using Newtonsoft.Json;

namespace MLEM.Data {
    /// <summary>
    /// A dynamic enum is a class that represents enum-like single-instance value behavior with additional capabilities, including dynamic addition of new arbitrary values.
    /// A dynamic enum uses <see cref="BigInteger"/> as its underlying type, allowing for an arbitrary number of enum values to be created, even when a <see cref="FlagsAttribute"/>-like structure is used that would only allow for up to 64 values in a regular enum.
    /// All enum operations including <see cref="And{T}"/>, <see cref="Or{T}"/>, <see cref="Xor{T}"/> and <see cref="Neg{T}"/> are supported and can be implemented in derived classes using operator overloads.
    /// To create a custom dynamic enum, simply create a class that extends <see cref="DynamicEnum"/>. New values can then be added using <see cref="Add{T}"/>, <see cref="AddValue{T}"/> or <see cref="AddFlag{T}"/>.
    ///
    /// This class, and its entire concept, are extremely terrible. If you intend on using this, there's probably at least one better solution available.
    /// Though if, for some weird reason, you need a way to have more than 64 distinct flags, this is a pretty good solution.
    /// </summary>
    /// <remarks>
    /// To include enum-like operator overloads in a dynamic enum named MyEnum, the following code can be used:
    /// <code>
    /// public static implicit operator BigInteger(MyEnum value) => GetValue(value);
    /// public static implicit operator MyEnum(BigInteger value) => GetEnumValue&lt;MyEnum&gt;(value);
    /// public static MyEnum operator |(MyEnum left, MyEnum right) => Or(left, right);
    /// public static MyEnum operator &amp;(MyEnum left, MyEnum right) => And(left, right);
    /// public static MyEnum operator ^(MyEnum left, MyEnum right) => Xor(left, right);
    /// public static MyEnum operator ~(MyEnum value) => Neg(value);
    /// </code>
    /// </remarks>
    [JsonConverter(typeof(DynamicEnumConverter))]
    public abstract class DynamicEnum {

        private static readonly Dictionary<Type, Storage> Storages = new Dictionary<Type, Storage>();
        private readonly BigInteger value;

        private Dictionary<DynamicEnum, bool> allFlagsCache;
        private Dictionary<DynamicEnum, bool> anyFlagsCache;
        private string name;

        /// <summary>
        /// Creates a new dynamic enum instance.
        /// This constructor is protected as it is only invoked via reflection.
        /// </summary>
        /// <param name="name">The name of the enum value</param>
        /// <param name="value">The value</param>
        protected DynamicEnum(string name, BigInteger value) {
            this.value = value;
            this.name = name;
        }

        /// <summary>
        /// Returns true if this enum value has ALL of the given <see cref="DynamicEnum"/> flags on it.
        /// This operation is equivalent to <see cref="Enum.HasFlag"/>.
        /// </summary>
        /// <seealso cref="HasAnyFlag"/>
        /// <param name="flags">The flags to query</param>
        /// <returns>True if all of the flags are present, false otherwise</returns>
        public bool HasFlag(DynamicEnum flags) {
            if (this.allFlagsCache == null)
                this.allFlagsCache = new Dictionary<DynamicEnum, bool>();
            if (!this.allFlagsCache.TryGetValue(flags, out var ret)) {
                ret = (DynamicEnum.GetValue(this) & DynamicEnum.GetValue(flags)) == DynamicEnum.GetValue(flags);
                this.allFlagsCache.Add(flags, ret);
            }
            return ret;
        }

        /// <summary>
        /// Returns true if this enum value has ANY of the given <see cref="DynamicEnum"/> flags on it
        /// </summary>
        /// <seealso cref="HasFlag"/>
        /// <param name="flags">The flags to query</param>
        /// <returns>True if one of the flags is present, false otherwise</returns>
        public bool HasAnyFlag(DynamicEnum flags) {
            if (this.anyFlagsCache == null)
                this.anyFlagsCache = new Dictionary<DynamicEnum, bool>();
            if (!this.anyFlagsCache.TryGetValue(flags, out var ret)) {
                ret = (DynamicEnum.GetValue(this) & DynamicEnum.GetValue(flags)) != 0;
                this.anyFlagsCache.Add(flags, ret);
            }
            return ret;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString() {
            if (this.name == null) {
                var included = new List<DynamicEnum>();
                if (DynamicEnum.GetValue(this) != 0) {
                    foreach (var v in DynamicEnum.GetValues(this.GetType())) {
                        if (this.HasFlag(v) && DynamicEnum.GetValue(v) != 0)
                            included.Add(v);
                    }
                }
                this.name = included.Count > 0 ? string.Join(" | ", included) : DynamicEnum.GetValue(this).ToString();
            }
            return this.name;
        }

        /// <summary>
        /// Adds a new enum value to the given enum type <typeparamref name="T"/>
        /// </summary>
        /// <param name="name">The name of the enum value to add</param>
        /// <param name="value">The value to add</param>
        /// <typeparam name="T">The type to add this value to</typeparam>
        /// <returns>The newly created enum value</returns>
        /// <exception cref="ArgumentException">Thrown if the name or value passed are already present</exception>
        public static T Add<T>(string name, BigInteger value) where T : DynamicEnum {
            var storage = DynamicEnum.GetStorage(typeof(T));

            // cached parsed values and names might be incomplete with new values
            storage.ClearCaches();

            if (storage.Values.ContainsKey(value))
                throw new ArgumentException($"Duplicate value {value}", nameof(value));
            foreach (var v in storage.Values.Values) {
                if (v.name == name)
                    throw new ArgumentException($"Duplicate name {name}", nameof(name));
            }

            var ret = DynamicEnum.Construct(typeof(T), name, value);
            storage.Values.Add(value, ret);
            return (T) ret;
        }

        /// <summary>
        /// Adds a new enum value to the given enum type <typeparamref name="T"/>.
        /// This method differs from <see cref="Add{T}"/> in that it automatically determines a value.
        /// The value determined will be the next free number in a sequence, which represents the default behavior in an enum if enum values are not explicitly numbered.
        /// </summary>
        /// <param name="name">The name of the enum value to add</param>
        /// <typeparam name="T">The type to add this value to</typeparam>
        /// <returns>The newly created enum value</returns>
        public static T AddValue<T>(string name) where T : DynamicEnum {
            BigInteger value = 0;
            while (DynamicEnum.GetStorage(typeof(T)).Values.ContainsKey(value))
                value++;
            return DynamicEnum.Add<T>(name, value);
        }

        /// <summary>
        /// Adds a new flag enum value to the given enum type <typeparamref name="T"/>.
        /// This method differs from <see cref="Add{T}"/> in that it automatically determines a value.
        /// The value determined will be the next free power of two, allowing enum values to be combined using bitwise operations to create <see cref="FlagsAttribute"/>-like behavior.
        /// </summary>
        /// <param name="name">The name of the enum value to add</param>
        /// <typeparam name="T">The type to add this value to</typeparam>
        /// <returns>The newly created enum value</returns>
        public static T AddFlag<T>(string name) where T : DynamicEnum {
            BigInteger value = 1;
            while (DynamicEnum.GetStorage(typeof(T)).Values.ContainsKey(value))
                value <<= 1;
            return DynamicEnum.Add<T>(name, value);
        }

        /// <summary>
        /// Returns a collection of all of the enum values that are explicitly defined for the given dynamic enum type <typeparamref name="T"/>.
        /// A value counts as explicitly defined if it has been added using <see cref="Add{T}"/>, <see cref="AddValue{T}"/> or <see cref="AddFlag{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type whose values to get</typeparam>
        /// <returns>The defined values for the given type</returns>
        public static IEnumerable<T> GetValues<T>() where T : DynamicEnum {
            return DynamicEnum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Returns a collection of all of the enum values that are explicitly defined for the given dynamic enum type <paramref name="type"/>.
        /// A value counts as explicitly defined if it has been added using <see cref="Add{T}"/>, <see cref="AddValue{T}"/> or <see cref="AddFlag{T}"/>.
        /// </summary>
        /// <param name="type">The type whose values to get</param>
        /// <returns>The defined values for the given type</returns>
        public static IEnumerable<DynamicEnum> GetValues(Type type) {
            return DynamicEnum.GetStorage(type).Values.Values;
        }

        /// <summary>
        /// Returns the bitwise OR (|) combination of the two dynamic enum values
        /// </summary>
        /// <param name="left">The left value</param>
        /// <param name="right">The right value</param>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <returns>The bitwise OR (|) combination</returns>
        public static T Or<T>(T left, T right) where T : DynamicEnum {
            var cache = DynamicEnum.GetStorage(typeof(T)).OrCache;
            if (!cache.TryGetValue((left, right), out var ret)) {
                ret = DynamicEnum.GetEnumValue<T>(DynamicEnum.GetValue(left) | DynamicEnum.GetValue(right));
                cache.Add((left, right), ret);
            }
            return (T) ret;
        }

        /// <summary>
        /// Returns the bitwise AND (&amp;) combination of the two dynamic enum values
        /// </summary>
        /// <param name="left">The left value</param>
        /// <param name="right">The right value</param>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <returns>The bitwise AND (&amp;) combination</returns>
        public static T And<T>(T left, T right) where T : DynamicEnum {
            var cache = DynamicEnum.GetStorage(typeof(T)).AndCache;
            if (!cache.TryGetValue((left, right), out var ret)) {
                ret = DynamicEnum.GetEnumValue<T>(DynamicEnum.GetValue(left) & DynamicEnum.GetValue(right));
                cache.Add((left, right), ret);
            }
            return (T) ret;
        }

        /// <summary>
        /// Returns the bitwise XOR (^) combination of the two dynamic enum values
        /// </summary>
        /// <param name="left">The left value</param>
        /// <param name="right">The right value</param>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <returns>The bitwise XOR (^) combination</returns>
        public static T Xor<T>(T left, T right) where T : DynamicEnum {
            var cache = DynamicEnum.GetStorage(typeof(T)).XorCache;
            if (!cache.TryGetValue((left, right), out var ret)) {
                ret = DynamicEnum.GetEnumValue<T>(DynamicEnum.GetValue(left) ^ DynamicEnum.GetValue(right));
                cache.Add((left, right), ret);
            }
            return (T) ret;
        }

        /// <summary>
        /// Returns the bitwise NEG (~) combination of the dynamic enum value
        /// </summary>
        /// <param name="value">The value</param>
        /// <typeparam name="T">The type of the values</typeparam>
        /// <returns>The bitwise NEG (~) value</returns>
        public static T Neg<T>(T value) where T : DynamicEnum {
            var cache = DynamicEnum.GetStorage(typeof(T)).NegCache;
            if (!cache.TryGetValue(value, out var ret)) {
                ret = DynamicEnum.GetEnumValue<T>(~DynamicEnum.GetValue(value));
                cache.Add(value, ret);
            }
            return (T) ret;
        }

        /// <summary>
        /// Returns the <see cref="BigInteger"/> representation of the given dynamic enum value
        /// </summary>
        /// <param name="value">The value whose number representation to get</param>
        /// <returns>The value's number representation</returns>
        public static BigInteger GetValue(DynamicEnum value) {
            return value?.value ?? 0;
        }

        /// <summary>
        /// Returns the defined or combined dynamic enum value for the given <see cref="BigInteger"/> representation
        /// </summary>
        /// <param name="value">The value whose dynamic enum value to get</param>
        /// <typeparam name="T">The type that the returned dynamic enum should have</typeparam>
        /// <returns>The defined or combined dynamic enum value</returns>
        public static T GetEnumValue<T>(BigInteger value) where T : DynamicEnum {
            return (T) DynamicEnum.GetEnumValue(typeof(T), value);
        }

        /// <summary>
        /// Returns the defined or combined dynamic enum value for the given <see cref="BigInteger"/> representation
        /// </summary>
        /// <param name="type">The type that the returned dynamic enum should have</param>
        /// <param name="value">The value whose dynamic enum value to get</param>
        /// <returns>The defined or combined dynamic enum value</returns>
        public static DynamicEnum GetEnumValue(Type type, BigInteger value) {
            var storage = DynamicEnum.GetStorage(type);

            // get the defined value if it exists
            if (storage.Values.TryGetValue(value, out var defined))
                return defined;

            // otherwise, cache the combined value
            if (!storage.FlagCache.TryGetValue(value, out var combined)) {
                combined = DynamicEnum.Construct(type, null, value);
                storage.FlagCache.Add(value, combined);
            }
            return combined;
        }

        /// <summary>
        /// Parses the given <see cref="string"/> into a dynamic enum value and returns the result.
        /// This method supports defined enum values as well as values combined using the pipe (|) character and any number of spaces.
        /// If no enum value can be parsed, null is returned.
        /// </summary>
        /// <param name="strg">The string to parse into a dynamic enum value</param>
        /// <typeparam name="T">The type of the dynamic enum value to parse</typeparam>
        /// <returns>The parsed enum value, or null if parsing fails</returns>
        public static T Parse<T>(string strg) where T : DynamicEnum {
            return (T) DynamicEnum.Parse(typeof(T), strg);
        }

        /// <summary>
        /// Parses the given <see cref="string"/> into a dynamic enum value and returns the result.
        /// This method supports defined enum values as well as values combined using the pipe (|) character and any number of spaces.
        /// If no enum value can be parsed, null is returned.        /// </summary>
        /// <param name="type">The type of the dynamic enum value to parse</param>
        /// <param name="strg">The string to parse into a dynamic enum value</param>
        /// <returns>The parsed enum value, or null if parsing fails</returns>
        public static DynamicEnum Parse(Type type, string strg) {
            var cache = DynamicEnum.GetStorage(type).ParseCache;
            if (!cache.TryGetValue(strg, out var cached)) {
                BigInteger? accum = null;
                foreach (var val in strg.Split('|')) {
                    foreach (var defined in DynamicEnum.GetValues(type)) {
                        if (defined.name == val.Trim()) {
                            accum = (accum ?? 0) | DynamicEnum.GetValue(defined);
                            break;
                        }
                    }
                }
                if (accum != null)
                    cached = DynamicEnum.GetEnumValue(type, accum.Value);
                cache.Add(strg, cached);
            }
            return cached;
        }

        private static Storage GetStorage(Type type) {
            if (!DynamicEnum.Storages.TryGetValue(type, out var storage)) {
                storage = new Storage();
                DynamicEnum.Storages.Add(type, storage);
            }
            return storage;
        }

        private static DynamicEnum Construct(Type type, string name, BigInteger value) {
            return (DynamicEnum) Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] {name, value}, CultureInfo.InvariantCulture);
        }

        private class Storage {

            public readonly Dictionary<BigInteger, DynamicEnum> Values = new Dictionary<BigInteger, DynamicEnum>();
            public readonly Dictionary<BigInteger, DynamicEnum> FlagCache = new Dictionary<BigInteger, DynamicEnum>();
            public readonly Dictionary<string, DynamicEnum> ParseCache = new Dictionary<string, DynamicEnum>();
            public readonly Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum> OrCache = new Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum>();
            public readonly Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum> AndCache = new Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum>();
            public readonly Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum> XorCache = new Dictionary<(DynamicEnum, DynamicEnum), DynamicEnum>();
            public readonly Dictionary<DynamicEnum, DynamicEnum> NegCache = new Dictionary<DynamicEnum, DynamicEnum>();

            public void ClearCaches() {
                this.FlagCache.Clear();
                this.ParseCache.Clear();
                this.OrCache.Clear();
                this.AndCache.Clear();
                this.XorCache.Clear();
                this.NegCache.Clear();
            }

        }

    }
}