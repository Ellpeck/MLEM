using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Pathfinding;
using NUnit.Framework;

namespace Tests;

public class PathfindingTests {

    [Test]
    public void TestConsistency() {
        var area = new[] {
            "XXXX",
            "X  X",
            "X  X",
            "XXXX"
        };

        var noDiagonals = PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 2), area, false).ToArray();
        Assert.AreEqual(noDiagonals.Length, 3);
        Assert.AreEqual(noDiagonals[0], new Point(1, 1));
        Assert.AreEqual(noDiagonals[2], new Point(2, 2));

        var diagonals = PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 2), area, true).ToArray();
        Assert.AreEqual(diagonals.Length, 2);
        Assert.AreEqual(diagonals[0], new Point(1, 1));
        Assert.AreEqual(diagonals[1], new Point(2, 2));
    }

    [Test]
    public void TestPathCost() {
        var area = new[] {
            "XXXXXXXX",
            "X 5   X",
            "X   5 X",
            "XXXXXXXX"
        };

        var firstPath = PathfindingTests.FindPathInArea(new Point(1, 1), new Point(3, 1), area, false).ToArray();
        var firstExpected = new[] {new Point(1, 1), new Point(1, 2), new Point(2, 2), new Point(3, 2), new Point(3, 1)};
        Assert.AreEqual(firstPath, firstExpected);

        var secondPath = PathfindingTests.FindPathInArea(new Point(1, 1), new Point(5, 2), area, false).ToArray();
        var secondExpected = firstExpected.Concat(new[] {new Point(4, 1), new Point(5, 1), new Point(5, 2)}).ToArray();
        Assert.AreEqual(secondPath, secondExpected);
    }

    [Test]
    public void TestBlocked() {
        var area = new[] {
            "XXXX",
            "X XX",
            "XX X",
            "X  X",
            "XXXX"
        };
        // non-diagonal pathfinding should get stuck in the corner
        Assert.IsNull(PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 3), area, false));
        // diagonal pathfinding should be able to cross the diagonal gap
        Assert.IsNotNull(PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 3), area, true));
    }

    [Test]
    public void TestSpecialDirections() {
        var area = new[] {
            "XXXX",
            "X XX",
            "X  X",
            "XXXX",
            "X  X",
            "XXXX"
        };

        // both types of traditional pathfinding should get stuck
        Assert.IsNull(PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 4), area, false));
        Assert.IsNull(PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 4), area, true));

        // but if we define a link across the wall, it should work
        Assert.IsNotNull(PathfindingTests.FindPathInArea(new Point(1, 1), new Point(2, 4), area, false, (p, n) => {
            if (p.X == 2 && p.Y == 2)
                n.Add(new Point(1, 4));
        }));
    }

    [Test]
    public void TestCostsAndMultipleGoals() {
        var area = new[] {
            "XXXXXXXX",
            "X  2  X",
            "XXXXX X",
            "X 53  X",
            "X XXX X",
            "X X X X",
            "XXXXXXXX"
        };
        var pathfinder = PathfindingTests.CreatePathfinder(area, false);

        // try to find paths to each goal individually
        var goals = new[] {new Point(1, 5), new Point(3, 5), new Point(5, 5)};
        var goalCosts = new[] {19, float.PositiveInfinity, 9};
        for (var i = 0; i < goals.Length; i++) {
            pathfinder.TryFindPath(new Point(1, 1), new[] {goals[i]}, out _, out var cost);
            Assert.AreEqual(goalCosts[i], cost);
        }

        // try to find paths to the best goal
        var expected = new[] {new Point(1, 1), new Point(2, 1), new Point(3, 1), new Point(4, 1), new Point(5, 1), new Point(5, 2), new Point(5, 3), new Point(5, 4), new Point(5, 5)};
        pathfinder.TryFindPath(new Point(1, 1), goals, out var path, out var bestCost);
        Assert.AreEqual(bestCost, 9);
        Assert.AreEqual(expected, path);
    }

    private static Stack<Point> FindPathInArea(Point start, Point end, IEnumerable<string> area, bool allowDiagonals, AStar2.CollectAdditionalNeighbors collectAdditionalNeighbors = null) {
        return PathfindingTests.CreatePathfinder(area, allowDiagonals, collectAdditionalNeighbors).FindPath(start, end);
    }

    private static AStar2 CreatePathfinder(IEnumerable<string> area, bool allowDiagonals, AStar2.CollectAdditionalNeighbors collectAdditionalNeighbors = null) {
        var costs = area.Select(s => s.Select(c => c switch {
            ' ' => 1,
            'X' => float.PositiveInfinity,
            _ => (float) char.GetNumericValue(c)
        }).ToArray()).ToArray();
        return new AStar2((_, p2) => costs[p2.Y][p2.X], allowDiagonals, 1, 64, collectAdditionalNeighbors);
    }

}
