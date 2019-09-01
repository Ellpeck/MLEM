using System;
using System.Collections.Generic;

namespace MLEM.Pathfinding {
    public abstract class AStar<T> {

        public readonly T[] AllDirections;
        public readonly T[] AdjacentDirections;
        public GetCost DefaultCostFunction;
        public float DefaultCost;
        public int DefaultMaxTries;
        public bool DefaultAllowDiagonals;
        public int LastTriesNeeded { get; private set; }
        public TimeSpan LastTimeNeeded { get; private set; }

        protected AStar(T[] allDirections, T[] adjacentDirections, GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) {
            this.AllDirections = allDirections;
            this.AdjacentDirections = adjacentDirections;
            this.DefaultCostFunction = defaultCostFunction;
            this.DefaultCost = defaultCost;
            this.DefaultMaxTries = defaultMaxTries;
            this.DefaultAllowDiagonals = defaultAllowDiagonals;
        }

        public Stack<T> FindPath(T start, T goal, GetCost costFunction = null, float? defaultCost = null, int? maxTries = null, bool? allowDiagonals = null) {
            var startTime = DateTime.UtcNow;

            var getCost = costFunction ?? this.DefaultCostFunction;
            var diags = allowDiagonals ?? this.DefaultAllowDiagonals;
            var tries = maxTries ?? this.DefaultMaxTries;
            var defCost = defaultCost ?? this.DefaultCost;

            var open = new List<PathPoint<T>>();
            var closed = new List<PathPoint<T>>();
            open.Add(new PathPoint<T>(start, this.GetManhattanDistance(start, goal), null, 0, defCost));

            var count = 0;
            Stack<T> ret = null;
            while (open.Count > 0) {
                PathPoint<T> current = null;
                var lowestF = float.MaxValue;
                foreach (var point in open)
                    if (point.F < lowestF) {
                        current = point;
                        lowestF = point.F;
                    }
                if (current == null)
                    break;

                open.Remove(current);
                closed.Add(current);

                if (current.Pos.Equals(goal)) {
                    ret = CompilePath(current);
                    break;
                }

                var dirsUsed = diags ? this.AllDirections : this.AdjacentDirections;
                foreach (var dir in dirsUsed) {
                    var neighborPos = this.AddPositions(current.Pos, dir);
                    var cost = getCost(current.Pos, neighborPos);
                    if (cost < float.MaxValue) {
                        var neighbor = new PathPoint<T>(neighborPos, this.GetManhattanDistance(neighborPos, goal), current, cost, defCost);
                        if (!closed.Contains(neighbor)) {
                            var alreadyIndex = open.IndexOf(neighbor);
                            if (alreadyIndex < 0) {
                                open.Add(neighbor);
                            } else {
                                var alreadyNeighbor = open[alreadyIndex];
                                if (neighbor.G < alreadyNeighbor.G) {
                                    open.Remove(alreadyNeighbor);
                                    open.Add(neighbor);
                                }
                            }
                        }
                    }
                }

                count++;
                if (count >= tries)
                    break;
            }
            
            this.LastTriesNeeded = count;
            this.LastTimeNeeded = DateTime.UtcNow - startTime;
            return ret;
        }

        protected abstract T AddPositions(T first, T second);

        protected abstract float GetManhattanDistance(T first, T second);

        private static Stack<T> CompilePath(PathPoint<T> current) {
            var path = new Stack<T>();
            while (current != null) {
                path.Push(current.Pos);
                current = current.Parent;
            }
            return path;
        }

        public delegate float GetCost(T currPos, T nextPos);

    }

    public class PathPoint<T> {

        public readonly PathPoint<T> Parent;
        public readonly T Pos;
        public readonly float F;
        public readonly float G;

        public PathPoint(T pos, float distance, PathPoint<T> parent, float terrainCostForThisPos, float defaultCost) {
            this.Pos = pos;
            this.Parent = parent;

            this.G = (parent == null ? 0 : parent.G) + terrainCostForThisPos;
            this.F = this.G + distance * defaultCost;
        }

        public override bool Equals(object obj) {
            if (obj == this)
                return true;
            return obj is PathPoint<T> point && point.Pos.Equals(this.Pos);
        }

        public override int GetHashCode() {
            return this.Pos.GetHashCode();
        }

    }
}