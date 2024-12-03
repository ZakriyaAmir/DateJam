using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Watermelon.BusStop.LevelData;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class ElementData
    {
        [SerializeField] LevelElement.Type elementType;
        public LevelElement.Type ElementType => elementType;

        [SerializeField] ElementPosition elementPosition;
        public ElementPosition ElementPosition => elementPosition;

        [SerializeField] SpecialEffectType specialEffectType;
        public SpecialEffectType SpecialEffectType => specialEffectType;

        public int temp;

        public ElementData(LevelElement.Type elementType, ElementPosition elementPosition, SpecialEffectType specialEffectType)
        {
            this.elementType = elementType;
            this.elementPosition = elementPosition;
            this.specialEffectType = specialEffectType;
        }
    }
}
