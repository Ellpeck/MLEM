using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MLEM.Pathfinding {
    /// <summary>
    /// This is an abstract implementation of the A* path finding algorithm.
    /// This implementation is used by <see cref="AStar2"/>, a 2-dimensional A* path finding algorithm, and <see cref="AStar3"/>, a 3-dimensional A* path finding algorithm.
    /// </summary>
    /// <typeparam name="T">The type of points used for this path</typeparam>
    public abstract class AStar<T> {

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
        /// The default <see cref="CollectAdditionalNeighbors"/> function.
        /// </summary>
        public CollectAdditionalNeighbors DefaultAdditionalNeighbors;

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
        /// <param name="defaultCostFunction">The default function for cost determination of a path point</param>
        /// <param name="defaultCost">The default cost for a path point</param>
        /// <param name="defaultMaxTries">The default amount of tries before path finding is aborted</param>
        /// <param name="defaultAdditionalNeighbors">The default <see cref="CollectAdditionalNeighbors"/> function.</param>
        protected AStar(GetCost defaultCostFunction, float defaultCost, int defaultMaxTries, CollectAdditionalNeighbors defaultAdditionalNeighbors) {
            this.DefaultCostFunction = defaultCostFunction;
            this.DefaultCost = defaultCost;
            this.DefaultMaxTries = defaultMaxTries;
            this.DefaultAdditionalNeighbors = defaultAdditionalNeighbors;
        }

        /// <summary>
        /// Finds a path between two points using this pathfinder's default settings or, alternatively, the supplied override settings.
        /// </summary>
        /// <param name="start">The point to start path finding at</param>
        /// <param name="goal">The point to find a path to</param>
        /// <param name="costFunction">The function that determines the cost for each path point</param>
        /// <param name="defaultCost">The default cost for each path point</param>
        /// <param name="maxTries">The maximum amount of tries before path finding is aborted</param>
        /// <param name="additionalNeighbors">A function that determines a set of additional neighbors to be considered for a given point.</param>
        /// <returns>A stack of path points, where the top item is the first point to go to, or null if no path was found.</returns>
        public Stack<T> FindPath(T start, T goal, GetCost costFunction = null, float? defaultCost = null, int? maxTries = null, CollectAdditionalNeighbors additionalNeighbors = null) {
            this.TryFindPath(start, new[] {goal}, out var path, out _, costFunction, defaultCost, maxTries, additionalNeighbors);
            return path;
        }

        /// <summary>
        /// Tries to find a path between two points using this pathfinder's default settings or, alternatively, the supplied override settings.
        /// </summary>
        /// <param name="start">The point to start path finding at</param>
        /// <param name="goals">The points to find a path to, one of which will be chosen as the closest or best destination</param>
        /// <param name="path">The path that was found, or <see langword="null"/> if no path was found.</param>
        /// <param name="totalCost">The total cost that was calculated for the path, or <see cref="float.PositiveInfinity"/> if no path was found.</param>
        /// <param name="costFunction">The function that determines the cost for each path point</param>
        /// <param name="defaultCost">The default cost for each path point</param>
        /// <param name="maxTries">The maximum amount of tries before path finding is aborted</param>
        /// <param name="additionalNeighbors">A function that determines a set of additional neighbors to be considered for a given point.</param>
        /// <returns>Whether a path was found.</returns>
        public bool TryFindPath(T start, ICollection<T> goals, out Stack<T> path, out float totalCost, GetCost costFunction = null, float? defaultCost = null, int? maxTries = null, CollectAdditionalNeighbors additionalNeighbors = null) {
            path = null;
            totalCost = float.PositiveInfinity;

            var stopwatch = Stopwatch.StartNew();
            var getCost = costFunction ?? this.DefaultCostFunction;
            var tries = maxTries ?? this.DefaultMaxTries;
            var defCost = defaultCost ?? this.DefaultCost;
            var additional = additionalNeighbors ?? this.DefaultAdditionalNeighbors;

            var neighbors = new HashSet<T>();
            var open = new Dictionary<T, PathPoint<T>>();
            var closed = new Dictionary<T, PathPoint<T>>();
            open.Add(start, new PathPoint<T>(start, this.GetMinHeuristicDistance(start, goals), null, 0, defCost));

            var count = 0;
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

                if (goals.Contains(current.Pos)) {
                    path = AStar<T>.CompilePath(current);
                    totalCost = current.F;
                    break;
                }

                neighbors.Clear();
                this.CollectNeighbors(current.Pos, neighbors);
                additional?.Invoke(current.Pos, neighbors);

                foreach (var neighborPos in neighbors) {
                    var cost = getCost(current.Pos, neighborPos);
                    if (!float.IsPositiveInfinity(cost) && cost < float.MaxValue && !closed.ContainsKey(neighborPos)) {
                        var neighbor = new PathPoint<T>(neighborPos, this.GetMinHeuristicDistance(neighborPos, goals), current, cost, defCost);
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
            return path != null;
        }

        /// <summary>
        /// This method should implement a heuristic that determines the total distance between the given <paramref name="start"/> position and the given second position <paramref name="position"/>.
        /// Note that this is multiplied with the <see cref="DefaultCost"/> automatically, so no costs need to be considered in this method's return value.
        /// </summary>
        /// <param name="start">The start position.</param>
        /// <param name="position">The position to get the distance to.</param>
        /// <returns>The total distance between the two positions.</returns>
        protected abstract float GetHeuristicDistance(T start, T position);

        /// <summary>
        /// This method should populate a set of positions that are considered <paramref name="neighbors"/> to the given <paramref name="position"/>. For example, this method might return directly adjacent positions, diagonal positions, or faraway positions that can be teleported to.
        /// </summary>
        /// <param name="position">The position whose neighbors to return.</param>
        /// <param name="neighbors">The set to populate with neighbors.</param>
        protected abstract void CollectNeighbors(T position, ISet<T> neighbors);

        private float GetMinHeuristicDistance(T start, IEnumerable<T> positions) {
            var min = float.MaxValue;
            foreach (var position in positions)
                min = Math.Min(min, this.GetHeuristicDistance(start, position));
            return min;
        }

        private static Stack<T> CompilePath(PathPoint<T> current) {
            var path = new Stack<T>();
            while (current != null) {
                path.Push(current.Pos);
                current = current.Parent;
            }
            return path;
        }

        /// <summary>
        /// A cost function for a given pair of neighboring path finding positions.
        /// If a path point should have the default cost, <see cref="AStar{T}.DefaultCost"/> should be returned.
        /// If a path point should be unreachable, <see cref="float.PositiveInfinity"/> or <see cref="float.MaxValue"/> should be returned.
        /// </summary>
        /// <param name="currPos">The current position in the path.</param>
        /// <param name="nextPos">The neighboring position whose cost to check.</param>
        public delegate float GetCost(T currPos, T nextPos);

        /// <summary>
        /// A delegate that determines a set of additional <paramref name="neighbors"/> to be considered for a given <paramref name="position"/>.
        /// </summary>
        /// <param name="position">The position whose neighbors to return.</param>
        /// <param name="neighbors">The set to populate with neighbors.</param>
        public delegate void CollectAdditionalNeighbors(T position, ISet<T> neighbors);

    }

    /// <summary>
    /// A point in a <see cref="AStar{T}"/> path
    /// </summary>
    /// <typeparam name="T">The type of point used for this path</typeparam>
    public class PathPoint<T> : IEquatable<PathPoint<T>> {

        /// <summary>
        /// The path point that this point originated from
        /// </summary>
        public readonly PathPoint<T> Parent;
        /// <summary>
        /// The position of this path point
        /// </summary>
        public readonly T Pos;
        /// <summary>
        /// The F cost of this path point, which is the estimated total distance from the start to the goal.
        /// </summary>
        public readonly float F;
        /// <summary>
        /// The G cost of this path point, which is the actual distance from the start to the current <see cref="Pos"/>.
        /// </summary>
        public readonly float G;

        /// <summary>
        /// Creates a new path point with the supplied settings.
        /// </summary>
        /// <param name="pos">The point's position.</param>
        /// <param name="heuristicDistance">The point's estimated distance from the <paramref name="pos"/> to the goal, based on the <paramref name="defaultCost"/>.</param>
        /// <param name="parent">The point's parent.</param>
        /// <param name="terrainCost">The terrain cost to move from the <paramref name="parent"/> to this point, based on <see cref="AStar{T}.GetCost"/>.</param>
        /// <param name="defaultCost">The default cost for a path point.</param>
        public PathPoint(T pos, float heuristicDistance, PathPoint<T> parent, float terrainCost, float defaultCost) {
            this.Pos = pos;
            this.Parent = parent;

            this.G = (parent == null ? 0 : parent.G) + terrainCost;
            this.F = this.G + heuristicDistance * defaultCost;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
        public bool Equals(PathPoint<T> other) {
            return object.ReferenceEquals(this, other) || EqualityComparer<T>.Default.Equals(this.Pos, other.Pos);
        }

        /// <summary>Indicates whether this instance and a specified object are equal.</summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><see langword="true" /> if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, <see langword="false" />.</returns>
        public override bool Equals(object obj) {
            return obj is PathPoint<T> other && this.Equals(other);
        }

        /// <summary>Returns the hash code for this instance.</summary>
        /// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(this.Pos);
        }

    }
}
