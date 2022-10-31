using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

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
        UpRight = Direction2.Up | Direction2.Right,
        /// <summary>
        /// The down and right direction, or +x, +y.
        /// </summary>
        [EnumMember]
        DownRight = Direction2.Down | Direction2.Right,
        /// <summary>
        /// The up and left direction, or -x, -y.
        /// </summary>
        [EnumMember]
        UpLeft = Direction2.Up | Direction2.Left,
        /// <summary>
        /// The down and left direction, or -x, +y.
        /// </summary>
        [EnumMember]
        DownLeft = Direction2.Down | Direction2.Left

    }

    /// <summary>
    /// A set of helper and extension methods for dealing with <see cref="Direction2"/>
    /// </summary>
    public static class Direction2Helper {

        /// <summary>
        /// All <see cref="Direction2"/> enum values
        /// </summary>
        public static readonly Direction2[] All = {
            Direction2.None, Direction2.Up, Direction2.Right, Direction2.Down, Direction2.Left,
            Direction2.UpRight, Direction2.DownRight, Direction2.UpLeft, Direction2.DownLeft
        };
        /// <summary>
        /// The <see cref="Direction2.Up"/> through <see cref="Direction2.Left"/> directions
        /// </summary>
        public static readonly Direction2[] Adjacent = Direction2Helper.All.Where(Direction2Helper.IsAdjacent).ToArray();
        /// <summary>
        /// The <see cref="Direction2.UpRight"/> through <see cref="Direction2.UpLeft"/> directions
        /// </summary>
        public static readonly Direction2[] Diagonals = Direction2Helper.All.Where(Direction2Helper.IsDiagonal).ToArray();
        /// <summary>
        /// All directions except <see cref="Direction2.None"/>
        /// </summary>
        public static readonly Direction2[] AllExceptNone = Direction2Helper.All.Where(dir => dir != Direction2.None).ToArray();

        private static readonly Direction2[] Clockwise = {Direction2.Up, Direction2.UpRight, Direction2.Right, Direction2.DownRight, Direction2.Down, Direction2.DownLeft, Direction2.Left, Direction2.UpLeft};
        private static readonly Dictionary<Direction2, int> ClockwiseLookup = Direction2Helper.Clockwise.Select((d, i) => (d, i)).ToDictionary(kv => kv.d, kv => kv.i);

        /// <summary>
        /// Returns if the given direction is considered an "adjacent" direction.
        /// An adjacent direction is one that is not a diagonal.
        /// </summary>
        /// <param name="dir">The direction to query</param>
        /// <returns>Whether the direction is adjacent</returns>
        public static bool IsAdjacent(this Direction2 dir) {
            return dir == Direction2.Up || dir == Direction2.Right || dir == Direction2.Down || dir == Direction2.Left;
        }

        /// <summary>
        /// Returns if the given direction is considered a diagonal direction.
        /// </summary>
        /// <param name="dir">The direction to query</param>
        /// <returns>Whether the direction is diagonal</returns>
        public static bool IsDiagonal(this Direction2 dir) {
            return dir == Direction2.UpRight || dir == Direction2.DownRight || dir == Direction2.UpLeft || dir == Direction2.DownLeft;
        }

        /// <summary>
        /// Returns the directional offset of a given direction.
        /// The offset direction will be exactly one unit in each axis that the direction represents.
        /// </summary>
        /// <param name="dir">The direction whose offset to query</param>
        /// <returns>The direction's offset</returns>
        public static Point Offset(this Direction2 dir) {
            switch (dir) {
                case Direction2.Up:
                    return new Point(0, -1);
                case Direction2.Right:
                    return new Point(1, 0);
                case Direction2.Down:
                    return new Point(0, 1);
                case Direction2.Left:
                    return new Point(-1, 0);
                case Direction2.UpRight:
                    return new Point(1, -1);
                case Direction2.DownRight:
                    return new Point(1, 1);
                case Direction2.DownLeft:
                    return new Point(-1, 1);
                case Direction2.UpLeft:
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
                case Direction2.Up:
                    return Direction2.Down;
                case Direction2.Right:
                    return Direction2.Left;
                case Direction2.Down:
                    return Direction2.Up;
                case Direction2.Left:
                    return Direction2.Right;
                case Direction2.UpRight:
                    return Direction2.DownLeft;
                case Direction2.DownRight:
                    return Direction2.UpLeft;
                case Direction2.DownLeft:
                    return Direction2.UpRight;
                case Direction2.UpLeft:
                    return Direction2.DownRight;
                default:
                    return Direction2.None;
            }
        }

        /// <summary>
        /// Returns the angle of the direction in radians, where <see cref="Direction2.Right"/> has an angle of 0.
        /// </summary>
        /// <param name="dir">The direction whose angle to get</param>
        /// <returns>The direction's angle</returns>
        public static float Angle(this Direction2 dir) {
            var off = dir.Offset();
            return (float) Math.Atan2(off.Y, off.X);
        }

        /// <summary>
        /// Rotates the given direction clockwise and returns the resulting direction.
        /// </summary>
        /// <param name="dir">The direction to rotate</param>
        /// <param name="fortyFiveDegrees">Whether to rotate by 45 degrees. If this is false, the rotation is 90 degrees instead.</param>
        /// <returns>The rotated direction</returns>
        public static Direction2 RotateCw(this Direction2 dir, bool fortyFiveDegrees = false) {
            if (!Direction2Helper.ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return Direction2.None;
            return Direction2Helper.Clockwise[(dirIndex + (fortyFiveDegrees ? 1 : 2)) % Direction2Helper.Clockwise.Length];
        }

        /// <summary>
        /// Rotates the given direction counter-clockwise and returns the resulting direction.
        /// </summary>
        /// <param name="dir">The direction to rotate counter-clockwise</param>
        /// <param name="fortyFiveDegrees">Whether to rotate by 45 degrees. If this is false, the rotation is 90 degrees instead.</param>
        /// <returns>The rotated direction</returns>
        public static Direction2 RotateCcw(this Direction2 dir, bool fortyFiveDegrees = false) {
            if (!Direction2Helper.ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return Direction2.None;
            var index = dirIndex - (fortyFiveDegrees ? 1 : 2);
            return Direction2Helper.Clockwise[index < 0 ? index + Direction2Helper.Clockwise.Length : index];
        }

        /// <summary>
        /// Returns the <see cref="Direction2"/> that is closest to the given position's facing direction.
        /// </summary>
        /// <param name="offset">The vector whose corresponding direction to get</param>
        /// <returns>The vector's direction</returns>
        public static Direction2 ToDirection(this Vector2 offset) {
            var offsetAngle = (float) Math.Atan2(offset.Y, offset.X);
            foreach (var dir in Direction2Helper.AllExceptNone) {
                if (Math.Abs(dir.Angle() - offsetAngle) <= MathHelper.PiOver4 / 2)
                    return dir;
            }
            return Direction2.None;
        }

        /// <summary>
        /// Returns the <see cref="Direction2"/> that is closest to the given position's facing direction, only taking <see cref="Adjacent"/> directions into account.
        /// Diagonal directions will be rounded to the nearest vertical direction.
        /// </summary>
        /// <param name="offset">The vector whose corresponding direction to get</param>
        /// <returns>The vector's direction</returns>
        public static Direction2 To90Direction(this Vector2 offset) {
            if (offset.X == 0 && offset.Y == 0)
                return Direction2.None;
            if (Math.Abs(offset.X) > Math.Abs(offset.Y))
                return offset.X > 0 ? Direction2.Right : Direction2.Left;
            return offset.Y > 0 ? Direction2.Down : Direction2.Up;
        }

        /// <summary>
        /// Rotates the given direction by a given reference direction
        /// </summary>
        /// <param name="dir">The direction to rotate</param>
        /// <param name="reference">The direction to rotate by</param>
        /// <param name="start">The direction to use as the default direction</param>
        /// <returns>The direction, rotated by the reference direction</returns>
        public static Direction2 RotateBy(this Direction2 dir, Direction2 reference, Direction2 start = Direction2.Up) {
            if (!Direction2Helper.ClockwiseLookup.TryGetValue(reference, out var refIndex))
                return Direction2.None;
            if (!Direction2Helper.ClockwiseLookup.TryGetValue(start, out var startIndex))
                return Direction2.None;
            if (!Direction2Helper.ClockwiseLookup.TryGetValue(dir, out var dirIndex))
                return Direction2.None;
            var diff = refIndex - startIndex;
            if (diff < 0)
                diff += Direction2Helper.Clockwise.Length;
            return Direction2Helper.Clockwise[(dirIndex + diff) % Direction2Helper.Clockwise.Length];
        }

    }
}
