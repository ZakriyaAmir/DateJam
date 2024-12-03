using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public struct ElementPosition
    {
        private static readonly int[] DIRECTION_OFFSET_X = new int[4] { 0, 1, 0, -1 };
        private static readonly int[] DIRECTION_OFFSET_Y = new int[4] { -1, 0, 1, 0 };

        [SerializeField] int x;
        public int X => x;

        [SerializeField] int y;
        public int Y => y;

        public ElementPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public ElementPosition(ElementPosition position)
        {
            x = position.x;
            y = position.y;
        }

        public static ElementPosition operator +(ElementPosition a, ElementPosition b) => new ElementPosition(a.X + b.X, a.Y + b.Y);

        public bool Equals(ElementPosition other)
        {
            return (x == other.x && y == other.y);
        }

        public bool Equals(int x, int y)
        {
            return (this.x == x && this.y == y);
        }

        public override int GetHashCode()
        {
            return ((x + y) * (x + y + 1) / 2) + y;
        }

        public override string ToString()
        {
            return string.Format("({0}:{1})", x, y);
        }

        public ElementPosition[] GetNeighbors()
        {
            ElementPosition[] neighbors = new ElementPosition[4];
            neighbors[0] = this + GetOffset(Direction.Forward);
            neighbors[1] = this + GetOffset(Direction.Right);
            neighbors[2] = this + GetOffset(Direction.Back);
            neighbors[3] = this + GetOffset(Direction.Left);

            return neighbors;
        }

        public static ElementPosition GetOffset(Direction direction)
        {
            int directionIndex = (int)direction;

            return new ElementPosition(DIRECTION_OFFSET_X[directionIndex], DIRECTION_OFFSET_Y[directionIndex]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ElementPosition operator +(ElementPosition first, int2 second) => new ElementPosition { x = first.x + second.x, y = first.y + second.y };
    }
}
