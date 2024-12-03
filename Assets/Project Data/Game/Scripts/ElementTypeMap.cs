using System.Runtime.CompilerServices;

namespace Watermelon.BusStop
{
    public class ElementTypeMap
    {
        private LevelElement.Type[,] map;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public LevelElement.Type this[int x, int y]
        {
            get => map[x, y];
            set => map[x, y] = value;
        }

        public LevelElement.Type this[ElementPosition pos]
        {
            get => map[pos.X, pos.Y];
            set => map[pos.X, pos.Y] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ElementTypeMap(int width, int height)
        {
            Width = width;
            Height = height;

            map = new LevelElement.Type[width, height];
        }
    }
}