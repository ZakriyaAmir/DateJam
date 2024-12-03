using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.Map
{
    [CreateAssetMenu(menuName = "Content/Data/Map", fileName = "Map Data")]
    public class MapData : ScriptableObject
    {
        public List<GameObject> chunks;
        public float currentLevelVerticalOffset = 0.4f;
        public float firstChunkMaxLevelVerticalOffset = 0.2f;
    }
}