using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Lives Data", menuName = "Content/Data/Lives")]
    public class LivesData : ScriptableObject
    {
        public int maxLivesCount = 5;
        [Tooltip("In seconds")]public int oneLifeRestorationDuration = 1200;

        [Space]
        public string fullText = "FULL!";
        public string timespanFormat = "{0:mm\\:ss}";
        public string longTimespanFormat = "{0:hh\\:mm\\:ss}";
    }
}