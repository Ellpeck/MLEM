using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Pathfinding {
    /// <summary>
    /// A 2-dimensional implementation of <see cref="AStar{T}"/> that uses <see cref="Point"/> for positions, and the manhattan distance as its heuristic.
    /// </summary>
    public class AStar2 : AStar<Point> {

        private readonly bool includeDiagonals;

        /// <inheritdoc />
        public AStar2(GetCost defaultCostFunction, bool includeDiagonals, float defaultCost = 1, int defaultMaxTries = 10000, CollectAdditionalNeighbors defaultAdditionalNeighbors = null) :
            base(defaultCostFunction, defaultCost, defaultMaxTries, defaultAdditionalNeighbors) {
            this.includeDiagonals = includeDiagonals;
        }

        /// <inheritdoc />
        protected override float GetHeuristicDistance(Point start, Point position) {
            return Math.Abs(position.X - start.X) + Math.Abs(position.Y - start.Y);
        }

        /// <inheritdoc />
        protected override void CollectNeighbors(Point position, ISet<Point> neighbors) {
            foreach (var dir in Direction2Helper.Adjacent)
                neighbors.Add(position + dir.Offset());

            if (this.includeDiagonals) {
                foreach (var dir in Direction2Helper.Diagonals)
                    neighbors.Add(position + dir.Offset());
            }
        }

    }
}
