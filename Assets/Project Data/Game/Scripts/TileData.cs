using UnityEngine;

namespace Watermelon.BusStop
{
    [CreateAssetMenu(fileName = "Tile Data", menuName = "Content/Data/Tiles")]
    public class TileData : ScriptableObject
    {
        public GameObject cellPrefab;
        public GameObject borderPrefab;
        public GameObject innerCornerPrefab;
        public GameObject outerCornerPrefab;
    }
}