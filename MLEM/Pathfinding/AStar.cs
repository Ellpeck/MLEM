using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MLEM.Pathfinding {
    public static class AStar {

        private static readonly Point[] AdjacentDirections = {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1)
        };

        private static readonly Point[] AllDirections = AdjacentDirections.Concat(new[] {
            new Point(1, 1),
            new Point(-1, 1),
            new Point(1, -1),
            new Point(-1, -1)
        }).ToArray();

        public static Stack<Point> FindPath(Point start, Point goal, int defaultCost, GetCost getCost, int maxTries = 10000, bool allowDiagonals = false) {
            var open = new List<PathPoint>();
            var closed = new List<PathPoint>();
            open.Add(new PathPoint(start, goal, null, 0, defaultCost));

            var count = 0;
            while (open.Count > 0) {
                PathPoint current = null;
                var lowestF = int.MaxValue;
                foreach (var point in open)
                    if (point.F < lowestF) {
                        current = point;
                        lowestF = point.F;
                    }
                if (current == null)
                    break;

                open.Remove(current);
                closed.Add(current);

                if (current.Pos.Equals(goal))
                    return CompilePath(current);

                var dirsUsed = allowDiagonals ? AllDirections : AdjacentDirections;
                foreach (var dir in dirsUsed) {
                    var neighborPos = current.Pos + dir;
                    var cost = getCost(neighborPos);
                    if (cost < int.MaxValue) {
                        var neighbor = new PathPoint(neighborPos, goal, current, cost, defaultCost);
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
                if (count >= maxTries)
                    break;
            }
            return null;
        }

        private static Stack<Point> CompilePath(PathPoint current) {
            var path = new Stack<Point>();
            while (current != null) {
                path.Push(current.Pos);
                current = current.Parent;
            }
            return path;
        }

        public delegate int GetCost(Point pos);

    }

    public class PathPoint {

        public readonly PathPoint Parent;
        public readonly Point Pos;
        public readonly int F;
        public readonly int G;

        public PathPoint(Point pos, Point goal, PathPoint parent, int terrainCostForThisPos, int defaultCost) {
            this.Pos = pos;
            this.Parent = parent;

            this.G = (parent == null ? 0 : parent.G) + terrainCostForThisPos;
            var manhattan = (Math.Abs(goal.X - pos.X) + Math.Abs(goal.Y - pos.Y)) * defaultCost;
            this.F = this.G + manhattan;
        }

        public override bool Equals(object obj) {
            if (obj == this)
                return true;
            return obj is PathPoint point && point.Pos.Equals(this.Pos);
        }

        public override int GetHashCode() {
            return this.Pos.GetHashCode();
        }

    }
}