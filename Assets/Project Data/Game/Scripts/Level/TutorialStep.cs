using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.BusStop
{
    [System.Serializable]
    public class TutorialStep
    {
        [SerializeField] ElementPosition elementPosition;
        public ElementPosition ElementPosition => elementPosition;
    }
}