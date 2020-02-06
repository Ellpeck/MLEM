using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MLEM.Pathfinding {
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
            AllDirs = dirs.ToArray();
        }

        public AStar3(GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) :
            base(AllDirs, AdjacentDirs, defaultCostFunction, defaultAllowDiagonals, defaultCost, defaultMaxTries) {
        }

        protected override Vector3 AddPositions(Vector3 first, Vector3 second) {
            return first + second;
        }

        protected override float GetManhattanDistance(Vector3 first, Vector3 second) {
            return Math.Abs(second.X - first.X) + Math.Abs(second.Y - first.Y) + Math.Abs(second.Z - first.Z);
        }

    }
}