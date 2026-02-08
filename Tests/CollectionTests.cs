using System;
using System.Collections.Generic;
using System.Linq;
using MLEM.Maths;
using MLEM.Misc;
using NUnit.Framework;

namespace Tests;

public class CollectionTests {

    [Test]
    public void TestCombinations() {
        var things = new[] {
            new[] {'1', '2', '3'},
            new[] {'A', 'B'},
            new[] {'+', '-'}
        };

        var expected = new[] {
            new[] {'1', 'A', '+'}, new[] {'1', 'A', '-'}, new[] {'1', 'B', '+'}, new[] {'1', 'B', '-'},
            new[] {'2', 'A', '+'}, new[] {'2', 'A', '-'}, new[] {'2', 'B', '+'}, new[] {'2', 'B', '-'},
            new[] {'3', 'A', '+'}, new[] {'3', 'A', '-'}, new[] {'3', 'B', '+'}, new[] {'3', 'B', '-'}
        };
        Assert.AreEqual(things.Combinations(), expected);

        var indices = new[] {
            new[] {0, 0, 0}, new[] {0, 0, 1}, new[] {0, 1, 0}, new[] {0, 1, 1},
            new[] {1, 0, 0}, new[] {1, 0, 1}, new[] {1, 1, 0}, new[] {1, 1, 1},
            new[] {2, 0, 0}, new[] {2, 0, 1}, new[] {2, 1, 0}, new[] {2, 1, 1}
        };
        Assert.AreEqual(things.IndexCombinations(), indices);
    }

    [Test]
    public void TestRandomWeightedEntryEqual([Values(0.1F, 1, 20, 23.5F, 99, 10000000)] float equalWeight, [Values(true, false)] bool integer) {
        var entries = new[] {"A", "B", "C", "D", "E"};
        var random = new Random(390453);
        var matches = new Dictionary<string, int>();
        for (var i = 0; i < 100000; i++) {
            var entry = integer ? random.GetRandomWeightedEntry(entries, _ => equalWeight.Ceil()) : random.GetRandomWeightedEntry(entries, _ => equalWeight);
            matches[entry] = matches.GetValueOrDefault(entry) + 1;
        }
        for (var i = 0; i < entries.Length; i++)
            Assert.AreEqual(100000 / entries.Length, matches[entries[i]], 1000);
    }

    [Test]
    public void TestRandomWeightedEntryVaried([Values(1, 37.283923F, 99)] float weightMult, [Values(true, false)] bool integer) {
        var weights = new[] {
            ("A", 1),
            ("B", 2),
            ("C", 3),
            ("D", integer ? 14 : 14.389238F),
            ("E", 20)
        };
        var random = new Random(234598223);
        var matches = new Dictionary<string, int>();
        for (var i = 0; i < 1000000; i++) {
            var entry = (integer ? random.GetRandomWeightedEntry(weights, e => (e.Item2 * weightMult).Ceil()) : random.GetRandomWeightedEntry(weights, e => e.Item2 * weightMult)).Item1;
            matches[entry] = matches.GetValueOrDefault(entry) + 1;
        }
        for (var i = 0; i < weights.Length; i++) {
            var expected = 1000000F / weights.Select(w => w.Item2).Sum() * weights[i].Item2;
            Assert.AreEqual(expected, matches[weights[i].Item1], 1000);
        }
    }

    [Test]
    public void TestRandomWeightedEntryFixedValues() {
        Assert.AreEqual(RandomExtensions.GetRandomWeightedEntry([1, 2, 3], _ => 1, 0), 1);
        Assert.AreEqual(RandomExtensions.GetRandomWeightedEntry([1, 2, 3], _ => 1, 0.5F), 2);
        Assert.AreEqual(RandomExtensions.GetRandomWeightedEntry([1, 2, 3], _ => 1, 0.99999999999999989F), 3);
        Assert.AreEqual(RandomExtensions.GetRandomWeightedEntry([1, 2, 3], _ => 1, 1), 3);
    }

}
