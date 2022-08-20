using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Pathfinding;
using NUnit.Framework;

namespace Tests {
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

        private static Stack<Point> FindPathInArea(Point start, Point end, IEnumerable<string> area, bool allowDiagonals) {
            var costs = area.Select(s => s.Select(c => c switch {
                ' ' => 1,
                'X' => float.PositiveInfinity,
                _ => (float) char.GetNumericValue(c)
            }).ToArray()).ToArray();
            var pathFinder = new AStar2((_, p2) => costs[p2.Y][p2.X], allowDiagonals);
            return pathFinder.FindPath(start, end);
        }

    }
}
