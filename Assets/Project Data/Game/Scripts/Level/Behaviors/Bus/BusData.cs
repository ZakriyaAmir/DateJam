using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.BusStop;

namespace Watermelon
{
    public class BusData : MonoBehaviour
    {
        public List<BusPrefab> busData;

        [System.Serializable]
        public class BusPrefab
        {
            public GameObject prefab;
            public LevelElement.Type type;
        }
    }
}