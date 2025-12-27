using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using NUnit.Framework;
using TypeHelper = MLEM.Data.TypeHelper;

namespace Tests;

public class TypeTests {

    [Test]
    public void TestConversions() {
        Type[] types = [
            typeof(string),
            typeof(int),
            typeof(Vector2),
            typeof(BitConverter),
            typeof(List<string>),
            typeof(Dictionary<int, string>),
            typeof(Dictionary<KeyValuePair<int, string>, string>)
        ];
        foreach (var type in types) {
            var split = TypeHelper.SplitAssemblyQualifiedName(type.AssemblyQualifiedName);
            Assert.AreEqual(type.FullName, split.Type);
            Assert.AreEqual(type.Assembly.GetName().FullName, split.Assembly);
            var joined = TypeHelper.JoinAssemblyQualifiedName(split.Type, split.Assembly);
            Assert.AreEqual(type.AssemblyQualifiedName, joined);
        }
    }

    [Test]
    public void TestGenerics() {
        const string spc = "System.Private.CoreLib";
        (Type, string[])[] types = [
            (typeof(string), []),
            (typeof(int), []),
            (typeof(Vector2), []),
            (typeof(List<string>), [$"System.String, {spc}"]),
            (typeof(Dictionary<int, string>), [$"System.Int32, {spc}", $"System.String, {spc}"]),
            (typeof(Dictionary<KeyValuePair<int, string>, string>), [$"System.Collections.Generic.KeyValuePair`2[[System.Int32, {spc}],[System.String, {spc}]], {spc}", $"System.String, {spc}"]),
        ];
        foreach (var (type, expected) in types) {
            var args = TypeHelper.GetGenericTypeArguments(type.AssemblyQualifiedName).Select(TypeHelper.RemoveAssemblyMetadata).ToArray();
            Assert.AreEqual(expected, args);
        }
    }

    [Test]
    public void TestRemoveAssemblyMetadata() {
        const string spc = "System.Private.CoreLib";
        (Type, string)[] types = [
            (typeof(string), $"System.String, {spc}"),
            (typeof(int), $"System.Int32, {spc}"),
            (typeof(Vector2), "Microsoft.Xna.Framework.Vector2, " +
#if KNI
                "Xna.Framework"
#elif FNA
                "FNA"
#else
                "MonoGame.Framework"
#endif
            ),
            (typeof(Dictionary<int, string>), $"System.Collections.Generic.Dictionary`2[[System.Int32, {spc}],[System.String, {spc}]], {spc}"),
        ];
        foreach (var (type, expected) in types) {
            Assert.IsTrue(type.AssemblyQualifiedName.Contains("PublicKeyToken"));
            Assert.AreEqual(expected, TypeHelper.RemoveAssemblyMetadata(type.AssemblyQualifiedName));
        }
    }

}
