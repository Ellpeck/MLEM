using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MLEM.Pathfinding {
    /// <summary>
    /// This is an abstract implementation of the A* path finding algorithm.
    /// This implementation is used by <see cref="AStar2"/>, a 2-dimensional A* path finding algorithm, and <see cref="AStar3"/>, a 3-dimensional A* path finding algorithm.
    /// </summary>
    /// <typeparam name="T">The type of points used for this path</typeparam>
    public abstract class AStar<T> {

        /// <summary>
        /// A value that represents an infinite path cost, or a cost for a location that cannot possibly be reached.
        /// </summary>
        public const float InfiniteCost = float.PositiveInfinity;
        /// <summary>
        /// The array of all directions that will be checked for path finding.
        /// Note that this array is only used if <see cref="DefaultAllowDiagonals"/> is true.
        /// </summary>
        public readonly T[] AllDirections;
        /// <summary>
        /// The array of all adjacent directions that will be checked for path finding.
        /// Note that this array is only used if <see cref="DefaultAllowDiagonals"/> is false.
        /// </summary>
        public readonly T[] AdjacentDirections;
        /// <summary>
        /// The default cost function that determines the cost for each path finding position.
        /// </summary>
        public GetCost DefaultCostFunction;
        /// <summary>
        /// The default cost for a path point.
        /// </summary>
        public float DefaultCost;
        /// <summary>
        /// The default amount of maximum tries that will be used before path finding is aborted.
        /// </summary>
        public int DefaultMaxTries;
        /// <summary>
        /// Whether or not diagonal directions are considered while finding a path.
        /// </summary>
        public bool DefaultAllowDiagonals;
        /// <summary>
        /// The amount of tries required for finding the last queried path
        /// </summary>
        public int LastTriesNeeded { get; private set; }
        /// <summary>
        /// The amount of time required for finding the last queried path
        /// </summary>
        public TimeSpan LastTimeNeeded { get; private set; }

        /// <summary>
        /// Creates a new A* pathfinder with the supplied default settings.
        /// </summary>
        /// <param name="allDirections">All directions that should be checked</param>
        /// <param name="adjacentDirections">All adjacent directions that should be checked</param>
        /// <param name="defaultCostFunction">The default function for cost determination of a path point</param>
        /// <param name="defaultAllowDiagonals">Whether or not diagonals should be allowed by default</param>
        /// <param name="defaultCost">The default cost for a path point</param>
        /// <param name="defaultMaxTries">The default amount of tries before path finding is aborted</param>
        protected AStar(T[] allDirections, T[] adjacentDirections, GetCost defaultCostFunction, bool defaultAllowDiagonals, float defaultCost = 1, int defaultMaxTries = 10000) {
            this.AllDirections = allDirections;
            this.AdjacentDirections = adjacentDirections;
            this.DefaultCostFunction = defaultCostFunction;
            this.DefaultCost = defaultCost;
            this.DefaultMaxTries = defaultMaxTries;
            this.DefaultAllowDiagonals = defaultAllowDiagonals;
        }

        /// <inheritdoc cref="FindPath"/>
        public Task<Stack<T>> FindPathAsync(T start, T goal, GetCost costFunction = null, float? defaultCost = null, int? maxTries = null, bool? allowDiagonals = null) {
            return Task.Run(() => this.FindPath(start, goal, costFunction, defaultCost, maxTries, allowDiagonals));
        }

        /// <summary>
        /// Finds a path between two points using this pathfinder's default settings or, alternatively, the supplied override settings.
        /// </summary>
        /// <param name="start">The point to start path finding at</param>
        /// <param name="goal">The point to find a path to</param>
        /// <param name="costFunction">The function that determines the cost for each path point</param>
        /// <param name="defaultCost">The default cost for each path point</param>
        /// <param name="maxTries">The maximum amount of tries before path finding is aborted</param>
        /// <param name="allowDiagonals">If diagonals should be looked at for path finding</param>
        /// <returns>A stack of path points, where the top item is the first point to go to, or null if no path was found.</returns>
        public Stack<T> FindPath(T start, T goal, GetCost costFunction = null, float? defaultCost = null, int? maxTries = null, bool? allowDiagonals = null) {
            var stopwatch = Stopwatch.StartNew();

            var getCost = costFunction ?? this.DefaultCostFunction;
            var diags = allowDiagonals ?? this.DefaultAllowDiagonals;
            var tries = maxTries ?? this.DefaultMaxTries;
            var defCost = defaultCost ?? this.DefaultCost;

            var open = new Dictionary<T, PathPoint<T>>();
            var closed = new Dictionary<T, PathPoint<T>>();
            open.Add(start, new PathPoint<T>(start, this.GetManhattanDistance(start, goal), null, 0, defCost));

            var count = 0;
            Stack<T> ret = null;
            while (open.Count > 0) {
                PathPoint<T> current = null;
                foreach (var point in open.Values) {
                    if (current == null || point.F < current.F)
                        current = point;
                }
                if (current == null)
                    break;

                open.Remove(current.Pos);
                closed.Add(current.Pos, current);

                if (current.Pos.Equals(goal)) {
                    ret = CompilePath(current);
                    break;
                }

                var dirsUsed = diags ? this.AllDirections : this.AdjacentDirections;
                foreach (var dir in dirsUsed) {
                    var neighborPos = this.AddPositions(current.Pos, dir);
                    var cost = getCost(current.Pos, neighborPos);
                    if (!float.IsPositiveInfinity(cost) && cost < float.MaxValue && !closed.ContainsKey(neighborPos)) {
                        var neighbor = new PathPoint<T>(neighborPos, this.GetManhattanDistance(neighborPos, goal), current, cost, defCost);
                        // check if we already have a waypoint at this location with a worse path
                        if (open.TryGetValue(neighborPos, out var alreadyNeighbor)) {
                            if (neighbor.G < alreadyNeighbor.G) {
                                open.Remove(neighborPos);
                            } else {
                                // if the old waypoint is better, we don't add ours
                                continue;
                            }
                        }
                        // add the new neighbor as a possible waypoint
                        open.Add(neighborPos, neighbor);
                    }
                }

                count++;
                if (count >= tries)
                    break;
            }

            stopwatch.Stop();
            this.LastTriesNeeded = count;
            this.LastTimeNeeded = stopwatch.Elapsed;
            return ret;
        }

        /// <summary>
        /// A helper method to add two positions together.
        /// </summary>
        protected abstract T AddPositions(T first, T second);

        /// <summary>
        /// A helper method to get the Manhattan Distance between two points.
        /// </summary>
        protected abstract float GetManhattanDistance(T first, T second);

        private static Stack<T> CompilePath(PathPoint<T> current) {
            var path = new Stack<T>();
            while (current != null) {
                path.Push(current.Pos);
                current = current.Parent;
            }
            return path;
        }

        /// <summary>
        /// A cost function for a given path finding position.
        /// If a path point should have the default cost, <see cref="AStar{T}.DefaultCost"/> should be returned.
        /// If a path point should be unreachable, <see cref="AStar{T}.InfiniteCost"/> should be returned.
        /// </summary>
        /// <param name="currPos">The current position in the path</param>
        /// <param name="nextPos">The position we're trying to reach from the current position</param>
        public delegate float GetCost(T currPos, T nextPos);

    }

    /// <summary>
    /// A point in a <see cref="AStar{T}"/> path
    /// </summary>
    /// <typeparam name="T">The type of point used for this path</typeparam>
    public class PathPoint<T> {

        /// <summary>
        /// The path point that this point originated from
        /// </summary>
        public readonly PathPoint<T> Parent;
        /// <summary>
        /// The position of this path point
        /// </summary>
        public readonly T Pos;
        /// <summary>
        /// The F cost of this path point
        /// </summary>
        public readonly float F;
        /// <summary>
        /// The G cost of this path point
        /// </summary>
        public readonly float G;

        /// <summary>
        /// Creates a new path point with the supplied settings.
        /// </summary>
        /// <param name="pos">The point's position</param>
        /// <param name="distance">The point's manhattan distance from the start point</param>
        /// <param name="parent">The point's parent</param>
        /// <param name="terrainCostForThisPos">The point's terrain cost, based on <see cref="AStar{T}.GetCost"/></param>
        /// <param name="defaultCost">The default cost for a path point</param>
        public PathPoint(T pos, float distance, PathPoint<T> parent, float terrainCostForThisPos, float defaultCost) {
            this.Pos = pos;
            this.Parent = parent;

            this.G = (parent == null ? 0 : parent.G) + terrainCostForThisPos;
            this.F = this.G + distance * defaultCost;
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) {
            if (obj == this)
                return true;
            return obj is PathPoint<T> point && point.Pos.Equals(this.Pos);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            return this.Pos.GetHashCode();
        }

    }
}