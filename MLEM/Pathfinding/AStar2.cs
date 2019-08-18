using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MLEM.Pathfinding {
    public class AStar2 : AStar<Point> {

        private static readonly Point[] AdjacentDirs = {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1)
        };

        private static readonly Point[] AllDirs = AdjacentDirs.Concat(new[] {
            new Point(1, 1),
            new Point(-1, 1),
            new Point(1, -1),
            new Point(-1, -1)
        }).ToArray();

        public AStar2(GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) :
            base(AllDirs, AdjacentDirs, defaultCostFunction, defaultAllowDiagonals, defaultCost, defaultMaxTries) {
        }

        protected override Point AddPositions(Point first, Point second) {
            return first + second;
        }

        protected override float GetManhattanDistance(Point first, Point second) {
            return Math.Abs(second.X - first.X) + Math.Abs(second.Y - first.Y);
        }

    }
}