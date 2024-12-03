using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class LevelElement
    {
        [SerializeField] Type elementType;
        public Type ElementType => elementType;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        private Pool pool;
        public Pool Pool => pool;

        public void Initialise()
        {
            pool = new Pool(new PoolSettings(elementType.ToString(), prefab, 1, true));
        }

        public void Unload()
        {
            pool.ReturnToPoolEverything(true);
        }

        public static bool IsCharacterElement(Type type)
        {
            int typeID = (int)type;

            return typeID >= 10 && typeID < 100;
        }

        public enum Type
        {
            Empty = 0,
            Wall = 1,

            // Use ids in range 10-100 for active blocks
            Block_Red = 10,
            Block_Green = 11,
            Block_Pink = 12,
            Block_Blue = 13,
            Block_Yellow = 14,
            Block_Teal = 15,
            Block_Purple = 16,

            Spawner = 100,
        }
    }
}
