using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MLEM.Pathfinding {
    /// <summary>
    /// A 3-dimensional implementation of <see cref="AStar{T}"/> that uses <see cref="Vector3"/> for positions.
    /// </summary>
    public class AStar3 : AStar<Vector3> {

        private static readonly Vector3[] AdjacentDirs = {
            new Vector3(1, 0, 0),
            new Vector3(-1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(0, -1, 0),
            new Vector3(0, 0, 1),
            new Vector3(0, 0, -1)
        };

        private static readonly Vector3[] AllDirs;

        static AStar3() {
            var dirs = new List<Vector3>();
            for (var x = -1; x <= 1; x++) {
                for (var y = -1; y <= 1; y++) {
                    for (var z = -1; z <= 1; z++) {
                        if (x == 0 && y == 0 && z == 0)
                            continue;
                        dirs.Add(new Vector3(x, y, z));
                    }
                }
            }
            AStar3.AllDirs = dirs.ToArray();
        }

        /// <inheritdoc />
        public AStar3(GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) :
            base(AStar3.AllDirs, AStar3.AdjacentDirs, defaultCostFunction, defaultAllowDiagonals, defaultCost, defaultMaxTries) {}

        /// <inheritdoc />
        protected override Vector3 AddPositions(Vector3 first, Vector3 second) {
            return first + second;
        }

        /// <inheritdoc />
        protected override float GetManhattanDistance(Vector3 first, Vector3 second) {
            return Math.Abs(second.X - first.X) + Math.Abs(second.Y - first.Y) + Math.Abs(second.Z - first.Z);
        }

    }
}
