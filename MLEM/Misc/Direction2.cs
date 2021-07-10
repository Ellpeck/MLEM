using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using static MLEM.Misc.Direction2;

namespace MLEM.Misc {
    /// <summary>
    /// An enum that represents two-dimensional directions.
    /// Both straight and diagonal directions are supported.
    /// There are several extension methods and arrays available in <see cref="Direction2Helper"/>.
    /// </summary>
    [Flags, DataContract]
    public enum Direction2 {

        /// <summary>
        /// No direction.
        /// </summary>
        [EnumMember]
        None = 0,

        /// <summary>
        /// The up direction, or -y.
        /// </summary>
        [EnumMember]
        Up = 1,
        /// <summary>
        /// The right direction, or +x.
        /// </summary>
        [EnumMember]
        Right = 2,
        /// <summary>
        /// The down direction, or +y.
        /// </summary>
        [EnumMember]
        Down = 4,
        /// <summary>
        /// The left direction, or -x.
        /// </summary>
        [EnumMember]
        Left = 8,

        /// <summary>
        /// The up and right direction, or +x, -y.
        /// </summary>
        [EnumMember]
        UpRight = Up | Right,
        /// <summary>
        /// The down and right direction, or +x, +y.
        /// </summary>
        [EnumMember]
        DownRight = Down | Right,
        /// <summary>
        /// The up and left direction, or -x, -y.
        /// </summary>
        [EnumMember]
        UpLeft = Up | Left,
        /// <summary>
        /// The down and left direction, or -x, +y.
        /// </summary>
        [EnumMember]
        DownLeft = Down | Left

    }

    /// <summary>
    /// A set of helper and extension methods for dealing with <see cref="Direction2"/>
    /// </summary>
    public static class Direction2Helper {

        /// <summary>
        /// All <see cref="Direction2"/> enum values
        /// </summary>
        public static readonly Direction2[] All = EnumHelper.GetValues<Direction2>().ToArray();
        /// <summary>
        /// The <see cref="Direction2.Up"/> through <see cref="Direction2.Left"/> directions
        /// </summary>
        public static readonly Direction2[] Adjacent = All.Where(IsAdjacent).ToArray();
        /// <summary>
        /// The <see cref="Direction2.UpRight"/> through <see cref="Direction2.UpLeft"/> directions
        /// </summary>
        public static readonly Direction2[] Diagonals = All.Where(IsDiagonal).ToArray();
        /// <summary>
        /// All directions except <see cref="Direction2.None"/>
        /// </summary>
        public static readonly Direction2[] AllExceptNone = All.Where(dir => dir != None).ToArray();

        private static readonly Direction2[] Clockwise = {Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft};
        private static readonly Dictionary<Direction2, int> ClockwiseLookup = Clockwise.Select((d, i) => (d, i)).ToDictionary(kv => kv.d, kv => kv.i);

        /// <summary>
        /// Returns if the given direction is considered an "adjacent" direction.
        /// An adjacent direction is one that is not a diagonal.
        /// </summary>
        /// <param name="dir">The direction to query</param>
        /// <returns>Whether the direction is adjacent</returns>
        public static bool IsAdjacent(this Direction2 dir) {
            return dir == Up || dir == Right || dir == Down || dir == Left;
        }

        /// <summary>
        /// Returns if the given direction is considered a diagonal direction.
        /// </summary>
        /// <param name="dir">The direction to query</param>
        /// <returns>Whether the direction is diagonal</returns>
        public static bool IsDiagonal(this Direction2 dir) {
            return dir == UpRight || dir == DownRight || dir == UpLeft || dir == DownLeft;
        }

        /// <summary>
        /// Returns the directional offset of a given direction.
        /// The offset direction will be exactly one unit in each axis that the direction represents.
        /// </summary>
        /// <param name="dir">The direction whose offset to query</param>
        /// <returns>The direction's offset</returns>
        public static Point Offset(this Direction2 dir) {
            switch (dir) {
                case Up:
                    return new Point(0, -1);
                case Right:
                    return new Point(1, 0);
                case Down:
                    return new Point(0, 1);
                case Left:
                    return new Point(-1, 0);
                case UpRight:
                    return new Point(1, -1);
                case DownRight:
                    return new Point(1, 1);
                case DownLeft:
                    return new Point(-1, 1);
                case UpLeft:
                    return new Point(-1, -1);
                default:
                    return Point.Zero;
            }
        }

        /// <summary>
        /// Maps each direction in the given enumerable of directions to its <see cref="Offset"/>.
        /// </summary>
        /// <param name="directions">The direction enumerable</param>
        /// <returns>The directions' offsets, in the same order as the input directions.</returns>
        public static IEnumerable<Point> Offsets(this IEnumerable<Direction2> directions) {
            foreach (var dir in directions)
                yield return dir.Offset();
        }

        /// <summary>
        /// Returns the opposite of the given direction.
        /// For "adjacent" directions, this is the direction that points into the same axis, but the opposite sign.
        /// For diagonal directions, this is the direction that points into the opposite of both the x and y axis.
        /// </summary>
        /// <param name="dir">The direction whose opposite to get</param>
        /// <returns>The opposite of the direction</returns>
        public static Direction2 Opposite(this Direction2 dir) {
            switch (dir) {
                case Up:
                    return Down;
                case Right:
                    return Left;
                case Down:
                    return Up;
                case Left:
                    return Right;
                case UpRight:
                    return DownLeft;
                case DownRight:
                    return UpLeft;
                case DownLeft:
                    return UpRight;
                case UpLeft:
                    return DownRight;
                default:
                    return None;
            }
        }

        /// <summary>
        /// Returns the angle of the direction in radians, where <see cref="Direction2.Right"/> has an angle of 0.
        /// </summary>
        /// <param name="dir">The direction whose angle to get</param>
        /// <returns>The direction's angle</returns>
        public static float Angle(this Direction2 dir) {
            var (x, y) = dir.Offset();
            return (float) Math.Atan2(y, x);
        }

        /// <summary>
        /// Rotates the given direction clockwise and returns the resulting direction.
        /// </summary>
        /// <param name="dir">The direction to rotate</param>
        /// <param name="fortyFiveDegrees">Whether to rotate by 45 degrees. If this is false, the rotation is 90 degrees instead.</param>
        /// <returns>The rotated direction</returns>
        public static Direction2 RotateCw(this Direction2 dir, bool fortyFiveDegrees = false) {
            if (!ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return None;
            return Clockwise[(dirIndex + (fortyFiveDegrees ? 1 : 2)) % Clockwise.Length];
        }

        /// <summary>
        /// Rotates the given direction counter-clockwise and returns the resulting direction.
        /// </summary>
        /// <param name="dir">The direction to rotate counter-clockwise</param>
        /// <param name="fortyFiveDegrees">Whether to rotate by 45 degrees. If this is false, the rotation is 90 degrees instead.</param>
        /// <returns>The rotated direction</returns>
        public static Direction2 RotateCcw(this Direction2 dir, bool fortyFiveDegrees = false) {
            if (!ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return None;
            var index = dirIndex - (fortyFiveDegrees ? 1 : 2);
            return Clockwise[index < 0 ? index + Clockwise.Length : index];
        }

        /// <summary>
        /// Returns the <see cref="Direction2"/> that is closest to the given position's facing direction.
        /// </summary>
        /// <param name="offset">The vector whose corresponding direction to get</param>
        /// <returns>The vector's direction</returns>
        public static Direction2 ToDirection(this Vector2 offset) {
            var offsetAngle = (float) Math.Atan2(offset.Y, offset.X);
            foreach (var dir in AllExceptNone) {
                if (Math.Abs(dir.Angle() - offsetAngle) <= MathHelper.PiOver4 / 2)
                    return dir;
            }
            return None;
        }

        /// <summary>
        /// Returns the <see cref="Direction2"/> that is closest to the given position's facing direction, only taking <see cref="Adjacent"/> directions into account.
        /// Diagonal directions will be rounded to the nearest vertical direction.
        /// </summary>
        /// <param name="offset">The vector whose corresponding direction to get</param>
        /// <returns>The vector's direction</returns>
        public static Direction2 To90Direction(this Vector2 offset) {
            if (offset.X == 0 && offset.Y == 0)
                return None;
            if (Math.Abs(offset.X) > Math.Abs(offset.Y))
                return offset.X > 0 ? Right : Left;
            return offset.Y > 0 ? Down : Up;
        }

        /// <summary>
        /// Rotates the given direction by a given reference direction
        /// </summary>
        /// <param name="dir">The direction to rotate</param>
        /// <param name="reference">The direction to rotate by</param>
        /// <param name="start">The direction to use as the default direction</param>
        /// <returns>The direction, rotated by the reference direction</returns>
        public static Direction2 RotateBy(this Direction2 dir, Direction2 reference, Direction2 start = Up) {
            if (!ClockwiseLookup.TryGetValue(reference, out var refIndex))
                return None;
            if (!ClockwiseLookup.TryGetValue(start, out var startIndex))
                return None;
            if (!ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return None;
            var diff = refIndex - startIndex;
            if (diff < 0)
                diff += Clockwise.Length;
            return Clockwise[(dirIndex + diff) % Clockwise.Length];
        }

    }
}