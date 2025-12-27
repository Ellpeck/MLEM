using System;
using System.Collections.Generic;
using System.Text;

namespace MLEM.Data {
    /// <summary>
    /// This class contains a set of static helper methods for dealing with the string representations of <see cref="Type"/> names, including generic and non-generic types, and especially for converting between types and their <see cref="Type.AssemblyQualifiedName"/>.
    /// </summary>
    public class TypeHelper {

        /// <summary>
        /// Returns the assembly-qualified names of the generic type arguments that the type with the given <paramref name="assemblyQualifiedName"/> has.
        /// Note that "recursive" generic type arguments (ie. generic type arguments that themselves have generic type arguments) are not automatically resolved.
        /// </summary>
        /// <param name="assemblyQualifiedName">The type's name, which can either be assembly-qualified or not.</param>
        /// <returns>The generic type arguments the the given type has, or an empty collection if it is not generic.</returns>
        /// <remarks>
        /// Adapted from https://github.com/JamesNK/Newtonsoft.Json/blob/4e13299d4b0ec96bd4df9954ef646bd2d1b5bf2a/Src/Newtonsoft.Json/Serialization/DefaultSerializationBinder.cs#L136.
        /// </remarks>
        public static IEnumerable<string> GetGenericTypeArguments(string assemblyQualifiedName) {
            var genericStart = assemblyQualifiedName.IndexOf('[');
            if (genericStart >= 0) {
                var depth = 0;
                var argStart = 0;
                for (var i = genericStart + 1; i < assemblyQualifiedName.Length - 1; i++) {
                    var c = assemblyQualifiedName[i];
                    if (c == '[') {
                        if (depth == 0)
                            argStart = i + 1;
                        depth++;
                    } else if (c == ']') {
                        depth--;
                        if (depth == 0)
                            yield return assemblyQualifiedName.Substring(argStart, i - argStart).Trim();
                    }
                }
            }
        }

        /// <summary>
        /// Removes the assembly metadata from the given <paramref name="assemblyQualifiedName"/>, specifically the version, public key token, and further metadata after the assembly name.
        /// A non-assembly-qualified type name is returned unchanged.
        /// </summary>
        /// <param name="assemblyQualifiedName">The type's name, which can either be assembly-qualified or not.</param>
        /// <returns>The <paramref name="assemblyQualifiedName"/> with assembly metadata like the version and public key token removed.</returns>
        /// <remarks>
        /// Adapted from https://github.com/JamesNK/Newtonsoft.Json/blob/36b605f683c48a976b00ee6a351b97dd2265c7c9/Src/Newtonsoft.Json/Utilities/ReflectionUtils.cs#L183.
        /// </remarks>
        public static string RemoveAssemblyMetadata(string assemblyQualifiedName) {
            var builder = new StringBuilder();
            var inAssemblyName = false;
            var skippingMetadata = false;
            var mayBeArray = false;
            for (var i = 0; i < assemblyQualifiedName.Length; i++) {
                var current = assemblyQualifiedName[i];
                switch (current) {
                    case '[':
                        inAssemblyName = false;
                        skippingMetadata = false;
                        mayBeArray = true;
                        builder.Append(current);
                        break;
                    case ']':
                        inAssemblyName = false;
                        skippingMetadata = false;
                        mayBeArray = false;
                        builder.Append(current);
                        break;
                    case ',':
                        if (mayBeArray) {
                            builder.Append(current);
                        } else if (!inAssemblyName) {
                            inAssemblyName = true;
                            builder.Append(current);
                        } else {
                            skippingMetadata = true;
                        }
                        break;
                    default:
                        mayBeArray = false;
                        if (!skippingMetadata)
                            builder.Append(current);
                        break;
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Joins the given <paramref name="type"/> name and <paramref name="assembly"/> into their assembly-qualified name, which is the type name, a comma and a space, and the assembly name.
        /// </summary>
        /// <param name="type">The type name.</param>
        /// <param name="assembly">The assembly name.</param>
        /// <returns>The assembly-qualified name.</returns>
        public static string JoinAssemblyQualifiedName(string type, string assembly) {
            return $"{type}, {assembly}";
        }

        /// <summary>
        /// Splits the given <paramref name="assemblyQualifiedName"/> into a type name and an assembly name (if present).
        /// </summary>
        /// <param name="assemblyQualifiedName">The assembly-qualified name to split. If this is a type name only, the returned assembly will be <see langword="null"/>.</param>
        /// <returns>A tuple containing the type name and the assembly name (if it was present in the original assembly-qualified name, otherwise <see langword="null"/>).</returns>
        public static (string Type, string Assembly) SplitAssemblyQualifiedName(string assemblyQualifiedName) {
            var commaIdx = -1;
            var genericDepth = 0;
            for (var i = 0; i < assemblyQualifiedName.Length; i++) {
                var c = assemblyQualifiedName[i];
                if (c == '[') {
                    genericDepth++;
                } else if (c == ']') {
                    genericDepth--;
                } else if (c == ',' && genericDepth == 0) {
                    commaIdx = i;
                    break;
                }
            }
            if (commaIdx < 0) {
                return (assemblyQualifiedName, null);
            } else {
                return (assemblyQualifiedName.Substring(0, commaIdx).Trim(), assemblyQualifiedName.Substring(commaIdx + 1).Trim());
            }
        }

    }
}
