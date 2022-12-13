using MLEM.Extensions;
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

}
