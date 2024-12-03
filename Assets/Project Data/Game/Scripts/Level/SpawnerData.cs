using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class SpawnerData
    {
        // Using as ID
        [SerializeField] ElementPosition elementPosition;
        public ElementPosition ElementPosition => elementPosition;

        [SerializeField] Direction direction;
        public Direction Direction => direction;

        [SerializeField] LevelElement.Type[] spawnQueue;
        public LevelElement.Type[] SpawnQueue => spawnQueue;
    }
}
