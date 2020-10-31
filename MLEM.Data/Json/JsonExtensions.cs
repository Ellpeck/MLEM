using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MLEM.Data.Json {
    /// <summary>
    /// A set of extensions for dealing with the Newtonsoft.JSON <see cref="JsonSerializer"/>.
    /// </summary>
    public static class JsonExtensions {

        /// <summary>
        /// Changes the <see cref="JsonSerializer.ContractResolver"/> to a contract resolver that queries each newly created <see cref="JsonProperty"/> and allows modifying it easily.
        /// This removes the need to create a new contract resolver class for modifying created json properties.
        /// </summary>
        /// <param name="serializer">The serializer to which to add the property modifier</param>
        /// <param name="propertyModifier">A function that takes in the json property and allows returning a modified property (or the same one)</param>
        /// <returns></returns>
        public static JsonSerializer SetPropertyModifier(this JsonSerializer serializer, Func<JsonProperty, JsonProperty> propertyModifier) {
            serializer.ContractResolver = new PropertyModifierResolver(propertyModifier);
            return serializer;
        }

        private class PropertyModifierResolver : DefaultContractResolver {

            private readonly Func<JsonProperty, JsonProperty> propertyModifier;

            public PropertyModifierResolver(Func<JsonProperty, JsonProperty> propertyModifier) {
                this.propertyModifier = propertyModifier;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
                var property = base.CreateProperty(member, memberSerialization);
                return this.propertyModifier(property);
            }

        }

    }
}