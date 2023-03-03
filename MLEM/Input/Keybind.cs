using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

#if NET452
using MLEM.Extensions;
#endif

namespace MLEM.Input {
    /// <summary>
    /// A keybind represents a generic way to trigger input.
    /// A keybind is made up of multiple key combinations, one of which has to be pressed for the keybind to be triggered.
    /// Note that this type is serializable using <see cref="DataContractAttribute"/>.
    /// Note that this class implements <see cref="IComparable"/> and <see cref="IComparable{T}"/>, which allows two combinations to be ordered based on how many <see cref="Combination.Modifiers"/> their combinations have.
    /// </summary>
    [DataContract]
    public class Keybind : IComparable<Keybind>, IComparable {

        private static readonly Combination[] EmptyCombinations = new Combination[0];
        private static readonly GenericInput[] EmptyInputs = new GenericInput[0];

        [DataMember]
        private Combination[] combinations = Keybind.EmptyCombinations;

        /// <summary>
        /// Creates a new keybind and adds the given key and modifiers using <see cref="Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/>
        /// </summary>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        public Keybind(GenericInput key, params GenericInput[] modifiers) {
            this.Add(key, modifiers);
        }

        /// <inheritdoc cref="Keybind(GenericInput, GenericInput[])"/>
        public Keybind(GenericInput key, ModifierKey modifier) {
            this.Add(key, modifier);
        }

        /// <summary>
        /// Creates a new keybind with no default combinations
        /// </summary>
        public Keybind() {}

        /// <summary>
        /// Adds a new key combination to this keybind that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Add(GenericInput key, params GenericInput[] modifiers) {
            return this.Add(new Combination(key, modifiers));
        }

        /// <summary>
        /// Adds the given <see cref="Combination"/> to this keybind that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="combination">The combination to add.</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Add(Combination combination) {
            this.combinations = this.combinations.Append(combination).ToArray();
            return this;
        }

        /// <inheritdoc cref="Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/>
        public Keybind Add(GenericInput key, ModifierKey modifier) {
            foreach (var mod in modifier.GetKeys())
                this.Add(key, mod);
            return this;
        }

        /// <summary>
        /// Inserts a new key combination into the given <paramref name="index"/> of this keybind's combinations that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="index">The index to insert this combination into.</param>
        /// <param name="key">The key to be pressed.</param>
        /// <param name="modifiers">The modifier keys that have to be held down.</param>
        /// <returns>This keybind, for chaining.</returns>
        public Keybind Insert(int index, GenericInput key, params GenericInput[] modifiers) {
            return this.Insert(index, new Combination(key, modifiers));
        }

        /// <summary>
        /// Inserts the given <see cref="Combination"/> into the given <paramref name="index"/> of this keybind's combinations that can optionally be pressed for the keybind to trigger.
        /// </summary>
        /// <param name="index">The index to insert this combination into.</param>
        /// <param name="combination">The combination to insert.</param>
        /// <returns>This keybind, for chaining.</returns>
        public Keybind Insert(int index, Combination combination) {
            this.combinations = this.combinations.Take(index).Append(combination).Concat(this.combinations.Skip(index)).ToArray();
            return this;
        }

        /// <inheritdoc cref="Insert(int,MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/>
        public Keybind Insert(int index, GenericInput key, ModifierKey modifier) {
            foreach (var mod in modifier.GetKeys().Reverse())
                this.Insert(index, key, mod);
            return this;
        }

        /// <summary>
        /// Clears this keybind, removing all active combinations.
        /// </summary>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Clear() {
            this.combinations = Keybind.EmptyCombinations;
            return this;
        }

        /// <summary>
        /// Removes all combinations that match the given predicate
        /// </summary>
        /// <param name="predicate">The predicate to match against</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind Remove(Func<Combination, int, bool> predicate) {
            this.combinations = this.combinations.Where((c, i) => !predicate(c, i)).ToArray();
            return this;
        }

        /// <summary>
        /// Copies all of the combinations from the given keybind into this keybind.
        /// Note that this doesn't <see cref="Clear"/> this keybind, so combinations will be merged rather than replaced.
        /// </summary>
        /// <param name="other">The keybind to copy from</param>
        /// <returns>This keybind, for chaining</returns>
        public Keybind CopyFrom(Keybind other) {
            this.combinations = this.combinations.Concat(other.combinations).ToArray();
            return this;
        }

        /// <summary>
        /// Returns whether this keybind is considered to be down.
        /// See <see cref="InputHandler.IsDown"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be down</returns>
        public bool IsDown(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.IsDown(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this keybind was considered to be down in the last update call.
        /// See <see cref="InputHandler.WasDown"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind was considered to be down</returns>
        public bool WasDown(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.WasDown(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this keybind is considered to be pressed.
        /// See <see cref="InputHandler.IsPressed"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be pressed</returns>
        public bool IsPressed(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.IsPressed(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this keybind is considered to be pressed and has not been consumed yet using<see cref="TryConsumePressed"/>.
        /// See <see cref="InputHandler.IsPressedAvailable"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be pressed</returns>
        public bool IsPressedAvailable(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.IsPressedAvailable(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether this keybind is considered to be pressed.
        /// See <see cref="InputHandler.TryConsumePressed"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether this keybind is considered to be pressed</returns>
        public bool TryConsumePressed(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.TryConsumePressed(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether any of this keybind's modifier keys are currently down.
        /// See <see cref="InputHandler.IsDown"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether any of this keyboard's modifier keys are down</returns>
        public bool IsModifierDown(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.IsModifierDown(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether any of this keybind's modifier keys were down in the last update call.
        /// See <see cref="InputHandler.WasDown"/> for more information.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>Whether any of this keyboard's modifier keys were down</returns>
        public bool WasModifierDown(InputHandler handler, int gamepadIndex = -1) {
            foreach (var combination in this.combinations) {
                if (combination.WasModifierDown(handler, gamepadIndex))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the amount of time that this keybind has been held down for.
        /// If this input isn't currently down, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</returns>
        public TimeSpan GetDownTime(InputHandler handler, int gamepadIndex = -1) {
            return this.combinations.Max(c => c.GetDownTime(handler, gamepadIndex));
        }

        /// <summary>
        /// Returns the amount of time that this keybind has been up for since the last time it was down.
        /// If this input isn't currently up, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input is being held.</returns>
        public TimeSpan GetUpTime(InputHandler handler, int gamepadIndex = -1) {
            return this.combinations.Min(c => c.GetUpTime(handler, gamepadIndex));
        }

        /// <summary>
        /// Returns the amount of time that has passed since this keybind last counted as pressed.
        /// If this input hasn't been pressed previously, or is currently pressed, this method returns <see cref="TimeSpan.Zero"/>.
        /// </summary>
        /// <param name="handler">The input handler to query the keys with</param>
        /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
        /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input has never been pressed, or is currently pressed.</returns>
        public TimeSpan GetTimeSincePress(InputHandler handler, int gamepadIndex = -1) {
            return this.combinations.Min(c => c.GetTimeSincePress(handler, gamepadIndex));
        }

        /// <summary>
        /// Returns an enumerable of all of the combinations that this keybind currently contains
        /// </summary>
        /// <returns>This keybind's combinations</returns>
        public IEnumerable<Combination> GetCombinations() {
            foreach (var combination in this.combinations)
                yield return combination;
        }

        /// <summary>
        /// Tries to retrieve the combination at the given <paramref name="index"/> within this keybind.
        /// </summary>
        /// <param name="index">The index of the combination to retrieve.</param>
        /// <param name="combination">The combination, or default if this method returns false.</param>
        /// <returns>Whether the combination could be successfully retrieved or the index was out of bounds of this keybind's combination collection.</returns>
        public bool TryGetCombination(int index, out Combination combination) {
            if (index >= 0 && index < this.combinations.Length) {
                combination = this.combinations[index];
                return true;
            } else {
                combination = default;
                return false;
            }
        }

        /// <summary>
        /// Converts this keybind into an easily human-readable string.
        /// When using <see cref="ToString()"/>, this method is used with <paramref name="joiner"/> set to ", ".
        /// </summary>
        /// <param name="joiner">The string to use to join combinations</param>
        /// <param name="combinationJoiner">The string to use for combination-internal joining, see <see cref="Combination.ToString(string,System.Func{MLEM.Input.GenericInput,string})"/></param>
        /// <param name="inputName">The function to use for determining the display name of generic inputs, see <see cref="Combination.ToString(string,System.Func{MLEM.Input.GenericInput,string})"/></param>
        /// <returns>A human-readable string representing this keybind</returns>
        public string ToString(string joiner, string combinationJoiner = " + ", Func<GenericInput, string> inputName = null) {
            return string.Join(joiner, this.combinations.Select(c => c.ToString(combinationJoiner, inputName)));
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
        public int CompareTo(Keybind other) {
            return this.combinations.Sum(c => other.combinations.Sum(c.CompareTo));
        }

        /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <exception cref="T:System.ArgumentException">
        /// <paramref name="obj" /> is not the same type as this instance.</exception>
        /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///
        /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="obj" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="obj" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="obj" /> in the sort order.</description></item></list></returns>
        public int CompareTo(object obj) {
            if (object.ReferenceEquals(null, obj))
                return 1;
            if (object.ReferenceEquals(this, obj))
                return 0;
            if (!(obj is Keybind other))
                throw new ArgumentException($"Object must be of type {nameof(Keybind)}");
            return this.CompareTo(other);
        }

        /// <summary>
        /// Converts this keybind into a string, separating every included <see cref="Combination"/> by a comma
        /// </summary>
        /// <returns>This keybind as a string</returns>
        public override string ToString() {
            return this.ToString(", ");
        }

        /// <summary>
        /// A key combination is a combination of a set of modifier keys and a key.
        /// All of the keys are <see cref="GenericInput"/> instances, so they can be keyboard-, mouse- or gamepad-based.
        /// Note that this class implements <see cref="IComparable"/> and <see cref="IComparable{T}"/>, which allows two combinations to be ordered based on how many <see cref="Modifiers"/> they have.
        /// </summary>
        [DataContract]
        public class Combination : IComparable<Combination>, IComparable {

            /// <summary>
            /// The inputs that have to be held down for this combination to be valid, which is queried in <see cref="IsModifierDown"/> and <see cref="WasModifierDown"/>.
            /// If this collection is empty, there are no required modifiers.
            /// </summary>
            [DataMember]
            public readonly GenericInput[] Modifiers;
            /// <summary>
            /// The inputs that have to be up for this combination to be valid, which is queried in <see cref="IsModifierDown"/> and <see cref="WasModifierDown"/>.
            /// If this collection is empty, there are no required inverse modifiers.
            /// </summary>
            [DataMember]
            public readonly GenericInput[] InverseModifiers;
            /// <summary>
            /// The input that has to be down (or pressed) for this combination to be considered down (or pressed).
            /// Note that <see cref="Modifiers"/> needs to be empty, or all of its values need to be down, as well.
            /// </summary>
            [DataMember]
            public readonly GenericInput Key;

            /// <summary>
            /// Creates a new combination with the given settings.
            /// To add a combination to a <see cref="Keybind"/>, use <see cref="Keybind.Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/> instead.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="modifiers">The modifiers.</param>
            public Combination(GenericInput key, params GenericInput[] modifiers) : this(key, modifiers, null) {}

            /// <summary>
            /// Creates a new combination with the given settings.
            /// To add a combination to a <see cref="Keybind"/>, use <see cref="Keybind.Add(MLEM.Input.GenericInput,MLEM.Input.GenericInput[])"/> instead.
            /// Note that inputs are automatically removed from <paramref name="inverseModifiers"/> if they are also present in <paramref name="modifiers"/>, or if they are the main <paramref name="key"/>.
            /// </summary>
            /// <param name="key">The key.</param>
            /// <param name="modifiers">The modifiers, or <see langword="null"/> to use no modifiers.</param>
            /// <param name="inverseModifiers">The inverse modifiers, or <see langword="null"/> to use no modifiers.</param>
            public Combination(GenericInput key, GenericInput[] modifiers, GenericInput[] inverseModifiers) {
                this.Key = key;
                this.Modifiers = modifiers ?? Keybind.EmptyInputs;
                this.InverseModifiers = inverseModifiers ?? Keybind.EmptyInputs;

                // make sure that inverse modifiers don't contain any modifiers or the key
                if (this.InverseModifiers.Length > 0)
                    this.InverseModifiers = this.InverseModifiers.Where(k => k != this.Key).Except(this.Modifiers).ToArray();
            }

            /// <summary>
            /// Creates a new empty combination using the default <see cref="GenericInput"/> with the <see cref="GenericInput.InputType.None"/> input type and no <see cref="Modifiers"/> or <see cref="InverseModifiers"/>.
            /// </summary>
            public Combination() : this(default, null, null) {}

            /// <summary>
            /// Returns whether this combination is currently down.
            /// See <see cref="InputHandler.IsDown"/> for more information.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination is down</returns>
            public bool IsDown(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsDown(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns whether this combination was down in the last upate call.
            /// See <see cref="InputHandler.WasDown"/> for more information.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination was down</returns>
            public bool WasDown(InputHandler handler, int gamepadIndex = -1) {
                return this.WasModifierDown(handler, gamepadIndex) && handler.WasDown(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns whether this combination is currently pressed.
            /// See <see cref="InputHandler.IsPressed"/> for more information.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination is pressed</returns>
            public bool IsPressed(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsPressed(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns whether this combination is currently pressed and has not been consumed yet using <see cref="TryConsumePressed"/>.
            /// See <see cref="InputHandler.IsPressedAvailable"/> for more information.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination is pressed</returns>
            public bool IsPressedAvailable(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.IsPressedAvailable(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns whether this combination is currently pressed and the press has not been consumed yet.
            /// A combination is considered consumed if this method has already returned true previously since the last <see cref="InputHandler.Update()"/> call.
            /// See <see cref="InputHandler.TryConsumePressed"/> for more information.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination is pressed</returns>
            public bool TryConsumePressed(InputHandler handler, int gamepadIndex = -1) {
                return this.IsModifierDown(handler, gamepadIndex) && handler.TryConsumePressed(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns whether all of this combination's <see cref="Modifiers"/> are currently down and <see cref="InverseModifiers"/> are currently up.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination's modifiers are down</returns>
            public bool IsModifierDown(InputHandler handler, int gamepadIndex = -1) {
                foreach (var modifier in this.Modifiers) {
                    if (!handler.IsDown(modifier, gamepadIndex))
                        return false;
                }
                foreach (var invModifier in this.InverseModifiers) {
                    if (handler.IsDown(invModifier, gamepadIndex))
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Returns whether all of this combination's <see cref="Modifiers"/> were down and <see cref="InverseModifiers"/> were up in the last update call.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>Whether this combination's modifiers were down</returns>
            public bool WasModifierDown(InputHandler handler, int gamepadIndex = -1) {
                foreach (var modifier in this.Modifiers) {
                    if (!handler.WasDown(modifier, gamepadIndex))
                        return false;
                }
                foreach (var invModifier in this.InverseModifiers) {
                    if (handler.WasDown(invModifier, gamepadIndex))
                        return false;
                }
                return true;
            }

            /// <summary>
            /// Returns the amount of time that this combination has been held down for.
            /// If this input isn't currently down, this method returns <see cref="TimeSpan.Zero"/>.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>The resulting down time, or <see cref="TimeSpan.Zero"/> if the input is not being held.</returns>
            public TimeSpan GetDownTime(InputHandler handler, int gamepadIndex = -1) {
                return handler.GetDownTime(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns the amount of time that this combination has been up for since the last time it was down.
            /// If this input isn't currently up, this method returns <see cref="TimeSpan.Zero"/>.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input is being held.</returns>
            public TimeSpan GetUpTime(InputHandler handler, int gamepadIndex = -1) {
                return handler.GetUpTime(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Returns the amount of time that has passed since this combination last counted as pressed.
            /// If this input hasn't been pressed previously, or is currently pressed, this method returns <see cref="TimeSpan.Zero"/>.
            /// </summary>
            /// <param name="handler">The input handler to query the keys with</param>
            /// <param name="gamepadIndex">The index of the gamepad to query, or -1 to query all gamepads</param>
            /// <returns>The resulting up time, or <see cref="TimeSpan.Zero"/> if the input has never been pressed, or is currently pressed.</returns>
            public TimeSpan GetTimeSincePress(InputHandler handler, int gamepadIndex = -1) {
                return handler.GetTimeSincePress(this.Key, gamepadIndex);
            }

            /// <summary>
            /// Converts this combination into an easily human-readable string.
            /// When using <see cref="ToString()"/>, this method is used with <paramref name="joiner"/> set to " + ".
            /// </summary>
            /// <param name="joiner">The string to use to join this combination's <see cref="Modifiers"/> and <see cref="Key"/> together</param>
            /// <param name="inputName">The function to use for determining the display name of a <see cref="GenericInput"/>. If this is null, the generic input's default <see cref="GenericInput.ToString"/> method is used.</param>
            /// <returns>A human-readable string representing this combination</returns>
            public string ToString(string joiner, Func<GenericInput, string> inputName = null) {
                return string.Join(joiner, this.Modifiers.Append(this.Key).Select(i => inputName?.Invoke(i) ?? i.ToString()));
            }

            /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
            /// <param name="other">An object to compare with this instance.</param>
            /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
            ///
            /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="other" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="other" /> in the sort order.</description></item></list></returns>
            public int CompareTo(Combination other) {
                return this.Modifiers.Length.CompareTo(other.Modifiers.Length);
            }

            /// <summary>Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.</summary>
            /// <param name="obj">An object to compare with this instance.</param>
            /// <exception cref="T:System.ArgumentException">
            /// <paramref name="obj" /> is not the same type as this instance.</exception>
            /// <returns>A value that indicates the relative order of the objects being compared. The return value has these meanings:
            ///
            /// <list type="table"><listheader><term> Value</term><description> Meaning</description></listheader><item><term> Less than zero</term><description> This instance precedes <paramref name="obj" /> in the sort order.</description></item><item><term> Zero</term><description> This instance occurs in the same position in the sort order as <paramref name="obj" />.</description></item><item><term> Greater than zero</term><description> This instance follows <paramref name="obj" /> in the sort order.</description></item></list></returns>
            public int CompareTo(object obj) {
                if (object.ReferenceEquals(null, obj))
                    return 1;
                if (object.ReferenceEquals(this, obj))
                    return 0;
                if (!(obj is Combination other))
                    throw new ArgumentException($"Object must be of type {nameof(Combination)}");
                return this.CompareTo(other);
            }

            /// <summary>Returns a string that represents the current object.</summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString() {
                return this.ToString(" + ");
            }

        }

    }
}
