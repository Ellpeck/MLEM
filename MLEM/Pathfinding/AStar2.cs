using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Pathfinding {
    /// <summary>
    /// A 2-dimensional implementation of <see cref="AStar{T}"/> that uses <see cref="Point"/> for positions.
    /// </summary>
    public class AStar2 : AStar<Point> {

        private static readonly Point[] AdjacentDirs = Direction2Helper.Adjacent.Offsets().ToArray();
        private static readonly Point[] AllDirs = Direction2Helper.All.Offsets().ToArray();

        /// <inheritdoc />
        public AStar2(GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) :
            base(AllDirs, AdjacentDirs, defaultCostFunction, defaultAllowDiagonals, defaultCost, defaultMaxTries) {
        }

        /// <inheritdoc />
        protected override Point AddPositions(Point first, Point second) {
            return first + second;
        }

        /// <inheritdoc />
        protected override float GetManhattanDistance(Point first, Point second) {
            return Math.Abs(second.X - first.X) + Math.Abs(second.Y - first.Y);
        }

    }
}