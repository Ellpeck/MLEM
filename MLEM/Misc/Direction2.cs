using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace MLEM.Misc {
    public enum Direction2 {

        Up,
        Right,
        Down,
        Left,

        UpRight,
        DownRight,
        DownLeft,
        UpLeft,

        None

    }

    public static class Direction2Helper {

        public static readonly Direction2[] All = EnumHelper.GetValues<Direction2>().ToArray();
        public static readonly Direction2[] Adjacent = All.Where(IsAdjacent).ToArray();
        public static readonly Direction2[] Diagonals = All.Where(IsDiagonal).ToArray();
        public static readonly Direction2[] AllExceptNone = All.Where(dir => dir != Direction2.None).ToArray();

        public static bool IsAdjacent(this Direction2 dir) {
            return dir <= Direction2.Left;
        }

        public static bool IsDiagonal(this Direction2 dir) {
            return dir >= Direction2.UpRight && dir <= Direction2.UpLeft;
        }

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

        public static IEnumerable<Point> Offsets(this IEnumerable<Direction2> directions) {
            foreach (var dir in directions)
                yield return dir.Offset();
        }

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

    }
}