using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MLEM.Pathfinding {
    /// <summary>
    /// A 3-dimensional implementation of <see cref="AStar{T}"/> that uses <see cref="Vector3"/> for positions, and the manhattan distance as its heuristic.
    /// </summary>
    public class AStar3 : AStar<Vector3> {

        private readonly bool includeDiagonals;

        /// <inheritdoc />
        public AStar3(GetCost defaultCostFunction, bool includeDiagonals, float defaultCost = 1, int defaultMaxTries = 10000, CollectAdditionalNeighbors defaultAdditionalNeighbors = null) :
            base(defaultCostFunction, defaultCost, defaultMaxTries, defaultAdditionalNeighbors) {
            this.includeDiagonals = includeDiagonals;
        }

        /// <inheritdoc />
        protected override float GetHeuristicDistance(Vector3 start, Vector3 position) {
            return Math.Abs(position.X - start.X) + Math.Abs(position.Y - start.Y) + Math.Abs(position.Z - start.Z);
        }

        /// <inheritdoc />
        protected override void CollectNeighbors(Vector3 position, ISet<Vector3> neighbors) {
            if (this.includeDiagonals) {
                for (var x = -1; x <= 1; x++) {
                    for (var y = -1; y <= 1; y++) {
                        for (var z = -1; z <= 1; z++) {
                            if (x == 0 && y == 0 && z == 0)
                                continue;
                            neighbors.Add(position + new Vector3(x, y, z));
                        }
                    }
                }
            } else {
                neighbors.Add(position + new Vector3(1, 0, 0));
                neighbors.Add(position + new Vector3(-1, 0, 0));
                neighbors.Add(position + new Vector3(0, 1, 0));
                neighbors.Add(position + new Vector3(0, -1, 0));
                neighbors.Add(position + new Vector3(0, 0, 1));
                neighbors.Add(position + new Vector3(0, 0, -1));
            }
        }

    }
}
